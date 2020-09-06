
namespace ThothBotCore.Models
{
    public class PlayerSpecial
    {
        public int _id { get; set; }
        public ulong discordID { get; set; }
        public bool streamer_bool { get; set; }
        public string streamer_link { get; set; }
        public bool pro_bool { get; set; }
        public string special { get; set; }
    }
}
