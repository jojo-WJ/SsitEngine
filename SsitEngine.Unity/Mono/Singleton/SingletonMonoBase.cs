using SsitEngine.Unity.Msic;
using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     继承Mono的单列
    /// </summary>
    public abstract class SingletonMonoBase : SsitMonoBase
    {
        public static bool IsApplicationQuit { get; set; } = false;

        public static T CreateMonoSingleton<T>() where T : MonoBase, ISingleton
        {
            if (IsApplicationQuit)
            {
                return null;
            }
            T instance = null;
            var managers = FindObjectsOfType(typeof(T)) as T[];
            if (managers != null && managers.Length != 0)
            {
                if (managers.Length == 1)
                {
                    instance = managers[0];
                    instance.gameObject.name = typeof(T).Name;
                    instance.OnSingletonInit();
                    return instance;
                }
                Debug.LogError("You have more than one " + typeof(T).Name +
                               " in the scene. You only need 1, it's a singleton!");
                foreach (var manager in managers)
                {
                    Destroy(manager.gameObject);
                }
            }
            var gO = new GameObject(typeof(T).Name, typeof(T));
            instance = gO.GetComponent<T>();
            instance.OnSingletonInit();
            DontDestroyOnLoad(gO);
            return instance;
        }
    }
}