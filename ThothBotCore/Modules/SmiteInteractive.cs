using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace ThothBotCore.Modules
{
    public class SmiteInteractive : InteractiveBase<SocketCommandContext>
    {
        //readonly string botIcon = "https://i.imgur.com/8qNdxse.png"; // https://i.imgur.com/AgNocjS.png
        static Random rnd = new Random();

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
