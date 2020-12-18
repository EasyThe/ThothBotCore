using MongoDB.Bson;

namespace ThothBotCore.Models
{
    public class BotSettingsModel
    {
        public ObjectId _id { get; set; }
        public string BotInviteLink { get; set; }
    }
}
