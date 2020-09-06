using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
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

        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }
    }
}
