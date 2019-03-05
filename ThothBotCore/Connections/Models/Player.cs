using System;

namespace ThothBotCore.Connections.Models
{
    public class Player
    {
        public struct PlayerStats
        {
            public string Avatar_URL { get; set; }
            public DateTime Created_Datetime { get; set; }
            public int HoursPlayed { get; set; }
            public int Id { get; set; }
            public DateTime Last_Login_Datetime { get; set; }
            public int Leaves { get; set; }
            public int Level { get; set; }
            public int Losses { get; set; }
            public int MasteryLevel { get; set; }
            public string Name { get; set; }
            public string Personal_Status_Message { get; set; }
            public int Rank_Stat_Conquest { get; set; }
            public int Rank_Stat_Duel { get; set; }
            public int Rank_Stat_Joust { get; set; }
            public RankedConquest RankedConquest { get; set; }
            public RankedDuel RankedDuel { get; set; }
            public RankedJoust RankedJoust { get; set; }
            public string Region { get; set; }
            public int TeamId { get; set; }
            public string Team_Name { get; set; }
            public int Tier_Conquest { get; set; }
            public int Tier_Duel { get; set; }
            public int Tier_Joust { get; set; }
            public int Total_Achievements { get; set; }
            public int Total_Worshippers { get; set; }
            public int Wins { get; set; }
            public object ret_msg { get; set; }
        }

        public struct RankedConquest
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public object Rank_Stat_Conquest { get; set; }
            public object Rank_Stat_Duel { get; set; }
            public object Rank_Stat_Joust { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }

        public struct RankedDuel
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public object Rank_Stat_Conquest { get; set; }
            public object Rank_Stat_Duel { get; set; }
            public object Rank_Stat_Joust { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }

        public struct RankedJoust
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public object Rank_Stat_Conquest { get; set; }
            public object Rank_Stat_Duel { get; set; }
            public object Rank_Stat_Joust { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }
    }
}
