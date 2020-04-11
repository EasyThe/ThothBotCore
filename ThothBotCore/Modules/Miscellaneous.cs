using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Miscellaneous : InteractiveBase<SocketCommandContext>
    {
        [Command("pishka")]
        public async Task Pishka()
        {
            string pishka = "";
            var embed = new EmbedBuilder();
            if (Context.Message.Author.Id == 171675309177831424)
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

        [Command("settimezone")]
        [Alias("stz")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetTimeZone([Remainder] string value)
        {
            List<TimeZoneInfo> allTimeZones = new List<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            string timezone = allTimeZones.Find(x => x.DisplayName.Contains(Text.ToTitleCase(value))).ToSerializedString();

            DateTime now = DateTime.UtcNow;

            // Saving to DB
            await Database.SetTimeZone(Context.Guild, timezone);

            // Deserialize timezone string from db
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FromSerializedString(timezone);

            // utc to timezone
            DateTime timezoned = TimeZoneInfo.ConvertTimeFromUtc(now, timeZoneInfo);

            await ReplyAsync($"UTC Now: {now}\n" +
                $"In your time: {timezoned}\n" +
                $"{Database.GetTimeZone(Context.Guild.Id).Result[0]}");
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
