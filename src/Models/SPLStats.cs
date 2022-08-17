
namespace ThothBotCore.Models
{
    public class SPLStats
    {
        public Potw[] potw { get; set; }
        public Trending[] trending { get; set; }
        public Team_Leaders[] team_leaders { get; set; }

        public class Potw
        {
            public string name { get; set; }
            public string role { get; set; }
            public string team { get; set; }
            public string short_name { get; set; }
            public int player_id { get; set; }
            public int team_id { get; set; }
        }

        public class Trending
        {
            public string title { get; set; }
            public string name { get; set; }
            public string role { get; set; }
            public string team { get; set; }
            public string short_name { get; set; }
            public int player_id { get; set; }
            public int team_id { get; set; }
            public Stat[] stats { get; set; }
        }

        public class Stat
        {
            public string title { get; set; }
            public object value { get; set; }
        }

        public class Team_Leaders
        {
            public string title { get; set; }
            public string team { get; set; }
            public string short_name { get; set; }
            public int team_id { get; set; }
            public Stat[] stats { get; set; }
        }
    }
}
