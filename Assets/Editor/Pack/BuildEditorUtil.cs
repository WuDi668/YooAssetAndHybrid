using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Commands;
using YooAsset.Editor;
using YooAsset;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public static class BuildEditorUtil
{
    /// <summary>
    /// 热更包构建流程：
    /// 1.HybridCLR构建热更DLL并复制DLL到对应的资源文件夹
    /// 2.构建补丁包（None）
    /// 3.补丁包上传至服务器
    /// 
    /// 整包构建流程：
    /// 1.HybridCLR构建所有DLL并复制DLL到对应的资源文件夹
    /// 2.构建补丁包（ClearAndCopyAll）
    /// 3.构建整包
    /// </summary>


    #region Dll
    [MenuItem("Tools/Build/HotUpdate Dll", priority = 100)]
    public static void BuildHotUpdateDll()
    {
        Debug.Log("[Unity] =======开始构建热更Dll=======");
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);
        Debug.Log("[Unity] =======热更DLL生成完毕，开始复制=======");
        FileEditorUtil.CopyAllDll();
        Debug.Log("[Unity] =======复制完毕=======");
    }

    [MenuItem("Tools/Build/All Dll", priority = 100)]
    public static void BuildAllDll()
    {
        Debug.Log("[Unity] =======开始构建Dll=======");
        PrebuildCommand.GenerateAll();
        Debug.Log("[Unity] =======DLL生成完毕，开始复制=======");
        FileEditorUtil.CopyAllDll();
        Debug.Log("[Unity] =======复制完毕=======");
    }
    #endregion

    #region 补丁包

    private static BuildHotUpdateParameters _hotUpdateParam;

    //默认，Jenkins出问题可能导致出包延期的可以直接改这里从Unity出包
    static void GetHotDefaultSetting()
    {
        _hotUpdateParam.PackName = "DefaultPackage";
        _hotUpdateParam.Version = "1.1";
        _hotUpdateParam.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
    }

    //读取命令行参数
    static void GetHotBuildSetting()
    {
        string[] parameters = Environment.GetCommandLineArgs();

        foreach (string s in parameters)
        {

        }
    }

    [MenuItem("Tools/Build/HotUpdate Pack（ClearAndCopyAll）", priority = 200)]
    public static void BuildHotUpdatePack()
    {
        #region 检测Jenkins传参
        GetHotBuildSetting();
        if (string.IsNullOrEmpty(_hotUpdateParam.PackName))
        {
            Debug.Log("[Unity] 未检测到Jenkins传参，将使用默认参数");
            GetHotDefaultSetting();
        }
        #endregion

        Debug.Log($"[Unity] 开始构建 : {_hotUpdateParam.BuildTarget}");

        var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

        //清空Bundles下的旧资源
        FileEditorUtil.FileClear(buildoutputRoot + "/" + EditorUserBuildSettings.activeBuildTarget.ToString());

        //构建参数
        BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
        buildParameters.BuildOutputRoot = buildoutputRoot;
        buildParameters.BuildinFileRoot = streamingAssetsRoot;
        buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
        buildParameters.BuildTarget = _hotUpdateParam.BuildTarget;
        buildParameters.PackageName = _hotUpdateParam.PackName;
        buildParameters.PackageVersion = _hotUpdateParam.Version;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
        buildParameters.FileNameStyle = EFileNameStyle.BundleName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
        buildParameters.BuildinFileCopyParams = string.Empty;
        //buildParameters.EncryptionServices = CreateEncryptionInstance(); //暂时不加密
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
        buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！

        // 执行构建
        BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            Debug.Log($"[Unity] 构建成功 : {buildResult.OutputPackageDirectory}");
        }
        else
        {
            Debug.LogError($"[Unity] 构建失败 : {buildResult.ErrorInfo}");
        }
    }

    [MenuItem("Tools/Build/HotUpdate Pack（None）", priority = 200)]
    public static void BuildHotUpdatePackAlone()
    {
        #region 检测Jenkins传参
        GetHotBuildSetting();
        if (string.IsNullOrEmpty(_hotUpdateParam.PackName))
        {
            Debug.Log("[Unity] 未检测到Jenkins传参，将使用默认参数");
            GetHotDefaultSetting();
        }
        #endregion

        Debug.Log($"[Unity] 开始构建 : {_hotUpdateParam.BuildTarget}");

        var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

        //清空Bundles下的旧资源
        FileEditorUtil.FileClear(buildoutputRoot + "/" + EditorUserBuildSettings.activeBuildTarget.ToString());

        //构建参数
        BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
        buildParameters.BuildOutputRoot = buildoutputRoot;
        buildParameters.BuildinFileRoot = streamingAssetsRoot;
        buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
        buildParameters.BuildTarget = _hotUpdateParam.BuildTarget;
        buildParameters.PackageName = _hotUpdateParam.PackName;
        buildParameters.PackageVersion = _hotUpdateParam.Version;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
        buildParameters.FileNameStyle = EFileNameStyle.BundleName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
        buildParameters.BuildinFileCopyParams = string.Empty;
        //buildParameters.EncryptionServices = CreateEncryptionInstance(); //暂时不加密
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
        buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！

        // 执行构建
        BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            Debug.Log($"[Unity] 构建成功 : {buildResult.OutputPackageDirectory}");
        }
        else
        {
            Debug.LogError($"[Unity] 构建失败 : {buildResult.ErrorInfo}");
        }
    }
    #endregion

    #region 整包

    private static BuildParameters _buildParm;

    static void GetDefaultSetting()
    {
        _buildParm.productName = "ProductName";
        _buildParm.companyName = "CompanyName";
        _buildParm.bundleVersion = "1.0";
        _buildParm.applicationIdentifier = "Default";
        _buildParm.bundleVersionCode = 180;
        _buildParm.minSdkVersion = 21;
        _buildParm.targetSdkVersion = 30;
        _buildParm.locationPathName = @"E:\AutoPack\testpack";
        _buildParm.isDevelopment = "false";
    }

    //读取命令行参数
    static void GetBuildSetting()
    {
        string[] parameters = Environment.GetCommandLineArgs();

        foreach (string s in parameters)
        {

        }
    }

    [MenuItem("Tools/Build/BuildPack", priority = 300)]
    public static void BuildPack()
    {
        #region 检测Jenkins传参
        GetBuildSetting();
        if (string.IsNullOrEmpty(_buildParm.productName))
        {
            Debug.Log("[Unity] 未检测到Jenkins传参，将使用默认参数");
            GetDefaultSetting();
        }
        #endregion

        BuildPlayerOptions options = new BuildPlayerOptions();

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenePaths = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            scenePaths[i] = SceneUtility.GetScenePathByBuildIndex(i);
        }

        PlayerSettings.productName = _buildParm.productName;//工程名
        PlayerSettings.companyName = _buildParm.companyName;//公司名
        PlayerSettings.bundleVersion = _buildParm.bundleVersion;//版本号
        PlayerSettings.applicationIdentifier = _buildParm.applicationIdentifier;//包名
        PlayerSettings.muteOtherAudioSources = false;//允许后台播放音乐
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);//设置编码模式

        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;//架构
        PlayerSettings.Android.bundleVersionCode = _buildParm.bundleVersionCode;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)_buildParm.minSdkVersion;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)_buildParm.targetSdkVersion;
