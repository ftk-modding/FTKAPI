using System;
using System.Collections.Generic;
using System.Reflection;
using FTKAPI.Managers;
using FTKItemName;
using GridEditor;
using UnityEngine;
using static Weapon;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects;

/// <summary>
/// <para>Items and Weapons are two seperate things in this game,
/// but this class will combine them into one class for simplicity</para>
///
/// <para>FTKItem          = functionality of item</para>
/// <para>FTK_weaponStats2 = details of weapon</para>
/// <para>FTK_items        = details of item</para>
/// 
/// <para>Weapons attacks/skills are referred to as an 'Proficiency' object,
/// this class will simplify them into a WeaponSkill object.</para>
/// </summary>
public class CustomItem : ConsumableBase
{
    internal string PLUGIN_ORIGIN = "null";
    internal FTK_items itemDetails = new FTK_items();
    internal FTK_weaponStats2 weaponDetails = new FTK_weaponStats2();

    public CustomItem(FTK_itembase.ID baseItem = FTK_itembase.ID.None)
    {
        if (baseItem != FTK_itembase.ID.None)
        {
            CustomItem source = ItemManager.GetItem(baseItem);
            this.itemDetails = source.itemDetails;
            this.weaponDetails = source.weaponDetails;
            foreach (FieldInfo field in typeof(CustomItem).GetFields())
            {
                field.SetValue(this, field.GetValue(source));
            }
        }
        else
        {
            this.weaponDetails.m_NoRegularAttack = true;
        }
    }

    /// <summary>
    /// This is the lookup string for the item, recommended to make this as unique as possible
    /// </summary>
    public string ID
    {
        get => this.itemDetails.m_ID;
        set
        {
            this.itemDetails.m_ID = value;
            this.weaponDetails.m_ID = value;
        }
    }

    private CustomLocalizedString name;

    /// <summary>
    /// This is the item's ingame display name, supports localized language
    /// </summary>
    public CustomLocalizedString Name
    {
        get => this.name;
        set { this.name = value; }
    }

    public string GetName()
    {
        if (this.name == null)
        {
            return this.ID;
        }

        return this.name.GetLocalizedString();
    }

    private CustomLocalizedString description;

    /// <summary>
    /// <para>This is the item's ingame description, supports localized language</para>
    /// <para>Custom Weapons do not use this field</para>
    /// </summary>
    public CustomLocalizedString Description
    {
        get => this.description;
        set { this.description = value; }
    }

    public override string GetDescription(CharacterOverworld _cow)
    {
        if (!string.IsNullOrEmpty(this.GetCannotUseReason(_cow)))
        {
            return this.GetCannotUseReason(_cow);
        }

        if (this.description == null)
        {
            return "This custom item is missing a description!";
        }

        string format = this.description.GetLocalizedString();
        int num = this.GetValue(_cow);
        if (num < 0 && this.DisplayPositiveValue())
        {
            num *= -1;
        }

        return string.Format(format, num);
    }

    public FTK_itemRarityLevel.ID ItemRarity
    {
        get => this.itemDetails.m_ItemRarity;
        set
        {
            this.itemDetails.m_ItemRarity = value;
            this.weaponDetails.m_ItemRarity = value;
        }
    }

    public string[] OnUseStatIncrementIDs
    {
        get => this.itemDetails.m_OnUseStatIncrementIDs;
        set
        {
            this.itemDetails.m_OnUseStatIncrementIDs = value;
            this.weaponDetails.m_OnUseStatIncrementIDs = value;
        }
    }

    public bool SuppressUseSound
    {
        get => this.itemDetails.m_SuppressUseSound;
        set
        {
            this.itemDetails.m_SuppressUseSound = value;
            this.weaponDetails.m_SuppressUseSound = value;
        }
    }

    public FTK_itembase.ObjectSlot ObjectSlot
    {
        get => this.itemDetails.m_ObjectSlot;
        set
        {
            this.itemDetails.m_ObjectSlot = value;
            this.weaponDetails.m_ObjectSlot = value;
        }
    }

    public FTK_itembase.ObjectType ObjectType
    {
        get => this.itemDetails.m_ObjectType;
        set
        {
            this.itemDetails.m_ObjectType = value;
            this.weaponDetails.m_ObjectType = value;
        }
    }

    public bool Useable
    {
        get => this.itemDetails._useable;
        set { this.itemDetails._useable = value; }
    }

    public bool IsWeapon
    {
        get { return this.ObjectType == FTK_itembase.ObjectType.weapon; }
    }

    public bool CursedItem
    {
        get => this.itemDetails.m_CursedItem;
        set
        {
            this.itemDetails.m_CursedItem = value;
            this.weaponDetails.m_CursedItem = value;
        }
    }

