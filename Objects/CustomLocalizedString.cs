using System;
using Google2u;
using UnityEngine;

namespace FTKAPI.Objects {
    public class CustomLocalizedString {
        public string _en; public string _fr; public string _it;
        public string _de; public string _es; public string _pt_br;
        public string _ru; public string _zh_cn; public string _zh_tw;
        public string _pl; public string _ja; public string _ko;


        /// <summary>
        /// This class provides multi-language support for a string
        /// </summary>
        /// <param name="__en"></param>
        /// <param name="__fr"></param>
        /// <param name="__it"></param>
        /// <param name="__de"></param>
        /// <param name="__es"></param>
        /// <param name="__pt_br"></param>
        /// <param name="__ru"></param>
        /// <param name="__zh_cn"></param>
        /// <param name="__zh_tw"></param>
        /// <param name="__pl"></param>
        /// <param name="__ja"></param>
        /// <param name="__ko"></param>
        public CustomLocalizedString(string __en = "", string __fr = "", string __it = "", string __de = "", string __es = "", string __pt_br = "", string __ru = "", string __zh_cn = "", string __zh_tw = "", string __pl = "", string __ja = "", string __ko = "") {
            this._en = __en; this._fr = __fr; this._it = __it;
            this._de = __de; this._es = __es; this._pt_br = __pt_br;
            this._ru = __ru; this._ko = __ko; this._zh_cn = __zh_cn;
            this._zh_tw = __zh_tw; this._pl = __pl; this._ja = __ja;
        }

        public string GetLocalizedString() {
            FTKHub.LanguageCode m_Language = FTKHub.LanguageCode._en;

            var textMiscRow = new TextMiscRow(
                "",
                this._en, this._fr, this._it,
                this._de, this._es, this._pt_br,
                this._ru, this._zh_cn, this._zh_tw,
                this._pl, this._ja, this._ko
            );

            string value = PlayerPrefs.GetString("Language");
            if (!string.IsNullOrEmpty(value)) {
                try {
                    m_Language = (FTKHub.LanguageCode)Enum.Parse(typeof(FTKHub.LanguageCode), value);
                }
                catch (ArgumentException) {
                    Debug.Log("Incorrect Language code");
                }
            }

            string langCode = m_Language.ToString().Substring(1);
            return textMiscRow.GetStringData(langCode.Replace("_", "-"));
        }
    }
}
