using Discord.Commands;
using System.Threading.Tasks;

namespace ThothBotCore.Modules
{
    public class Bot : ModuleBase<SocketCommandContext>
    {
        [Command("thoth", true)]
        public async Task BasicInfoCommand()
        {
            await ReplyAsync($"Only slash `/` commands are working!");
        }
    }
}
