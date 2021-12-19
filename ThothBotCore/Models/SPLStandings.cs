
namespace ThothBotCore.Models
{
    public class SPLStandings
    {
        public int team_id { get; set; }
        public string team_name { get; set; }
        public string team_shortname { get; set; }
        public int esports_team_id { get; set; }
        public string matches { get; set; }
        public string wins { get; set; }
        public string losses { get; set; }
        public string win_percent { get; set; }
        public string total { get; set; }
        public int split { get; set; }
        public string next_game { get; set; }
        public int rank { get; set; }
    }
}
