using System.Linq;
using System.Reflection;
using FTKAPI.Managers;
using GridEditor;

namespace FTKAPI.Objects {
    public class CustomClass : FTK_playerGameStart {
        internal string PLUGIN_ORIGIN = "null";

        public CustomClass(ID baseClass = FTK_playerGameStart.ID.blacksmith) {
            var source = ClassManager.GetClass(baseClass);
            foreach (FieldInfo field in typeof(FTK_playerGameStart).GetFields()) {
                field.SetValue(this, field.GetValue(source));
            }
        }

        public CustomClass AddToStartItems(FTK_itembase.ID[] newStartItems) {
            if (this.StartItems == null) {
                this.StartItems = newStartItems;
                return this;
            }
            if (newStartItems == null) {
                return this;
            }
            this.StartItems = this.StartItems.Concat(newStartItems).ToArray();
            return this;
        }

        /// <summary>
        /// This is the lookup string for the class, recommended to make this as unique as possible
        /// </summary>
        public new string ID {
            get => this.m_ID;
            set => this.m_ID = value;
        }
        private CustomLocalizedString name;
        public CustomLocalizedString Name {
            get => this.name;
            set {
                this.name = value;
                this.m_DisplayName = this.name.GetLocalizedString();
            }
        }
        private CustomLocalizedString description;
        public CustomLocalizedString Description {
            get => this.description;
            set {
                this.description = value;
                this.m_Flavor = this.description.GetLocalizedString();
            }
        }
        public FTK_dlc.ID DLC {
            get => this.m_DLC;
            set => this.m_DLC = value;
        }
        public FTK_itembase.ID[] StartItems {
            get => this.m_StartItems;
            set => this.m_StartItems = value;
        }
        public FTK_itembase.ID StartWeapon {
            get => this.m_StartWeapon;
            set => this.m_StartWeapon = value;
        }
        public bool Development {
            get => this.m_Development;
            set => this.m_Development = value;
        }
        public bool Release {
            get => this.m_Release;
            set => this.m_Release = value;
        }
        public CharacterSkills CharacterSkills {
            get => this.m_CharacterSkills;
            set => this.m_CharacterSkills = value;
        }
        public float ChanceToTaunt {
            get => this.m_ChanceToTaunt;
            set => this.m_ChanceToTaunt = value;
        }
        public int BaseFocus {
            get => this._basefocus;
            set => this._basefocus = value;
        }
        public int StartingGold {
            get => this._startinggold;
            set => this._startinggold = value;
        }
        public bool PublicTest {
            get => this.m_PublicTest;
            set => this.m_PublicTest = value;
        }
        public float Strength {
            get => this._toughness;
            set => this._toughness = value;
        }
        public float Vitality {
            get => this._vitality;
            set => this._vitality = value;
        }
        public float Intelligence {
            get => this._fortitude;
            set => this._fortitude = value;
        }
        public float Talent {
            get => this._talent;
            set => this._talent = value;
        }
        public float Awareness {
            get => this._awareness;
            set => this._awareness = value;
        }
        public float Speed {
            get => this._quickness;
            set => this._quickness = value;
        }
        private float luck = -1f;
        /// <summary>
        /// <para>Luck cannot be lower than 0f</para>
        /// <para>Note: Luck is actually a hidden stat until your character spawns (i.e, it won't show up in character info)</para>
        /// </summary>
        public float Luck {
            get => this.luck;
            set => this.luck = value;
        }
        public FTK_weaponStats2.SkillType PrimaryWeaponStat {
            get => this.m_PrimaryWeaponStat;
            set => this.m_PrimaryWeaponStat = value;
        }
        public bool IsMale {
            get => this.m_IsMale;
            set => this.m_IsMale = value;
        }
        public FTK_skinset.ID[] Skinsets {
            get => this.m_Skinsets;
            set => this.m_Skinsets = value;
        }
        public SkinType DefaultSkinType {
            get => this.m_DefaultSkinType;
            set => this.m_DefaultSkinType = value;
        }
    }
}
