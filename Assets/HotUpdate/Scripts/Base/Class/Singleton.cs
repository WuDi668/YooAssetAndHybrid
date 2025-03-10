using UnityEngine;

public abstract class Singleton<T> where T : class,new()
{
    private static T _instance = null;
    private static readonly object padlock = new object();
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }

    public Singleton()
    {
        if(_instance != null)
        {
            Debug.LogError(string.Format("单例类{0}不为空",typeof(T).ToString()));
        }
        Init();
    }

    public virtual void Init() { }
}
