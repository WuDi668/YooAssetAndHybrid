using Unity.VisualScripting;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType(typeof(T)) as T;
                //仍为空，创建
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).ToString());
                    _instance = obj.AddComponent<T>();
                }
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }
}
