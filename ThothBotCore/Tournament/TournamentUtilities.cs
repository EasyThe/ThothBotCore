using Discord.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using ThothBotCore.Discord;
using ThothBotCore.Models;

namespace ThothBotCore.Tournament
{
    public class TournamentUtilities
    {
        readonly private static string directory = "Tourneys";
        public static string GetTournamentFileName(string name)
        {
            string[] nz = Directory.GetFiles(directory);
            var list = nz.ToList();
            string result = list.FindLast(x => x.Contains(name));
            return list.FindLast(x => x.Contains(name));
        }

        public static string FindAllTournamentFiles()
        {
            string[] nz = Directory.GetFiles(directory);
            var sb = new StringBuilder();
            foreach (var file in nz)
            {
                sb.AppendLine(file);
            }
            return sb.ToString();
        }

        public static async void SaveTournamentFile(VulpisPlayerModel.BaseTourney tournament)
        {
            string json = JsonConvert.SerializeObject(tournament, Formatting.Indented);
            await File.WriteAllTextAsync(GetTournamentFileName("soloqcq"), json);
            // TO DO: make it usable for the rest tournaments too
        }

        public static bool IsTournamentManagerCheck(SocketCommandContext context)
        {
            var user = Connection.Client.GetGuild(321367254983770112).GetUser(context.Message.Author.Id);

            foreach (var role in user.Roles)
            {
                if (role.Name.ToLowerInvariant().Contains("tournament managers") || role.Name.ToLowerInvariant().Contains("presidency"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
