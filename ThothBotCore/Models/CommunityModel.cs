using MongoDB.Bson;

namespace ThothBotCore.Models
{
    public class CommunityModel
    {
        public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoLink { get; set; }
        public ulong[] Mods { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
    }
}
