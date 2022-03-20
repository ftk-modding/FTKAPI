using System.Reflection;
using HarmonyLib;

namespace FTKAPI.Utils;

internal static class HarmonyExtensions
{
    public static void PatchNested<T>(this Harmony harmony)
    {
        foreach (var nestedType in typeof(T).GetNestedTypes(BindingFlags.NonPublic))
        {
            harmony.CreateClassProcessor(nestedType).Patch();
        }
    }
}