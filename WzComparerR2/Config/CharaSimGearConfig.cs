﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WzComparerR2.Config
{
    public class CharaSimGearConfig : ConfigurationElement
    {
        [ConfigurationProperty("showID", DefaultValue = true)]
        public bool ShowID
        {
            get { return (bool)this["showID"]; }
            set { this["showID"] = value; }
        }

        [ConfigurationProperty("showWeaponSpeed", DefaultValue = false)]
        public bool ShowWeaponSpeed
        {
            get { return (bool)this["showWeaponSpeed"]; }
            set { this["showWeaponSpeed"] = value; }
        }

        [ConfigurationProperty("showLevelOrSealed", DefaultValue = true)]
        public bool ShowLevelOrSealed
        {
            get { return (bool)this["showLevelOrSealed"]; }
            set { this["showLevelOrSealed"] = value; }
        }

        [ConfigurationProperty("showSoldPrice", DefaultValue = true)]
        public bool ShowSoldPrice
        {
            get { return (bool)this["showSoldPrice"]; }
            set { this["showSoldPrice"] = value; }
        }

        [ConfigurationProperty("showCashPurchasePrice", DefaultValue = true)]
        public bool ShowCashPurchasePrice
        {
            get { return (bool)this["showCashPurchasePrice"]; }
            set { this["showCashPurchasePrice"] = value; }
        }

        [ConfigurationProperty("showMedalTag", DefaultValue = true)]
        public bool ShowMedalTag
        {
            get { return (bool)this["showMedalTag"]; }
            set { this["showMedalTag"] = value; }
        }

        [ConfigurationProperty("autoTitleWrap", DefaultValue = true)]
        public bool AutoTitleWrap
        {
            get { return (bool)this["autoTitleWrap"]; }
            set { this["autoTitleWrap"] = value; }
        }
        [ConfigurationProperty("showCombatPower", DefaultValue = true)]
        public bool ShowCombatPower
        {
            get { return (bool)this["showCombatPower"]; }
            set { this["showCombatPower"] = value; }
        }
    }
}
