using System.Collections.Generic;

namespace ThothBotCore.Models
{
    public class VulpisPlayerModel
    {
        public class BaseTourney
        {
            public Tournament Tournament { get; set; } = new Tournament();
            public List<Player> Players { get; set; } = new List<Player>();
        }

        public class Tournament
        {
            public string Type { get; set; }
            public bool SignupsAllowed { get; set; }
            public bool CheckinsAllowed { get; set; }
            public ulong AnnouncementChannelID { get; set; }
        }
        public class Player
        {
            public string Name { get; set; }
            public string PrimaryRole { get; set; }
            public string SecondaryRole { get; set; }
            public string DiscordName { get; set; }
            public ulong DiscordID { get; set; }
            public bool CheckedIn { get; set; }
            public bool IsPro { get; set; }
        }
    }
}
