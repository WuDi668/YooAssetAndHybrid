using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 事件总线
/// 1.对于不能继承/接口的静态类，可单独定义data类和handle类
/// 2.data类集中在EventData中
/// </summary>
public class EventBus : Singleton<EventBus>
{
    private readonly Dictionary<Type, List<Action<EventData>>> _events = new();

    public override void Init()
    {
        
    }
    #region 订阅
    public void Subscribe<TEventData>(Action<EventData> action)
    {
        if (!_events.ContainsKey(typeof(TEventData)))
        {
            _events.Add(typeof(TEventData), new List<Action<EventData>>() { action });
            return;
        }

        List<Action<EventData>> handlerActions = _events[typeof(TEventData)];
        if (!handlerActions.Contains(action))
        {
            handlerActions.Add(action);
            _events[typeof(TEventData)] = handlerActions;
        }
    }

    public void UnSubscribe<TEventData>(Action<EventData> action)
    {
        if (!_events.ContainsKey(typeof(TEventData)))
        {
            Debug.LogWarning("不存在的事件" + typeof(TEventData).ToString());
            return;
        }
        List<Action<EventData>> handlerActions = _events[typeof(TEventData)];
        if (handlerActions.Contains(action))
        {
            handlerActions.Remove(action);
            _events[typeof(TEventData)] = handlerActions;
        }
    }
    #endregion

    #region 发布
    public void Publish<TEventData>(EventData eventData) where TEventData : IEventData
    {
        if (!_events.ContainsKey(typeof(TEventData)))
        {
            Debug.LogWarning("不存在的事件" + typeof(TEventData).ToString());
            return;
        }

        List<Action<EventData>> handlers = _events[eventData.GetType()];
        if (handlers != null && handlers.Count > 0)
        {
            foreach (var handler in handlers)
            {
                handler.Invoke(eventData);
            }
        }
    }

    public void ClearPublish<TEventData>(EventData eventData) where TEventData : IEventData
    {
        if (!_events.ContainsKey(typeof(TEventData)))
        {
            Debug.LogWarning("不存在的事件" + typeof(TEventData).ToString());
            return;
        }

        _events.Remove(eventData.GetType());
    }

    public void ClearAll()
    {
        _events.Clear();
    }
    #endregion


}
