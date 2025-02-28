using UnityEngine;
using YooAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using System.Linq;

public class LoadDllUtil : SingletonMono<LoadDllUtil>
{
    public EPlayMode PlayMode = EPlayMode.HostPlayMode;
    private ResourcePackage _defaultPackage;

    private bool _isReady = false;
    public bool IsReady { get => _isReady; }

    //元数据
    private static List<string> AOTMetaAssemblyFiles { get; } = new() { 
        "mscorlib.dll", 
        "System.dll", 
        "System.Core.dll"
    };
    private static Dictionary<string, TextAsset> s_assetDatas = new Dictionary<string, TextAsset>();
    private static Assembly _hotUpdateAss;

    #region 初始化
    IEnumerator InitYooAssets(Action onDownloadComplete)
    {
        YooAssets.Initialize();

        string packageName = "DefaultPackage";
        var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
        YooAssets.SetDefaultPackage(package);

        
        if(PlayMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            var initParameters = new EditorSimulateModeParameters();
            initParameters.EditorFileSystemParameters = editorFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;
            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
                StartCoroutine(RequestPackageVersion(package, onDownloadComplete));
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }else if(PlayMode == EPlayMode.HostPlayMode){
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            var initParameters = new HostPlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            initParameters.CacheFileSystemParameters = cacheFileSystemParams;

            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;
            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
                StartCoroutine(RequestPackageVersion(package, onDownloadComplete));
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }else if(PlayMode == EPlayMode.OfflinePlayMode){
            var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
                StartCoroutine(RequestPackageVersion(package, onDownloadComplete));
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }else if(PlayMode == EPlayMode.WebPlayMode)
        {
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
            var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载

            var initParameters = new WebPlayModeParameters();
            initParameters.WebServerFileSystemParameters = webServerFileSystemParams;
            initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;

            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
                StartCoroutine(RequestPackageVersion(package, onDownloadComplete));
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }
    }
    #endregion

    #region 获取版本和热更资源
    IEnumerator RequestPackageVersion(ResourcePackage package, Action onDownloadComplete)
    {
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //更新成功
            string packageVersion = operation.PackageVersion;
            Debug.Log($"Request package Version : {packageVersion}");
            StartCoroutine(UpdatePackageManifest(package, packageVersion, onDownloadComplete)); //开始更新资源清单
        }
        else
        {
            //更新失败
            Debug.LogError("版本信息获取失败：" + operation.Error);
        }
    }

    IEnumerator UpdatePackageManifest(ResourcePackage package, string packageVersion, Action onDownloadComplete)
    {
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("资源清单更新成功");
        }
        else
        {
            Debug.LogError("资源清单更新失败：" + operation.Error);
        }

        yield return Download(package);

        //拼接Dll字符串
        var assets = new List<string> { "HotUpdate.dll" }.Concat(AOTMetaAssemblyFiles);
        foreach (var asset in assets)
        {
            var handle = package.LoadAssetAsync<TextAsset>(asset);
            yield return handle;
            var assetObj = handle.AssetObject as TextAsset;
            s_assetDatas[asset] = assetObj;
            Debug.Log($"dll:{asset}   {assetObj == null}");
        }
        _defaultPackage = package;
        onDownloadComplete?.Invoke();
    }
    #endregion

    #region 下载资源
    IEnumerator Download(ResourcePackage package)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;

        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有需要下载的资源");
            yield break;
        }

        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        //注册回调方法
        downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
        downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
        downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
        downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件

        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("下载成功");
        }
        else
        {
            Debug.LogError("下载失败" + downloader.Error);
        }
    }

    #endregion

    #region 下载回调
    private void OnDownloadFileBeginFunction(DownloadFileData data)
    {
        Debug.Log(string.Format("开始下载：包名{0}，文件名：{1}，文件大小：{2}", data.PackageName, data.FileName, data.FileSize));
    }

    private void OnDownloadFinishFunction(DownloaderFinishData data)
    {
        Debug.Log(string.Format("下载完成：包名{0} 状态：{1}", data.PackageName, data.Succeed ? "成功" : "失败"));
    }

    private void OnDownloadUpdateFunction(DownloadUpdateData data)
    {
        Debug.Log(string.Format("更新中：包名{0}，更新进度{1}/{2}", data.PackageName, data.CurrentDownloadBytes, data.TotalDownloadBytes));
    }

    private void OnDownloadErrorFunction(DownloadErrorData data)
    {
        Debug.LogError(string.Format("下载错误：包名{0}，文件名：{1}", data.PackageName, data.FileName));
    }

    #endregion

    #region 元数据
    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        if (s_assetDatas.ContainsKey(dllName))
        {
            return s_assetDatas[dllName].bytes;
        }

        return Array.Empty<byte>();
    }

    private static void LoadMetadataForAOTAssemblies()
    {
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
    #endregion

    private string GetHostServerURL()
    {
        return "http://127.0.0.1:8848/Project/Test";
    }
    
    #region 开始游戏
    public void ActiveHotDll()
    {
        StartCoroutine(InitYooAssets(LoadHotDll));
    }
    private void LoadHotDll()
    {
        LoadMetadataForAOTAssemblies();
#if !UNITY_EDITOR
        _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets("HotUpdate.dll"));
#else
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        _isReady = true;
        Debug.Log("热更资源准备就绪");
        Main.Instance.StartGame();
    }
    #endregion

}
