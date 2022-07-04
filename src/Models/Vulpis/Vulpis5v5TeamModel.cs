using System.Collections.Generic;

namespace ThothBotCore.Models.Vulpis
{
    public class Vulpis5v5TeamModel
    {
        public TeamInfo Teams { get; set; }
        public SoloPlayers soloPlayers { get; set; }
        public DuoPlayers duoPlayers { get; set; }
        public TrioPlayers trioPlayers { get; set; }
        public FourPlayers fourPlayers { get; set; }

        public class TeamInfo
        {
            public string TeamName { get; set; }
            public List<Players> Players { get; set; }
            public ulong CaptainDiscordID { get; set; }

        }
        public class Players
        {
            public string PlayerName { get; set; }
            public ulong DiscordID { get; set; }
        }
        public class SoloPlayers
        {
            public Players Player { get; set; }
        }
        public class DuoPlayers
        {
            public List<Players> Player { get; set; }
        }
        public class TrioPlayers
        {
            public List<Players> Player { get; set; }
        }
        public class FourPlayers
        {
            public List<Players> Player { get; set; }
        }
    }
}
