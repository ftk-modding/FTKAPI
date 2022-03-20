using System;

namespace FTKAPI.Utils;

internal static class Utils
{
    public static void SafeInvoke(Action action, Action after)
    {
        try
        {
            action();
            after();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
    }
    
    public static T SafeInvoke<T>(Func<T> action, Action after)
    {
        T r = default;
        try
        {
            r = action();
            after();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
        return r;
    }
}