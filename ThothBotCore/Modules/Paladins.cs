using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Paladins : ModuleBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new HiRezAPI();
        readonly string botIcon = "https://i.imgur.com/2Uuwyur.png";

        [Command("pnz")] // keep it simple pls
        [RequireOwner]
        public async Task PaladinsNzVrat(string endpoint, [Remainder]string value)
        {
            string json = "";
            try
            {
                json = await hirezAPI.PaladinsAPITestMethod(endpoint, value);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);

                await ReplyAsync($"```json\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}```");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync("paladinstestmethod.json", json);
                    await ReplyAsync("Saved as paladinstestmethod.json");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        [Command("pst")]
        [Alias("pstat", "pst", "pstata", "пст", "пстатс", "pns")]
        public async Task PaladinsStats([Remainder]string username)
        {
            try
            {
                var search = await hirezAPI.SearchPlayersPaladins(username);

                if (search.Count != 0 && search[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
                {
                    await Context.Channel.TriggerTypingAsync();
                    List<PaladinsPlayer.Player> playerStats = JsonConvert.DeserializeObject<List<PaladinsPlayer.Player>>(await hirezAPI.GetPlayerPaladins(search[0].player_id.ToString())); // GetPlayer

                    List<PaladinsGodRanks> godRanks = JsonConvert.DeserializeObject<List<PaladinsGodRanks>>(await hirezAPI.GetGodRanksPaladins(playerStats[0].ActivePlayerId)); //GodRanks
                                                                                                                                                                                // SMITE API ONLY PlayerAchievements playerAchievements = JsonConvert.DeserializeObject<PlayerAchievements>(achievementsjson);
                    List<PaladinsPlayer.PaladinsPlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PaladinsPlayer.PaladinsPlayerStatus>>(await hirezAPI.GetPlayerStatusPaladins(playerStats[0].ActivePlayerId));

                    string defaultEmoji = ""; //🔹 <:gems:443919192748589087>
                    string checkedPlayerName = playerStats[0].hz_player_name == null ? playerStats[0].Name : playerStats[0].Name;

                    string rPlayerName = "";

                    if (checkedPlayerName.Contains("]"))
                    {
                        string[] splitName = checkedPlayerName.Split(']');
                        rPlayerName = splitName[1];
                        rPlayerName = rPlayerName + $", {splitName[0]}]{playerStats[0].Team_Name}";
                    }
                    else
                    {
                        rPlayerName = checkedPlayerName;
                    }
                    string rPlayerCreated = Text.InvariantDate(playerStats[0].Created_Datetime);
                    string rHoursPlayed = playerStats[0].HoursPlayed.ToString() + " hours";
                    double rWinRate = 0;
                    if (playerStats[0].Wins != 0 && playerStats[0].Losses != 0)
                    {
                        rWinRate = (double)playerStats[0].Wins * 100 / (playerStats[0].Wins + playerStats[0].Losses);
                    }

                    var embed = new EmbedBuilder();
                    embed.WithThumbnailUrl(botIcon);
                    embed.WithAuthor(author =>
                    {
                        author
                            .WithName($"{rPlayerName}")
                            .WithUrl($"https://paladins.guru/profile/{playerStats[0].ActivePlayerId}")
                            .WithIconUrl(botIcon);
                    });
                    //embed.WithTitle(Text.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId.ToString()).Result);
                    if (playerStatus[0].status == 0)
                    {
                        embed.WithDescription($":eyes: **Last Login:** {Text.PrettyDate(playerStats[0].Last_Login_Datetime)}");
                        embed.WithColor(new Color(220, 147, 4));
                        defaultEmoji = ":small_orange_diamond:";
                    }
                    else
                    {
                        defaultEmoji = "🔹"; // 🔹 <:blank:570291209906552848>
                        embed.WithColor(Constants.DefaultBlueColor);
                        if (playerStatus[0].Match != 0)
                        {
                            //await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                            List<PaladinsMatchPlayerDetails.PlayerMatchDetails> matchPlayerDetails = JsonConvert.DeserializeObject<List<PaladinsMatchPlayerDetails.PlayerMatchDetails>>(await hirezAPI.GetMatchPlayerDetailsPaladins(playerStatus[0].Match));
                            for (int s = 0; s < matchPlayerDetails.Count; s++)
                            {
                                if (matchPlayerDetails[s].playerId == playerStats[0].ActivePlayerId)
                                {
                                    embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueNamePaladins(matchPlayerDetails[0].Queue)}**, playing as {matchPlayerDetails[s].ChampionName}");
                                }
                            }
                        }
                        else
                        {
                            embed.WithDescription($":eyes: {playerStatus[0].status_string}");
                        }
                    }
                    // invisible character \u200b
                    string region = "";
                    switch (playerStats[0].Region)
                    {
                        case "Europe": region = $":flag_eu:{playerStats[0].Region}"; break;
                        case "North America": region = $":flag_us:{playerStats[0].Region}"; break;
                        case "Brazil": region = $":flag_br:{playerStats[0].Region}"; break;
                        case "Australia": region = $":flag_au:{playerStats[0].Region}"; break;
                        case "": region = $"{defaultEmoji}n/a"; break;
                        default: region = $"{defaultEmoji}{playerStats[0].Region}"; break;
                    } // Region
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = ($"<:Paladins:588196531019186193>**Account**");
                        field.Value = ($"{defaultEmoji}Level: {playerStats[0].Level}\n" +
                        $"{defaultEmoji}Mastery Level: {playerStats[0].MasteryLevel}\n" +
                        $"{defaultEmoji}Platform: {playerStats[0].Platform}\n" +
                        $"{region}");
                    });
                    int matches = playerStats[0].Wins + playerStats[0].Losses;
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $"<:Paladins:588196531019186193>**Win Ratio** [{Math.Round(rWinRate, 2)}%]";
                        field.Value = $"{defaultEmoji}Matches: {matches}\n" +
                        $":trophy:Wins: {playerStats[0].Wins}\n" +
                        $":flag_white:Losses: {playerStats[0].Losses}\n" +
                        $":runner:Leaves: {playerStats[0].Leaves}";
                    });
                    // Ranked
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $"<:Paladins:588196531019186193>**{Text.GetRankedConquest(playerStats[0].Tier_RankedKBM).Item1}**";
                        field.Value = $"{defaultEmoji}Season: {playerStats[0].RankedKBM.Season}\n" +
                        $"{defaultEmoji}Rank: {playerStats[0].RankedKBM.Rank}\n" +
                        $"{defaultEmoji}Wins: {playerStats[0].RankedKBM.Wins}\n" +
                        $"{defaultEmoji}Losses: {playerStats[0].RankedKBM.Losses}";
                    });
                    if (godRanks.Count != 0)
                    {
                        switch (godRanks.Count)
                        {
                            case 1:
                                embed.AddField(field =>
                                {
                                    field.IsInline = true;
                                    field.Name = "<:Paladins:588196531019186193>**Top Champions**";
                                    field.Value = $":first_place:{godRanks[0].champion}";
                                });
                                break;
                            case 2:
                                embed.AddField(field =>
                                {
                                    field.IsInline = true;
                                    field.Name = "<:Paladins:588196531019186193>**Top Champions**";
                                    field.Value = $":first_place:{godRanks[0].champion}\n" +
                                    $":second_place:{godRanks[1].champion}\n";
                                });
                                break;
                            default:
                                embed.AddField(field =>
                                {
                                    field.IsInline = true;
                                    field.Name = "<:Paladins:588196531019186193>**Top Champions**";
                                    field.Value = $":first_place:{godRanks[0].champion}\n" +
                                    $":second_place:{godRanks[1].champion}\n" +
                                    $":third_place:{godRanks[2].champion}";
                                });
                                break;
                        }
                    } // Top Champions
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $":video_game:**Playing since**";
                        field.Value = $"{defaultEmoji}{rPlayerCreated}";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $":hourglass:**Playtime**";
                        field.Value = $"{defaultEmoji}{rHoursPlayed}";
                    });
                    embed.WithFooter(footer =>
                    {
                        footer
                            .WithText(playerStats[0].Personal_Status_Message)
                            .WithIconUrl(botIcon);
                    });

                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    await ReplyAsync($"<:X_:579151621502795777>*{username}* is hidden or not found!");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Oops.. Either this player was not found or an unexpected error has occured.");
                await ErrorTracker.SendError($"**__Paladins__Stats Command**\n" +
                    $"**Message: **{Context.Message.Content}\n" +
                    $"**User: **{Context.Message.Author.Username}[{Context.Message.Author.Id}]\n" +
                    $"**Server and Channel: **ID:{Context.Guild.Id}[{Context.Channel.Id}]\n" +
                    $"**Error: **{ex.Message}\n" +
                    $"**InnerException: ** {ex.InnerException}");
            }
        }
    }
}
