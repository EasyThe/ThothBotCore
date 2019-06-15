using CoreHtmlToImage;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Models;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.Player;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Smite : ModuleBase<SocketCommandContext>
    {
        readonly string botIcon = "https://i.imgur.com/8qNdxse.png"; // https://i.imgur.com/AgNocjS.png
        static Random rnd = new Random();

        HiRezAPI hirezAPI = new HiRezAPI();
        DominantColor domColor = new DominantColor();

        [Command("stats", RunMode = RunMode.Async)]
        [Alias("stat", "pc", "st", "stata", "ст", "статс", "ns")]
        public async Task Stats([Remainder] string username)
        {
            try
            {
                List<SearchPlayers> searchPlayer = await hirezAPI.SearchPlayer(username);
                if (searchPlayer.Count != 0)
                {
                    List<SearchPlayers> realSearchPlayers = new List<SearchPlayers>();
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == username.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                if (searchPlayer.Count == 0 || searchPlayer[0].Name.ToLowerInvariant() != username.ToLowerInvariant())
                {
                    await ReplyAsync($"<:X_:579151621502795777>*{username}* is hidden or not found!");
                }
                else if (searchPlayer[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
                {

                    await Context.Channel.TriggerTypingAsync();
                    try
                    {
                        string statusJson = await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id);
                        List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                        string matchJson = "";
                        if (playerStatus[0].Match != 0)
                        {
                            matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        }

                        EmbedBuilder embed = EmbedHandler.PlayerStatsEmbed(
                            await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString()),
                            await hirezAPI.GetGodRanks(searchPlayer[0].player_id),
                            await hirezAPI.GetPlayerAchievements(searchPlayer[0].player_id),
                            await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id),
                            matchJson,
                            searchPlayer[0].portal_id).Result;
                        var message = await Context.Channel.SendMessageAsync("", false, embed.Build());

                        try
                        {
                            List<AllQueueStats> allQueue = new List<AllQueueStats>();
                            for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                            {
                                int matches = 0;
                                try
                                {
                                    List<QueueStats> queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(searchPlayer[0].player_id, Text.LegitQueueIDs()[i]));
                                    for (int c = 0; c < queueStats.Count; c++)
                                    {
                                        if (queueStats[c].Matches != 0)
                                        {
                                            matches = matches + queueStats[c].Matches;
                                        }
                                    }
                                    allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                                }
                                catch (Exception)

                                {
                                }
                            }
                            List<AllQueueStats> orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
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
                                    field.Name = ($"<:matches:579604410569850891>**Most Played Modes**");
                                    field.Value = (topMatchesValue);
                                });

                                await message.ModifyAsync(x =>
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
                        await ErrorTracker.SendError($"Stats Error: \n{ex.Message}");
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
                    $"**Error: **{ex.Message}");
            }
        }

        [Command("istats")]
        [Alias("istat", "ipc", "ist", "istata", "ист", "истатс")]
        [RequireOwner] // vremenno
        public async Task ImageStats([Remainder] string username)
        {
            var converter = new HtmlConverter();
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
                    string[] splitPlayerName = playerStats[0].Name.Split(']');
                    rPlayerName = splitPlayerName[1];
                    rTeamName = splitPlayerName[0] + "]" + playerStats[0].Team_Name;
                }
                else
                {
                    rPlayerName = playerStats[0].Name;
                }
                int rPlayerLevel = playerStats[0].Level;
                int rPlayerWins = playerStats[0].Wins;
                int rPlayerLosses = playerStats[0].Losses;
                string rPlayerRegion = playerStats[0].Region;
                int rPlayerLeaves = playerStats[0].Leaves;
                int rPlayerMasteryLevel = playerStats[0].MasteryLevel;
                int rTotalWorsh = playerStats[0].Total_Worshippers;
                string rPlayerStatus = playerStats[0].Personal_Status_Message;
                string rPlayerCreated = playerStats[0].Created_Datetime.ToString("dd.MM.yyyy");
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

                string css = "<html>\n\n<head>\n    <meta charset=\"UTF-8\">\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n    <link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css\" integrity=\"sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO\"\n        crossorigin=\"anonymous\">\n    <style>\n        html {\n            height: 100%;\n        }\n        \n        body {\n            background-color: transparent;\n            color: white;\n            background-image: url(https://web2.hirez.com/smite/v3/s5/HarpyNest_S4.jpg);\n            background-repeat: no-repeat;\n            background-size: cover;\n            background-position: top;\n            margin: 5px;\n        }\n\n        .avatarPos {\n            position: absolute;\n            top: 25px;\n            left: 25px;\n        }\n\n        .imgA1 {\n            z-index: 1;\n            width: 70px;\n            left: 8px;\n            top: 8px;\n        }\n\n        .imgB1 {\n            z-index: 3;\n            width: 110px;\n            left: -12;\n            top: -11px;\n        }\n\n        .row {\n            border: 2px solid #26728d;\n            background-color: rgba(14, 25, 38, .9);\n            color: #c9f9fb;\n            padding: 10px;\n        }\n\n        .col {\n            border: 2px solid #26728d;\n        }\n\n        .AccStats {\n            font-size: 18px;\n        }\n\n        .names {\n            padding-left: 100px;\n        }\n\n        .levels {\n            position: fixed;\n            top: 18;\n            right: 18px;\n        }\n\n        .left-col {\n            float: left;\n            width: 50%;\n            background-color: rgba(14, 25, 38, .9);\n            background-image: url(https://i.imgur.com/MfBC9I6.png);\n            background-position: right;\n            background-repeat: no-repeat;\n            background-size: contain;\n            height: 140px;\n            padding-left: 10px;\n        }\n        \n        .right-col {\n            float: right;\n            width: 50%;\n            background-color: rgba(14, 25, 38, .9);\n            background-image: url(https://i.imgur.com/skbL9IZ.png);\n            background-position: left;\n            background-repeat: no-repeat;\n            background-size: contain;\n            height: 140px;\n            text-align: right;\n            padding-right: 10px;\n        }\n\n        .conquest-col {\n            width: 100%;\n            height: 140px;\n            background-image: url(https://i.imgur.com/ayyGCkZ.png);\n            background-size: cover;\n            background-position: center;\n            background-repeat: no-repeat;\n            border-bottom: 2px solid #26728d;\n            padding-left: 10px;\n        }\n    </style>\n</head>";
                string html = $"<body>\n    <div class=\"container-fluid\">\n        <div class=\"row\" style=\"height: 112px; border-bottom: 1px solid #26728d;\">\n            <div class=\"col-1\">\n                <img class=\"avatarPos imgA1\" src=\"{rAvatarURL}\">\n                <img class=\"avatarPos imgB1\" src=\"{rAvatarBorderURL}\">\n            </div>\n            <div class=\"names col-11\">\n                <h2>{rPlayerName}</h2>\n                <h5>{rTeamName}</h5>\n                <div class=\"levels\">\n                    <img src=\"https://i.imgur.com/8IluUqL.png\" />{rPlayerLevel}<br>\n                    <img src=\"https://i.imgur.com/cSFMiWX.png\" />{rPlayerMasteryLevel}<br>\n                    <img src=\"https://i.imgur.com/baSKFnW.png\" width=\"28px\" />{rTotalWorsh} \n                    <div style=\"position: fixed; top: 25px; right: 120; text-align: center; border-left: 1px solid #26728d; border-right: 1px solid #26728d; padding-left: 10px; padding-right: 10px;\">\n                            <h3>Playtime</h3>\n                            <h5>{rHoursPlayed}</h5>\n                    </div>\n                </div>\n            </div>\n        </div>\n        <div class=\"row\" style=\"border-top: 0px !important; padding-top: 5px !important; padding-bottom: 5px !important;\">\n            <div class=\"col-12\" style=\"padding-top: 0px !important; padding-bottom: 0px !important; text-align: center;\">\n                <h5 style=\"margin-bottom: 5px !important;\">{rPlayerStatus}</h5>\n            </div>\n        </div>\n        <div class=\"row\" style=\"margin-top: 3px; padding: 0; height: 284px;\">\n            <div class=\"conquest-col\" style=\"border-right: 1px solid #26728d;\">\n                <img src=\"{rConquestTierImg}\" />\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Conquest<br>{rConquestTier}</h5>\n            </div>\n            <div class=\"left-col\">\n                <img src=\"{rJoustTierImg}\" />\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Joust<br>{rJoustTier}</h5>\n            </div>\n            <div class=\"right-col\">\n                <h5 style=\"display: inline-block; vertical-align: middle;\">Ranked Duel<br>{rDuelTier}</h5>\n                <img src=\"{rDuelTierImg}\" />\n            </div>\n        </div>\n    </div>\n</body>\n\n</html>";

                try
                {
                    var jpegbytes = converter.FromHtmlString(css + html, 800);
                    //await Context.Channel.SendFileAsync(new MemoryStream(jpegbytes), $"{playerStats[0].Id}.jpg");
                    if (!Directory.Exists("Storage/PlayerImages"))
                    {
                        Directory.CreateDirectory("Storage/PlayerImages");
                    }
                    File.WriteAllBytes($"Storage/PlayerImages/{playerStats[0].ActivePlayerId}.jpg", jpegbytes);

                    var embed = new EmbedBuilder();
                    var fileName = $"Storage/PlayerImages/{playerStats[0].ActivePlayerId}.jpg";
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = ($":hourglass:Playtime");
                        field.Value = ($":small_blue_diamond:{rHoursPlayed}");
                    });
                    embed.WithImageUrl($"attachment://{playerStats[0].ActivePlayerId}.jpg");
                    embed.WithFooter(footer =>
                    {
                        footer
                            .WithText($"{playerStats[0].Personal_Status_Message}")
                            .WithIconUrl(botIcon);
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

        [Command("god")] // Get specific God information
        [Alias("g", "gods")]
        public async Task GodInfo([Remainder] string god)
        {
            string titleCaseGod = Text.ToTitleCase(god);

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
                if (gods[0].Pros != null)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = false;
                        field.Name = "Pros";
                        field.Value = gods[0].Pros;
                    });
                }
                embed.WithFooter(x =>
                {
                    x.IconUrl = botIcon;
                    x.Text = "More info soon™..";
                });

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("rgod", true)] // Random God
        [Alias("rg", "randomgod", "random")]
        public async Task RandomGod()
        {
            List<Gods.God> gods = Database.GetRandomGod();

            int r = rnd.Next(gods.Count);

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(gods[r].Name);
                author.WithIconUrl(gods[r].godIcon_URL);
            });
            embed.WithTitle(gods[r].Title);
            embed.WithThumbnailUrl(gods[r].godIcon_URL);
            if (gods[0].DomColor != 0)
            {
                embed.WithColor(new Color((uint)gods[r].DomColor));
            }
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Pantheon";
                field.Value = gods[r].Pantheon;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Type";
                field.Value = gods[r].Type;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Role";
                field.Value = gods[r].Roles;
            });

            await ReplyAsync($"{Context.Message.Author.Mention}, your random god is:", false, embed.Build());
        }

        [Command("status", true)] // SMITE Server Status 
        [Alias("статус", "statis", "s", "с", "server", "servers", "se", "се", "serverstatus")]
        public async Task ServerStatusCheck()
        {
            await StatusPage.GetStatusSummary();
            ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(StatusPage.statusSummary);
            //ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(File.ReadAllText("test.json"));

            await ReplyAsync("", false, EmbedHandler.ServerStatusEmbed(ServerStatus).Build()); // Server Status POST
            bool maint = false;
            bool inci = false;
            // Incidents
            if (ServerStatus.incidents.Count >= 1)
            {
                for (int i = 0; i < ServerStatus.incidents.Count; i++)
                {
                    if (ServerStatus.incidents[i].name.Contains("Smite") ||
                        ServerStatus.incidents[i].components.Any(x => x.name.ToLowerInvariant().Contains("smite")))
                    {
                        inci = true;
                    }
                }
            }
            if (inci == true)
            {
                await ReplyAsync("", false, EmbedHandler.StatusIncidentEmbed(ServerStatus).Build());
            }
            // Scheduled Maintenances
            if (ServerStatus.scheduled_maintenances.Count >= 1)
            {
                for (int c = 0; c < ServerStatus.scheduled_maintenances.Count; c++)
                {
                    if (ServerStatus.scheduled_maintenances[c].name.Contains("Smite"))
                    {
                        maint = true;
                    }
                }
            }
            if (maint == true)
            {
                await ReplyAsync("", false, EmbedHandler.StatusMaintenanceEmbed(ServerStatus).Build());
            }
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
                author.WithIconUrl(botIcon);
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

        // test
        [Command("test")] // Get specific God information
        [RequireOwner]
        public async Task TestAbilities()
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());

            if (gods.Count == 0)
            {
                //await ReplyAsync($"{titleCaseGod} was not found");
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods[63].Name);
                    author.WithIconUrl(gods[63].godIcon_URL);
                });
                embed.WithTitle(gods[63].Ability1);
                embed.WithDescription(gods[63].abilityDescription1.itemDescription.description);
                for (int z = 0; z < gods[63].abilityDescription1.itemDescription.menuitems.Count; z++)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[63].abilityDescription1.itemDescription.menuitems[z].description);
                        field.Value = (gods[63].abilityDescription1.itemDescription.menuitems[z].value);
                    });
                }
                for (int a = 0; a < gods[63].abilityDescription1.itemDescription.rankitems.Count; a++)
                {
                    //gods[0].abilityDescription1.itemDescription.rankitems.Count

                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[63].abilityDescription1.itemDescription.rankitems[a].description);
                        field.Value = (gods[63].abilityDescription1.itemDescription.rankitems[a].value);
                    });
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cooldown");
                    field.Value = (gods[63].abilityDescription1.itemDescription.cooldown);
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cost");
                    field.Value = (gods[63].abilityDescription1.itemDescription.cost);
                });

                // gods[0].abilityDescription1.itemDescription.cooldown

                embed.WithThumbnailUrl(gods[63].godAbility1_URL);
                if (gods[63].DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods[63].DomColor));
                }

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("matchdetails")]
        [RequireOwner]
        public async Task MatchDetailsCommand(int id)
        {
            await hirezAPI.GetMatchDetails(id);

            List<MatchDetails.MatchDetailsPlayer> matchDetails = JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(hirezAPI.matchDetails);

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(85, 172, 238));
            embed.WithAuthor(author =>
            {
                author.WithName(Text.GetQueueName(matchDetails[0].match_queue_id));
                author.WithIconUrl(botIcon);
                author.WithUrl($"https://smite.guru/match/{matchDetails[0].Match}");
            });
            embed.WithThumbnailUrl(botIcon);

            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[0].playerPortalId)}{matchDetails[0].Reference_Name}, Lvl {matchDetails[0].Account_Level}";// left
                field.Value = ":small_blue_diamond:" + matchDetails[0].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[5].playerPortalId)}{matchDetails[5].Reference_Name}, Lvl {matchDetails[5].Account_Level}";// loss
                field.Value = ":small_blue_diamond:" + matchDetails[5].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[1].playerPortalId)}{matchDetails[1].Reference_Name}, Lvl {matchDetails[1].Account_Level}";// left
                field.Value = ":small_blue_diamond:" + matchDetails[1].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[6].playerPortalId)}{matchDetails[6].Reference_Name}, Lvl {matchDetails[6].Account_Level}";
                field.Value = ":small_blue_diamond:" + matchDetails[6].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[2].playerPortalId)}{matchDetails[2].Reference_Name}, Lvl {matchDetails[2].Account_Level}";// left
                field.Value = ":small_blue_diamond:" + matchDetails[2].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[7].playerPortalId)}{matchDetails[7].Reference_Name}, Lvl {matchDetails[7].Account_Level}";
                field.Value = ":small_blue_diamond:" + matchDetails[7].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[3].playerPortalId)}{matchDetails[3].Reference_Name}, Lvl {matchDetails[3].Account_Level}";// left
                field.Value = ":small_blue_diamond:" + matchDetails[3].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[8].playerPortalId)}{matchDetails[8].Reference_Name}, Lvl {matchDetails[8].Account_Level}";
                field.Value = ":small_blue_diamond:" + matchDetails[8].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[4].playerPortalId)}{matchDetails[4].Reference_Name}, Lvl {matchDetails[4].Account_Level}";// left
                field.Value = ":small_blue_diamond:" + matchDetails[4].hz_player_name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{Text.GetPortalIcon(matchDetails[9].playerPortalId)}{matchDetails[9].Reference_Name}, Lvl {matchDetails[9].Account_Level}";
                field.Value = ":small_blue_diamond:" + matchDetails[9].hz_player_name;
            });
            embed.WithFooter(x =>
            {
                x.IconUrl = botIcon;
                x.Text = "For more info, press the queue name on top.";
            });

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("tt")]
        [RequireOwner]
        public async Task TestGetPlayer(string username)
        {
            List<SearchPlayers> searchPlayer = await hirezAPI.SearchPlayer(username);
            if (searchPlayer.Count == 0 || searchPlayer[0].Name.ToLowerInvariant() != username.ToLowerInvariant())
            {
                await ReplyAsync($"<:X_:579151621502795777>*{username}* is hidden or not found!");
            }
            else if (searchPlayer[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
            {

                await Context.Channel.TriggerTypingAsync();
                string statusJson = await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id);
                List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                string matchJson = "";
                if (playerStatus[0].Match != 0)
                {
                    matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                }

                EmbedBuilder embed = EmbedHandler.PlayerStatsEmbed(
                    await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString()),
                    await hirezAPI.GetGodRanks(searchPlayer[0].player_id),
                    await hirezAPI.GetPlayerAchievements(searchPlayer[0].player_id),
                    await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id),
                    matchJson,
                    searchPlayer[0].portal_id).Result;
                // Sending initial stats
                var message = await Context.Channel.SendMessageAsync("", false, embed.Build());

                List<AllQueueStats> allQueue = new List<AllQueueStats>();
                for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                {
                    int matches = 0;
                    List<QueueStats> queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(searchPlayer[0].player_id, Text.LegitQueueIDs()[i]));
                    for (int c = 0; c < queueStats.Count; c++)
                    {
                        if (queueStats[c].Matches != 0)
                        {
                            matches = matches + queueStats[c].Matches;
                        }
                    }
                    allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                }
                List<AllQueueStats> orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
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
                        field.Name = ($"<:matches:579604410569850891>**Most Played Modes**");
                        field.Value = (topMatchesValue);
                    });

                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                }
            }
        }

        [Command("zz")]
        [RequireOwner]
        public async Task GetGodsCommand(string type)
        {
            var items = await Database.GetActiveItems(3);

            List<Item> magicalItems = new List<Item>();
            List<Item> physicalItems = new List<Item>();
            List<Item> relics = new List<Item>();

            foreach (var item in items)
            {
                if (true)
                {

                }
            }

            StringBuilder sb = new StringBuilder();

            if (type == "magical")
            {
                
            }
            else
            {

            }

            for (int i = 0; i < 6; i++)
            {
                int r = rnd.Next(items.Count);
                sb.Append(items[r].DeviceName);
                sb.Append("\n");
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("nz")] // keep it simple pls
        [RequireOwner]
        public async Task NzVrat(string endpoint, [Remainder]string value)
        {
            string json = "";
            try
            {
                //List<PlayerSpecial> playerSpecial = await GetPlayerSpecials(id);
                //StringBuilder sb = new StringBuilder();
                //sb.Append($"{playerSpecial[0].special}");
                //await ReplyAsync(Text.CheckSpecialsForPlayer(id).Result);
                json = await hirezAPI.APITestMethod(endpoint, value);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);

                await ReplyAsync($"```json\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}```");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync("testmethod.json", json);
                    await ReplyAsync("Saved as testmethod.json");
                }
                Console.WriteLine(ex.Message);
            }
        }

        [Command("dai", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task NewStats([Remainder] string username)
        {
            try
            {
                List<SearchPlayers> searchPlayer = await hirezAPI.SearchPlayer(username);
                List<SearchPlayers> realSearchPlayers = new List<SearchPlayers>();
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

                if (realSearchPlayers.Count != 1)
                {
                    List<PlayerStats> multiplePlayerStats = new List<PlayerStats>();
                    for (int i = 0; i < realSearchPlayers.Count; i++)
                    {
                        multiplePlayerStats.Add(JsonConvert.DeserializeObject<List<PlayerStats>>(await hirezAPI.GetPlayer(realSearchPlayers[i].player_id.ToString()))[0]);
                    }

                    await ReplyAsync("", false, EmbedHandler.MultiplePlayers(multiplePlayerStats).Result.Build());
                }

                if (searchPlayer.Count == 0 || searchPlayer[0].Name.ToLowerInvariant() != username.ToLowerInvariant())
                {
                    await ReplyAsync($"<:X_:579151621502795777>*{username}* is hidden or not found!");
                }
                else if (searchPlayer[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
                {
                    await EmbedHandler.LoadingStats(searchPlayer[0].Name);
                    //await Context.Channel.TriggerTypingAsync();
                    try
                    {
                        string statusJson = await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id);
                        List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
                        string matchJson = "";
                        if (playerStatus[0].Match != 0)
                        {
                            matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        }

                        EmbedBuilder embed = EmbedHandler.PlayerStatsEmbed(
                            await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString()),
                            await hirezAPI.GetGodRanks(searchPlayer[0].player_id),
                            await hirezAPI.GetPlayerAchievements(searchPlayer[0].player_id),
                            await hirezAPI.GetPlayerStatus(searchPlayer[0].player_id),
                            matchJson,
                            searchPlayer[0].portal_id).Result;
                        var message = await Context.Channel.SendMessageAsync("", false, embed.Build());

                        try
                        {
                            List<AllQueueStats> allQueue = new List<AllQueueStats>();
                            for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                            {
                                int matches = 0;
                                try
                                {
                                    List<QueueStats> queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(searchPlayer[0].player_id, Text.LegitQueueIDs()[i]));
                                    for (int c = 0; c < queueStats.Count; c++)
                                    {
                                        if (queueStats[c].Matches != 0)
                                        {
                                            matches = matches + queueStats[c].Matches;
                                        }
                                    }
                                    allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                                }
                                catch (Exception)
                                {
                                }
                            }
                            List<AllQueueStats> orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
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
                                    field.Name = ($"<:matches:579604410569850891>**Most Played Modes**");
                                    field.Value = (topMatchesValue);
                                });

                                await message.ModifyAsync(x =>
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
                        await ErrorTracker.SendError($"Stats Error: \n{ex.Message}");
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
                    $"**Error: **{ex.Message}");
            }
        }

        // Owner Commands

        [Command("updateplayers")]
        [RequireOwner]
        public async Task UpdatePlayers()
        {
            await ReplyAsync("do not use");

            var playersList = new List<PlayerStats>(GetAllPlayers().Result);

            for (int i = 0; i < playersList.Count; i++)
            {
                List<PlayerStats> playerList = JsonConvert.DeserializeObject<List<PlayerStats>>(await hirezAPI.GetPlayer(playersList[i].ActivePlayerId.ToString()));
                try
                {
                    Console.WriteLine($"[{playerList[0].ActivePlayerId}]{playerList[0].hz_player_name} Updated!");
                    //await Database.AddPlayerToDb(playerList);
                    // commented out after adding portal_id to the database
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{playersList[i].hz_player_name}\n{ex.Message}");
                }
            }
        }
    }
}
