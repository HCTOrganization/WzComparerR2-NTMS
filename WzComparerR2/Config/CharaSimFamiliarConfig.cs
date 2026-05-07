using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace WzComparerR2.Config
{
    public class CharaSimFamiliarConfig : ConfigurationElement
    {
        [ConfigurationProperty("allowOutOfBounds", DefaultValue = false)]
        public bool AllowOutOfBounds
        {
            get { return (bool)this["allowOutOfBounds"]; }
            set { this["allowOutOfBounds"] = value; }
        }

        [ConfigurationProperty("useCTFamiliarUI", DefaultValue = true)]
        public bool UseCTFamiliarUI
        {
            get { return (bool)this["useCTFamiliarUI"]; }
            set { this["useCTFamiliarUI"] = value; }
        }
    }
}
