using System;
using System.Collections.Generic;
using System.Text;

namespace ThothBotCore.Connections.Models
{
    public class PaladinsMatchPlayerDetails
    {
        public class PlayerMatchDetails
        {
            public int Account_Level { get; set; }
            public int ChampionId { get; set; }
            public string ChampionName { get; set; }
            public int Mastery_Level { get; set; }
            public int Match { get; set; }
            public int Queue { get; set; }
            public int SkinId { get; set; }
            public int Tier { get; set; }
            public string mapGame { get; set; }
            public string playerCreated { get; set; }
            public int playerId { get; set; }
            public string playerName { get; set; }
            public string playerRegion { get; set; }
            public object ret_msg { get; set; }
            public int taskForce { get; set; }
            public int tierLosses { get; set; }
            public int tierWins { get; set; }
        }
    }
}
