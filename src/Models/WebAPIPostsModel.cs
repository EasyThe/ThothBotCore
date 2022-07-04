using System;

namespace ThothBotCore.Models
{
    public class WebAPIPostsModel
    {
        public int id { get; set; }
        public string featured_image { get; set; }
        public string large_image { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public DateTime timestamp { get; set; }
        public string real_categories { get; set; }
        public string slug { get; set; }
    }
}
