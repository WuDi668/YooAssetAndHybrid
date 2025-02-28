using UnityEngine;

public abstract class Singleton<T> where T : class,new()
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    protected Singleton()
    {
        if(_instance == null)
        {
            Debug.LogError(string.Format("单例类{0}为空",typeof(T).ToString()));
            return;
        }
        Init();
    }

    public virtual void Init() { }
}
