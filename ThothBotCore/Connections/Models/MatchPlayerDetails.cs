
namespace ThothBotCore.Connections.Models
{
    public class MatchPlayerDetails
    {
        public class PlayerMatchDetails
        {
            public int Account_Gods_Played { get; set; }
            public int Account_Level { get; set; }
            public int GodId { get; set; }
            public int GodLevel { get; set; }
            public string GodName { get; set; }
            public int Mastery_Level { get; set; }
            public int Match { get; set; }
            public string Queue { get; set; }
            public double Rank_Stat { get; set; }
            public int SkinId { get; set; }
            public int Tier { get; set; }
            public string mapGame { get; set; }
            public string playerCreated { get; set; }
            public string playerId { get; set; }
            public string playerName { get; set; }
            public string playerRegion { get; set; }
            public object ret_msg { get; set; }
            public int taskForce { get; set; }
            public int tierLosses { get; set; }
            public int tierWins { get; set; }
        }
    }
}
