using System;
using FTKAPI.Utils;

namespace FTKAPI.Managers;


public abstract class BaseManager<T> where T : BaseManager<T>, new() {
    private static Lazy<T> Lazy;

    public static bool IsInited => Lazy.IsValueCreated;

    public static T Instance {
        get {
            if (Lazy == null) {
                Lazy = new(() => Activator.CreateInstance(typeof(T), true) as T);
                Plugin.Log.LogInfo("Initializing " + Lazy.Value.GetType().Name);
                Lazy.Value.Init();
                Plugin.Instance.Managers.Add(Lazy.Value);
            }
            return Lazy.Value;
        }
    }

    internal static void AssertCreated()
    {
        _ = Instance;
    }

    /// <summary>
    ///     Initialize manager class
    /// </summary>
    internal virtual void Init() { }
}