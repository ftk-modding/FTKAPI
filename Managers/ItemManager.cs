using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using FTKAPI.Objects;
using FTKAPI.Utils;
using FTKItemName;
using GridEditor;
using HarmonyLib;

namespace FTKAPI.Managers;

/// <summary>
///    Manager for handling items in the game.
/// </summary>
public class ItemManager : BaseManager<ItemManager> {
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomItem> customDictionary = new();
    public Dictionary<int, CustomItem> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    /// <summary>
    /// Gets a class from TableManager's FTK_itemsDB
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The item's id.</param>
    /// <returns>Returns the item as a CustomItem</returns>
    public static CustomItem GetItem(FTK_itembase.ID id) {
        CustomItem customItem = new();
        FTK_itemsDB itemsDB = TableManager.Instance.Get<FTK_itemsDB>();

        var itemDetails = itemsDB.GetEntry(id);
        customItem.itemDetails = itemDetails;

        if (itemDetails.m_IsWeapon) {
            FTK_weaponStats2DB weaponsDB = TableManager.Instance.Get<FTK_weaponStats2DB>();

            var weaponDetails = weaponsDB.GetEntry(id);
            customItem.weaponDetails = weaponDetails;
        }
        return customItem;
    }

    /// <summary>
    /// Add a custom item.
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="customItem">The custom item to be added.</param>
    /// <param name="plugin">Allows FTKModLib to know which plugin called this method. Not required but recommended to make debugging easier.</param>
    /// <returns>Returns FTK_itembase.ID enum as int</returns>
    public static int AddItem(CustomItem customItem, BaseUnityPlugin plugin = null) {
        if (plugin != null) customItem.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        ItemManager itemManager = ItemManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try {
            customItem.ForceUpdatePrefab(); // this is necessary to apply Weapon fields
            FTK_itembase itemDetails = customItem.itemDetails;
            itemManager.enums.Add(itemDetails.m_ID,
                // Anything over 100000? is a weapon, anything under is a regular item.
                !customItem.IsWeapon ? 100000 - (itemManager.successfulLoads + 1) :
                    (int)Enum.GetValues(typeof(FTK_itembase.ID)).Cast<FTK_itembase.ID>().Max() + (itemManager.successfulLoads + 1)
            );
            itemManager.customDictionary.Add(itemManager.enums[itemDetails.m_ID], customItem);
            GEDataArrayBase geDataArrayBase = tableManager.Get(!customItem.IsWeapon ? typeof(FTK_itemsDB) : typeof(FTK_weaponStats2DB));
            if (!geDataArrayBase.IsContainID(itemDetails.m_ID)) {
                geDataArrayBase.AddEntry(itemDetails.m_ID);
                if (!customItem.IsWeapon) ((FTK_itemsDB)geDataArrayBase).m_Array[tableManager.Get<FTK_itemsDB>().m_Array.Length - 1] = (FTK_items)itemDetails;
                else ((FTK_weaponStats2DB)geDataArrayBase).m_Array[tableManager.Get<FTK_weaponStats2DB>().m_Array.Length - 1] = customItem.weaponDetails;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!customItem.IsWeapon) {
                if (!((FTK_itemsDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_itemsDB>().m_Array.Length - 1)) {
                    ((FTK_itemsDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_itemsDB>().m_Array.Length - 1, (FTK_items)itemDetails);
                }
            }
            else {
                if (!((FTK_weaponStats2DB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_weaponStats2DB>().m_Array.Length - 1)) {
                    ((FTK_weaponStats2DB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_weaponStats2DB>().m_Array.Length - 1, customItem.weaponDetails);
                }
            }

            itemManager.successfulLoads++;
            Logger.LogInfo($"Successfully added item '{customItem.ID}' of type '{customItem.ObjectType}' from {customItem.PLUGIN_ORIGIN}");
            return itemManager.enums[customItem.ID];
        }
        catch (Exception e) {
            Logger.LogError(e);
            Logger.LogError($"Failed to add item '{customItem.ID}' of type '{customItem.ObjectType}' from {customItem.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    /// <summary>
    /// Modifies an item from TableManager's FTK_itemsDB (and FTK_weaponStats2DB if weapon)
    /// <para>Must be called in a TableManager.Initialize postfix patch.</para>
    /// </summary>
    /// <param name="id">The item's id.</param>
    /// <param name="customItem">The new item to override over.</param>
    /// <returns></returns>
    public static void ModifyItem(FTK_itembase.ID id, CustomItem customItem) {
        FTK_itemsDB itemsDB = TableManager.Instance.Get<FTK_itemsDB>();
        itemsDB.m_Array[(int)id] = customItem.itemDetails;
        itemsDB.m_Dictionary[(int)id] = customItem.itemDetails;
        ItemManager.Instance.moddedDictionary.Add((int)id, customItem);

        if (FTK_itembase.GetItemBase(id).m_IsWeapon) {
            FTK_weaponStats2DB weaponsDB = TableManager.Instance.Get<FTK_weaponStats2DB>();
            weaponsDB.m_Array[(int)id] = customItem.weaponDetails;
            weaponsDB.m_Dictionary[(int)id] = customItem.weaponDetails;
            ItemManager.Instance.moddedDictionary.Add((int)id, customItem);
        }
        Logger.LogInfo($"Successfully modified item '{id}' of type '{customItem.ObjectType}'");
    }

    /// <summary>
    ///    <para>The patches necessary to make adding custom items possible.</para>
    ///    <para>Many of them are used to fix issues calling an invalid 'FTK_itembase.ID' 
    ///    by substituting with 'ItemManager.enums' dictionary value.</para>
    /// </summary>
    class HarmonyPatches {
        [HarmonyPatch(typeof(TableManager), "Initialize")]
        // ReSharper disable once InconsistentNaming
        class TableManager_Initialize_Patch {
            static void Prefix() {
                Logger.LogInfo("Preparing to load custom items");
                ItemManager.Instance.successfulLoads = 0;
                ItemManager.Instance.enums.Clear();
                ItemManager.Instance.customDictionary.Clear();
                ItemManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_itembase), "GetItemBase")]
        // ReSharper disable once InconsistentNaming
        class FTK_itembase_GetItemBase_Patch {
            static bool Prefix(ref FTK_itembase __result, FTK_itembase.ID _id) {
                if (ItemManager.Instance.customDictionary.TryGetValue((int)_id, out CustomItem customItem)) {
                    if (!customItem.IsWeapon)
                        __result = customItem.itemDetails;
                    else
                        __result = customItem.weaponDetails;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_itembase), "GetLocalizedName")]
        // ReSharper disable once InconsistentNaming
        class FTK_itembase_GetLocalizedName_Patch {
            static bool Prefix(ref string __result, FTK_itembase __instance) {
                if (ItemManager.Instance.enums.TryGetValue(__instance.m_ID, out int dictKey)) {
                    __result = ItemManager.Instance.customDictionary[dictKey].GetName();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTKItem), "Get")]
        // ReSharper disable once InconsistentNaming
        class FTKItem_Get_Patch {
            static bool Prefix(ref FTKItem __result, FTK_itembase.ID _id) {
                if (ItemManager.Instance.customDictionary.TryGetValue((int)_id, out CustomItem customItem)) {
                    FieldInfo private_gItemDictionary = typeof(FTKItem).GetField("gItemDictionary", BindingFlags.NonPublic | BindingFlags.Static);

                    if ((Dictionary<FTK_itembase.ID, FTKItem>)private_gItemDictionary.GetValue(private_gItemDictionary) == null) {
                        private_gItemDictionary.SetValue(private_gItemDictionary, new Dictionary<FTK_itembase.ID, FTKItem>());
                    }
                    FieldInfo protected_ItemID = customItem.GetType().GetField("m_ItemID", BindingFlags.NonPublic | BindingFlags.Instance);
                    protected_ItemID.SetValue(customItem, _id);
                    __result = customItem;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_itemsDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_itemsDB_GetIntFromID_Patch {
            static bool Prefix(ref int __result, FTK_itemsDB __instance, string _id) {
                //Attempts to return our enum and calls the original function if it errors.
                if (ItemManager.Instance.enums.ContainsKey(_id)) {
                    __result = ItemManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_weaponStats2DB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_weaponStats2DB_GetEntry_Patch {
            static bool Prefix(ref FTK_weaponStats2 __result, FTK_itembase.ID _enumID) {
                if (ItemManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomItem customItem)) {
                    __result = customItem.weaponDetails;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_weaponStats2DB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_weaponStats2DB_GetIntFromID_Patch {
            static bool Prefix(ref int __result, FTK_weaponStats2DB __instance, string _id) {
                //Attempts to return our enum and calls the original function if it errors.
                if (ItemManager.Instance.enums.ContainsKey(_id)) {
                    __result = ItemManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_itembase), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_itembase_GetEnum_Patch {
            static bool Prefix(ref FTK_itembase.ID __result, string _id) {
                //Not 100% sure if this is required.
                if (ItemManager.Instance.enums.ContainsKey(_id)) {
                    __result = (FTK_itembase.ID)ItemManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }
    }

    /// <summary>
    /// Debug patches that may assist in debugging FTKModLib
    /// </summary>
    class HarmonyDebugPatches {
        //[HarmonyPatch(typeof(FTKNetworkObject), "StateDataDeserialize")]
        // ReSharper disable once InconsistentNaming
        class FTKNetworkObject_StateDataDeserialize_Patch {
            static void Prefix(string _s, bool _callDone = true) {
                Logger.LogError("DESERIALIZED:");
                Logger.LogError(_s);
            }
        }

        //[HarmonyPatch(typeof(FTKNetworkObject), "StateDataSerialize")]
        // ReSharper disable once InconsistentNaming
        class FTKNetworkObject_StateDataSerialize_Patch {
            static void Postfix(ref string __result, bool _prep = true) {
                Logger.LogError("SERIALIZED:");
                Logger.LogError(__result);
            }
        }
    }
}