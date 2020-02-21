
namespace ThothBotCore.Connections.Models
{
    public class SearchPlayers
    {
        public string Name { get; set; }
        public int player_id { get; set; }
        public int portal_id { get; set; }
        public string privacy_flag { get; set; }
        public object ret_msg { get; set; }
    }
}
