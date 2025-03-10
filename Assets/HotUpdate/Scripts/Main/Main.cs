using System.Collections;
using UnityEngine;
using YooAsset;

public class Main : SingletonMono<Main>, IEventHandler<EventData>
{
    void Start()
    {
        LoadDllUtil.Instance.ActiveHotDll();
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PackageEventData>(HandleEvent);
    }
    private void OnDisable()
    {
        EventBus.Instance.UnSubscribe<PackageEventData>(HandleEvent);
    }
    public void StartGame()
    {
        LoadUtil.LoadDefaultPackage();
        TestCube();
    }

    private void TestCube()
    {
        Debug.Log("准备实例化");
        GameObject go;
        LoadUtil.LoadAssetAsync<GameObject>("HotTest", (obj) =>
        {
            go = obj.InstantiateSync();
            go.name = go.name.Replace("(Clone)", "");
            Debug.Log($"预制体名称： {go.name}");
        });
    }

    public void HandleEvent(EventData eventData)
    {
        if(eventData is PackageEventData)
        {
            StartGame();
        }
    }
}
