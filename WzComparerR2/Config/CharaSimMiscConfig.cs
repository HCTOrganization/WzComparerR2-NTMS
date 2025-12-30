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
    }
}