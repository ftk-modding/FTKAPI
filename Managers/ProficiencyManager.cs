using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using FTKAPI.Objects;
using FTKAPI.Utils;
using GridEditor;
using HarmonyLib;
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Managers;

public class ProficiencyManager : BaseManager<ProficiencyManager> {
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomProficiency> customDictionary = new();
    public Dictionary<int, CustomProficiency> moddedDictionary = new();
    
    
    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    /// <summary>
    /// Gets a class from TableManager's FTK_proficiencyTable
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The class's id.</param>
    /// <returns>Returns FTK_proficiencyTable</returns>
    public static FTK_proficiencyTable GetProficiency(FTK_proficiencyTable.ID id) {
        FTK_proficiencyTableDB proficienciesDB = TableManager.Instance.Get<FTK_proficiencyTableDB>();
        return proficienciesDB.m_Array[(int)id];
    }

    /// <summary>
    /// Add a custom proficiency.
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="customProf">The custom proficiency to be added.</param>
    /// <param name="plugin">Allows FTKModLib to know which plugin called this method. Not required but recommended to make debugging easier.</param>
    /// <returns>Returns FTK_proficiencyTable.ID enum as int</returns>
    public static int AddProficiency(CustomProficiency customProf, BaseUnityPlugin plugin = null) {
        if (plugin != null) customProf.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        ProficiencyManager profManager = ProficiencyManager.Instance;
        FTK_proficiencyTableDB db = TableManager.Instance.Get<FTK_proficiencyTableDB>();

        try {
            profManager.enums.Add(customProf.ID,
                (int)Enum.GetValues(typeof(FTK_proficiencyTable.ID)).Cast<FTK_proficiencyTable.ID>().Max() + (profManager.successfulLoads + 1)
            );
            var e = profManager.enums[customProf.ID];
            profManager.customDictionary.Add(e, (CustomProficiency)new GameObject(customProf.ID).AddComponent(customProf.GetType()));
            profManager.customDictionary[e].ProficiencyID = (FTK_proficiencyTable.ID)e;
            profManager.customDictionary[e].ProficiencyPrefab = profManager.customDictionary[e];

            if (!db.IsContainID(customProf.ID)) {
                db.AddEntry(customProf.ID);
                db.m_Array[db.m_Array.Length - 1] = profManager.customDictionary[e].m_ProficiencyData;
                db.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!db.m_Dictionary.ContainsKey(db.m_Array.Length - 1)) {
                db.m_Dictionary.Add(db.m_Array.Length - 1, profManager.customDictionary[e].m_ProficiencyData);
            }

            profManager.successfulLoads++;
            Logger.LogInfo($"Successfully added proficiency '{customProf.ID}' of name '{customProf.Name._en}' from {customProf.PLUGIN_ORIGIN}");
            return profManager.enums[customProf.ID];
        }
        catch (Exception e) {
            Logger.LogError(e);
            Logger.LogError($"Failed to add proficiency '{customProf.ID}' of name '{customProf.Name._en}' from {customProf.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    /// <summary>
    /// Modifies a class from TableManager's FTK_proficiencyTableDB
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The proficiency's id.</param>
    /// <param name="customProf">The new class to override over.</param>
    /// <returns></returns>
    public static void ModifyProficiency(FTK_proficiencyTable.ID id, CustomProficiency customProf) {
        FTK_proficiencyTableDB playerClassesDB = TableManager.Instance.Get<FTK_proficiencyTableDB>();
        playerClassesDB.m_Array[(int)id] = customProf.m_ProficiencyData;
        playerClassesDB.m_Dictionary[(int)id] = customProf.m_ProficiencyData;
        ProficiencyManager.Instance.moddedDictionary.Add((int)id, customProf);
        Logger.LogInfo($"Successfully modified proficiency '{id}'");
    }

    /// <summary>
    ///    <para>The patches necessary to make adding custom items possible.</para>
    ///    <para>Many of them are used to fix issues calling an invalid 'FTK_proficiencyTable.ID' 
    ///    by substituting with 'ProficiencyManager.enums' dictionary value.</para>
    /// </summary>
    class HarmonyPatches {
        [HarmonyPatch(typeof(TableManager), "Initialize")]
        class TableManager_Initialize_Patch {
            static void Prefix() {
                Logger.LogInfo("Preparing to load custom proficiencies");
                ProficiencyManager.Instance.successfulLoads = 0;
                ProficiencyManager.Instance.enums.Clear();
                ProficiencyManager.Instance.customDictionary.Clear();
                ProficiencyManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_proficiencyTableDB), "GetEntry")]
        class FTK_proficiencyTableDB_GetEntry_Patch {
            static bool Prefix(ref FTK_proficiencyTable __result, FTK_proficiencyTable.ID _enumID) {
                if (ProficiencyManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomProficiency customProf)) {
                    __result = customProf.m_ProficiencyData;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_proficiencyTableDB), "GetIntFromID")]
        class FTK_proficiencyTableDB_GetIntFromID_Patch {
            static bool Prefix(ref int __result, FTK_proficiencyTableDB __instance, string _id) {
                //Attempts to return our enum and calls the original function if it errors.
                if (ProficiencyManager.Instance.enums.ContainsKey(_id)) {
                    __result = ProficiencyManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }
    }
}