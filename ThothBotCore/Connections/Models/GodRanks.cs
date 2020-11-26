
namespace ThothBotCore.Connections.Models
{
    public class GodRanks
    {
        public int Assists { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public int Losses { get; set; }
        public int MinionKills { get; set; }
        public int Rank { get; set; }
        public int Wins { get; set; }
        public int Worshippers { get; set; }
        public string god { get; set; }
        public string god_id { get; set; }
        public string player_id { get; set; }
        public object ret_msg { get; set; }
    }
}