    public bool BackpackEquip
    {
        get => this.itemDetails.m_BackpackEquip;
        set
        {
            this.itemDetails.m_BackpackEquip = value;
            this.weaponDetails.m_BackpackEquip = value;
        }
    }

    public string CollectLoreItemUnlock
    {
        get => this.itemDetails.m_CollectLoreItemUnlock;
        set
        {
            this.itemDetails.m_CollectLoreItemUnlock = value;
            this.weaponDetails.m_CollectLoreItemUnlock = value;
        }
    }

    public bool FilterDebug
    {
        get => this.itemDetails.m_FilterDebug;
        set
        {
            this.itemDetails.m_FilterDebug = value;
            this.weaponDetails.m_FilterDebug = value;
        }
    }

    public bool FilterEndDungeon
    {
        get => this.itemDetails.m_FilterEndDungeon;
        set
        {
            this.itemDetails.m_FilterEndDungeon = value;
            this.weaponDetails.m_FilterEndDungeon = value;
        }
    }

    public bool Dropable
    {
        get => this.itemDetails.m_Dropable;
        set
        {
            this.itemDetails.m_Dropable = value;
            this.weaponDetails.m_Dropable = value;
        }
    }

    public bool DungeonMerchant
    {
        get => this.itemDetails.m_DungeonMerchant;
        set
        {
            this.itemDetails.m_DungeonMerchant = value;
            this.weaponDetails.m_DungeonMerchant = value;
        }
    }

    public bool TownMarket
    {
        get => this.itemDetails.m_TownMarket;
        set
        {
            this.itemDetails.m_TownMarket = value;
            this.weaponDetails.m_TownMarket = value;
        }
    }

    public int MaxLevel
    {
        get => this.itemDetails.m_MaxLevel;
        set
        {
            this.itemDetails.m_MaxLevel = value;
            this.weaponDetails.m_MaxLevel = value;
        }
    }

    public bool NightMarket
    {
        get => this.itemDetails.m_NightMarket;
        set
        {
            this.itemDetails.m_NightMarket = value;
            this.weaponDetails.m_NightMarket = value;
        }
    }

    public bool PriceScale
    {
        get => this.itemDetails.m_PriceScale;
        set
        {
            this.itemDetails.m_PriceScale = value;
            this.weaponDetails.m_PriceScale = value;
        }
    }

    public int ShopStock
    {
        get => this.itemDetails._shopStock;
        set
        {
            this.itemDetails._shopStock = value;
            this.weaponDetails._shopStock = value;
        }
    }

    public int GoldValue
    {
        get => this.itemDetails._goldValue;
        set
        {
            this.itemDetails._goldValue = value;
            this.weaponDetails._goldValue = value;
        }
    }

    /// <summary>
    /// <para>Prefab of the item.</para>
    /// <para>If item is a weapon and the prefab is null, it will default to an unarmed prefab.</para>
    /// <para>Else, it will use a default cube or nothing at all.</para>
    /// </summary>
    public GameObject Prefab
    {
        get
        {
            if (this.IsWeapon)
            {
                if (this.weaponDetails.m_Prefab == null)
                {
                    this.weaponDetails.m_Prefab = FTKHub.Instance.m_UnarmedWeapons[0];
                }

                return this.weaponDetails.m_Prefab;
            }

            return this.itemDetails.m_Prefab;
        }
        set
        {
            if (value == null)
            {
                Logger.LogError("Trying to set a null prefab? Defaulting to unarmed prefab is item is a weapon.");
                return;
            }

            this.itemDetails.m_Prefab = value;
            this.weaponDetails.m_Prefab = value;
        }
    }

