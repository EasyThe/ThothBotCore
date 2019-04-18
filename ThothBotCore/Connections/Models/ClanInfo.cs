
namespace ThothBotCore.Connections.Models
{
    public class ClanInfo
    {
        public string Founder { get; set; }
        public int FounderId { get; set; }
        public int Losses { get; set; }
        public string Name { get; set; }
        public int Players { get; set; }
        public int Rating { get; set; }
        public string Tag { get; set; }
        public int TeamId { get; set; }
        public int Wins { get; set; }
        public object ret_msg { get; set; }
    }
}
