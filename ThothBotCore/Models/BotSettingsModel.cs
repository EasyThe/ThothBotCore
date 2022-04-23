using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

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
        public Dictionary<string, string> SmiteQueues { get; set; }
        public Dictionary<string, string> AboutLinks { get; set; }
        public string[] Placeholders { get; set; }
        public List<Top> Pantheons { get; set; }
        public List<Top> Skins { get; set; }
        public string Changelog { get; set; }
        
        public class Top
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }
}
