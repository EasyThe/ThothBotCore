using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class SlashEsports : InteractionModuleBase
    {
        [SlashCommand("splschedule", "Check the Smite Pro League Schedule.")]
        public async Task SlashSPLScheduleCommandAsync()
        {
            try
            {
                await DeferAsync();
                await EventIdChecker(Context);

                var schedule = await APIInteractions.GetEsportsSchedule();
                var calendar = await APIInteractions.GetSCCCalendarEvents();
                var emb = await EmbedHandler.BuildEsportsScheduleEmbedAsync(schedule, calendar);

                await FollowupAsync(embed: emb);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }
        [SlashCommand("splstandings", "Check the Smite Pro League Standings.")]
        public async Task SlashSPLStandingsCommandAsync()
        {
            try
            {
                await DeferAsync();
                await EventIdChecker(Context);
                var standings = await APIInteractions.GetEsportsStandings();
                var emb = await EmbedHandler.BuildEsportsStandingsEmbedAsync(standings);

                await FollowupAsync(embed: emb);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }
        [SlashCommand("splstats", "Check the Smite Pro League Stats.")]
        public async Task SlashSPLStatsCommandAsync()
        {
            try
            {
                await DeferAsync();
                await EventIdChecker(Context);
                var stats = await APIInteractions.GetEsportsStats();
                var emb = await EmbedHandler.BuildEsportsStatsEmbedAsync(stats);

                await FollowupAsync(embed: emb);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }
        [SlashCommand("swc", "SMITE World Championship schedule and links.")]
        public async Task SWCSchedule()
        {
            // this needs to be updated to use text and links from the settings from db, the proper way ;)
            var settings = MongoConnection.GetBotSettings();
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = "SMITE World Championship";
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
                x.Value = $"[⚡Info Here]({settings.s[6]}?utm_source=ThothBot&utm_campaign=swc)";
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
            await RespondAsync(embed: embed.Build());
        }

        private static async Task EventIdChecker(IInteractionContext context)
        {
            try
            {
                // This doesn't work anymore
                return;
                var appFile = await APIInteractions.GetEsportsAppFile();
                var appSplit = appFile.Split("<script src=\"/app-");
                var appF = $"app-{appSplit[1].Split("\"")[0]}";
                var result = await APIInteractions.GetEsportsEventID(appF);
                if (result.Contains("Not Found"))
                {
                    return;
                }
                var split = result.Split("eventId:");
                var hopefullyID = split[1].Split('"');
                if (hopefullyID.Length != 1 && 
                    hopefullyID[1] != null && 
                    hopefullyID[1].Length == 4)
                {
                    if (hopefullyID[1] != Constants.BotSettings.s[4])
                    {
                        var botSettings = MongoConnection.GetBotSettings();
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Updated Esports Event ID from `{botSettings.s[4]}` to `{hopefullyID[1]}`.");
                        botSettings.s[4] = hopefullyID[1];
                        await MongoConnection.SaveBotSettingsAsync(botSettings);
                        await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());
                    }
                }
            }
            catch (Exception ex)
            {
                await Reporter.SlashSendException(ex, context);
            }
        }
    }
}
