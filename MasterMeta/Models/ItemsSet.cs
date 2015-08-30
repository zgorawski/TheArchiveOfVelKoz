using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterMeta.Models
{
    public class ItemsSet
    {
        public string title { get; set; }
        public string type { get; set; }
        public string map { get; set; }
        public string mode { get; set; }
        public bool priority { get; set; }
        public int sortrank { get; set; }
        public List<ItemsSetBlock> blocks { get; set; }

        /// <summary>
        /// timestamp
        /// -- itemId : count
        /// </summary>
        public ItemsSet(Dictionary<long, Dictionary<int, int>> itemsData)
        {
            title = "Vel'Koz Archive set";
            type = "custom";
            map = "any";
            mode = "any";
            priority = false;
            sortrank = 0;

            int purchaseNumber = 1;
            blocks = new List<ItemsSetBlock>();
            foreach (var item in itemsData.Values)
            {
                if (item.Count > 0)
                {
                    string blockName = purchaseNumber.Ordinal() + " purchase";
                    purchaseNumber++;
                    blocks.Add(new ItemsSetBlock(blockName, item));
                }
            }
        }
    }

    public class ItemsSetBlock
    {
        public string type { get; set; }
        public bool recMath { get; set; }
        public int minSummonerLevel { get; set; }
        public int maxSummonerLevel { get; set; }
        public string showIfSummonerSpell { get; set; }
        public string hideIfSummonerSpell { get; set; }
        public List<ItemsSetItem> items { get; set; }
        
        public ItemsSetBlock(string type, Dictionary<int,int> itemsData)
        {
            this.type = type;

            recMath = false;
            minSummonerLevel = -1;
            maxSummonerLevel = -1;
            showIfSummonerSpell = "";
            hideIfSummonerSpell = "";

            items = new List<ItemsSetItem>();

            foreach (var item in itemsData)
            {
                items.Add(new ItemsSetItem() { id = item.Key.ToString(), count = item.Value });
            }
        }
    }

    public class ItemsSetItem
    {
        public string id { get; set; }
        public int count { get; set; }
    }
}
