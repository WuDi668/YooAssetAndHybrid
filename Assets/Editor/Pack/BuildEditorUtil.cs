using UnityEngine;
using UnityEditor;
using HybridCLR.Editor.Commands;

public static class BuildEditorUtil
{
    [MenuItem("Tools/Build/HotUpdate Dll")]
    public static void BuildHotUpdateDll()
    {
        Debug.Log("=======开始构建热更Dll=======");
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);
        Debug.Log("=======热更DLL生成完毕，开始复制=======");
        FileEditorUtil.CopyAllDll();
        Debug.Log("=======复制完毕=======");
    }

    [MenuItem("Tools/Build/All Dll")]
    public static void BuildAllDll()
    {
        Debug.Log("=======开始构建Dll=======");
        PrebuildCommand.GenerateAll();
        Debug.Log("=======DLL生成完毕，开始复制=======");
        FileEditorUtil.CopyAllDll();
        Debug.Log("=======复制完毕=======");
    }
}
