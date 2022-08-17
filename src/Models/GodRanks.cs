using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class GodRanks
    {
        public GodRanksIDs _id { get; set; } = new();
        public int Assists { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public int Losses { get; set; }
        public int MinionKills { get; set; }
        public int Rank { get; set; }
        public int Wins { get; set; }
        public double WinRate { get; set; }
        public int Worshippers { get; set; }
        public DateTime? Last_Updated { get; set; }
        public string god { get; set; }
        public string god_id
        {
            get => _id.god_id;
            set => _id.god_id = value;
        }
        public string player_id
        {
            get => _id.player_id;
            set => _id.player_id = value;
        }
        public object ret_msg { get; set; }
    }
    public class GodRanksIDs
    {
        public string player_id { get; set; }
        public string god_id { get; set; }
    }
}
