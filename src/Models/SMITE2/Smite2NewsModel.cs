using System;

namespace ThothBotCore.Models.SMITE2
{
    public class Smite2NewsModel
    {
        public int id { get; set; }
        public Attributes attributes { get; set; }

        public class Attributes
        {
            public string title { get; set; }
            public string slug { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime publishedAt { get; set; }
            public string locale { get; set; }
            public string content { get; set; }
            public bool update_notes_formatting { get; set; }
            public bool featured_news { get; set; }
            public Header_Image header_image { get; set; }
        }

        public class Header_Image
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public int id { get; set; }
            public Attributes1 attributes { get; set; }
        }

        public class Attributes1
        {
            public string name { get; set; }
            public object alternativeText { get; set; }
            public object caption { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public float size { get; set; }
            public string url { get; set; }
            public object previewUrl { get; set; }
            public string provider { get; set; }
            public object provider_metadata { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
        }

        public class ImageOrSomething
        {
            public string name { get; set; }
            public string hash { get; set; }
            public string ext { get; set; }
            public string mime { get; set; }
            public object path { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public float size { get; set; }
            public string url { get; set; }
        }
    }
}
