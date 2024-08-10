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

        public static bool IsTournamentManagerCheck(SocketCommandContext context)
        {
            var user = Connection.Client.GetGuild(321367254983770112).GetUser(context.Message.Author.Id);

            foreach (var role in user.Roles)
            {
                if (role.Id == 388419600502489099 || role.Id == 351813900305563659 || role.Id == 388210991844032513)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
