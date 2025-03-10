using System;
using UnityEngine;
using YooAsset;

#region 事件源数据
public interface IEventData
{
    DateTime EventTime { get; set; }
    object EventSource { get; set; }
}

public class EventData : IEventData
{
    public DateTime EventTime { get; set; }
    public object EventSource { get; set; }

    public EventData()
    {
        EventTime = DateTime.Now;
        Init();
    }

    protected virtual void Init() { }
}
#endregion

#region 事件接口
public interface IEventHandler
{

}
/// <summary>
/// <list type="bullet">
/// <item>订阅单个事件，可创建对应的data类和handle类</item>
/// <item>订阅多个事件，事件源接口可传入EventData类，再根据具体的子类分别处理，由于EventData的模糊性，所以无法通过事件总线反射自动绑定，调用Subscribe对各个事件手动订阅</item>
/// <item>继承自MonoBehaviour的挂载脚本仍可通过OnEnable/OnDisable动态订阅和取消订阅</item>
/// </list>
/// </summary>
/// <typeparam name="TEventData"></typeparam>
public interface IEventHandler<TEventData> : IEventHandler where TEventData : IEventData
{
    void HandleEvent(TEventData eventData);
}
#endregion

public class PackageEventData : EventData
{
    public ResourcePackage Package;
    protected override void Init()
    {
        base.Init();
    }
}