using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    [BsonIgnoreExtraElements]
    public class GuildSettingsModel
    {
        public ulong _id { get; set; }
        public List<Feed> Feeds { get; set; } = new List<Feed>()
        {
            new Feed()
            {
                Type = FeedType.ServerStatus,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.UpdateNotes,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.BlogPosts,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.Datamining,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.GameTwitter,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.GameYouTube,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.ProTwitter,
                WebhookID = 0,
                WebhookToken = null
            },
            new Feed()
            {
                Type = FeedType.ProBlogPosts,
                WebhookID = 0,
                WebhookToken = null
            },
        };

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
            ServerStatus,
            UpdateNotes,
            BlogPosts,
            Datamining,
            GameTwitter,
            GameYouTube,
            ProTwitter,
            ProBlogPosts
        }
    }
}