    public void ForceUpdatePrefab()
    {
        if (this.IsWeapon)
        {
            Weapon weapon = this.Prefab.GetComponentInChildren<Weapon>();
            if (weapon == null)
            {
                // if a weapon component doesn't exist, its probably a custom prefab
                var mesh = this.Prefab.transform.Find("root");
                if (mesh == null)
                {
                    Logger.LogError("Weapon prefab does not contain a child called 'root'!");
                    throw new Exception();
                }

                weapon = mesh.gameObject.AddComponent<Weapon>();
                Logger.LogInfo($"Added Weapon Script to {this.Prefab}");
            }

            weapon.m_ProficiencyEffects = new Dictionary<ProficiencyID, HitEffect>();
            foreach (var prof in this.ProficiencyEffects)
            {
                weapon.m_ProficiencyEffects.Add(
                    new ProficiencyID(prof.Key),
                    TableManager.Instance.Get<FTK_hitEffectDB>().GetEntry(prof.Value).m_Prefab
                );
            }

            weapon.m_HitTargetVel = this.HitTargetVel;
            weapon.m_WeaponSize = this.WeaponSize;
            weapon.m_HitTarget ??= this.HitTarget;
            weapon.m_LuteMiss ??= this.LuteMiss;
            weapon.m_LuteHit ??= this.LuteHit;
            weapon.m_BowStringRenderer ??= this.BowStringRenderer;
            weapon.m_DropWeaponVelScale = this.DropWeaponVelScale;
            weapon.m_IsHideWeaponWhenDrop = this.IsHideWeaponWhenDrop;
            weapon.m_IsPlayAttackParticleWhenDrop = this.IsPlayAttackParticleWhenDrop;
            weapon.m_IsDetachAttackParticle = this.IsDetachAttackParticle;
            weapon.m_AttackParticle ??= this.AttackParticle;
            weapon.m_ParticleTarget ??= this.ParticleTarget;
            weapon.m_BreakRoot ??= this.BreakRoot;
            weapon.m_WeaponHolderName ??= this.WeaponHolderName;
            weapon.m_Particles ??= this.Particles;
            weapon.m_WeaponType = this.WeaponType;
            weapon.m_WeaponSubType = this.WeaponSubType;
            weapon.m_OffHand ??= this.OffHand;
            weapon.m_AnimationController ??= this.AnimationController;
            weapon.m_IdleAnimOverride = this.IdleAnimOverride;
            weapon.m_AmmoCapacity = this.AmmoCapacity;
            weapon.m_ImpactSound ??= this.ImpactSound;
            weapon.m_ImpactSoundOverride ??= this.ImpactSoundOverride;
            weapon.m_WeaponMaterial = this.WeaponMaterial;
        }
    }

    // fields used for weaponDetails
    public string AttackDisplay
    {
        get => this.weaponDetails.m_AttackDisplay;
        set { this.weaponDetails.m_AttackDisplay = value; }
    }

    /// <summary>
    /// <para>This is the default number of rolls for attacks</para>
    /// <para>This cannot be less than one.</para>
    /// </summary>
    public int Slots
    {
        get => this.weaponDetails._slots;
        set
        {
            if (value <= 0) value = 1;
            this.weaponDetails._slots = value;
        }
    }

    public float MaxDmg
    {
        get => this.weaponDetails._maxdmg;
        set { this.weaponDetails._maxdmg = value; }
    }

    public FTK_weaponStats2.DamageType DmgType
    {
        get => this.weaponDetails._dmgtype;
        set { this.weaponDetails._dmgtype = value; }
    }

    public FTK_weaponStats2.SkillType SkillType
    {
        get => this.weaponDetails._skilltest;
        set { this.weaponDetails._skilltest = value; }
    }

    public float DmgGain
    {
        get => this.weaponDetails._dmggain;
        set { this.weaponDetails._dmggain = value; }
    }

    public bool CanBreak
    {
        get => this.weaponDetails.m_CanBreak;
        set { this.weaponDetails.m_CanBreak = value; }
    }

    public bool NoFocus
    {
        get => this.weaponDetails.m_NoFocus;
        set { this.weaponDetails.m_NoFocus = value; }
    }

    /// <summary>
    /// <para>The weapon's default attack/skill, does raw damage, no effects.</para>
    /// </summary>
    public bool NoRegularAttack
    {
        get => this.weaponDetails.m_NoRegularAttack;
        set { this.weaponDetails.m_NoRegularAttack = value; }
    }

    // fields used for Weapon component inside prefab
    /// <summary>
    /// <para>The weapon's attacks/skills, using the vanilla proficiencies</para>
    /// </summary>
    public Dictionary<FTK_proficiencyTable.ID, FTK_hitEffect.ID> ProficiencyEffects = new();

    //public Dictionary<int, int> CustomProficiencyEffects = new();
    public Vector3 HitTargetVel;
    public FTK_ragdollDeath.ID WeaponSize;
    public Transform HitTarget;
    public AkLuteSoundID LuteMiss;
    public AkLuteSoundID LuteHit;
    public BowStringRenderer BowStringRenderer;
    public float DropWeaponVelScale = 0.5f;
    public bool IsHideWeaponWhenDrop;
    public bool IsPlayAttackParticleWhenDrop;
    public bool IsDetachAttackParticle;
    public MagicParticle AttackParticle;
    public Transform ParticleTarget;
    public Transform BreakRoot;
    public string WeaponHolderName = "WEAPON_HOLDER";
    public GameObject Particles;
    public WeaponType WeaponType = WeaponType.unarmed;
    public WeaponSubType WeaponSubType;
    public GameObject OffHand;
    public RuntimeAnimatorController AnimationController;
    public IdleAnimOverride IdleAnimOverride;
    public int AmmoCapacity;
    public AkEventID ImpactSound;
    public AkEventID ImpactSoundOverride;
    public WeaponMaterial WeaponMaterial;
}