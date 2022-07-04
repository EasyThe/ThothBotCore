using MongoDB.Bson;

namespace ThothBotCore.Models
{
    public class BadgeModel
    {
        public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
        public string Key { get; set; }
        public string Emote { get; set; }
        public string Title { get; set; }
    }
}
