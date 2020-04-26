using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using ThothBotCore.Utilities.Smite;
using static ThothBotCore.Connections.Models.Player;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Smite : InteractiveBase<SocketCommandContext>
    {
        static Random rnd = new Random();
        Stopwatch stopWatch = new Stopwatch();

        HiRezAPI hirezAPI = new HiRezAPI();
        TrelloAPI trelloAPI = new TrelloAPI();
        DominantColor domColor = new DominantColor();
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        [Command("stats", true, RunMode = RunMode.Async)]
        [Summary("Display stats for the provided `PlayerName`.")]
        [Alias("stat", "pc", "st", "stata", "ст", "статс", "ns")]
        [RequireBotPermission(ChannelPermission.EmbedLinks)]
        public async Task Stats([Remainder] string PlayerName = "")
        {
            try
            {
                stopWatch.Start();
                int playerID = 0;
                var getPlayerByDiscordID = new List<PlayerSpecial>();

                // Checking if we are searching for Player who linked his Discord with SMITE account
                if (PlayerName == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Command usage: `!!stats InGameName`");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                else if (Context.Message.MentionedUsers.Count != 0 && PlayerName == $"<@!{Context.Message.MentionedUsers.Last().Id}>")
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                if (PlayerName.Contains("\\") || PlayerName.Contains("/"))
                {
                    if (PlayerName.Contains("\\"))
                    {
                        PlayerName = PlayerName.Replace("\\", String.Empty);
                    }
                    if (PlayerName.Contains("/"))
                    {
                        PlayerName = PlayerName.Replace("/", String.Empty);
                    }
                }
                // Finding all occurences of provided username and adding them in a list
                var searchPlayer = await hirezAPI.SearchPlayer(PlayerName);
                var realSearchPlayers = new List<SearchPlayers>();
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == PlayerName.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                // Checking the new list for count of users in it
                if (realSearchPlayers.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await ReplyAsync(embed: embed.Build());
                }
                else if (!(realSearchPlayers.Count > 1))
                {
                    if (realSearchPlayers[0].privacy_flag != "n")
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserIsHidden(PlayerName));
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    await Context.Channel.TriggerTypingAsync();
                    try
                    {
                        if (playerID == 0)
                        {
                            playerID = realSearchPlayers[0].player_id;
                        }
                        string statusJson = await hirezAPI.GetPlayerStatus(playerID);
                        var playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                        string matchJson = "";
                        if (playerStatus[0].Match != 0)
                        {
                            matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        }

                        var embed = await EmbedHandler.PlayerStatsEmbed(
                            await hirezAPI.GetPlayer(playerID.ToString()),
                            await hirezAPI.GetGodRanks(playerID),
                            await hirezAPI.GetPlayerAchievements(playerID),
                            await hirezAPI.GetPlayerStatus(playerID),
                            matchJson,
                            realSearchPlayers[0].portal_id);
                        var message = await Context.Channel.SendMessageAsync(embed: embed.Build());
                        TimeSpan ts = stopWatch.Elapsed;

                        // Format and display the TimeSpan value.
                        string elapsedTime = String.Format("{0:00}:{1:00}", 
                            ts.Seconds,
                            ts.Milliseconds / 10);
                        Console.WriteLine("Sent " + elapsedTime);
                        // Getting the top queues
                        try
                        {
                            var allQueue = new List<AllQueueStats>();
                            int legitQueueCount = Text.LegitQueueIDs().Count;
                            for (int i = 0; i < legitQueueCount; i++)
                            {
                                int matches = 0;
                                try
                                {
                                    var queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(playerID, Text.LegitQueueIDs()[i]).ConfigureAwait(false));
                                    for (int c = 0; c < queueStats.Count; c++)
                                    {
                                        if (queueStats[c].Matches != 0)
                                        {
                                            matches += queueStats[c].Matches;
                                        }
                                    }
                                    allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                                }
                                catch (Exception)
                                {
                                }
                            }
                            var orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
                            string topMatchesValue = "";
                            if (orderedQueues.Count != 0)
                            {
                                switch (orderedQueues.Count)
                                {
                                    case 1:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]";
                                        break;
                                    case 2:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                                        $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]";
                                        break;
                                    default:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                                        $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]\n" +
                                                        $":third_place:{orderedQueues[2].queueName} [{orderedQueues[2].matches}]";
                                        break;
                                }
                                embed.AddField(field =>
                                {
                                    field.IsInline = true;
                                    field.Name = $"<:matches:579604410569850891>**Most Played Modes**";
                                    field.Value = topMatchesValue;
                                });

                                await message.ModifyAsync(x =>
                                {
                                    x.Embed = embed.Build();
                                });

                                stopWatch.Stop();
                                // Get the elapsed time as a TimeSpan value.
                                ts = stopWatch.Elapsed;

                                // Format and display the TimeSpan value.
                                elapsedTime = String.Format("{0:00}:{1:00}",
                                    ts.Seconds,
                                    ts.Milliseconds / 10);
                                Console.WriteLine("Completed " + elapsedTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            await ErrorTracker.SendError($"Error in topmatches\n{ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await ErrorTracker.SendError($"Stats Error: \n{ex.Message}\n**InnerException: **{ex.InnerException}");
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("An unexpected error has occured. Please try again later.\nIf the error persists, don't hesitate to contact the bot owner for further assistance.");
                        await ReplyAsync(embed: embed);
                    }
                }
                else
                {
                    // Multiple Players?
                    var result = await MultiplePlayersHandler(realSearchPlayers, Context);
                    if (result.searchPlayers != null && result.searchPlayers.player_id == 0)
                    {
                        var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                        await ReplyAsync(embed: embed.Build());
                        return;
                    }
                    else if (result.searchPlayers == null && result.userMessage == null)
                    {
                        return;
                    }
                    playerID = result.searchPlayers.player_id;
                    try
                    {
                        string statusJson = await hirezAPI.GetPlayerStatus(playerID);
                        List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                        string matchJson = "";
                        if (playerStatus[0].Match != 0)
                        {
                            matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        }

                        var embed = await EmbedHandler.PlayerStatsEmbed(
                            await hirezAPI.GetPlayer(playerID.ToString()),
                            await hirezAPI.GetGodRanks(playerID),
                            await hirezAPI.GetPlayerAchievements(playerID),
                            await hirezAPI.GetPlayerStatus(playerID),
                            matchJson,
                            result.searchPlayers.portal_id);

                        // Getting the top queues
                        try
                        {
                            var allQueue = new List<AllQueueStats>();
                            for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                            {
                                int matches = 0;
                                try
                                {
                                    var queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(playerID, Text.LegitQueueIDs()[i]));
                                    for (int c = 0; c < queueStats.Count; c++)
                                    {
                                        if (queueStats[c].Matches != 0)
                                        {
                                            matches += queueStats[c].Matches;
                                        }
                                    }
                                    allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                                }
                                catch (Exception)
                                {
                                }
                            }
                            var orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
                            string topMatchesValue = "";
                            if (orderedQueues.Count != 0)
                            {
                                switch (orderedQueues.Count)
                                {
                                    case 1:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]";
                                        break;
                                    case 2:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                                        $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]";
                                        break;
                                    default:
                                        topMatchesValue = $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                                        $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]\n" +
                                                        $":third_place:{orderedQueues[2].queueName} [{orderedQueues[2].matches}]";
                                        break;
                                }
                                embed.AddField(field =>
                                {
                                    field.IsInline = true;
                                    field.Name = $"<:matches:579604410569850891>**Most Played Modes**";
                                    field.Value = topMatchesValue;
                                });

                                await result.userMessage.ModifyAsync(x =>
                                {
                                    x.Embed = embed.Build();
                                });

                                stopWatch.Stop();
                                // Get the elapsed time as a TimeSpan value.
                                TimeSpan ts = stopWatch.Elapsed;

                                // Format and display the TimeSpan value.
                                string elapsedTime = String.Format("{0:00}:{1:00}",
                                    ts.Seconds,
                                    ts.Milliseconds / 10);
                                Console.WriteLine("Completed " + elapsedTime);
                            }
                            else
                            {
                                await result.userMessage.ModifyAsync(x =>
                                {
                                    x.Embed = embed.Build();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await ErrorTracker.SendError($"Error in topmatches\n{ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await ErrorTracker.SendError($"Stats Error: \n{ex.Message}\n**InnerException: **{ex.InnerException}");
                        await ReplyAsync("Oops.. I've encountered an error. :sob:");
                    }
                }
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
                if (!(ex.Message.ToLowerInvariant().Contains("api is unavailable")))
                {
                    await ErrorTracker.SendError($"**Stats Command**\n" +
                    $"**Message: **{Context.Message.Content}\n" +
                    $"**User: **{Context.Message.Author.Username}[{Context.Message.Author.Id}]\n" +
                    $"**Server and Channel: **ID:{Context.Guild.Id}[{Context.Channel.Id}]\n" +
                    $"**Error: **{ex.Message}\n" +
                    $"**InnerException: ** {ex.InnerException}");
                }
            }
        }

        [Command("istats")]
        [Alias("istat", "ipc", "ist", "istata", "ист", "истатс")]
        [RequireOwner] // vremenno
        public async Task ImageStats([Remainder] string username)
        {
            string json = await hirezAPI.GetPlayer(username);
            if (json == "[]")
            {
                await Context.Channel.SendMessageAsync($":exclamation:*{Text.ToTitleCase(username)}* is hidden or not found!");
            }
            else
            {
                await Context.Channel.TriggerTypingAsync();

                List<PlayerStats> playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(json);

                string rPlayerName = "";
                string rTeamName = "";

                if (playerStats[0].Name.Contains("]"))
                {
                    string[] splitName = playerStats[0].Name.Split(']');
                    rPlayerName = splitName[1];
                    if (playerStats[0].hz_player_name != null)
                    {
                        rPlayerName = playerStats[0].hz_player_name;
                        rTeamName = splitName[0] + "]" + playerStats[0].Team_Name;
                    }
                    else
                    {
                        rPlayerName = playerStats[0].hz_gamer_tag;
                        rTeamName = splitName[0] + "]" + playerStats[0].Team_Name;
                    }
                }
                else
                {
                    if (playerStats[0].hz_player_name != null)
                    {
                        rPlayerName = playerStats[0].hz_player_name;
                    }
                    else
                    {
                        rPlayerName = playerStats[0].hz_gamer_tag;
                    }
                }
                int rPlayerLevel = playerStats[0].Level;
                int rPlayerWins = playerStats[0].Wins;
                int rPlayerLosses = playerStats[0].Losses;
                string rPlayerRegion = playerStats[0].Region;
                int rPlayerLeaves = playerStats[0].Leaves;
                int rPlayerMasteryLevel = playerStats[0].MasteryLevel;
                int rTotalWorsh = playerStats[0].Total_Worshippers;
                string rPlayerStatus = playerStats[0].Personal_Status_Message;
                string rPlayerCreated = playerStats[0].Created_Datetime;
                string rHoursPlayed = playerStats[0].HoursPlayed.ToString() + " hours";
                double rWinRate = playerStats[0].Wins * 100 / (playerStats[0].Wins + playerStats[0].Losses);
                string rAvatarURL = playerStats[0].Avatar_URL;
                string rAvatarBorderURL = "";
                string rConquestTier = "";
                string rConquestTierImg = "";
                string rJoustTier = "";
                string rJoustTierImg = "";
                string rDuelTier = "";
                string rDuelTierImg = "";

                switch (playerStats[0].Tier_Conquest)
                {
                    case 0:
                        rConquestTier = "Qualifying";
                        rConquestTierImg = "https://i.imgur.com/lgtkkrX.png";
                        break;
                    case 1:
                        rConquestTier = "Bronze V";
                        rConquestTierImg = "https://i.imgur.com/MRHZNi3.png";
                        break;
                    case 2:
                        rConquestTier = "Bronze IV";
                        rConquestTierImg = "https://i.imgur.com/MRHZNi3.png";
                        break;
                    case 3:
                        rConquestTier = "Bronze III";
                        rConquestTierImg = "https://i.imgur.com/MRHZNi3.png";
                        break;
                    case 4:
                        rConquestTier = "Bronze II";
                        rConquestTierImg = "https://i.imgur.com/MRHZNi3.png";
                        break;
                    case 5:
                        rConquestTier = "Bronze I";
                        rConquestTierImg = "https://i.imgur.com/MRHZNi3.png";
                        break;
                    case 6:
                        rConquestTier = "Silver V";
                        rConquestTierImg = "https://i.imgur.com/UqcijNl.png";
                        break;
                    case 7:
                        rConquestTier = "Silver IV";
                        rConquestTierImg = "https://i.imgur.com/UqcijNl.png";
                        break;
                    case 8:
                        rConquestTier = "Silver II";
                        rConquestTierImg = "https://i.imgur.com/UqcijNl.png";
                        break;
                    case 9:
                        rConquestTier = "Silver II";
                        rConquestTierImg = "https://i.imgur.com/UqcijNl.png";
                        break;
                    case 10:
                        rConquestTier = "Silver I";
                        rConquestTierImg = "https://i.imgur.com/UqcijNl.png";
                        break;
                    case 11:
                        rConquestTier = "Gold V";
                        rConquestTierImg = "https://i.imgur.com/ZthzUZV.png";
                        break;
                    case 12:
                        rConquestTier = "Gold IV";
                        rConquestTierImg = "https://i.imgur.com/ZthzUZV.png";
                        break;
                    case 13:
                        rConquestTier = "Gold III";
                        rConquestTierImg = "https://i.imgur.com/ZthzUZV.png";
                        break;
                    case 14:
                        rConquestTier = "Gold II";
                        rConquestTierImg = "https://i.imgur.com/ZthzUZV.png";
                        break;
                    case 15:
                        rConquestTier = "Gold I";
                        rConquestTierImg = "https://i.imgur.com/ZthzUZV.png";
                        break;
                    case 16:
                        rConquestTier = "Platinum V";
                        rConquestTierImg = "https://i.imgur.com/WN3bMu8.png";
                        break;
                    case 17:
                        rConquestTier = "Platinum IV";
                        rConquestTierImg = "https://i.imgur.com/WN3bMu8.png";
                        break;
                    case 18:
                        rConquestTier = "Platinum III";
                        rConquestTierImg = "https://i.imgur.com/WN3bMu8.png";
                        break;
                    case 19:
                        rConquestTier = "Platinum II";
                        rConquestTierImg = "https://i.imgur.com/WN3bMu8.png";
                        break;
                    case 20:
                        rConquestTier = "Platinum I";
                        rConquestTierImg = "https://i.imgur.com/WN3bMu8.png";
                        break;
                    case 21:
                        rConquestTier = "Diamond V";
                        rConquestTierImg = "https://i.imgur.com/FtEARpH.png";
                        break;
                    case 22:
                        rConquestTier = "Diamond IV";
                        rConquestTierImg = "https://i.imgur.com/FtEARpH.png";
                        break;
                    case 23:
                        rConquestTier = "Diamond III";
                        rConquestTierImg = "https://i.imgur.com/FtEARpH.png";
                        break;
                    case 24:
                        rConquestTier = "Diamond II";
                        rConquestTierImg = "https://i.imgur.com/FtEARpH.png";
                        break;
                    case 25:
                        rConquestTier = "Diamond I";
                        rConquestTierImg = "https://i.imgur.com/FtEARpH.png";
                        break;
                    case 26:
                        rConquestTier = "Masters";
                        rConquestTierImg = "https://i.imgur.com/ojNo3yw.png";
                        break;
                    case 27:
                        rConquestTier = "Grandmaster";
                        rConquestTierImg = "https://i.imgur.com/MOPNkd0.png";
                        break;
                    default:
                        break;
                }

                switch (playerStats[0].Tier_Joust)
                {
                    case 0:
                        rJoustTier = "Qualifying";
                        rJoustTierImg = "https://i.imgur.com/lgtkkrX.png";
                        break;
                    case 1:
                        rJoustTier = "Bronze V";
                        rJoustTierImg = "https://i.imgur.com/btuvteO.png";
                        break;
                    case 2:
                        rJoustTier = "Bronze IV";
                        rJoustTierImg = "https://i.imgur.com/btuvteO.png";
                        break;
                    case 3:
                        rJoustTier = "Bronze III";
                        rJoustTierImg = "https://i.imgur.com/btuvteO.png";
                        break;
                    case 4:
                        rJoustTier = "Bronze II";
                        rJoustTierImg = "https://i.imgur.com/btuvteO.png";
                        break;
                    case 5:
                        rJoustTier = "Bronze I";
                        rJoustTierImg = "https://i.imgur.com/btuvteO.png";
                        break;
                    case 6:
                        rJoustTier = "Silver V";
                        rJoustTierImg = "https://i.imgur.com/wfVO4If.png";
                        break;
                    case 7:
                        rJoustTier = "Silver IV";
                        rJoustTierImg = "https://i.imgur.com/wfVO4If.png";
                        break;
                    case 8:
                        rJoustTier = "Silver II";
                        rJoustTierImg = "https://i.imgur.com/wfVO4If.png";
                        break;
                    case 9:
                        rJoustTier = "Silver II";
                        rJoustTierImg = "https://i.imgur.com/wfVO4If.png";
                        break;
                    case 10:
                        rJoustTier = "Silver I";
                        rJoustTierImg = "https://i.imgur.com/wfVO4If.png";
                        break;
                    case 11:
                        rJoustTier = "Gold V";
                        rJoustTierImg = "https://i.imgur.com/1g3nDnO.png";
                        break;
                    case 12:
                        rJoustTier = "Gold IV";
                        rJoustTierImg = "https://i.imgur.com/1g3nDnO.png";
                        break;
                    case 13:
                        rJoustTier = "Gold III";
                        rJoustTierImg = "https://i.imgur.com/1g3nDnO.png";
                        break;
                    case 14:
                        rJoustTier = "Gold II";
                        rJoustTierImg = "https://i.imgur.com/1g3nDnO.png";
                        break;
                    case 15:
                        rJoustTier = "Gold I";
                        rJoustTierImg = "https://i.imgur.com/1g3nDnO.png";
                        break;
                    case 16:
                        rJoustTier = "Platinum V";
                        rJoustTierImg = "https://i.imgur.com/vXOdpiK.png";
                        break;
                    case 17:
                        rJoustTier = "Platinum IV";
                        rJoustTierImg = "https://i.imgur.com/vXOdpiK.png";
                        break;
                    case 18:
                        rJoustTier = "Platinum III";
                        rJoustTierImg = "https://i.imgur.com/vXOdpiK.png";
                        break;
                    case 19:
                        rJoustTier = "Platinum II";
                        rJoustTierImg = "https://i.imgur.com/vXOdpiK.png";
                        break;
                    case 20:
                        rJoustTier = "Platinum I";
                        rJoustTierImg = "https://i.imgur.com/vXOdpiK.png";
                        break;
                    case 21:
                        rJoustTier = "Diamond V";
                        rJoustTierImg = "https://i.imgur.com/ZelrinZ.png";
                        break;
                    case 22:
                        rJoustTier = "Diamond IV";
                        rJoustTierImg = "https://i.imgur.com/ZelrinZ.png";
                        break;
                    case 23:
                        rJoustTier = "Diamond III";
                        rJoustTierImg = "https://i.imgur.com/ZelrinZ.png";
                        break;
                    case 24:
                        rJoustTier = "Diamond II";
                        rJoustTierImg = "https://i.imgur.com/ZelrinZ.png";
                        break;
                    case 25:
                        rJoustTier = "Diamond I";
                        rJoustTierImg = "https://i.imgur.com/ZelrinZ.png";
                        break;
                    case 26:
                        rJoustTier = "Masters";
                        rJoustTierImg = "https://i.imgur.com/d9TRRg7.png";
                        break;
                    case 27:
                        rJoustTier = "Grandmaster";
                        rJoustTierImg = "https://i.imgur.com/dDrGirx.png";
                        break;
                    default:
                        break;
                }

                switch (playerStats[0].Tier_Duel)
                {
                    case 0:
                        rDuelTier = "Qualifying";
                        rDuelTierImg = "https://i.imgur.com/lgtkkrX.png";
                        break;
                    case 1:
                        rDuelTier = "Bronze V";
                        rDuelTierImg = "https://i.imgur.com/zinPtsM.png";
                        break;
                    case 2:
                        rDuelTier = "Bronze IV";
                        rDuelTierImg = "https://i.imgur.com/zinPtsM.png";
                        break;
                    case 3:
                        rDuelTier = "Bronze III";
                        rDuelTierImg = "https://i.imgur.com/zinPtsM.png";
                        break;
                    case 4:
                        rDuelTier = "Bronze II";
                        rDuelTierImg = "https://i.imgur.com/zinPtsM.png";
                        break;
                    case 5:
                        rDuelTier = "Bronze I";
                        rDuelTierImg = "https://i.imgur.com/zinPtsM.png";
                        break;
                    case 6:
                        rDuelTier = "Silver V";
                        rDuelTierImg = "https://i.imgur.com/oC2wFN2.png";
                        break;
                    case 7:
                        rDuelTier = "Silver IV";
                        rDuelTierImg = "https://i.imgur.com/oC2wFN2.png";
                        break;
                    case 8:
                        rDuelTier = "Silver II";
                        rDuelTierImg = "https://i.imgur.com/oC2wFN2.png";
                        break;
                    case 9:
                        rDuelTier = "Silver II";
                        rDuelTierImg = "https://i.imgur.com/oC2wFN2.png";
                        break;
                    case 10:
                        rDuelTier = "Silver I";
                        rDuelTierImg = "https://i.imgur.com/oC2wFN2.png";
                        break;
                    case 11:
                        rDuelTier = "Gold V";
                        rDuelTierImg = "https://i.imgur.com/MOytELW.png";
                        break;
                    case 12:
                        rDuelTier = "Gold IV";
                        rDuelTierImg = "https://i.imgur.com/MOytELW.png";
                        break;
                    case 13:
                        rDuelTier = "Gold III";
                        rDuelTierImg = "https://i.imgur.com/MOytELW.png";
                        break;
                    case 14:
                        rDuelTier = "Gold II";
                        rDuelTierImg = "https://i.imgur.com/MOytELW.png";
                        break;
                    case 15:
                        rDuelTier = "Gold I";
                        rDuelTierImg = "https://i.imgur.com/MOytELW.png";
                        break;
                    case 16:
                        rDuelTier = "Platinum V";
                        rDuelTierImg = "https://i.imgur.com/0ZA0kIc.png";
                        break;
                    case 17:
                        rDuelTier = "Platinum IV";
                        rDuelTierImg = "https://i.imgur.com/0ZA0kIc.png";
                        break;
                    case 18:
                        rDuelTier = "Platinum III";
                        rDuelTierImg = "https://i.imgur.com/0ZA0kIc.png";
                        break;
                    case 19:
                        rDuelTier = "Platinum II";
                        rDuelTierImg = "https://i.imgur.com/0ZA0kIc.png";
                        break;
                    case 20:
                        rDuelTier = "Platinum I";
                        rDuelTierImg = "https://i.imgur.com/0ZA0kIc.png";
                        break;
                    case 21:
                        rDuelTier = "Diamond V";
                        rDuelTierImg = "https://i.imgur.com/QX3vj4U.png";
                        break;
                    case 22:
                        rDuelTier = "Diamond IV";
                        rDuelTierImg = "https://i.imgur.com/QX3vj4U.png";
                        break;
                    case 23:
                        rDuelTier = "Diamond III";
                        rDuelTierImg = "https://i.imgur.com/QX3vj4U.png";
                        break;
                    case 24:
                        rDuelTier = "Diamond II";
                        rDuelTierImg = "https://i.imgur.com/QX3vj4U.png";
                        break;
                    case 25:
                        rDuelTier = "Diamond I";
                        rDuelTierImg = "https://i.imgur.com/QX3vj4U.png";
                        break;
                    case 26:
                        rDuelTier = "Masters";
                        rDuelTierImg = "https://i.imgur.com/IDviWM2.png";
                        break;
                    case 27:
                        rDuelTier = "Grandmaster";
                        rDuelTierImg = "https://i.imgur.com/1JDyPJb.png";
                        break;
                    default:
                        break;
                }

                if (rAvatarURL == "")
                {
                    rAvatarURL = "https://i.imgur.com/VZPrD8S.png";
                }

                int borderNum = 0;
                if (rPlayerLevel >= 31 && rPlayerLevel < 51)
                {
                    borderNum = 1;
                }

                else if (rPlayerLevel >= 51 && rPlayerLevel < 71)
                {
                    borderNum = 2;
                }

                else if (rPlayerLevel >= 71 && rPlayerLevel < 91)
                {
                    borderNum = 3;
                }

                else if (rPlayerLevel >= 91 && rPlayerLevel < 111)
                {
                    borderNum = 4;
                }

                else if (rPlayerLevel >= 111 && rPlayerLevel < 131)
                {
                    borderNum = 5;
                }

                else if (rPlayerLevel >= 131 && rPlayerLevel < 151) // Immortal Prestige
                {
                    borderNum = 6;
                }

                else if (rPlayerLevel >= 151 && rPlayerLevel < 160) // Godlike Prestige
                {
                    borderNum = 7;
                }

                else if (rPlayerLevel == 160) // Ultimate Prestige
                {
                    borderNum = 8;
                }

                switch (borderNum)
                {
                    case 1:
                        rAvatarBorderURL = "https://i.imgur.com/amtJ0Lo.png";
                        break;
                    case 2:
                        rAvatarBorderURL = "https://i.imgur.com/noDuPCV.png";
                        break;
                    case 3:
                        rAvatarBorderURL = "https://i.imgur.com/7RpvKLe.png";
                        break;
                    case 4:
                        rAvatarBorderURL = "https://i.imgur.com/P4pj6QN.png";
                        break;
                    case 5:
                        rAvatarBorderURL = "https://i.imgur.com/x89o9On.png";
                        break;
                    case 6:
                        rAvatarBorderURL = "https://i.imgur.com/OT6CfFI.png";
                        break;
                    case 7:
                        rAvatarBorderURL = "https://i.imgur.com/PLhAqbN.png";
                        break;
                    case 8:
                        rAvatarBorderURL = "https://i.imgur.com/0eHoWT1.png";
                        break;
                    default:
                        rAvatarBorderURL = "";
                        break;
                }
                var css = "<html>\n\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n    <link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css\" integrity=\"sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO\"\n        crossorigin=\"anonymous\">\n    <style>\n        html {\n            height: 100%;\n        }\n        \n        body {\n            background-color: transparent;\n            color: white;\n            background-image: url(https://web2.hirez.com/smite/v3/s5/HarpyNest_S4.jpg);\n            background-repeat: no-repeat;\n            background-size: cover;\n            background-position: top;\n            margin: 5px;\n        }\n\n        .avatarPos {\n            position: absolute;\n            top: 25px;\n            left: 25px;\n        }\n\n        .imgA1 {\n            z-index: 1;\n            width: 70px;\n            left: 8px;\n            top: 8px;\n        }\n\n        .imgB1 {\n            z-index: 3;\n            width: 110px;\n            left: -12;\n            top: -11px;\n        }\n\n        .row {\n            border: 2px solid #26728d;\n            background-color: rgba(14, 25, 38, .9);\n            color: #c9f9fb;\n            padding: 10px;\n        }\n\n        .col {\n            border: 2px solid #26728d;\n        }\n\n        .AccStats {\n            font-size: 18px;\n        }\n\n        .names {\n            padding-left: 100px;\n        }\n\n        .levels {\n            position: fixed;\n            top: 18;\n            right: 18px;\n        }\n\n        .left-col {\n            float: left;\n            width: 50%;\n            background-color: rgba(14, 25, 38, .9);\n            background-image: url(https://i.imgur.com/MfBC9I6.png);\n            background-position: right;\n            background-repeat: no-repeat;\n            background-size: contain;\n            height: 140px;\n            padding-left: 10px;\n        }\n        \n        .right-col {\n            float: right;\n            width: 50%;\n            background-color: rgba(14, 25, 38, .9);\n            background-image: url(https://i.imgur.com/skbL9IZ.png);\n            background-position: left;\n            background-repeat: no-repeat;\n            background-size: contain;\n            height: 140px;\n            text-align: right;\n            padding-right: 10px;\n        }\n\n        .conquest-col {\n            width: 100%;\n            height: 140px;\n            background-image: url(https://i.imgur.com/ayyGCkZ.png);\n            background-size: cover;\n            background-position: center;\n            background-repeat: no-repeat;\n            border-bottom: 2px solid #26728d;\n            padding-left: 10px;\n        }\n    </style>\n</head>";
                var html = $"<body>\n    <div class=\"container-fluid\">\n        <div class=\"row\" style=\"height: 112px; border-bottom: 1px solid #26728d;\">\n            <div class=\"col-1\">\n                <img class=\"avatarPos imgA1\" src=\"{rAvatarURL}\">\n                <img class=\"avatarPos imgB1\" src=\"{rAvatarBorderURL}\">\n            </div>\n            <div class=\"names col-11\">\n                <h2>{rPlayerName}</h2>\n                <h5>{rTeamName}</h5>\n                <div class=\"levels\">\n                    <img src=\"https://i.imgur.com/8IluUqL.png\" />{rPlayerLevel}<br>\n                    <img src=\"https://i.imgur.com/cSFMiWX.png\" />{rPlayerMasteryLevel}<br>\n                    <img src=\"https://i.imgur.com/baSKFnW.png\" width=\"28px\" />{rTotalWorsh} \n                    <div style=\"position: fixed; top: 25px; right: 120; text-align: center; border-left: 1px solid #26728d; border-right: 1px solid #26728d; padding-left: 10px; padding-right: 10px;\">\n                            <h3>Playtime</h3>\n                            <h5>{rHoursPlayed}</h5>\n                    </div>\n                </div>\n            </div>\n        </div>\n        <div class=\"row\" style=\"border-top: 0px !important; padding-top: 5px !important; padding-bottom: 5px !important;\">\n            <div class=\"col-12\" style=\"padding-top: 0px !important; padding-bottom: 0px !important; text-align: center;\">\n                <h5 style=\"margin-bottom: 5px !important;\">{rPlayerStatus}</h5>\n            </div>\n        </div>\n        <div class=\"row\" style=\"margin-top: 3px; padding: 0; height: 284px;\">\n            <div class=\"conquest-col\" style=\"border-right: 1px solid #26728d;\">\n                <img src=\"{rConquestTierImg}\" />\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Conquest<br>{rConquestTier}</h5>\n            </div>\n            <div class=\"left-col\">\n                <img src=\"{rJoustTierImg}\" />\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Joust<br>{rJoustTier}</h5>\n            </div>\n            <div class=\"right-col\">\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Duel<br>{rDuelTier}</h5>\n                <img src=\"{rDuelTierImg}\" />\n            </div>\n        </div>\n    </div>\n</body>\n\n</html>";
                try
                {
                    if (!Directory.Exists("Storage/PlayerImages"))
                    {
                        Directory.CreateDirectory("Storage/PlayerImages");
                    }
                    var embed = new EmbedBuilder();
                    var fileName = $"Storage/PlayerImages/{playerStats[0].ActivePlayerId}.jpg";
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = ($":hourglass:Playtime");
                        field.Value = ($"🔹{rHoursPlayed}");
                    });
                    embed.WithImageUrl($"attachment://{playerStats[0].ActivePlayerId}.jpg");
                    embed.WithFooter(footer =>
                    {
                        footer
                            .WithText($"{playerStats[0].Personal_Status_Message}")
                            .WithIconUrl(Constants.botIcon);
                    });
                    await Context.Channel.SendFileAsync(fileName, embed: embed.Build());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        [Command("ranked", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task RankedCommand([Remainder] string username = "")
        {
            try
            {
                int playerID = 0;
                var getPlayerByDiscordID = new List<PlayerSpecial>();

                // Checking if we are searching for Player who linked his Discord with SMITE account
                if (username == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        username = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        await ReplyAsync("Command usage: `!!stats InGameName`");
                        return;
                    }
                }
                else if (Context.Message.MentionedUsers.Count != 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        username = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        await ReplyAsync("This Discord user has not linked his/hers Discord and SMITE account. To link your Discord and SMITE accounts, use `!!link` and follow the instructions.");
                        return;
                    }
                }

                // Searching for player with username
                var searchPlayer = await hirezAPI.SearchPlayer(username);
                var realSearchPlayers = new List<SearchPlayers>();
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == username.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                if (realSearchPlayers.Count == 0 || realSearchPlayers[0].Name.ToLowerInvariant() != username.ToLowerInvariant())
                {
                    await ReplyAsync($"<:X_:579151621502795777>*{username}* is hidden or not found!");
                }
                else if (realSearchPlayers[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
                {
                    await Context.Channel.TriggerTypingAsync();
                    try
                    {
                        if (playerID == 0)
                        {
                            playerID = realSearchPlayers[0].player_id;
                        }
                        string statusJson = await hirezAPI.GetPlayerStatus(playerID);
                        List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                        string matchJson = "";
                        if (playerStatus[0].Match != 0)
                        {
                            matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        }
                        var embed = await EmbedHandler.PlayerStatsEmbed(
                            await hirezAPI.GetPlayer(playerID.ToString()),
                            await hirezAPI.GetGodRanks(playerID),
                            await hirezAPI.GetPlayerAchievements(playerID),
                            await hirezAPI.GetPlayerStatus(playerID),
                            matchJson,
                            realSearchPlayers[0].portal_id);
                        var message = await Context.Channel.SendMessageAsync(embed: embed.Build());
                    }
                    catch (Exception ex)
                    {
                        await ErrorTracker.SendError($"Stats Error: \n{ex.Message}\n**InnerException: **{ex.InnerException}");
                        await ReplyAsync("Oops.. I've encountered an error. :sob:");
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Oops.. Either this player was not found or an unexpected error has occured.");
                await ErrorTracker.SendError($"**Stats Command**\n" +
                    $"**Message: **{Context.Message.Content}\n" +
                    $"**User: **{Context.Message.Author.Username}[{Context.Message.Author.Id}]\n" +
                    $"**Server and Channel: **ID:{Context.Guild.Id}[{Context.Channel.Id}]\n" +
                    $"**Error: **{ex.Message}\n" +
                    $"**InnerException: ** {ex.InnerException}");
            }
        }

        [Command("gods", true)]
        [Summary("Overall information about the gods in the game and current free god rotation.")]
        [Alias("годс")]
        public async Task GodsCommand()
        {
            List<Gods.God> gods = LoadAllGods();

            if (gods.Count != 0)
            {
                StringBuilder onRotation = new StringBuilder();
                var embed = new EmbedBuilder();
                var latestGod = gods.Find(x => x.latestGod == "y");
                var mages = gods.FindAll(x => x.Roles.Contains("Mage"));
                var hunters = gods.FindAll(x => x.Roles.Contains("Hunter"));
                var guardians = gods.FindAll(x => x.Roles.Contains("Guardian"));
                var assassins = gods.FindAll(x => x.Roles.Contains("Assassin"));
                var warriors = gods.FindAll(x => x.Roles.Contains("Warrior"));

                foreach (var god in gods)
                {
                    if (god.OnFreeRotation == "true")
                    {
                        onRotation.Append($"{god.Emoji} {god.Name}\n");
                    }
                }
                embed.WithColor(Constants.DefaultBlueColor);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"Gods: {gods.Count}";
                    x.Value = $"<:Mage:607990144380698625> {mages.Count}\n" +
                    $"<:Hunter:607990144271646740> {hunters.Count}\n" +
                    $"<:Guardian:607990144385024000> {guardians.Count}\n" +
                    $"<:Assassin:607990143915261983> {assassins.Count}\n" +
                    $"<:Warrior:607990144338886658> {warriors.Count}";
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "On Free Rotation";
                    x.Value = onRotation.ToString();
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Latest God";
                    x.Value = $"{latestGod.Emoji} {latestGod.Name}\n🔹 {latestGod.Title}\n🔹 {latestGod.Type}, {latestGod.Roles}";
                });
                embed.WithFooter(x =>
                {
                    x.Text = $"For God specific information use !!god GodName";
                });

                await ReplyAsync("", false, embed.Build());

            }
            else
            {
                await ReplyAsync("Something is not right... Please report this in my support server.");
            }
        }

        [Command("god", true)] // Get specific God information
        [Summary("Provides information about `GodName`.")]
        [Alias("g")]
        public async Task GodInfo([Remainder] string GodName)
        {
            string titleCaseGod = Text.ToTitleCase(GodName);
            if (titleCaseGod.Contains("'"))
            {
                titleCaseGod = titleCaseGod.Replace("'", "''");
            }
            List<Gods.God> gods = LoadGod(titleCaseGod);

            if (gods.Count == 0)
            {
                await ReplyAsync($"{titleCaseGod} was not found.");
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods[0].Name);
                    author.WithIconUrl(gods[0].godIcon_URL);
                });
                embed.WithTitle(gods[0].Title);
                embed.WithThumbnailUrl(gods[0].godIcon_URL);
                if (gods[0].DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods[0].DomColor));
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Pantheon";
                    field.Value = gods[0].Pantheon;
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Type";
                    field.Value = gods[0].Type;
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Role";
                    field.Value = gods[0].Roles;
                });
                if (gods[0].Pros != null || gods[0].Pros != " " || gods[0].Pros != "")
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = false;
                        field.Name = "Pros";
                        field.Value = gods[0].Pros + "\u200b";
                    });
                }
                embed.WithFooter(x =>
                {
                    x.IconUrl = Constants.botIcon;
                    x.Text = "More info soon™..";
                });

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("rgod", true)] // Random God
        [Summary("Gives you a random God and randomised build.")]
        [Alias("rg", "randomgod", "random")]
        public async Task RandomGod()
        {
            List<Gods.God> gods = Database.LoadAllGodsWithLessInfo();

            int rr = rnd.Next(gods.Count);
            string godType = "";
            StringBuilder sb = new StringBuilder();

            // Random Build START

            if (gods[rr].Roles.Contains("Mage") || gods[rr].Roles.Contains("Guardian"))
            {
                godType = "magical";
            }
            else
            {
                godType = "physical";
            }

            // Random Relics
            var active = await Database.GetActiveActives();
            for (int a = 0; a < 2; a++)
            {
                int ar = rnd.Next(active.Count);
                sb.Append(active[ar].Emoji);
                active.RemoveAt(ar);
            }

            // Boots or Shoes depending on the god type
            if (!gods[rr].Name.Contains("Ratatoskr"))
            {
                var boots = await Database.GetBootsOrShoes(godType);
                int boot = rnd.Next(boots.Count);
                sb.Append(boots[boot].Emoji);
            }
            else
            {
                var boots = await Database.GetBootsOrShoes(gods[rr].Name);
                sb.Append(boots[0].Emoji);
            }

            var items = await Database.GetActiveItemsByGodType(godType, gods[rr].Roles.ToLowerInvariant().Trim());

            // Finishing the build with 5 items
            for (int i = 0; i < 5; i++)
            {
                int r = rnd.Next(items.Count);
                sb.Append(items[r].Emoji);
                items.RemoveAt(r);
            }

            // Random Build END

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(gods[rr].Name);
                author.WithIconUrl(gods[rr].godIcon_URL);
            });
            embed.WithTitle(gods[rr].Title);
            embed.WithThumbnailUrl(gods[rr].godIcon_URL);
            if (gods[0].DomColor != 0)
            {
                embed.WithColor(new Color((uint)gods[rr].DomColor));
            }
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Role";
                field.Value = gods[rr].Roles;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Type";
                field.Value = gods[rr].Type;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Random Build";
                x.Value = sb.ToString();
            });
            await ReplyAsync($"{Context.Message.Author.Mention}, your random god is:", false, embed.Build());
        }

        [Command("rteam", true)]
        [Summary("Gives you `number` random Gods with randomised builds for them.")]
        [Alias("team", "ртеам", "теам", "rt")]
        public async Task RandomTeam(int number)
        {
            if (!(number > 5))
            {
                var embed = new EmbedBuilder();
                var gods = Database.LoadAllGodsWithLessInfo();

                embed.WithColor(Constants.DefaultBlueColor);

                for (int i = 0; i < number; i++)
                {
                    int rr = rnd.Next(gods.Count);
                    string godType = "";
                    StringBuilder sb = new StringBuilder();

                    // Random Build START

                    if (gods[rr].Roles.Contains("Mage") || gods[rr].Roles.Contains("Guardian"))
                    {
                        godType = "magical";
                    }
                    else
                    {
                        godType = "physical";
                    }

                    // Random Relics
                    var active = await Database.GetActiveActives();
                    for (int a = 0; a < 2; a++)
                    {
                        int ar = rnd.Next(active.Count);
                        sb.Append(active[ar].Emoji);
                        active.RemoveAt(ar);
                    }

                    // Boots or Shoes depending on the god type
                    if (!gods[rr].Name.Contains("Ratatoskr"))
                    {
                        var boots = await Database.GetBootsOrShoes(godType);
                        int boot = rnd.Next(boots.Count);
                        sb.Append(boots[boot].Emoji);
                    }
                    else
                    {
                        var boots = await Database.GetBootsOrShoes(gods[rr].Name);
                        sb.Append(boots[0].Emoji);
                    }

                    var items = await Database.GetActiveItemsByGodType(godType, gods[rr].Roles.ToLowerInvariant().Trim());

                    // Finishing the build with 5 items
                    for (int b = 0; b < 5; b++)
                    {
                        int r = rnd.Next(items.Count);
                        sb.Append(items[r].Emoji);
                        items.RemoveAt(r);
                    }

                    // Random Build END

                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{gods[rr].Emoji} {gods[rr].Name}";
                        x.Value = sb.ToString();
                    });
                }

                await ReplyAsync($"Team of {number} for you, {Context.Message.Author.Mention}!", false, embed.Build());
            }
            else
            {
                await ReplyAsync("That's not a proper number for a team, don't ya' think?");
            }
        }

        [Command("status", true)] // SMITE Server Status 
        [Summary("Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.")]
        [Alias("статус", "statis", "s", "с", "server", "servers", "se", "се", "serverstatus")]
        public async Task ServerStatusCheck()
        {
            var smiteServerStatus = JsonConvert.DeserializeObject<ServerStatus>(await StatusPage.GetStatusSummary());
            string discjson = await StatusPage.GetDiscordStatusSummary();
            var discordStatus = new ServerStatus();
            if (discjson != "")
            {
                discordStatus = JsonConvert.DeserializeObject<ServerStatus>(discjson);
            }

            await ReplyAsync("", false, EmbedHandler.ServerStatusEmbed(smiteServerStatus, discordStatus).Build()); // Server Status POST
            bool maint = false;
            bool inci = false;
            // Incidents
            if (smiteServerStatus.incidents.Count >= 1)
            {
                for (int i = 0; i < smiteServerStatus.incidents.Count; i++)
                {
                    if ((smiteServerStatus.incidents[i].name.ToLowerInvariant().Contains("smite") ||
                         smiteServerStatus.incidents[i].incident_updates[0].body.ToLowerInvariant().Contains("smite")) && 
                         !(smiteServerStatus.incidents[i].name.ToLowerInvariant().Contains("blitz")))
                    {
                        inci = true;
                    }
                }
            }
            if (inci == true)
            {
                var embed = EmbedHandler.StatusIncidentEmbed(smiteServerStatus);
                if (embed != null)
                {
                    await ReplyAsync("", false, EmbedHandler.StatusIncidentEmbed(smiteServerStatus).Build());
                }
            }
            // Scheduled Maintenances
            if (smiteServerStatus.scheduled_maintenances.Count >= 1)
            {
                for (int c = 0; c < smiteServerStatus.scheduled_maintenances.Count; c++)
                {
                    if (smiteServerStatus.scheduled_maintenances[c].name.ToLowerInvariant().Contains("smite"))
                    {
                        maint = true;
                    }
                }
            }
            if (maint == true)
            {
                await ReplyAsync("", false, EmbedHandler.StatusMaintenanceEmbed(smiteServerStatus).Build());
            }
        }

        [Command("statusupdates")]
        [Summary("When SMITE incidents and scheduled maintenances appear in the status page they will be sent to #channel")]
        [Alias("statusupd", "su")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task SetStatusUpdatesChannel(SocketChannel message)
        {
            await SetNotifChannel(Context.Guild.Id, Context.Guild.Name, message.Id);
            SocketTextChannel channel = Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(message.Id);
            try
            {
                await channel.SendMessageAsync($":white_check_mark: {channel.Mention} is now set to receive notifications about SMITE Server Status updates.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Send Messages** permission for {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.Contains("Missing Access"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Access** to {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.ToLowerInvariant().Contains("multiple matches"))
                {
                    await Context.Channel.SendMessageAsync("Multiple matches found. Please #mention the channel.");
                }
                else
                {
                    await ReplyAsync(":warning: Something went wrong. This error was reported to the bot creator and will soon be checked.");
                    await ErrorTracker.SendError($"**Error in StatusUpdates command**\n" +
                        $"{ex.Message}\n**Message: **{message}\n" +
                        $"**Server: **{Context.Guild.Name}[{Context.Guild.Id}]\n" +
                        $"**Channel: **{Context.Channel.Name}[{Context.Channel.Id}]");
                }
            }
        }

        [Command("stopstatusupdates", true)]
        [Summary("Stops sending messages from the SMITE status page.")]
        [Alias("ssu")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task StopStatusUpdates()
        {
            await StopNotifs(Context.Guild.Id);

            await ReplyAsync($"**{Context.Guild.Name}** will no longer receive SMITE Server Status updates.");
        }

        [Command("claninfo")]
        [Alias("clan", "c")]
        [RequireOwner] // vremenno
        public async Task ClanInfoCommand(int id)
        {
            string json = await hirezAPI.GetTeamDetails(id);
            List<ClanInfo> clanList = JsonConvert.DeserializeObject<List<ClanInfo>>(json);
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(clanList[0].Name);
                author.WithIconUrl(Constants.botIcon);
            });
            embed.WithColor(0, 255, 0);
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Tag");
                field.WithValue(clanList[0].Tag == "" ? "n/a" : clanList[0].Tag);
            });
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Founder");
                field.WithValue(clanList[0].Founder == "" ? "n/a" : clanList[0].Founder);
            });
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Players");
                field.WithValue(clanList[0].Players.ToString());
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("item")]
        [Summary("Provides information about `ItemName`.")]
        [Alias("i")]
        public async Task ItemInfoCommand([Remainder]string ItemName)
        {
            if (ItemName.Contains("'"))
            {
                ItemName = ItemName.Replace("'", "''");
            }
            var item = await Database.GetSpecificItem(ItemName);
            if (item.Count != 0)
            {
                string secondaryDesc = item[0].SecondaryDescription;
                var embed = new EmbedBuilder();

                if (secondaryDesc != null || secondaryDesc == "")
                {
                    if (secondaryDesc.Contains("PASSIVE"))
                    {
                        secondaryDesc = secondaryDesc.Replace("PASSIVE", "**PASSIVE**");
                    }
                    if (secondaryDesc.Contains("<font color='#42F46E'>"))
                    {
                        secondaryDesc = secondaryDesc.Replace("<font color='#42F46E'>", "");
                    }
                    if (secondaryDesc.Contains("<font color='#F44242'>"))
                    {
                        secondaryDesc = secondaryDesc.Replace("<font color='#F44242'>", "");
                    }
                    else if (secondaryDesc.Contains("AURA"))
                    {
                        secondaryDesc = secondaryDesc.Replace("AURA", "**AURA**");
                    }
                    else if (secondaryDesc.Contains("ROLE QUEST"))
                    {
                        secondaryDesc = secondaryDesc.Replace("ROLE QUEST", "**ROLE QUEST**");
                    }
                }
                embed.WithAuthor(x =>
                {
                    x.Name = item[0].DeviceName;
                    x.IconUrl = item[0].itemIcon_URL;
                });
                embed.WithColor(new Color((uint)item[0].DomColor));
                embed.WithThumbnailUrl(item[0].itemIcon_URL);
                embed.WithTitle(item[0].ItemBenefits);
                embed.WithDescription($"{item[0].ItemDescription}\n\n{secondaryDesc}");

                // we doin calculations bois
                var itemsForPrice = new List<Item>();
                var itemsForPriceTwo = new List<Item>();
                int itemPrice = 0;

                if (item[0].ChildItemId != 0)
                {
                    itemsForPrice = await Database.GetSpecificItemByID(item[0].ChildItemId);
                    if (itemsForPrice[0].ChildItemId != 0)
                    {
                        itemsForPriceTwo = await Database.GetSpecificItemByID(itemsForPrice[0].ChildItemId);
                        itemPrice = itemsForPriceTwo[0].Price;
                    }
                    itemPrice = itemPrice + item[0].Price + itemsForPrice[0].Price;
                }
                // yeah sure

                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Total Price (Price)";
                    x.Value = $"<:coins:590942235474919464>{itemPrice} ({item[0].Price})";
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Tier";
                    x.Value = $"{item[0].ItemTier}";
                });
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                await ReplyAsync($"{item.Count} results found for `{ItemName}`.");
            }
        }

        [Command("trello", true)]
        [Summary("Checks the [SMITE Community Issues Trello Board](https://trello.com/b/d4fJtBlo/smite-community-issues).")]
        [Alias("issues", "bugs", "board")]
        public async Task TrelloBoardCommand()
        {
            var embed = new EmbedBuilder();
            var result = await trelloAPI.GetTrelloCards();

            StringBuilder topIssues = new StringBuilder(2048);
            StringBuilder hotfixNotes = new StringBuilder(1024);
            StringBuilder incominghotfix = new StringBuilder(1024);

            foreach (var item in result) // Top Issues
            {
                if (item.idList == "5c740d7d4e18c107890167ea")
                {
                    topIssues.Append($"🔹[{item.name}]({item.shortUrl})\n");
                }
            }
            foreach (var item in result) // Hotfix PatchNotes
            {
                if (item.idList == "5c740da2ff81b93a4039da81")
                {
                    hotfixNotes.Append($"🔹[{item.name}]({item.shortUrl})\n");
                }
            }
            foreach (var item in result) // Incoming hotfix
            {
                if (item.idList == "5c804623d75e55500472cf9a")
                {
                    incominghotfix.Append($"🔹[{item.name}]({item.shortUrl})\n");
                }
            }

            embed.WithAuthor(x =>
            {
                x.Name = "SMITE Community Issues Trello Board";
                x.Url = "https://trello.com/b/d4fJtBlo/smite-community-issues";
                x.IconUrl = "https://cdn3.iconfinder.com/data/icons/popular-services-brands-vol-2/512/trello-512.png";
            });

            // Incoming Hotfix
            if (incominghotfix.ToString() != "")
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Incoming Hotfix";
                    x.Value = incominghotfix.ToString();
                });
            }

            // Hotfix Patch Notes
            if (hotfixNotes.ToString() != "")
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Hotfix Patch Notes";
                    x.Value = hotfixNotes.ToString();
                });
            }

            embed.WithColor(210, 144, 52);
            embed.WithTitle("All Platforms Top Issues");
            if (!(topIssues.ToString().Length > 2048))
            {
                embed.WithDescription(topIssues.ToString());
            }
            else
            {
                // Its longer than 2048
                embed.WithDescription(Text.Truncate(topIssues.ToString(), 2048));
            }

            await ReplyAsync("", false, embed.Build());
        }

        [Command("livematch", RunMode = RunMode.Async)]
        [Summary("Match details if provided `PlayerName` is in a match.")]
        [Alias("live", "lm", "l")]
        public async Task LiveMatchCommand([Remainder]string PlayerName = "")
        {
            try
            {
                int playerIDint = 0;
                var onMultiplePlayersResult = new MultiplePlayersStruct();
                var getPlayerByDiscordID = new List<PlayerSpecial>();

                await Context.Channel.TriggerTypingAsync();
                // Checking if the message author has linked account
                if (PlayerName == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerIDint = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Command usage: `!!livematch InGameName`");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                // Checking if mentioned user has linked account
                else if (Context.Message.MentionedUsers.Count != 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerIDint = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                // Searching for the player with the provided username
                if (playerIDint == 0)
                {
                    var searchPlayer = await hirezAPI.SearchPlayer(PlayerName);
                    var realSearchPlayers = new List<SearchPlayers>();
                    if (searchPlayer.Count != 0)
                    {
                        foreach (var player in searchPlayer)
                        {
                            if (player.Name.ToLowerInvariant() == PlayerName.ToLowerInvariant())
                            {
                                realSearchPlayers.Add(player);
                            }
                        }
                    }
                    // Checking the new list for count of users in it
                    if (realSearchPlayers.Count == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserNotFound(PlayerName));
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    else if (!(realSearchPlayers.Count > 1))
                    {
                        if (realSearchPlayers[0].privacy_flag == "y")
                        {
                            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserIsHidden(PlayerName));
                            await ReplyAsync(embed: embed);
                            return;
                        }
                        playerIDint = realSearchPlayers[0].player_id;
                        PlayerName = realSearchPlayers[0].Name;
                    }
                    else
                    {
                        //multiple players
                        onMultiplePlayersResult = await MultiplePlayersHandler(realSearchPlayers, Context);
                        if (onMultiplePlayersResult.searchPlayers != null && onMultiplePlayersResult.searchPlayers.player_id == 0)
                        {
                            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserNotFound(PlayerName));
                            await ReplyAsync(embed: embed);
                            return;
                        }
                        else if (onMultiplePlayersResult.searchPlayers == null && onMultiplePlayersResult.userMessage == null)
                        {
                            return;
                        }
                        playerIDint = onMultiplePlayersResult.searchPlayers.player_id;
                        PlayerName = onMultiplePlayersResult.searchPlayers.Name;
                    }
                }

                var playerstatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(await hirezAPI.GetPlayerStatus(playerIDint));
                // Checking if the player is online and is in match
                if (playerstatus[0].Match == 0)
                {
                    if (onMultiplePlayersResult.userMessage == null)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{PlayerName} is not in a match.");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{PlayerName} is not in a match.");
                        await onMultiplePlayersResult.userMessage.ModifyAsync(x =>
                        {
                            x.Embed = embed;
                        });
                        return;
                    }
                }
                var matchPlayerDetails = new List<MatchPlayerDetails.PlayerMatchDetails>(JsonConvert.DeserializeObject<List<MatchPlayerDetails.PlayerMatchDetails>>(await hirezAPI.GetMatchPlayerDetails(playerstatus[0].Match)));

                if (matchPlayerDetails[0].ret_msg == null)
                {
                    if (onMultiplePlayersResult.userMessage == null)
                    {
                        var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                        await onMultiplePlayersResult.userMessage.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                    }
                }
                else
                {
                    await ReplyAsync(matchPlayerDetails[0].ret_msg.ToString());
                }
            }
            catch (Exception ex)
            {
                await ErrorTracker.SendException(ex, Context);
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("matchdetails", RunMode = RunMode.Async)]
        [Summary("Match details for the provided `MatchID` or latest match played of provided `PlayerName`.")]
        [Alias("md", "мд")]
        public async Task MatchDetailsCommand([Remainder]string MatchID = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();

                var onMultiplePlayersResult = new MultiplePlayersStruct();
                var getPlayerByDiscordID = new List<PlayerSpecial>();
                var getPlayerIdByName = new List<PlayerIDbyName>();
                var matchHistory = new List<MatchHistoryModel>();

                //Checking the linked account
                if (MatchID == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        matchHistory = await hirezAPI.GetMatchHistory(getPlayerByDiscordID[0].active_player_id);
                        MatchID = matchHistory[0].Match.ToString();
                    }
                    else
                    {
                        await ReplyAsync("Command usage: `!!matchdetails MatchID`");
                        return;
                    }
                }
                else if (Context.Message.MentionedUsers.Count != 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        matchHistory = await hirezAPI.GetMatchHistory(getPlayerByDiscordID[0].active_player_id);
                        MatchID = matchHistory[0].Match.ToString();
                    }
                    else
                    {
                        await ReplyAsync(Constants.NotLinked);
                        return;
                    }
                }
                else if (!MatchID.All(char.IsDigit))
                {
                    // Searching for the player
                    var searchPlayer = await hirezAPI.SearchPlayer(MatchID);
                    var realSearchPlayers = new List<SearchPlayers>();
                    if (searchPlayer.Count != 0)
                    {
                        foreach (var player in searchPlayer)
                        {
                            if (player.Name.ToLowerInvariant() == MatchID.ToLowerInvariant())
                            {
                                realSearchPlayers.Add(player);
                            }
                        }
                    }
                    // Checking the new list for count of users in it
                    if (realSearchPlayers.Count == 0)
                    {
                        await ReplyAsync(Text.UserNotFound(MatchID));
                        return;
                    }
                    else if (!(realSearchPlayers.Count > 1))
                    {
                        if (realSearchPlayers[0].privacy_flag == "y")
                        {
                            await ReplyAsync(Text.UserIsHidden(MatchID));
                            return;
                        }
                        matchHistory = await hirezAPI.GetMatchHistory(realSearchPlayers[0].player_id);
                        MatchID = matchHistory[0].Match.ToString();
                    }
                    else
                    {
                        //On Multiple players
                        onMultiplePlayersResult = await MultiplePlayersHandler(realSearchPlayers, Context);
                        if (onMultiplePlayersResult.searchPlayers != null && onMultiplePlayersResult.searchPlayers.player_id == 0)
                        {
                            await ReplyAsync(Text.UserNotFound(MatchID));
                            return;
                        }
                        else if (onMultiplePlayersResult.searchPlayers == null && onMultiplePlayersResult.userMessage == null)
                        {
                            return;
                        }
                        matchHistory = await hirezAPI.GetMatchHistory(onMultiplePlayersResult.searchPlayers.player_id);
                        MatchID = matchHistory[0].Match.ToString();
                    }
                }
                if (MatchID == "0")
                {
                    await ReplyAsync($"{MatchID} has no recent matches in record.");
                    return;
                }
                string matchDetailsString = await hirezAPI.GetMatchDetails(Int32.Parse(MatchID));
                if (matchDetailsString.ToLowerInvariant().Contains("<"))
                {
                    await ReplyAsync("Hi-Rez API sent a weird response...");
                    await ErrorTracker.SendError(matchDetailsString);
                    return;
                }
                var matchDetails = JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(matchDetailsString);
                if (onMultiplePlayersResult.userMessage != null)
                {
                    var embed = await EmbedHandler.MatchDetailsEmbed(matchDetails);
                    await onMultiplePlayersResult.userMessage.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                }
                else
                {
                    var embed = await EmbedHandler.MatchDetailsEmbed(matchDetails);
                    await ReplyAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
            }
        }
        
        [Command("matchhistory", true, RunMode = RunMode.Async)]
        [Summary("[WIP] Latest match history for `PlayerName`.")]
        [Alias("mh", "мх", "history")]
        public async Task MatchHistoryCommand([Remainder]string PlayerName = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();

                var onMultiplePlayersResult = new MultiplePlayersStruct();
                var getPlayerByDiscordID = new List<PlayerSpecial>();
                var getPlayerIdByName = new List<PlayerIDbyName>();
                var matchHistory = new List<MatchHistoryModel>();

                //Checking the linked account
                if (PlayerName == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        matchHistory = await hirezAPI.GetMatchHistory(getPlayerByDiscordID[0].active_player_id);
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Command usage: `!!matchhistory PlayerName`");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                else if (Context.Message.MentionedUsers.Count != 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        matchHistory = await hirezAPI.GetMatchHistory(getPlayerByDiscordID[0].active_player_id);
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                else
                {
                    // Searching for the player
                    var searchPlayer = await hirezAPI.SearchPlayer(PlayerName);
                    var realSearchPlayers = new List<SearchPlayers>();
                    if (searchPlayer.Count != 0)
                    {
                        foreach (var player in searchPlayer)
                        {
                            if (player.Name.ToLowerInvariant() == PlayerName.ToLowerInvariant())
                            {
                                realSearchPlayers.Add(player);
                            }
                        }
                    }
                    // Checking the new list for count of users in it
                    if (realSearchPlayers.Count == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserNotFound(PlayerName));
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    else if (!(realSearchPlayers.Count > 1))
                    {
                        if (realSearchPlayers[0].privacy_flag == "y")
                        {
                            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserIsHidden(PlayerName));
                            await ReplyAsync(embed: embed);
                            return;
                        }
                        matchHistory = await hirezAPI.GetMatchHistory(realSearchPlayers[0].player_id);
                    }
                    else
                    {
                        //On Multiple players
                        onMultiplePlayersResult = await MultiplePlayersHandler(realSearchPlayers, Context);
                        if (onMultiplePlayersResult.searchPlayers != null && onMultiplePlayersResult.searchPlayers.player_id == 0)
                        {
                            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserNotFound(PlayerName));
                            await ReplyAsync(embed: embed);
                            return;
                        }
                        else if (onMultiplePlayersResult.searchPlayers == null && onMultiplePlayersResult.userMessage == null)
                        {
                            return;
                        }
                        matchHistory = await hirezAPI.GetMatchHistory(onMultiplePlayersResult.searchPlayers.player_id);
                    }
                }

                var finalembed = await EmbedHandler.BuildMatchHistoryEmbedAsync(matchHistory);
                await ReplyAsync(embed: finalembed);
                // do dat
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("motd", true, RunMode = RunMode.Async)]
        [Summary("Information about upcoming MOTDs in the game.")]
        [Alias("motds", "мотд", "мотдс")]
        public async Task MotdCommand()
        {
            try
            {
                string json = await hirezAPI.GetMOTD();
                var motdList = JsonConvert.DeserializeObject<List<Motd>>(json);
                string desc = "";
                string shitman = "";

                var embed = new EmbedBuilder();

                embed.WithColor(0, 80, 188);
                embed.WithAuthor(x =>
                {
                    x.Name = "Matches Of The Day";
                    x.IconUrl = Constants.botIcon;
                });
                Motd motdDay = new Motd();
                for (int i = 0; i < 5; i++)
                {
                    string[] finalDesc = { };
                    motdDay = motdList.Find(x => x.startDateTime.Date == DateTime.Today.AddDays(i));
                    if (motdDay == null)
                    {
                        await ReplyAsync("", false, embed.Build());
                        return;
                    }
                    desc = Text.ReFormatMOTDText(motdDay.description);
                    if (desc.Contains("**Map"))
                    {
                        finalDesc = desc.Split("**Map");
                        shitman = "**Map" + finalDesc[1];
                    }
                    else
                    {
                        shitman = desc;
                    }
                    embed.AddField(x =>
                    {
                        x.Name = $":large_blue_diamond: **{motdDay.title}** - {motdDay.startDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}";
                        x.Value = $"{shitman}";
                    });
                }

                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("wp", RunMode = RunMode.Async)]
        [Summary("Shows all masteries for gods played by the provided `PlayerName`.")]
        [Alias("wps", "worshipers", "mastery", "masteries")]
        public async Task WorshipersCommand([Remainder]string PlayerName = "")
        {
            try
            {
                int playerID = 0;
                var getPlayerByDiscordID = new List<PlayerSpecial>();

                // Checking if we are searching for Player who linked his Discord with SMITE account
                if (PlayerName == "")
                {
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(Context.Message.Author.Id);

                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Command usage: `!!wp InGameName`");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                else if (Context.Message.MentionedUsers.Count != 0 && PlayerName == $"<@!{Context.Message.MentionedUsers.Last().Id}>")
                {
                    var mentionedUser = Context.Message.MentionedUsers.Last();
                    getPlayerByDiscordID = await GetPlayerSpecialsByDiscordID(mentionedUser.Id);
                    if (getPlayerByDiscordID.Count != 0)
                    {
                        playerID = getPlayerByDiscordID[0].active_player_id;
                        PlayerName = getPlayerByDiscordID[0].Name;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }
                // Finding all occurences of provided username and adding them in a list
                var searchPlayer = await hirezAPI.SearchPlayer(PlayerName);
                var realSearchPlayers = new List<SearchPlayers>();
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == PlayerName.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                // Checking the new list for count of users in it
                if (realSearchPlayers.Count == 0)
                {
                    // Profile doesn't exist
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await ReplyAsync(embed: embed.Build());
                }
                else if (!(realSearchPlayers.Count > 1))
                {
                    if (realSearchPlayers[0].privacy_flag != "n")
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Text.UserIsHidden(PlayerName));
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    await Context.Channel.TriggerTypingAsync();
                    if (playerID == 0)
                    {
                        playerID = realSearchPlayers[0].player_id;
                    }
                    string json = await hirezAPI.GetGodRanks(playerID);
                    var ranks = JsonConvert.DeserializeObject<List<GodRanks>>(json);
                    var embedz = await EmbedHandler.BuildWorshipersEmbedAsync(ranks, realSearchPlayers[0]);
                    var message = await Context.Channel.SendMessageAsync(embed: embedz);
                }
                else
                {
                    // Multiple Players?
                    var result = await MultiplePlayersHandler(realSearchPlayers, Context);
                    if (result.searchPlayers != null && result.searchPlayers.player_id == 0)
                    {
                        var embedz = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                        await ReplyAsync(embed: embedz.Build());
                        return;
                    }
                    else if (result.searchPlayers == null && result.userMessage == null)
                    {
                        return;
                    }
                    playerID = result.searchPlayers.player_id;

                    string json = await hirezAPI.GetGodRanks(playerID);
                    var ranks = JsonConvert.DeserializeObject<List<GodRanks>>(json);
                    var embed = await EmbedHandler.BuildWorshipersEmbedAsync(ranks, result.searchPlayers);
                    await result.userMessage.ModifyAsync(x =>
                    {
                        x.Embed = embed;
                    });
                }
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("link", true)]
        [Summary("Link your Discord and SMITE accounts.")]
        public async Task LinkingInfoCommand()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = "Thoth Account Linking";
                x.IconUrl = Constants.botIcon;
            });
            embed.WithDescription($"Thoth Account Linking will link your Discord and SMITE account in our database which will " +
                $"allow executing commands without providing InGameName. \nCommands using this feature are:\n" +
                $"`{Credentials.botConfig.prefix}stats`, `{Credentials.botConfig.prefix}livematch`, " +
                $"`{Credentials.botConfig.prefix}matchdetails`, `{Credentials.botConfig.prefix}matchhistory`, " +
                $"`{Credentials.botConfig.prefix}wp`\n" +
                $"❗**Before starting the linking process, make sure your account in SMITE is NOT hidden! " +
                $"Linking requires changing your Personal Status Message in-game to verify that the said account is yours.** " +
                $"You can change it to your previous status message after linking is completed." +
                $"\n\n__To start the linking process write **{Credentials.botConfig.prefix}startlink** and follow the instructions.__");
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithFooter(x => x.Text = "This is not official Hi-Rez linking!");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("startlink", true, RunMode = RunMode.Async)]
        [Alias("startlinking")]
        public async Task LinkAccountsCommand()
        {
            try
            {
                int playerID = 0;
                var onMultiplePlayersResult = new MultiplePlayersStruct();

                var embed = new EmbedBuilder();
                embed.WithDescription("Please enter your ingame name and **make sure your account is not hidden and you are logged in SMITE**" +
                    "\n*you have 120 seconds to respond*");
                embed.WithAuthor(x =>
                {
                    x.Name = "Thoth Account Linking";
                    x.IconUrl = Constants.botIcon;
                });
                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithFooter(x => x.Text = "This is not official Hi-Rez linking!");

                var message = await ReplyAsync("", false, embed.Build());
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));

                if (response == null)
                {
                    embed.Description = ":red_circle: **Time is up, linking cancelled**";
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }
                else if (response.Content.StartsWith(Credentials.botConfig.prefix) || 
                    response.Content.StartsWith(GetServerConfig(Context.Guild.Id).Result[0].prefix))
                {
                    embed.WithDescription(Text.UserNotFound(response.Content) + "\n🔴 Linking cancelled.");
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }

                // Finding all occurences of provided username and adding them in a list
                var searchPlayer = await hirezAPI.SearchPlayer(response.Content);
                var realSearchPlayers = new List<SearchPlayers>();
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == response.Content.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                // Checking the new list for count of users in it
                if (realSearchPlayers.Count == 0)
                {
                    embed.WithDescription(Text.UserNotFound(response.Content) + "\n🔴 Linking cancelled.");
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }
                else if (!(realSearchPlayers.Count > 1))
                {
                    if (realSearchPlayers[0].privacy_flag == "y")
                    {
                        embed.WithDescription($"{realSearchPlayers[0].Name} is hidden. " +
                            $"Please unhide your profile by unchecking the \"Hide my Profile\" under the Profile tab and try again.");
                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                        return;
                    }
                    playerID = realSearchPlayers[0].player_id;
                }
                else
                {
                    //On Multiple players
                    onMultiplePlayersResult = await MultiplePlayersHandler(realSearchPlayers, Context, message);
                    if (onMultiplePlayersResult.searchPlayers != null && onMultiplePlayersResult.searchPlayers.player_id == 0)
                    {
                        embed.WithDescription("🔴 Linking cancelled.");
                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                        return;
                    }
                    else if (onMultiplePlayersResult.searchPlayers == null && onMultiplePlayersResult.userMessage == null)
                    {
                        return;
                    }
                    playerID = onMultiplePlayersResult.searchPlayers.player_id;
                }

                string getplayerJSON = await hirezAPI.GetPlayer(playerID.ToString());
                var getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);

                string generatedString = GenerateString();
                embed.Author.IconUrl = Context.Message.Author.GetAvatarUrl();
                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                embed.WithDescription($"<:level:529719212017451008>**Level**: {getplayerList[0].Level}\n" +
                    $"📅**Account Created**: {getplayerList[0].Created_Datetime}\n\n" +
                    $"**If this is your account, change your __Personal Status Message__ to: `{generatedString}` so we can be sure " +
                    $"it's your account. You can change it to your previous status message after linking is completed. " +
                    $"**\n*You have 120 seconds to perform this action.*\n" +
                    $"\nWhen you are done, write `done` or anything else to cancel.");
                embed.ImageUrl = "https://media.discordapp.net/attachments/528621646626684928/656237343405244416/Untitled-1.png";

                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                embed.ImageUrl = null;
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));
                if (response == null || response.Content.ToLowerInvariant() == "done")
                {
                    getplayerJSON = await hirezAPI.GetPlayer(playerID.ToString());
                    getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);
                    if (getplayerList[0].Personal_Status_Message.ToLowerInvariant() == generatedString)
                    {
                        await Database.SetPlayerSpecials(playerID,
                            (getplayerList[0].hz_player_name == null || getplayerList[0].hz_player_name == "" ? getplayerList[0].hz_gamer_tag : getplayerList[0].hz_player_name),
                            Context.Message.Author.Id);
                        embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                        embed.WithDescription($":tada: Congratulations, you've successfully linked your Discord and SMITE account into the Thoth Database.");
                        embed.ImageUrl = null;

                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                    }
                    else
                    {
                        embed.WithDescription($":red_circle: Linking cancelled. {(response == null ? "Time is up! " : "")}Your Personal Status Message is `{getplayerList[0].Personal_Status_Message}`");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
                else
                {
                    embed.WithDescription($":red_circle: Linking cancelled.");
                    embed.ImageUrl = null;
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await ErrorTracker.RespondToCommandOnErrorAsync(ex.Message);
                await ReplyAsync(embed: embed);
                await ErrorTracker.SendError($"**LINKING ERROR**\n{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}\n{ex.Source}\n{ex.Data}");
            }
        }

        // test
        [Command("test")] // Get specific God information
        [RequireOwner]
        public async Task TestAbilities()
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());

            if (gods.Count == 0)
            {
                //titlecasegod was not found imashe tuk nz
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods[64].Name);
                    author.WithIconUrl(gods[64].godIcon_URL);
                });
                embed.WithTitle(gods[64].Ability1);
                embed.WithDescription(gods[64].abilityDescription1.itemDescription.description);
                for (int z = 0; z < gods[64].abilityDescription1.itemDescription.menuitems.Count; z++)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[64].abilityDescription1.itemDescription.menuitems[z].description);
                        field.Value = (gods[64].abilityDescription1.itemDescription.menuitems[z].value);
                    });
                }
                for (int a = 0; a < gods[64].abilityDescription1.itemDescription.rankitems.Count; a++)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[64].abilityDescription1.itemDescription.rankitems[a].description);
                        field.Value = (gods[64].abilityDescription1.itemDescription.rankitems[a].value);
                    });
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cooldown");
                    field.Value = (gods[64].abilityDescription1.itemDescription.cooldown);
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cost");
                    field.Value = (gods[64].abilityDescription1.itemDescription.cost);
                });

                embed.WithThumbnailUrl(gods[64].godAbility1_URL);
                if (gods[64].DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods[64].DomColor));
                }

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("p", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task PaginatedGodsbro(int god, int ability = 1)
        {
            //var gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());
            var json = await File.ReadAllTextAsync("getgods.json");
            var gods = JsonConvert.DeserializeObject<List<Gods.God>>(json);
            var embed = new EmbedBuilder();
            //var json = JsonConvert.SerializeObject(gods, Formatting.Indented);
            //await File.WriteAllTextAsync("getgods.json", json);
            
            try
            {
                if (gods.Count == 0)
                {
                    //titlecasegod was not found imashe tuk nz
                    return;
                }
                else
                {
                    embed.WithAuthor(author =>
                    {
                        author.WithName(gods[god].Name);
                        author.WithIconUrl(gods[god].godIcon_URL);
                    });
                    embed.WithTitle(gods[god].Ability5);
                    embed.WithDescription(gods[god].abilityDescription5.itemDescription.description);
                    for (int z = 0; z < gods[god].abilityDescription5.itemDescription.menuitems.Count; z++)
                    {
                        if (gods[god].abilityDescription5.itemDescription.menuitems[z].value.Length != 0)
                        {
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = gods[god].abilityDescription5.itemDescription.menuitems[z].description;
                                field.Value = gods[god].abilityDescription5.itemDescription.menuitems[z].value;
                            });
                        }
                    }
                    for (int a = 0; a < gods[god].abilityDescription5.itemDescription.rankitems.Count; a++)
                    {
                        if (gods[god].abilityDescription5.itemDescription.rankitems[a].value.Length != 0)
                        {
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = gods[god].abilityDescription5.itemDescription.rankitems[a].description;
                                field.Value = gods[god].abilityDescription5.itemDescription.rankitems[a].value;
                            });
                        }
                    }
                    if (gods[god].abilityDescription5.itemDescription.cooldown.Length != 0)
                    {
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "Cooldown";
                            field.Value = gods[god].abilityDescription5.itemDescription.cooldown;
                        });
                    }
                    if (gods[god].abilityDescription5.itemDescription.cost.Length != 0)
                    {
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "Cost";
                            field.Value = gods[god].abilityDescription5.itemDescription.cost;
                        });
                    }
                    embed.WithThumbnailUrl(gods[god].godAbility5_URL);
                    if (gods[god].DomColor != 0)
                    {
                        embed.WithColor(new Color((uint)gods[god].DomColor));
                    }


                }
                var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        [Command("kbrat")]
        [RequireOwner]
        public async Task AnotherTestCommand()
        {
            var embed = new EmbedBuilder();
            embed.WithDescription($"[:thinking: Streamer](https://leovoel.github.io/embed-visualizer/)");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("tt", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TestGetPlayer([Remainder]string username = "")
        {
            if (username == "")
            {
                await ReplyAsync("Command usage: `!!stats InGameName`");
            }
            else if (Context.Message.MentionedUsers.Count > 0)
            {
                // checking in db
            }
            else if (username != "")
            {
                var searchPlayers = await SmiteStatsUtils.SearchPlayersUtil(hirezAPI, username);
                if (searchPlayers.Count == 0)
                {
                    await ReplyAsync("Not found");
                }
                else if (searchPlayers.Count == 1)
                {
                    // do dat
                    await ReplyAsync("Just one");
                }
                else
                {
                    await ReplyAsync("More than one. " + searchPlayers.Count);
                }
            }
        }

        [Command("testdbstuff")]
        [RequireOwner]
        public async Task LiterallyTestDbThings(string args)
        {
            var thing = await Database.GetBootsOrShoes(args);

            StringBuilder result = new StringBuilder();
            foreach (var item in thing)
            {
                result.Append(item.Emoji);
            }

            await ReplyAsync(result.ToString());
        }

        [Command("nz")] // keep it simple pls
        [RequireOwner]
        public async Task NzVrat(string endpoint, [Remainder]string value)
        {
            string json = "";
            try
            {
                json = await hirezAPI.APITestMethod(endpoint, value);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);

                await ReplyAsync($"```json\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}```");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync($"{endpoint}.json", json);
                    await Context.Channel.SendFileAsync($"{endpoint}.json");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        [Command("bitems", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task NewStats()
        {
            try
            {
                var items = await Database.GetActiveTierItems(3);
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    if (item.ItemBenefits.ToLowerInvariant().Contains("protections"))
                    {
                        sb.Append(item.DeviceName);
                    }
                }

                await ReplyAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                await ReplyAsync($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        [Command("seby")]
        public async Task Seby()
        {
            var embed = new EmbedBuilder();

            embed.AddField(x =>
            {
                x.Name = "**Team 1**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "**Team 2**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "**Team 3**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "**Team 4**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "**Team 5**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });
            embed.AddField(x =>
            {
                x.Name = "**Team 6**";
                x.Value = "Player1\n" +
                "Player2\n" +
                "Player3\n" +
                "Player4\n" +
                "Player5";
                x.IsInline = true;
            });

            await ReplyAsync("", false, embed.Build());
        }

        // Owner Commands

        [Command("updateplayers")]
        [RequireOwner]
        public async Task UpdatePlayers()
        {
            await ReplyAsync("do not use");

            var playersList = new List<PlayerStats>(await GetAllPlayers());

            for (int i = 0; i < playersList.Count; i++)
            {
                List<PlayerStats> playerList = JsonConvert.DeserializeObject<List<PlayerStats>>(await hirezAPI.GetPlayer(playersList[i].ActivePlayerId.ToString()));
                try
                {
                    Console.WriteLine($"[{playerList[0].ActivePlayerId}]{playerList[0].hz_player_name} Updated!");
                    //adding player to DB
                    //commented out after adding portal_id to the database
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{playersList[i].hz_player_name}\n{ex.Message}");
                }
            }
        }

        // Shit/fun commands lul

        [Command("rank", true, RunMode = RunMode.Async)]
        [Summary("Gives you random ranked division.")]
        [Alias("ранк")]
        public async Task RandomRankCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                int n;
                n = rnd.Next(0, 28);
                embed.WithColor(rnd.Next(255), rnd.Next(255), rnd.Next(255));
                embed.WithAuthor(x =>
                {
                    x.IconUrl = Context.Message.Author.GetAvatarUrl();
                    x.Name = $"{Context.Message.Author.Username}'s rank is:";
                });
                if (Context.Message.MentionedUsers.Count > 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.First();
                    embed.Author.Name = $"{mentionedUser.Username}'s rank is:";
                    embed.Author.IconUrl = mentionedUser.GetAvatarUrl();
                }
                embed.WithTitle($"{Text.GetRankedConquest(n).Item2} {Text.GetRankedConquest(n).Item1}");

                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        private static string GenerateString()
        {
            char[] chars = new char[7];
            for (int i = 0; i < 7; i++)
            {
                chars[i] = alphabet[rnd.Next(alphabet.Length)];
            }
            return new string(chars);
        }
        public async Task<MultiplePlayersStruct> MultiplePlayersHandler(List<SearchPlayers> searchPlayers, SocketCommandContext context, IUserMessage message = null)
        {
            var nz = new MultiplePlayersStruct();
            nz.searchPlayers = null;
            nz.userMessage = null;
            if (searchPlayers.Count > 20)
            {
                await context.Channel.SendMessageAsync($"There are more than 20 accounts({searchPlayers.Count}) with the username **{searchPlayers[0].Name}**. Please contact the bot owner for further assistance.");
                return nz;
            }
            var embed = await EmbedHandler.MultiplePlayers(searchPlayers);
            if (message == null)
            {
                message = await context.Channel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
            }
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || !(response.Content.All(char.IsDigit)))
            {
                embed.WithFooter(x =>
                {
                    x.Text = "CANCELLED";
                });
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return nz;
            }
            int responseNum = Int32.Parse(response.Content);
            --responseNum;
            if (!(responseNum > searchPlayers.Count))
            {
                if (searchPlayers[responseNum].privacy_flag == "y")
                {
                    embed = await EmbedHandler.HiddenProfileEmbed(searchPlayers[responseNum].Name);
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return nz;
                }
                embed = await EmbedHandler.LoadingStats(Text.GetPortalIcon(searchPlayers[responseNum].portal_id.ToString()) + searchPlayers[responseNum].Name);
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                nz.searchPlayers = searchPlayers[responseNum];
                nz.userMessage = message;
                return nz;
            }
            else
            {
                await ReplyAsync("Invalid number");
                embed.WithFooter(x =>
                {
                    x.Text = "CANCELLED";
                });
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return nz;
            }
        }
        public struct MultiplePlayersStruct
        {
            public SearchPlayers searchPlayers;
            public IUserMessage userMessage;
        }
    }
}
