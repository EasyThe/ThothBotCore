using System;
using System.Collections.Generic;
using System.Text;

namespace ThothBotCore.Connections.Models
{
    public class PaladinsPlayer
    {
        public class Player
        {
            public int ActivePlayerId { get; set; }
            public DateTime Created_Datetime { get; set; }
            public int HoursPlayed { get; set; }
            public int Id { get; set; }
            public DateTime Last_Login_Datetime { get; set; }
            public int Leaves { get; set; }
            public int Level { get; set; }
            public int Losses { get; set; }
            public int MasteryLevel { get; set; }
            public object MergedPlayers { get; set; }
            public string Name { get; set; }
            public string Personal_Status_Message { get; set; }
            public string Platform { get; set; }
            public RankedConquest RankedConquest { get; set; }
            public RankedController RankedController { get; set; }
            public RankedKBM RankedKBM { get; set; }
            public string Region { get; set; }
            public int TeamId { get; set; }
            public string Team_Name { get; set; }
            public int Tier_Conquest { get; set; }
            public int Tier_RankedController { get; set; }
            public int Tier_RankedKBM { get; set; }
            public int Total_Achievements { get; set; }
            public int Total_Worshippers { get; set; }
            public int Wins { get; set; }
            public object hz_gamer_tag { get; set; }
            public string hz_player_name { get; set; }
            public object ret_msg { get; set; }
        }

        public class RankedConquest
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }

        public class RankedController
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }

        public class RankedKBM
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }

        public class PaladinsPlayerStatus
        {
            public int Match { get; set; }
            public int match_queue_id { get; set; }
            public object personal_status_message { get; set; }
            public object ret_msg { get; set; }
            public int status { get; set; }
            public string status_string { get; set; }
        }
    }
}
