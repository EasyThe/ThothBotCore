
namespace ThothBotCore.Models
{
    public class PlayerBuildsModel
    {
        public int _id { get; set; }
        public bool IsPrivate { get; set; }
        public long DiscordId { get; set; }
        public string BuildName { get; set; }
        public int GodId { get; set; }
        public int[] Items { get; set; }
        public string CreatedDate { get; set; }
    }
}
