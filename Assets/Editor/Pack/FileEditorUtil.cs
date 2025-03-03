using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Codice.Client.BaseCommands;
using UnityEditor;
using YooAsset;
using System;

public static class FileEditorUtil
{
    /// <summary>
    /// 文件操作类
    /// </summary>

    #region 热更Dll复制
    static List<string> aotList = new List<string>()
    {
           "mscorlib.dll",
           "System.Core.dll",
           "System.dll",
    };

    static List<string> hotUpdateList = new List<string>()
    {
           "HotUpdate.dll"
    };

    [MenuItem("Tools/File/Copy All Dll To Project")]
    public static void CopyAllDll()
    {
        //复制aot文件
        foreach (var fileName in aotList)
        {
            CopyHotUpdateFile(fileName, true);
        }
        //复制热更文件
        foreach (var fileName in hotUpdateList)
        {
            CopyHotUpdateFile(fileName, false);
        }
    }

    [MenuItem("Tools/File/Copy AOT Dll To Project")]
    public static void CopyAOTDll()
    {
        //复制aot文件
        foreach (var fileName in aotList)
        {
            CopyHotUpdateFile(fileName, true);
        }
    }

    [MenuItem("Tools/File/Copy HotUpdate Dll To Project")]
    public static void CopyHotUpdateDll()
    {
        //复制热更文件
        foreach (var fileName in hotUpdateList)
        {
            CopyHotUpdateFile(fileName, false);
        }
    }

    private static void CopyHotUpdateFile(string fileFullName, bool isAot)
    {
        string sourceFilePath = GetCompileFilePath(fileFullName, isAot);

        string rootFolder = GetRootPath();

        string destinationPath = string.Empty;
        string newFileName = string.Empty;
        string newFilePath = string.Empty;

        destinationPath =  $"{rootFolder}/Assets/GameAssets/Scripts/Dll";
        Debug.Log(destinationPath);
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        newFileName = Path.GetFileName(sourceFilePath) + ".bytes";
        newFilePath = $"{destinationPath}/{newFileName}";
        Debug.Log(newFilePath);
        File.Copy(sourceFilePath, newFilePath, true);
        AssetDatabase.Refresh();

        string log = $"文件 {fileFullName} 已成功复制到 {newFilePath} 并修改后缀名为 {newFileName}";
        Debug.Log(log);
    }

    private static string GetCompileFilePath(string fileName, bool isAot)
    {
        string path = "";
        string rootFolder = GetRootPath();
        string platformName = GetPlatformName();
        path = isAot ? $"{rootFolder}/HybridCLRData/AssembliesPostIl2CppStrip/{platformName}/{fileName}" : $"{rootFolder}/HybridCLRData/HotUpdateDlls/{platformName}/{fileName}";
        return path;
    }

    private static string GetRootPath()
    {
        string rootFolder = Directory.GetParent(Application.dataPath)?.FullName;
        rootFolder = rootFolder.Replace("\\", "/");
        return rootFolder;
    }

    private static string GetPlatformName()
    {
        BuildTarget activePlatform = EditorUserBuildSettings.activeBuildTarget;
        string platformName = activePlatform.ToString();
        return platformName;
    }
    #endregion

    #region 文件操作
    public static void FileClear(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("不存在的文件夹路径：" + path);
            return;
        }

        try {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo item in files)
            {
                if(item is DirectoryInfo)
                {
                    DirectoryInfo subdir = new DirectoryInfo(item.FullName);
                    subdir.Delete(true);
                }
                else
                {
                    File.Delete(item.FullName);
                }
            }
            Debug.Log("已清空路径：" + path);
        } 
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    #endregion
}
