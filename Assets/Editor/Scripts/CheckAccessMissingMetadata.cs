using HybridCLR.Editor.HotUpdate;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

public class CheckAccessMissingMetadata
{
    [MenuItem("CheckAccessMissingMetadata/Run")]
    public static void CheckProjectAccessMissingMetadata()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        // aotDir指向 构建主包时生成的裁剪aot dll目录，而不是最新的SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录。
        // 一般来说，发布热更新包时，由于中间可能调用过generate/all，SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录中包含了最新的aot dll，
        // 肯定无法检查出类型或者函数裁剪的问题。
        // 需要在构建完主包后，将当时的aot dll保存下来，供后面补充元数据或者裁剪检查。
        string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);

        // 第2个参数hotUpdateAssNames为热更新程序集列表。对于旗舰版本，该列表需要包含DHE程序集，即SettingsUtil.HotUpdateAndDHEAssemblyNamesIncludePreserved。
        var checker = new MissingMetadataChecker(aotDir, SettingsUtil.HotUpdateAssemblyNamesIncludePreserved);

        string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotUpdateDir}/{dll}";
            bool notAnyMissing = checker.Check(dllPath);
            if (!notAnyMissing)
            {
                // DO SOMETHING
                Debug.Log("未检测到引用被裁剪的类型");
            }
        }
    }

}
