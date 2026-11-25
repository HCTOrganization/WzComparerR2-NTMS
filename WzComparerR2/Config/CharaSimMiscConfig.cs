using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WzComparerR2.Config
{
    public class CharaSimMiscConfig : ConfigurationElement
    {
        [ConfigurationProperty("enableWorldArchive", DefaultValue = true)]
        public bool EnableWorldArchive
        {
            get { return (bool)this["enableWorldArchive"]; }
            set { this["enableWorldArchive"] = value; }
        }

        [ConfigurationProperty("locatePetEquip", DefaultValue = false)]
        public bool LocatePetEquip
        {
            get { return (bool)this["locatePetEquip"]; }
            set { this["locatePetEquip"] = value; }
        }

        /// <summary>
        /// Unity ベースのクライアントの装備ツールチップに旧来のレンダラーを使用するかどうか。
        /// </summary>
        [ConfigurationProperty("enableLegacyUnityUI", DefaultValue = true)]
        public bool EnableLegacyUnityUI
        {
            get { return (bool)this["enableLegacyUnityUI"]; }
            set { this["enableLegacyUnityUI"] = value; }
        }
    }
}