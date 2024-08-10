using System;
using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class TeamDetailsModel
    {
        public string AvatarURL { get; set; }
        public string Founder { get; set; }
        public string FounderId { get; set; }
        public int Losses { get; set; }
        public string Name { get; set; }
        public int Players { get; set; }
        public int Rating { get; set; }
        public string Tag { get; set; }
        public int TeamId { get; set; }
        public int Wins { get; set; }
        public object ret_msg { get; set; }
    }

    public class TeamPlayersModel
    {
        public List<TeamPlayer> PlayerList { get; set; }

        public class TeamPlayer
        {
            public int AccountLevel { get; set; }
            public string JoinedDatetime { get; set; }
            public string LastLoginDatetime { get; set; }
            public string Name { get; set; }
            public string PlayerId { get; set; }
            public object ret_msg { get; set; }
        }
    }
}
