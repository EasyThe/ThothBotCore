using System;
using System.Collections.Generic;
using System.Text;

namespace ThothBotCore.Connections.Models
{
    public class PaladinsGodRanks
    {
        public int Assists { get; set; }
        public int Deaths { get; set; }
        public int Gold { get; set; }
        public int Kills { get; set; }
        public string LastPlayed { get; set; }
        public int Losses { get; set; }
        public int MinionKills { get; set; }
        public int Minutes { get; set; }
        public int Rank { get; set; }
        public int Wins { get; set; }
        public int Worshippers { get; set; }
        public string champion { get; set; }
        public string champion_id { get; set; }
        public string player_id { get; set; }
        public object ret_msg { get; set; }
    }
}
