
namespace ThothBotCore.Storage.Models
{
    public class PlayerSpecial
    {
        public int active_player_id { get; set; }
        public string Name { get; set; }
        public ulong discordID { get; set; }
        public int streamer_bool { get; set; }
        public string streamer_link { get; set; }
        public int pro_bool { get; set; }
        public string special { get; set; }
    }
}
