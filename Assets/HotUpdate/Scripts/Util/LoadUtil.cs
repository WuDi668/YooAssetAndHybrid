using System;
using UnityEngine;
using YooAsset;

public static class LoadUtil
{
    /// <summary>
    /// 资源加载类 Enable Addressable已开启 支持文件名称/完整资源路径加载
    /// 功能：
    /// 1.游戏/热更/配置资源加载
    /// 2.检测资源是否需要更新下载
    /// 3.获取资源信息列表
    /// </summary>

    private static ResourcePackage _package;
    public static ResourcePackage Package { get => _package; }

    public static void LoadDefaultPackage()
    {
        _package = YooAssets.GetPackage("DefaultPackage");
    }

    public static void LoadAssetAsync<T>(string path,Action<T> callback) where T : UnityEngine.Object
    {
        AssetHandle handle = _package.LoadAssetAsync<T>(path);
        handle.Completed += (obj) => {
            callback(obj.AssetObject as T);
        };
    }

    public static void LoadAssetAsync<T>(string path, Action<AssetHandle> callback) where T : UnityEngine.Object
    {
        AssetHandle handle = _package.LoadAssetAsync<T>(path);
        handle.Completed += (obj) => {
            callback(obj);
        };
    }
}
