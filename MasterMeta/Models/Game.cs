using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterMeta.Models
{
    public class Games
    {
        public List<Game> games { get; set; }
    }

    public class Game
    {
        public List<FellowPlayer> fellowPlayers { get; set; }
        public GameStats stats { get; set; }
        public long gameId { get; set; }
        public string subType { get; set; }
        public int championId { get; set; }
        public int teamId { get; set; }

    }

    public class FellowPlayer
    {
        public int championId { get; set; }
        public long summonerId { get; set; }
        public int teamId { get; set; }        
    }

    public class GameStats
    {
        public bool win { get; set; }

        public int championsKilled { get; set; }
        public int numDeaths { get; set; }
        public int assists { get; set; }

        public int item0 { get; set; }
        public int item1 { get; set; }
        public int item2 { get; set; }
        public int item3 { get; set; }
        public int item4 { get; set; }
        public int item5 { get; set; }
        public int item6 { get; set; }

        public int[] ItemsIds
        {
           get
            {
                return new int[] { item0, item1, item2, item3, item4, item5, item6};
            }
        }
    }
}