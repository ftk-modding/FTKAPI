
using System.Reflection;
using GridEditor;
using UnityEngine;

namespace FTKAPI.Objects;

public class CustomProficiency : ProficiencyBase {
    internal string PLUGIN_ORIGIN = "null";
    internal FTK_proficiencyTable proficiencyDetails {
        get => this.m_ProficiencyData;
        set => this.m_ProficiencyData = value;
    }

    public CustomProficiency(FTK_proficiencyTable.ID baseProf = FTK_proficiencyTable.ID.None) {
        if (baseProf != FTK_proficiencyTable.ID.None) {
            FTK_proficiencyTable source = Managers.ProficiencyManager.GetProficiency(baseProf);
            foreach (FieldInfo field in typeof(FTK_proficiencyTable).GetFields()) {
                field.SetValue(this.proficiencyDetails, field.GetValue(source));
            }
        } else {
            if(this.proficiencyDetails == null) this.proficiencyDetails = new();
        }
    }

    public FTK_proficiencyTable.ID ProficiencyID {
        get => this.m_ProficiencyID;
        set => this.m_ProficiencyID = value;
    }
    public new Category Category {
        get => this.m_Category;
        set => this.m_Category = value;
    }
    public new SubCategory SubCategory {
        get => this.m_SubCategory;
        set => this.m_SubCategory = value;
    }
    public bool IsEndOnTurn {
        get => this.m_IsEndOnTurn;
        set => this.m_IsEndOnTurn = value;
    }
    public float CustomValue {
        get {
            return this.m_CustomValue;
        }
        set {
            this.m_CustomValue = value;
            this.m_ProficiencyData.m_CustomValue = value;
        }
    }

    /// <summary>
    /// This is the lookup string for the proficiency, recommended to make this as unique as possible
    /// </summary>
    public string ID {
        get => this.m_ProficiencyData.m_ID;
        set => this.m_ProficiencyData.m_ID = value;
    }
    private new CustomLocalizedString name;
    public CustomLocalizedString Name {
        get => this.name;
        set {
            this.name = value;
            this.m_ProficiencyData.m_DisplayTitle = this.name.GetLocalizedString();
        }
    }
    public ProficiencyBase ProficiencyPrefab {
        get => this.m_ProficiencyData.m_ProficiencyPrefab;
        set => this.m_ProficiencyData.m_ProficiencyPrefab = value;
    }
    public float TendencyWeight {
        get => this.m_ProficiencyData.m_TendencyWeight;
        set => this.m_ProficiencyData.m_TendencyWeight = value;
    }
    public CharacterStats.EnemyTendency Tendency {
        get => this.m_ProficiencyData.m_Tendency;
        set => this.m_ProficiencyData.m_Tendency = value;
    }
    public HitEffect HitEffectOverride {
        get => this.m_ProficiencyData.m_HitEffectOverride;
        set => this.m_ProficiencyData.m_HitEffectOverride = value;
    }
    public Weapon.WeaponType WpnTypeOverride {
        get => this.m_ProficiencyData.m_WpnTypeOverride;
        set => this.m_ProficiencyData.m_WpnTypeOverride = value;
    }
    public FTK_weaponStats2.DamageType DmgTypeOverride {
        get => this.m_ProficiencyData.m_DmgTypeOverride;
        set => this.m_ProficiencyData.m_DmgTypeOverride = value;
    }
    public int SlotOverride {
        get => this.m_ProficiencyData.m_SlotOverride;
        set => this.m_ProficiencyData.m_SlotOverride = value;
    }
    public float PerSlotSkillRoll {
        get => this.m_ProficiencyData.m_PerSlotSkillRoll;
        set => this.m_ProficiencyData.m_PerSlotSkillRoll = value;
    }
    public bool IgnoresArmor {
        get => this.m_ProficiencyData.m_IgnoresArmor;
        set => this.m_ProficiencyData.m_IgnoresArmor = value;
    }
    public float DmgMultiplier {
        get => this.m_ProficiencyData.m_DmgMultiplier;
        set => this.m_ProficiencyData.m_DmgMultiplier = value;
    }
    public bool ChaosOption {
        get => this.m_ProficiencyData.m_ChaosOption;
        set => this.m_ProficiencyData.m_ChaosOption = value;
    }
    public bool Suicide {
        get => this.m_ProficiencyData.m_Suicide;
        set => this.m_ProficiencyData.m_Suicide = value;
    }
    public bool Harmless {
        get => this.m_ProficiencyData.m_Harmless;
        set => this.m_ProficiencyData.m_Harmless = value;
    }
    public bool TargetFriendly {
        get => this.m_ProficiencyData.m_TargetFriendly;
        set => this.m_ProficiencyData.m_TargetFriendly = value;
    }
    public bool GunShot {
        get => this.m_ProficiencyData.m_GunShot;
        set => this.m_ProficiencyData.m_GunShot = value;
    }
    public CharacterDummy.TargetType Target {
        get => this.m_ProficiencyData.m_Target;
        set => this.m_ProficiencyData.m_Target = value;
    }
    public string DisplayTitle {
        get => this.m_ProficiencyData.m_DisplayTitle;
        set => this.m_ProficiencyData.m_DisplayTitle = value;
    }
    public bool CheckID {
        get => this.m_ProficiencyData.m_CheckID;
        set => this.m_ProficiencyData.m_CheckID = value;
    }
    public Color Tint {
        get => this.m_ProficiencyData.m_Tint;
        set => this.m_ProficiencyData.m_Tint = value;
    }
    public bool AlwaysHitFx {
        get => this.m_ProficiencyData.m_AlwaysHitFx;
        set => this.m_ProficiencyData.m_AlwaysHitFx = value;
    }
    public Sprite BattleButton {
        get => this.m_ProficiencyData.m_BattleButton;
        set => this.m_ProficiencyData.m_BattleButton = value;
    }
    public FTK_ragdollDeath.ID WeaponSizeOverride {
        get => this.m_ProficiencyData.m_WeaponSizeOverride;
        set => this.m_ProficiencyData.m_WeaponSizeOverride = value;
    }
    public FTK_ragdollDeath.DirectionOverride DirectionOverride {
        get => this.m_ProficiencyData.m_DirectionOverride;
        set => this.m_ProficiencyData.m_DirectionOverride = value;
    }
    public Sprite SlotIcon {
        get => this.m_ProficiencyData.m_SlotIcon;
        set => this.m_ProficiencyData.m_SlotIcon = value;
    }
    public int RepeatCount {
        get => this.m_ProficiencyData.m_RepeatCount;
        set => this.m_ProficiencyData.m_RepeatCount = value;
    }
    public float ChanceToAffect {
        get => this.m_ProficiencyData.m_ChanceToAffect;
        set => this.m_ProficiencyData.m_ChanceToAffect = value;
    }
    public int DamagePerAttack {
        get => this.m_ProficiencyData.m_DamagePerAttack;
        set => this.m_ProficiencyData.m_DamagePerAttack = value;
    }
    public int BoatDamage {
        get => this.m_ProficiencyData.m_BoatDamage;
        set => this.m_ProficiencyData.m_BoatDamage = value;
    }
    public bool FullSlots {
        get => this.m_ProficiencyData.m_FullSlots;
        set => this.m_ProficiencyData.m_FullSlots = value;
    }
    public float Quickness {
        get => this.m_ProficiencyData.m_Quickness;
        set => this.m_ProficiencyData.m_Quickness = value;
    }
}