
namespace ThothBotCore.Storage.Models
{
    public class PlayerSpecial
    {
        public int active_player_id { get; set; }
        public ulong discordID { get; set; }
        public int streamer_bool { get; set; }
        public int pro_bool { get; set; }
        public string special { get; set; }
    }
}
