using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class SPLSchedule
    {
        public string stream_url { get; set; }
        public string date_range { get; set; }
        public int start_date { get; set; }
        public int end_date { get; set; }
        public List<int> match_list { get; set; }
        public List<Phase> phases { get; set; }
        public List<Schedule> schedule { get; set; }
    }
    public class Phase
    {
        public string title { get; set; }
        public int id { get; set; }
        public int start_date { get; set; }
        public int end_date { get; set; }
        public int is_bracket { get; set; }
    }

    public class Match
    {
        public int match_id { get; set; }
        public int time { get; set; }
        public int in_progress { get; set; }
        public int team_1_score { get; set; }
        public string team_1_name { get; set; }
        public string team_1_shortname { get; set; }
        public int team_1_esports_team_id { get; set; }
        public string team_1_top { get; set; }
        public int team_2_score { get; set; }
        public string team_2_name { get; set; }
        public string team_2_shortname { get; set; }
        public int team_2_esports_team_id { get; set; }
        public string team_2_top { get; set; }
        public string top_1_name { get; set; }
        public string top_1_team { get; set; }
        public string top_1_team_shortname { get; set; }
        public string top_2_name { get; set; }
        public string top_2_team { get; set; }
        public string top_2_team_shortname { get; set; }
        public string winning_team { get; set; }
        public string playlist_url { get; set; }
        public string summary { get; set; }
        public string esports_match_id { get; set; }
        public object next_match_id { get; set; }
    }

    public class Schedule
    {
        public int date { get; set; }
        public string phase { get; set; }
        public int phase_id { get; set; }
        public List<Match> matches { get; set; }
        public string week { get; set; }
    }
}
