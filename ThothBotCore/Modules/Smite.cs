using CoreHtmlToImage;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Models;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.MatchPlayerDetails;
using static ThothBotCore.Connections.Models.Player;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Smite : ModuleBase<SocketCommandContext>
    {
        readonly string prefix = Credentials.botConfig.prefix;
        readonly string botIcon = "https://i.imgur.com/8qNdxse.png"; // https://i.imgur.com/AgNocjS.png
        static Random rnd = new Random();
        
        HiRezAPI hirezAPI = new HiRezAPI();
        DominantColor domColor = new DominantColor();

        [Command("stats")]
        [Alias("stat", "pc", "st", "stata", "ст", "статс")]
        public async Task Stats([Remainder] string username)
        {
            string json = await hirezAPI.GetPlayer(username);
            if (json == "[]")
            {
                await ReplyAsync($":x:*{Text.ToTitleCase(username)}* is hidden or not found!");
            }
            else
            {
                List<PlayerStats> playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(json);
                List<GodRanks> godRanks = JsonConvert.DeserializeObject<List<GodRanks>>(await hirezAPI.GetGodRanks(playerStats[0].ActivePlayerId));

                await hirezAPI.GetPlayerStatus(playerStats[0].ActivePlayerId);
                List<PlayerStatus> playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(hirezAPI.playerStatus);

                string defaultEmoji = ""; //:small_blue_diamond: <:gems:443919192748589087>
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
                        rConquestTierImg = "<:q_:528617317534269450>";
                        break;
                    case 1:
                        rConquestTier = "Bronze V";
                        rConquestTierImg = "<:cqbr:528617350027673620>";
                        break;
                    case 2:
                        rConquestTier = "Bronze IV";
                        rConquestTierImg = "<:cqbr:528617350027673620>";
                        break;
                    case 3:
                        rConquestTier = "Bronze III";
                        rConquestTierImg = "<:cqbr:528617350027673620>";
                        break;
                    case 4:
                        rConquestTier = "Bronze II";
                        rConquestTierImg = "<:cqbr:528617350027673620>";
                        break;
                    case 5:
                        rConquestTier = "Bronze I";
                        rConquestTierImg = "<:cqbr:528617350027673620>";
                        break;
                    case 6:
                        rConquestTier = "Silver V";
                        rConquestTierImg = "<:cqsi:528617356151488512>";
                        break;
                    case 7:
                        rConquestTier = "Silver IV";
                        rConquestTierImg = "<:cqsi:528617356151488512>";
                        break;
                    case 8:
                        rConquestTier = "Silver II";
                        rConquestTierImg = "<:cqsi:528617356151488512>";
                        break;
                    case 9:
                        rConquestTier = "Silver II";
                        rConquestTierImg = "<:cqsi:528617356151488512>";
                        break;
                    case 10:
                        rConquestTier = "Silver I";
                        rConquestTierImg = "<:cqsi:528617356151488512>";
                        break;
                    case 11:
                        rConquestTier = "Gold V";
                        rConquestTierImg = "<:cqgo:528617356491227136>";
                        break;
                    case 12:
                        rConquestTier = "Gold IV";
                        rConquestTierImg = "<:cqgo:528617356491227136>";
                        break;
                    case 13:
                        rConquestTier = "Gold III";
                        rConquestTierImg = "<:cqgo:528617356491227136>";
                        break;
                    case 14:
                        rConquestTier = "Gold II";
                        rConquestTierImg = "<:cqgo:528617356491227136>";
                        break;
                    case 15:
                        rConquestTier = "Gold I";
                        rConquestTierImg = "<:cqgo:528617356491227136>";
                        break;
                    case 16:
                        rConquestTier = "Platinum V";
                        rConquestTierImg = "<:cqpl:528617357485015041>";
                        break;
                    case 17:
                        rConquestTier = "Platinum IV";
                        rConquestTierImg = "<:cqpl:528617357485015041>";
                        break;
                    case 18:
                        rConquestTier = "Platinum III";
                        rConquestTierImg = "<:cqpl:528617357485015041>";
                        break;
                    case 19:
                        rConquestTier = "Platinum II";
                        rConquestTierImg = "<:cqpl:528617357485015041>";
                        break;
                    case 20:
                        rConquestTier = "Platinum I";
                        rConquestTierImg = "<:cqpl:528617357485015041>";
                        break;
                    case 21:
                        rConquestTier = "Diamond V";
                        rConquestTierImg = "<:cqdi:528617356625313792>";
                        break;
                    case 22:
                        rConquestTier = "Diamond IV";
                        rConquestTierImg = "<:cqdi:528617356625313792>";
                        break;
                    case 23:
                        rConquestTier = "Diamond III";
                        rConquestTierImg = "<:cqdi:528617356625313792>";
                        break;
                    case 24:
                        rConquestTier = "Diamond II";
                        rConquestTierImg = "<:cqdi:528617356625313792>";
                        break;
                    case 25:
                        rConquestTier = "Diamond I";
                        rConquestTierImg = "<:cqdi:528617356625313792>";
                        break;
                    case 26:
                        rConquestTier = "Masters";
                        rConquestTierImg = "<:cqma:528617357669826560>";
                        break;
                    case 27:
                        rConquestTier = "Grandmaster";
                        rConquestTierImg = "<:cqgm:528617358500298753>";
                        break;
                    default:
                        break;
                }

                switch (playerStats[0].Tier_Joust)
                {
                    case 0:
                        rJoustTier = "Qualifying";
                        rJoustTierImg = "<:q_:528617317534269450>";
                        break;
                    case 1:
                        rJoustTier = "Bronze V";
                        rJoustTierImg = "<:jobr:528617414171164697>";
                        break;
                    case 2:
                        rJoustTier = "Bronze IV";
                        rJoustTierImg = "<:jobr:528617414171164697>";
                        break;
                    case 3:
                        rJoustTier = "Bronze III";
                        rJoustTierImg = "<:jobr:528617414171164697>";
                        break;
                    case 4:
                        rJoustTier = "Bronze II";
                        rJoustTierImg = "<:jobr:528617414171164697>";
                        break;
                    case 5:
                        rJoustTier = "Bronze I";
                        rJoustTierImg = "<:jobr:528617414171164697>";
                        break;
                    case 6:
                        rJoustTier = "Silver V";
                        rJoustTierImg = "<:josi:528617415903412244>";
                        break;
                    case 7:
                        rJoustTier = "Silver IV";
                        rJoustTierImg = "<:josi:528617415903412244>";
                        break;
                    case 8:
                        rJoustTier = "Silver II";
                        rJoustTierImg = "<:josi:528617415903412244>";
                        break;
                    case 9:
                        rJoustTier = "Silver II";
                        rJoustTierImg = "<:josi:528617415903412244>";
                        break;
                    case 10:
                        rJoustTier = "Silver I";
                        rJoustTierImg = "<:josi:528617415903412244>";
                        break;
                    case 11:
                        rJoustTier = "Gold V";
                        rJoustTierImg = "<:jogo:528617415500890112>";
                        break;
                    case 12:
                        rJoustTier = "Gold IV";
                        rJoustTierImg = "<:jogo:528617415500890112>";
                        break;
                    case 13:
                        rJoustTier = "Gold III";
                        rJoustTierImg = "<:jogo:528617415500890112>";
                        break;
                    case 14:
                        rJoustTier = "Gold II";
                        rJoustTierImg = "<:jogo:528617415500890112>";
                        break;
                    case 15:
                        rJoustTier = "Gold I";
                        rJoustTierImg = "<:jogo:528617415500890112>";
                        break;
                    case 16:
                        rJoustTier = "Platinum V";
                        rJoustTierImg = "<:jopl:528617415677050909>";
                        break;
                    case 17:
                        rJoustTier = "Platinum IV";
                        rJoustTierImg = "<:jopl:528617415677050909>";
                        break;
                    case 18:
                        rJoustTier = "Platinum III";
                        rJoustTierImg = "<:jopl:528617415677050909>";
                        break;
                    case 19:
                        rJoustTier = "Platinum II";
                        rJoustTierImg = "<:jopl:528617415677050909>";
                        break;
                    case 20:
                        rJoustTier = "Platinum I";
                        rJoustTierImg = "<:jopl:528617415677050909>";
                        break;
                    case 21:
                        rJoustTier = "Diamond V";
                        rJoustTierImg = "<:jodi:528617416452997120>";
                        break;
                    case 22:
                        rJoustTier = "Diamond IV";
                        rJoustTierImg = "<:jodi:528617416452997120>";
                        break;
                    case 23:
                        rJoustTier = "Diamond III";
                        rJoustTierImg = "<:jodi:528617416452997120>";
                        break;
                    case 24:
                        rJoustTier = "Diamond II";
                        rJoustTierImg = "<:jodi:528617416452997120>";
                        break;
                    case 25:
                        rJoustTier = "Diamond I";
                        rJoustTierImg = "<:jodi:528617416452997120>";
                        break;
                    case 26:
                        rJoustTier = "Masters";
                        rJoustTierImg = "<:joma:528617417170223144>";
                        break;
                    case 27:
                        rJoustTier = "Grandmaster";
                        rJoustTierImg = "<:jogm:528617416331362334>";
                        break;
                    default:
                        break;
                }

                switch (playerStats[0].Tier_Duel)
                {
                    case 0:
                        rDuelTier = "Qualifying";
                        rDuelTierImg = "<:q_:528617317534269450>";
                        break;
                    case 1:
                        rDuelTier = "Bronze V";
                        rDuelTierImg = "<:dubr:528617383011549184>";
                        break;
                    case 2:
                        rDuelTier = "Bronze IV";
                        rDuelTierImg = "<:dubr:528617383011549184>";
                        break;
                    case 3:
                        rDuelTier = "Bronze III";
                        rDuelTierImg = "<:dubr:528617383011549184>";
                        break;
                    case 4:
                        rDuelTier = "Bronze II";
                        rDuelTierImg = "<:dubr:528617383011549184>";
                        break;
                    case 5:
                        rDuelTier = "Bronze I";
                        rDuelTierImg = "<:dubr:528617383011549184>";
                        break;
                    case 6:
                        rDuelTier = "Silver V";
                        rDuelTierImg = "<:dusi:528617384395931649>";
                        break;
                    case 7:
                        rDuelTier = "Silver IV";
                        rDuelTierImg = "<:dusi:528617384395931649>";
                        break;
                    case 8:
                        rDuelTier = "Silver II";
                        rDuelTierImg = "<:dusi:528617384395931649>";
                        break;
                    case 9:
                        rDuelTier = "Silver II";
                        rDuelTierImg = "<:dusi:528617384395931649>";
                        break;
                    case 10:
                        rDuelTier = "Silver I";
                        rDuelTierImg = "<:dusi:528617384395931649>";
                        break;
                    case 11:
                        rDuelTier = "Gold V";
                        rDuelTierImg = "<:dugo:528617384463040533>";
                        break;
                    case 12:
                        rDuelTier = "Gold IV";
                        rDuelTierImg = "<:dugo:528617384463040533>";
                        break;
                    case 13:
                        rDuelTier = "Gold III";
                        rDuelTierImg = "<:dugo:528617384463040533>";
                        break;
                    case 14:
                        rDuelTier = "Gold II";
                        rDuelTierImg = "<:dugo:528617384463040533>";
                        break;
                    case 15:
                        rDuelTier = "Gold I";
                        rDuelTierImg = "<:dugo:528617384463040533>";
                        break;
                    case 16:
                        rDuelTier = "Platinum V";
                        rDuelTierImg = "<:dupl:528617384848785446>";
                        break;
                    case 17:
                        rDuelTier = "Platinum IV";
                        rDuelTierImg = "<:dupl:528617384848785446>";
                        break;
                    case 18:
                        rDuelTier = "Platinum III";
                        rDuelTierImg = "<:dupl:528617384848785446>";
                        break;
                    case 19:
                        rDuelTier = "Platinum II";
                        rDuelTierImg = "<:dupl:528617384848785446>";
                        break;
                    case 20:
                        rDuelTier = "Platinum I";
                        rDuelTierImg = "<:dupl:528617384848785446>";
                        break;
                    case 21:
                        rDuelTier = "Diamond V";
                        rDuelTierImg = "<:dudi:528617385310289922>";
                        break;
                    case 22:
                        rDuelTier = "Diamond IV";
                        rDuelTierImg = "<:dudi:528617385310289922>";
                        break;
                    case 23:
                        rDuelTier = "Diamond III";
                        rDuelTierImg = "<:dudi:528617385310289922>";
                        break;
                    case 24:
                        rDuelTier = "Diamond II";
                        rDuelTierImg = "<:dudi:528617385310289922>";
                        break;
                    case 25:
                        rDuelTier = "Diamond I";
                        rDuelTierImg = "<:dudi:528617385310289922>";
                        break;
                    case 26:
                        rDuelTier = "Masters";
                        rDuelTierImg = "<:duma:528617385452634122>";
                        break;
                    case 27:
                        rDuelTier = "Grandmaster";
                        rDuelTierImg = "<:dugm:528617385410822154>";
                        break;
                    default:
                        break;
                }

                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"{rPlayerName}")
                        .WithUrl($"https://smite.guru/profile/{playerStats[0].ActivePlayerId}")
                        .WithIconUrl(botIcon);
                });
                embed.WithTitle(Text.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId.ToString()).Result);
                if (playerStatus[0].status == 0)
                {
                    embed.WithDescription($":eyes: **Last Login:** {Text.PrettyDate(playerStats[0].Last_Login_Datetime)}");
                    embed.WithColor(new Color(220, 147, 4));
                    defaultEmoji = ":small_orange_diamond:";
                }
                else
                {
                    defaultEmoji = ":small_blue_diamond:"; // :small_blue_diamond: <:blank:570291209906552848>
                    embed.WithColor(new Color(85, 172, 238));
                    if (playerStatus[0].Match != 0)
                    {
                        await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                        List<PlayerMatchDetails> matchPlayerDetails = JsonConvert.DeserializeObject<List<PlayerMatchDetails>>(hirezAPI.matchPlayerDetails);

                        for (int s = 0; s < matchPlayerDetails.Count; s++)
                        {
                            if (matchPlayerDetails[s].playerId == playerStats[0].ActivePlayerId)
                            {
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(matchPlayerDetails[0].Queue)}**, playing as {matchPlayerDetails[s].GodName}");
                            }
                        }
                    }
                    else if (playerStatus[0].status == 2)
                    {
                        embed.WithDescription($":eyes: In {playerStatus[0].status_string}");
                    }
                    else
                    {
                        embed.WithDescription($":eyes: {playerStatus[0].status_string}");
                    }
                }
                // invisible character \u200b
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:level:529719212017451008>**Level**");
                    field.Value = ($"{defaultEmoji}{playerStats[0].Level}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:mastery:529719212076433418>**Mastery Level**");
                    field.Value = ($"{defaultEmoji}{playerStats[0].MasteryLevel}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:wp:552579445475508229>**Total Worshippers**");
                    field.Value = ($"{defaultEmoji}{playerStats[0].Total_Worshippers}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":trophy:**Wins** [{Math.Round(rWinRate, 1)}%]");
                    field.Value = ($"{defaultEmoji}{playerStats[0].Wins}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":flag_white:**Losses**");
                    field.Value = ($"{defaultEmoji}{playerStats[0].Losses}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":runner:**Leaves**");
                    field.Value = ($"{defaultEmoji}{playerStats[0].Leaves}");
                });
                // Ranked Conquest
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:conquesticon:528673820060418061>**Ranked Conquest**");
                    field.Value = ($"{rConquestTierImg}**{rConquestTier}** [{playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}]");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:jousticon:528673820018737163>**Ranked Joust**");
                    field.Value = ($"{rJoustTierImg}**{rJoustTier}** [{playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}]");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:jousticon:528673820018737163>**Ranked Duel**");
                    field.Value = ($"{rDuelTierImg}**{rDuelTier}** [{playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}]");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":video_game:**Playing SMITE since**");
                    field.Value = ($"{defaultEmoji}{rPlayerCreated}");
                });
                switch (playerStats[0].Region)
                {
                    case "Europe":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($":flag_eu:{playerStats[0].Region}");
                        });
                        break;
                    case "North America":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($":flag_us:{playerStats[0].Region}");
                        });
                        break;
                    case "Brazil":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($":flag_br:{playerStats[0].Region}");
                        });
                        break;
                    case "Australia":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($":flag_au:{playerStats[0].Region}");
                        });
                        break;
                    case "":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($"{defaultEmoji}n/a");
                        });
                        break;
                    default:
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:**Region**");
                            field.Value = ($"{defaultEmoji}{playerStats[0].Region}");
                        });
                        break;
                } // Region
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":hourglass:**Playtime**");
                    field.Value = ($"{defaultEmoji}{rHoursPlayed}");
                });
                if (godRanks.Count != 0)
                {
                    switch (godRanks.Count)
                    {
                        case 1:
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = "<:Gods:567146088985919498>Top Gods";
                                field.Value = $"{defaultEmoji}{godRanks[0].god} [{godRanks[0].Worshippers}]\n";
                            });
                            break;
                        case 2:
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = "<:Gods:567146088985919498>Top Gods";
                                field.Value = $"{defaultEmoji}{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                                $"{defaultEmoji}{godRanks[1].god} [{godRanks[1].Worshippers}]\n";
                            });
                            break;
                        default:
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = "<:Gods:567146088985919498>Top Gods";
                                field.Value = $"{defaultEmoji}{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                                $"{defaultEmoji}{godRanks[1].god} [{godRanks[1].Worshippers}]\n" +
                                $"{defaultEmoji}{godRanks[2].god} [{godRanks[2].Worshippers}]";
                            });
                            break;
                    }
                } // Top Gods
                embed.WithFooter(footer =>
                {
                    footer
                        .WithText(playerStats[0].Personal_Status_Message)
                        .WithIconUrl(botIcon);
                });
                await Context.Channel.SendMessageAsync("", false, embed.Build());

                // Saving the player to the database
                await Database.AddPlayerToDb(playerStats);
            }
        }

        [Command("istats")]
        [Alias("istat", "ipc", "ist", "istata", "ист", "истатс")]
        [RequireOwner] // vremenno
        public async Task ImageStats([Remainder] string username)
        {
            var converter = new HtmlConverter();
            await hirezAPI.GetPlayer(username);
            if (hirezAPI.playerResult == "[]")
            {
                await Context.Channel.SendMessageAsync($":exclamation:*{Text.ToTitleCase(username)}* is hidden or not found!");
            }
            else
            {
                await Context.Channel.TriggerTypingAsync();

                List<PlayerStats> playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(hirezAPI.playerResult);

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

            List<Gods.God> gods = Database.LoadGod(titleCaseGod);

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
                    domColor.DoAllGodColors();
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
                domColor.DoAllGodColors();
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
        [Alias("статус", "statis", "s", "с", "server", "servers", "se", "се")]
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
                    if (ServerStatus.incidents[i].name.Contains("Smite"))
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

        [Command("newstats")]
        [Alias("ns")]
        [RequireOwner]
        public async Task NewStatsTest([Remainder] string username)
        {
            try
            {
                List<SearchPlayers> searchPlayer = await hirezAPI.SearchPlayer(username);
                if (searchPlayer.Count == 0)
                {
                    await ReplyAsync($":x:*{username}* is hidden or not found!");
                }
                else if (searchPlayer[0].Name.ToLowerInvariant() == username.ToLowerInvariant())
                {
                    await Context.Channel.SendMessageAsync("", false, EmbedHandler.PlayerStatsEmbed(await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString())).Result.Build());
                }
                else if (searchPlayer.Count > 1)
                {
                    int playerCount = 0;
                    for (int i = 0; i < searchPlayer.Count; i++)
                    {
                        if (searchPlayer[i].Name.ToLowerInvariant() == username.ToLowerInvariant())
                        {
                            playerCount = playerCount + 1;
                        }
                    }

                    if (playerCount == 1)
                    {
                        await Context.Channel.SendMessageAsync("", false, EmbedHandler.PlayerStatsEmbed(await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString())).Result.Build());
                    }
                }
                else if (searchPlayer.Count == 1 && searchPlayer[0].Name.ToLower() == username.ToLower())
                {
                    await Context.Channel.SendMessageAsync("", false, EmbedHandler.PlayerStatsEmbed(await hirezAPI.GetPlayer(searchPlayer[0].player_id.ToString())).Result.Build());
                    //await hirezAPI.GetPlayer(username);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Path"))
                {
                    await ReplyAsync($":x:*{username}* is hidden or not found!");
                }
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
                    domColor.DoAllGodColors();
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
            embed.WithAuthor(author =>
            {
                author.WithName(Text.GetQueueName(matchDetails[0].match_queue_id));
                author.WithIconUrl(botIcon);
            });
            embed.WithThumbnailUrl(botIcon);

            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[0].hz_player_name;// win
                field.Value = "<:level:529719212017451008>" + matchDetails[0].Account_Level + "\n:small_blue_diamond:" + matchDetails[0].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[5].hz_player_name;// loss
                field.Value = "<:level:529719212017451008>" + matchDetails[5].Account_Level + "\n:small_blue_diamond:" + matchDetails[5].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[1].hz_player_name;// win
                field.Value = "<:level:529719212017451008>" + matchDetails[1].Account_Level + "\n:small_blue_diamond:" + matchDetails[1].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[6].hz_player_name;
                field.Value = "<:level:529719212017451008>" + matchDetails[6].Account_Level + "\n:small_blue_diamond:" + matchDetails[6].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[2].hz_player_name;// win
                field.Value = "<:level:529719212017451008>" + matchDetails[2].Account_Level + "\n:small_blue_diamond:" + matchDetails[2].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[7].hz_player_name;
                field.Value = "<:level:529719212017451008>" + matchDetails[7].Account_Level + "\n:small_blue_diamond:" + matchDetails[7].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[3].hz_player_name;// win
                field.Value = "<:level:529719212017451008>" + matchDetails[3].Account_Level + "\n:small_blue_diamond:" + matchDetails[3].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[8].hz_player_name;
                field.Value = "<:level:529719212017451008>" + matchDetails[8].Account_Level + "\n:small_blue_diamond:" + matchDetails[8].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[4].hz_player_name;// win
                field.Value = "<:level:529719212017451008>" + matchDetails[4].Account_Level + "\n:small_blue_diamond:" + matchDetails[4].Reference_Name;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = matchDetails[9].hz_player_name;
                field.Value = "<:level:529719212017451008>" + matchDetails[9].Account_Level + "\n:small_blue_diamond:" + matchDetails[9].Reference_Name;
            });

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("getplayer")]
        [RequireOwner]
        public async Task TestGetPlayer(string username)
        {
            List<SearchPlayers> test = await hirezAPI.SearchPlayer(username);
            await ReplyAsync(test[0].Name);
        }

        [Command("getgods")]
        [RequireOwner]
        public async Task GetGodsCommand(int id)
        {
            int totalMatches = 0;
            int conq = 0; //426
            int joust = 0; //448
            int arena = 0; //435
            int siege = 0; //459
            int clash = 0; //466
            int assault = 0; //445
            int rankcq = 0; //451
            int rankjs = 0; //450
            int rankdu = 0; //440

            List<QueueStats> cqList = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(id, 426));
            for (int i = 0; i < cqList.Count; i++)
            {
                conq = conq + cqList[i].Matches;
            }

            await ReplyAsync($"Conquest: {conq}");

            //string json = await hirezAPI.GetQueueStats(id);
            //File.WriteAllText("queuestats.json", json);
        }

        [Command("nz")] // keep it simple pls
        [RequireOwner]
        public async Task NzVrat(string id)
        {
            try
            {
                List<PlayerSpecial> playerSpecial = await GetPlayerSpecials(id);
                StringBuilder sb = new StringBuilder();
                sb.Append($"{playerSpecial[0].special}");
                await ReplyAsync(Text.CheckSpecialsForPlayer(id).Result);
            }
            catch (Exception ex)
            {
                await ReplyAsync("opa\n" +
                    $"{ex.Message}\n" +
                    $"{ex.TargetSite}");
            }
        }

        // Owner Commands

        [Command("updateplayers")]
        [RequireOwner]
        public async Task UpdatePlayers()
        {
            await ReplyAsync("do not use");

            var playersList = new List<PlayerStats>(GetAllPlayers());

            for (int i = 0; i < playersList.Count; i++)
            {
                await hirezAPI.GetPlayer(playersList[i].ActivePlayerId.ToString());
                List<PlayerStats> playerList = JsonConvert.DeserializeObject<List<PlayerStats>>(hirezAPI.playerResult);
                Console.WriteLine(playersList[i].hz_player_name + " Updated!");
                await Database.AddPlayerToDb(playerList);
            }
        }
    }
}
