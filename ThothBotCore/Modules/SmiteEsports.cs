using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    [Name("Smite Pro League")]
    public class SmiteEsports : ModuleBase<SocketCommandContext>
    {
        [Command("splschedule", true)]
        [Summary("Check the Smite Pro League Schedule")]
        [Alias("splsc")]
        public async Task SPLScheduleCommandAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            try
            {
                var schedule = await APIInteractions.GetEsportsSchedule();
                var calendar = await APIInteractions.GetSCCCalendarEvents();
                var emb = await EmbedHandler.BuildEsportsScheduleEmbedAsync(schedule, calendar);

                await ReplyAsync(embed: emb);
            }
            catch (System.Exception ex)
            {
                await Reporter.RespondToCommandOnErrorAsync(ex, Context, ex.Message);
            }
        }
        [Command("splstandings", true)]
        [Summary("Check the Smite Pro League Standings")]
        [Alias("splstanding", "splst")]
        public async Task SPLStandingsCommandAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            try
            {
                var standings = await APIInteractions.GetEsportsStandings();
                var emb = await EmbedHandler.BuildEsportsStandingsEmbedAsync(standings);

                await ReplyAsync(embed: emb);
            }
            catch (System.Exception ex)
            {
                await Reporter.RespondToCommandOnErrorAsync(ex, Context, ex.Message);
            }
        }
        [Command("swc", true)]
        [Summary("SWC 2022 Schedule and links")]
        [Alias("swcs", "swcschedule", "schedule")]
        public async Task SWCSchedule()
        {
            var settings = MongoConnection.GetBotSettings();
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = "SMITE World Championship 2022";
                x.Url = "https://www.hirezshowcase.com/?utm_source=ThothBot&utm_campaign=swc2022";
                x.IconUrl = "https://i.imgur.com/sOsJR9V.png";
            });
            embed.AddField(x =>
            {
                x.Name = "SmiteGame Twitch";
                x.Value = "[<:Twitch:579125715874742280>SmiteGame](https://www.twitch.tv/smitegame/?utm_source=ThothBot&utm_campaign=swc2022)";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "Twitch Drops";
                x.Value = $"[⚡Info Here]({settings.s[6]}?utm_source=ThothBot&utm_campaign=swc2022)";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "TimeZone Converter";
                x.Value = "[🌍WorldTimeBuddy](https://www.worldtimebuddy.com)";
                x.IsInline = true;
            });
            embed.WithColor(new Color(47, 49, 54));
            embed.WithImageUrl(settings.s[5]);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
