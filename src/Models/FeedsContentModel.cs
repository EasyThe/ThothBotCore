using MongoDB.Bson.Serialization.Attributes;

namespace ThothBotCore.Models
{
    [BsonIgnoreExtraElements]
    public class FeedsContentModel
    {
        public string _id { get; set; }
    }
}
