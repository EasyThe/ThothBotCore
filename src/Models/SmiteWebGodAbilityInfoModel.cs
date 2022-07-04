namespace ThothBotCore.Models
{
    public class SmiteWebGodAbilityInfoModel
    {
        public Acf acf { get; set; }
        public class Acf
        {
            public string god_id { get; set; }
            public string god_header_image { get; set; }
            public string youtube_lore_video { get; set; }
            public string ability_video_passive { get; set; }
            public string abilitiy_video_1 { get; set; }
            public string abilitiy_video_2 { get; set; }
            public string abilitiy_video_3 { get; set; }
            public string abilitiy_video_4 { get; set; }
            public bool test { get; set; }
        }
    }
}
