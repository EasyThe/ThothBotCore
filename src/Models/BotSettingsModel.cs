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
        public Dictionary<string, string> Healers { get; set; } = new()
        {
            // Assault Healers
            // godid, tier
            { "1898", "1" }, // aphrodite
            { "1718", "1" }, // hel
            { "3811", "1" }, // yemoja
            { "1698", "2" }, // ra
            { "1763", "2" }, // guan yu
            { "4242", "2" }, // ix chel
            { "1921", "2" }, // change
            { "2030", "2" }, // sylvanus
            { "2147", "2" }, // terra
            { "3611", "2" }, // horus
            { "3518", "3" }, // baron samedi
            { "3664", "3" }, // olorun
            { "1918", "3" }, // eset
            { "3336", "3" }, // artio
            { "1778", "3" }, // cupid
        };
        public List<string[]> UpdateNotes { get; set; } = new();

        public class Top
        {
            public string Emoji { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }
}
