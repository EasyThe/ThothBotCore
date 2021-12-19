using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ThothBotCore.Models
{
    [BsonIgnoreExtraElements]
    public class BotSettingsModel
    {
        public ObjectId _id { get; set; }
        /// <summary>
        /// 0 - Invite link
        /// 1 - Website
        /// 2 - Privacy Policy
        /// 3 - Support Server
        /// 4 - Esports Id
        /// 5 - SWC Schedule
        /// 6 - Twitch Drops link
        /// </summary>
        public string[] s { get; set; }
    }
}
