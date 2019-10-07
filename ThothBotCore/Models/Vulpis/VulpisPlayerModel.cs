
namespace ThothBotCore.Models
{
    public class VulpisPlayerModel
    {
        public class Player
        {
            public string Name { get; set; }
            public string PrimaryRole { get; set; }
            public string SecondaryRole { get; set; }
            public string DiscordName { get; set; }
            public ulong DiscordID { get; set; }
            public bool CheckedIn { get; set; } = false;
        }
    }
}
