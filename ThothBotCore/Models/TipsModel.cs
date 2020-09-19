using MongoDB.Bson;

namespace ThothBotCore.Models
{
    public class TipsModel
    {
        public ObjectId _id { get; set; } = ObjectId.GenerateNewId();
        public string TipText { get; set; }
    }
}
