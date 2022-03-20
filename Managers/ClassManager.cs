using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using FTKAPI.Objects;
using FTKAPI.Utils;
using GridEditor;
using HarmonyLib;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Managers;

/// <summary>
///    Manager for handling player classes in the game.
/// </summary>
public class ClassManager : BaseManager<ClassManager> {
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomClass> customDictionary = new();
    public Dictionary<int, CustomClass> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    /// <summary>
    /// Gets a class from TableManager's FTK_playerGameStartDB
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The class's id.</param>
    /// <returns>Returns FTK_playerGameStart</returns>
    public static FTK_playerGameStart GetClass(FTK_playerGameStart.ID id) {
        FTK_playerGameStartDB playerClassesDB = TableManager.Instance.Get<FTK_playerGameStartDB>();
        return playerClassesDB.m_Array[(int)id];
    }

    /// <summary>
    /// Add a custom class.
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="customClass">The custom class to be added.</param>
    /// <param name="plugin">Allows FTKModLib to know which plugin called this method. Not required but recommended to make debugging easier.</param>
    /// <returns>Returns FTK_playerGameStart.ID enum as int</returns>
    public static int AddClass(CustomClass customClass, BaseUnityPlugin plugin = null) {
        if (plugin != null) customClass.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        ClassManager classManager = ClassManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try {
            classManager.enums.Add(customClass.m_ID,
                (int)Enum.GetValues(typeof(FTK_playerGameStart.ID)).Cast<FTK_playerGameStart.ID>().Max() + (classManager.successfulLoads + 1)
            );
            classManager.customDictionary.Add(classManager.enums[customClass.m_ID], customClass);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_playerGameStartDB));
            if (!geDataArrayBase.IsContainID(customClass.m_ID)) {
                geDataArrayBase.AddEntry(customClass.m_ID);
                ((FTK_playerGameStartDB)geDataArrayBase).m_Array[tableManager.Get<FTK_playerGameStartDB>().m_Array.Length - 1] = customClass;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!((FTK_playerGameStartDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_playerGameStartDB>().m_Array.Length - 1)) {
                ((FTK_playerGameStartDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_playerGameStartDB>().m_Array.Length - 1, customClass);
            }

            classManager.successfulLoads++;
            Logger.LogInfo($"Successfully added class '{customClass.ID}' of name '{customClass.Name._en}' from {customClass.PLUGIN_ORIGIN}");
            return classManager.enums[customClass.m_ID];
        }
        catch (Exception e) {
            Logger.LogError(e);
            Logger.LogError($"Failed to add class '{customClass.ID}' of name '{customClass.Name._en}' from {customClass.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    /// <summary>
    /// Modifies a class from TableManager's FTK_playerGameStartDB
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The class's id.</param>
    /// <param name="customClass">The new class to override over.</param>
    /// <returns></returns>
    public static void ModifyClass(FTK_playerGameStart.ID id, CustomClass customClass) {
        FTK_playerGameStartDB playerClassesDB = TableManager.Instance.Get<FTK_playerGameStartDB>();
        playerClassesDB.m_Array[(int)id] = customClass;
        playerClassesDB.m_Dictionary[(int)id] = customClass;
        ClassManager.Instance.moddedDictionary.Add((int)id, customClass);
        Logger.LogInfo($"Successfully modified class '{id}'");
    }

    /// <summary>
    ///    <para>The patches necessary to make adding custom items possible.</para>
    ///    <para>Many of them are used to fix issues calling an invalid 'FTK_playerGameStart.ID' 
    ///    by substituting with 'ClassManager.enums' dictionary value.</para>
    /// </summary>
    class HarmonyPatches {
        [HarmonyPatch(typeof(TableManager), "Initialize")]
        // ReSharper disable once InconsistentNaming
        class TableManager_Initialize_Patch {
            static void Prefix() {
                Logger.LogInfo("Preparing to load custom classes");
                ClassManager.Instance.successfulLoads = 0;
                ClassManager.Instance.enums.Clear();
                ClassManager.Instance.customDictionary.Clear();
                ClassManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_playerGameStartDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_playerGameStartDB_GetEntry_Patch {
            static bool Prefix(ref FTK_playerGameStart __result, FTK_playerGameStart.ID _enumID) {
                if (ClassManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomClass customClass)) {
                    __result = customClass;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_playerGameStartDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_playerGameStartDB_GetIntFromID_Patch {
            static bool Prefix(ref int __result, FTK_playerGameStartDB __instance, string _id) {
                //Attempts to return our enum and calls the original function if it errors.
                if (ClassManager.Instance.enums.ContainsKey(_id)) {
                    __result = ClassManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "RawLuck", MethodType.Getter)]
        // ReSharper disable once InconsistentNaming
        class CharacterStats_get_RawLuck_Patch {
            public static bool Prefix(ref float __result, CharacterStats __instance) {
                var id = (int)__instance.m_CharacterClass;
                Dictionary<int, CustomClass> dict = new();
                if (ClassManager.Instance.customDictionary.ContainsKey(id)) dict = ClassManager.Instance.customDictionary;
                else if (ClassManager.Instance.moddedDictionary.ContainsKey(id)) dict = ClassManager.Instance.moddedDictionary;

                if (dict.Count > 0 && dict.TryGetValue(id, out CustomClass customClass)) {
                    if (customClass.Luck < 0f) return true;
                    FieldInfo private_m_ModLuck = typeof(CharacterStats).GetField("m_ModLuck", BindingFlags.Instance | BindingFlags.NonPublic);
                    float m_ModLuck = (float)private_m_ModLuck.GetValue(__instance);
                    __result = customClass.Luck + m_ModLuck + __instance.m_AugmentedLuck + GameFlow.Instance.GameDif.m_StatBonus;
                    return false;
                }
                return true;
            }
        }
    }
}