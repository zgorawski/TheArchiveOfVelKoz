using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterMeta.Models
{
    public class Champions
    {
        public Dictionary<string, Champion> data { get; set; }
        public string version { get; set; }

        public Dictionary<int, Champion> ChampionIdToChampion()
        {
            Dictionary<int, Champion> result = new Dictionary<int, Champion>();

            foreach (var champion in data.Values)
            {
                result.Add(champion.id, champion);
            }

            return result;
        }
    }

    public class Champion
    {
        public int id { get; set; }
        public string name { get; set; }
        public ChampionImage image { get; set; }

        //public string key { get; set; }
        //public string title { get; set; }

    }

    public class ChampionImage
    {
        public string full { get; set; }
    }
}