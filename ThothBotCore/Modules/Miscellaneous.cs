using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System.Threading.Tasks;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Miscellaneous : InteractiveBase<SocketCommandContext>
    {
        [Command("pishka")]
        public async Task Pishka()
        {
            var embed = new EmbedBuilder();
            string pishka;
            if (Context.Message.Author.Id == Constants.OwnerID)
            {
                pishka = $"{Context.Message.Author.Username}'s pishka\n8=====================D";
            }
            else
            {
                pishka = $"{Context.Message.Author.Username}'s pishka\n8=D";
            }
            embed.WithTitle("pishka size machine");
            embed.WithDescription(pishka);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("is")]
        public async Task NoYouCommand([Remainder] string message)
        {
            await ReplyAsync("no u");
        }
    }
}
