using System;

namespace ThothBotCore.Connections.Models
{
    public class QueueStats
    {
        public int Assists { get; set; }
        public int Deaths { get; set; }
        public string God { get; set; }
        public int GodId { get; set; }
        public int Gold { get; set; }
        public int Kills { get; set; }
        public DateTime LastPlayed { get; set; }
        public int Losses { get; set; }
        public int Matches { get; set; }
        public int Minutes { get; set; }
        public string Queue { get; set; }
        public int Wins { get; set; }
        public int player_id { get; set; }
        public object ret_msg { get; set; }
    }
}