#if UNITY_ANDROID
        //密钥视情况而定填写内容 如果没有可以不设置
        Debug.Log("[Unity]构建Android端");
        PlayerSettings.Android.keystoreName = Directory.GetParent(Directory.GetCurrentDirectory()) + @"\tools\keystore\key.keystore"; 
        PlayerSettings.Android.keystorePass = "MyKey";
        PlayerSettings.Android.keyaliasName = "key";
        PlayerSettings.Android.keyaliasPass = "keyPass";
        Debug.Log("[Unity]Android密钥路径：" + Directory.GetParent(Directory.GetCurrentDirectory()) + @"\tools\keystore\key.keystore");
        Debug.Log("[Unity]Android密钥密码：" + PlayerSettings.Android.keystorePass);

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle; //设定成Gradle模式
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;//设置导出安卓工程

        options.target = BuildTarget.Android;
#elif UNITY_IOS || UNITY_IPHONE
        Debug.Log("[Unity]构建IOS端");
        options.target = BuildTarget.iOS;
#else
        Debug.Log("[Unity]构建PC端");
        options.target = BuildTarget.StandaloneWindows64;
#endif
        options.scenes = scenePaths;
        options.options = BuildOptions.None;
        options.locationPathName = $@"{_buildParm.locationPathName}";

        BuildPipeline.BuildPlayer(options);

        Debug.Log("[Unity] 工程已导出");
    }

    #endregion

    #region 补丁包流程
    [MenuItem("Tools/Build/Build HotPack Flow", priority = 400)]
    public static void BuildHotFlow()
    {
        BuildHotUpdateDll();
        BuildHotUpdatePackAlone();
    }
    #endregion

    #region 整包流程
    [MenuItem("Tools/Build/Build Pack Flow", priority = 400)]
    public static void BuildFlow()
    {
        BuildAllDll();
        BuildHotUpdatePack();
        BuildPack();
    }
    #endregion

    #region 参数
    public struct BuildHotUpdateParameters
    {
        public string PackName;
        public string Version;
        public BuildTarget BuildTarget;
    }

    public struct BuildParameters
    {
        public string productName;
        public string companyName;
        public string bundleVersion;
        public string applicationIdentifier;
        public int bundleVersionCode;
        public int minSdkVersion;
        public int targetSdkVersion;
        public string locationPathName;
        public string isDevelopment;
    }
    #endregion
}
