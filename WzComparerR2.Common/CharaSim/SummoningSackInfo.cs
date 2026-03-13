using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzComparerR2.CharaSim
{
    public class SummoningSackInfo
    {
        public SummoningSackInfo()
        {
            this.MobID = -1;
            this.Rate = new List<int>();
        }

        public int MobID { get; set; }
        public List<int> Rate { get; set; }

        public static List<SummoningSackInfo> Consolidate(List<SummoningSackInfo> ssiList)
        {
            List<SummoningSackInfo> newSsiList = new List<SummoningSackInfo>();
            Dictionary<int, List<int>> mobRates = new Dictionary<int, List<int>>();
            foreach (var ssi in ssiList)
            {

                if (!mobRates.ContainsKey(ssi.MobID))
                {
                    mobRates[ssi.MobID] = new List<int>();
                }
                mobRates[ssi.MobID].Add(ssi.Rate[0]);
            }
            foreach (var kvp in mobRates)
            {
                SummoningSackInfo ssi = new SummoningSackInfo();
                ssi.MobID = kvp.Key;
                ssi.Rate = mobRates[kvp.Key];
                newSsiList.Add(ssi);
            }
            return newSsiList;
        }

    }
}
