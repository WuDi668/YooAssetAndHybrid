using System.Collections;
using UnityEngine;
using YooAsset;

public class Main : SingletonMono<Main>
{
    private ResourcePackage _package;
    public ResourcePackage Package { get => _package; }

    void Start()
    {
        LoadDllUtil.Instance.ActiveHotDll();
    }
    public void StartGame()
    {
        LoadPackage();
        TestCube();
    }

    private void LoadPackage()
    {
        _package = YooAssets.GetPackage("DefaultPackage");
    }

    private void TestCube()
    {
        StartCoroutine(TestCubeCoroutine());
    }

    IEnumerator TestCubeCoroutine()
    {
        var handle = _package.LoadAssetAsync<GameObject>("HotTest");
        yield return handle;
        handle.Completed += Handle_Completed;
    }

    private void Handle_Completed(AssetHandle obj)
    {
        Debug.Log("准备实例化");
        GameObject go = obj.InstantiateSync();
        Debug.Log($"Prefab name is {go.name}");
    }
}
