using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NET6_0_OR_GREATER
namespace WzComparerR2.OpenAPI
{
    public class LoadedAvatarData
    {
        public int Gender;
        public int Preset;
        public long CashPreset;
        public List<string> ItemList;
        public List<string> CashPresetItemList;
        public List<string> CashBaseItemList;
        public Dictionary<string, string> HairInfo;
        public Dictionary<string, string> FaceInfo;
        public Dictionary<string, string> SkinInfo;
    }
}
#endif