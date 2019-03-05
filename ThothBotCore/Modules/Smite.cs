using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using NReco.ImageGenerator;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Models;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Modules
{
    public class Smite : ModuleBase<SocketCommandContext>
    {
        readonly string prefix = Credentials.botConfig.prefix;
        readonly string botIcon = "https://i.imgur.com/AgNocjS.png";
        static Random rnd = new Random();

        HiRezAPI hirezAPI = new HiRezAPI();
        DominantColor domColor = new DominantColor();

        [Command("stats")]
        [Alias("stat", "pc", "st", "stata", "ст", "статс")]
        public async Task Stats([Remainder] string username)
        {
            await hirezAPI.GetPlayer(username);
            if (hirezAPI.playerResult == "[]")
            {
                await Context.Channel.SendMessageAsync($":exclamation:*{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(username)}* is hidden or not found!");
            }
            else
            {
                List<PlayerStats> playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(hirezAPI.playerResult);

                string rPlayerName = "";
                string rPlayerNameLink = "";

                if (playerStats[0].Name.Contains("]"))
                {
                    rPlayerName = $"{playerStats[0].Name}, {playerStats[0].Team_Name}";
                    string[] splitPlayerName = playerStats[0].Name.Split(']');
                    rPlayerNameLink = splitPlayerName[1];
                }
                else
                {
                    rPlayerName = playerStats[0].Name;
                    rPlayerNameLink = playerStats[0].Name;
                }
                string rPlayerCreated = playerStats[0].Created_Datetime.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
                string rHoursPlayed = playerStats[0].HoursPlayed.ToString() + " hours";
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
                embed.WithDescription($"<:level:529719212017451008>Level: {playerStats[0].Level} **|** <:mastery:529719212076433418>Mastery Level: {playerStats[0].MasteryLevel}");
                embed.WithAuthor(author =>
                {
                    author
                        .WithName($"{rPlayerName}")
                        .WithUrl($"http://smite.guru/profile/pc/{rPlayerNameLink}")
                        .WithIconUrl(botIcon);
                });
                embed.WithColor(new Color(0, 255, 0));
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":trophy:Wins");
                    field.Value = ($":small_blue_diamond:{playerStats[0].Wins}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":flag_white:Losses");
                    field.Value = ($":small_blue_diamond:{playerStats[0].Losses}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":runner:Leaves");
                    field.Value = ($":small_blue_diamond:{playerStats[0].Leaves}");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:conquesticon:528673820060418061>Ranked Conquest");
                    field.Value = ($"{rConquestTierImg}**{rConquestTier}**");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:jousticon:528673820018737163>Ranked Joust");
                    field.Value = ($"{rJoustTierImg}**{rJoustTier}**");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"<:jousticon:528673820018737163>Ranked Duel");
                    field.Value = ($"{rDuelTierImg}**{rDuelTier}**");
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":video_game:Playing SMITE since");
                    field.Value = ($":small_blue_diamond:{rPlayerCreated}");
                });
                switch (playerStats[0].Region)
                {
                    case "Europe":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:Region");
                            field.Value = ($":flag_eu: {playerStats[0].Region}");
                        });
                        break;
                    case "North America":
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:Region");
                            field.Value = ($":flag_us: {playerStats[0].Region}");
                        });
                        break;
                    default:
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = ($":globe_with_meridians:Region");
                            field.Value = ($":small_blue_diamond:{playerStats[0].Region}");
                        });
                        break;
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":hourglass:Playtime");
                    field.Value = ($":small_blue_diamond:{rHoursPlayed}");
                });
                embed.WithFooter(footer =>
                {
                    footer
                        .WithText($"{playerStats[0].Personal_Status_Message}")
                        .WithIconUrl(botIcon);
                });
                await Context.Channel.SendMessageAsync("", false, embed.Build());

                // Saving the player in the database
                // TO DO...
                //UserAccounts.GetAccount(responseFromServer); // Get or create
            }
        }

        [Command("istats", RunMode = RunMode.Async)]
        [Alias("istat", "ipc", "ist", "istata", "ист", "истатс")]
        public async Task ImageStats([Remainder] string username)
        {
            await hirezAPI.GetPlayer(username);
            if (hirezAPI.playerResult == "[]")
            {
                await Context.Channel.SendMessageAsync($":exclamation:*{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(username)}* is hidden or not found!");
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
                string rPlayerLastLogin = "";
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

                // stops here for some reason...
                var converter = new HtmlToImageConverter
                {
                    Width = 800,
                    Height = 600
                };
                var imgBytes = converter.GenerateImage(css + html, NReco.ImageGenerator.ImageFormat.Jpeg);

                await Context.Channel.SendFileAsync(new MemoryStream(imgBytes), $"{playerStats[0].Id}.jpg");
            }
        }

        [Command("god")] // Get specific God information
        [Alias("g", "gods")]
        public async Task GodInfo([Remainder] string god)
        {
            string titleCaseGod = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(god);

            List<Gods.God> gods = Database.LoadGod(titleCaseGod);

            if (gods.Count == 0)
            {
                await ReplyAsync($"{titleCaseGod} was not found");
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

        [Command("rgod")] // Random God
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

        [Command("status")] // SMITE Server Status 
        [Alias("статус", "statis", "s", "с")]
        public async Task ServerStatusCheck()
        {
            string result;
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "http://stk4xr7r1y0r.statuspage.io/api/v2/summary.json"))
                {
                    var response = await httpClient.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }

            ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(result);

            var foundPC = ServerStatus.components.Find(x => x.name == "Smite PC");
            var foundXBO = ServerStatus.components.Find(x => x.name == "Smite Xbox");
            var foundPS4 = ServerStatus.components.Find(x => x.name.Contains("Smite PS4"));
            var foundAPI = ServerStatus.components.Find(x => x.name.Contains("API"));

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("Server Status");
                author.WithUrl("http://status.hirezstudios.com/");
                author.WithIconUrl(botIcon);
            });

            if (foundPC.status.Contains("operational") && foundPS4.status.Contains("operational") && foundXBO.status.Contains("operational"))
            {
                embed.WithColor(new Color(0, 255, 0));
            }
            else if (ServerStatus.incidents.Count >= 1)
            {
                // Incident
                embed.WithColor(new Color(239, 167, 32));
            }
            else if (ServerStatus.scheduled_maintenances.Count >= 1)
            {
                // Maintenance
                embed.WithColor(new Color(52, 152, 219));
            }
            string pcValue = foundPC.status.Contains("_") ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundPC.status.Replace("_", " ")) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundPC.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:pcicon:537746891610259467> " + foundPC.name; // PC
                field.Value = $"{pcValue}";
            });
            string ps4Value = foundPS4.status.Contains("_") ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundPS4.status.Replace("_", " ")) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundPS4.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:playstationicon:537745670518472714> " + foundPS4.name; // PS4
                field.Value = $"{ps4Value}";
            });
            string xbValue = foundXBO.status.Contains("_") ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundXBO.status.Replace("_", " ")) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundXBO.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:xboxicon:537749895029850112> " + foundXBO.name; // Xbox
                field.Value = $"{xbValue}";
            });
            embed.AddField(field =>
            {
                field.IsInline = false;
                field.Name = foundAPI.name; // Hi-Rez API
                field.Value = foundAPI.status.Contains("_") ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundAPI.status.Replace("_", " ")) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(foundAPI.status);
            });
            //embed.AddField(field =>
            //{
            //    field.IsInline = true;
            //    field.Name = "<:switchicon:537752006719176714> Smite Switch";
            //    field.Value = "n/a";
            //});

            await ReplyAsync("", false, embed.Build()); // Server Status POST

            if (ServerStatus.incidents.Count >= 1) // Check for incidents
            {
                var incidentEmbed = new EmbedBuilder();
                bool inci = false;

                for (int n = 0; n < ServerStatus.incidents.Count; n++)
                {
                    if (ServerStatus.incidents[n].name.Contains("Smite"))
                    {
                        incidentEmbed.WithColor(new Color(239, 167, 32));
                        incidentEmbed.WithAuthor(author =>
                        {
                            author.WithName("Incidents");
                            author.WithIconUrl("https://i.imgur.com/oTHjKkE.png");
                        });
                        string incidentValue = "";
                        for (int c = 0; c < ServerStatus.incidents[n].incident_updates.Count; c++)
                        {
                            incidentValue += $"**[{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ServerStatus.incidents[n].incident_updates[c].status)}]({ServerStatus.incidents[n].shortlink})** - " +
                                $"{ServerStatus.incidents[n].incident_updates[c].updated_at.ToUniversalTime().ToString("d MMM, HH:mm", CultureInfo.InvariantCulture)} UTC\n" +
                                $"{ServerStatus.incidents[n].incident_updates[c].body}\n";
                        }
                        string incidentPlatIcons = "";

                        if (ServerStatus.incidents[n].name.Contains("PC"))
                        {
                            incidentPlatIcons += "<:pcicon:537746891610259467> ";
                        }
                        if (ServerStatus.incidents[n].name.Contains("PS4"))
                        {
                            incidentPlatIcons += "<:playstationicon:537745670518472714> ";
                        }
                        if (ServerStatus.incidents[n].name.Contains("Switch"))
                        {
                            incidentPlatIcons += "<:switchicon:537752006719176714> ";
                        }
                        if (ServerStatus.incidents[n].name.Contains("Xbox"))
                        {
                            incidentPlatIcons += "<:xboxicon:537749895029850112> ";
                        }

                        incidentEmbed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{incidentPlatIcons} {ServerStatus.incidents[n].name}";
                            field.Value = incidentValue;
                        });
                    }
                }

                if (inci == true)
                {
                    await ReplyAsync("", false, incidentEmbed.Build());
                }
            }

            if (ServerStatus.scheduled_maintenances.Count >= 1) // Check for maintenances
            {
                var scheduledEmbed = new EmbedBuilder();
                bool maint = false;
                for (int i = 0; i < ServerStatus.scheduled_maintenances.Count; i++)
                {
                    if (ServerStatus.scheduled_maintenances[i].name.Contains("Smite"))
                    {
                        maint = true;
                        scheduledEmbed.WithColor(new Color(52, 152, 219));
                        string platIcon = "";

                        for (int k = 0; k < ServerStatus.scheduled_maintenances[i].components.Count; k++)
                        {
                            if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PC"))
                            {
                                platIcon += "<:pcicon:537746891610259467> ";
                            }
                            if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PS4"))
                            {
                                platIcon += "<:playstationicon:537745670518472714> ";
                            }
                            if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("Xbox"))
                            {
                                platIcon += "<:xboxicon:537749895029850112> ";
                            }
                        }

                        if (ServerStatus.scheduled_maintenances[i].incident_updates[0].body.Contains("Switch") || ServerStatus.scheduled_maintenances[i].name.Contains("Switch"))
                        {
                            platIcon += "<:switchicon:537752006719176714> ";
                        }

                        scheduledEmbed.WithAuthor(author =>
                        {
                            author.WithName("Scheduled Maintenances");
                            author.WithIconUrl("https://i.imgur.com/qGjA3nY.png");
                        });

                        string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Contains("_") ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Replace("_", " ")) : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status);
                        TimeSpan expDwntime = ServerStatus.scheduled_maintenances[i].scheduled_until - ServerStatus.scheduled_maintenances[i].scheduled_for;
                        scheduledEmbed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{platIcon}{ServerStatus.scheduled_maintenances[i].name}";
                            field.Value = $"**[{maintStatus}]({ServerStatus.scheduled_maintenances[i].shortlink})**\n__**Expected downtime: ~{expDwntime.Hours} hours**__, {ServerStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {ServerStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {ServerStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n{ServerStatus.scheduled_maintenances[i].incident_updates[0].body}";
                        });
                    }
                }
                if (maint == true)
                {
                    await ReplyAsync("", false, scheduledEmbed.Build());
                }
            }
        }
    }
}
