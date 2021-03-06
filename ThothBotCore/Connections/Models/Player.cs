﻿using System;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections.Models
{
    public class Player
    {
        public class PlayerStats
        {
            public int ActivePlayerId { get; set; }
            public string Avatar_URL { get; set; }
            public string Created_Datetime { get; set; }
            public string Last_Updated { get; set; } = Text.InvariantDefaultDate(DateTime.UtcNow);
            public int HoursPlayed { get; set; }
            public int Id { get; set; }
            public string Last_Login_Datetime { get; set; }
            public int Leaves { get; set; }
            public int Level { get; set; }
            public int Losses { get; set; }
            public int MasteryLevel { get; set; }
            public object MergedPlayers { get; set; }
            public int MinutesPlayed { get; set; }
            public string Name { get; set; }
            public string Personal_Status_Message { get; set; }
            public string Platform { get; set; }
            public double Rank_Stat_Conquest { get; set; }
            public double Rank_Stat_Conquest_Controller { get; set; }
            public double Rank_Stat_Duel { get; set; }
            public double Rank_Stat_Duel_Controller { get; set; }
            public double Rank_Stat_Joust { get; set; }
            public double Rank_Stat_Joust_Controller { get; set; }
            public RankedQueues RankedConquest { get; set; }
            public RankedQueues RankedConquestController { get; set; }
            public RankedQueues RankedDuel { get; set; }
            public RankedQueues RankedDuelController { get; set; }
            public RankedQueues RankedJoust { get; set; }
            public RankedQueues RankedJoustController { get; set; }
            public string Region { get; set; }
            public int TeamId { get; set; }
            public string Team_Name { get; set; }
            public int Tier_Conquest { get; set; }
            public int Tier_Duel { get; set; }
            public int Tier_Joust { get; set; }
            public int Total_Achievements { get; set; }
            public int Total_Worshippers { get; set; }
            public int Wins { get; set; }
            public string hz_gamer_tag { get; set; }
            public string hz_player_name { get; set; }
            public object ret_msg { get; set; }
        }

        public class RankedQueues
        {
            public int Leaves { get; set; }
            public int Losses { get; set; }
            public string Name { get; set; }
            public int Points { get; set; }
            public int PrevRank { get; set; }
            public int Rank { get; set; }
            public double Rank_Stat { get; set; }
            public double Rank_Variance { get; set; }
            public int Season { get; set; }
            public int Tier { get; set; }
            public int Trend { get; set; }
            public int Wins { get; set; }
            public object player_id { get; set; }
            public object ret_msg { get; set; }
        }
        public class PlayerStatus
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
