using System;

namespace ThothBotCore.Models
{
    public class WebAPIPostModel
    {
        public int id { get; set; }
        public string featured_image { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public DateTime timestamp { get; set; }
        public string real_categories { get; set; }
        public string content { get; set; }
    }
}
