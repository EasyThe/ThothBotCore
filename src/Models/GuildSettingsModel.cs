using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    [BsonIgnoreExtraElements]
    public class GuildSettingsModel
    {
        public ulong _id { get; set; }
        public List<Feed> Feeds { get; set; } =
        [
            new()
            {
                Type = FeedType.SMITEServerStatus,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.SMITE2News,
                WebhookID = 0,
                WebhookToken = null
            }
        ];

        public class Feed
        {
            public FeedType Type { get; set; }
            /// <summary>
            /// Legacy support
            /// </summary>
            public ulong ChannelID { get; set; }
            public ulong WebhookID { get; set; }
            public string WebhookToken { get; set; }
        }

        public enum FeedType
        {
            SMITEServerStatus,
            SMITE2News
            //UpdateNotes,
            //BlogPosts,
            //Datamining,
            //GameTwitter,
            //GameYouTube,
            //ProTwitter,
            //ProBlogPosts
        }
    }
}
