using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.MatchPlayerDetails;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Discord
{
    public class EmbedHandler
    {
        static string botIcon = "https://i.imgur.com/8qNdxse.png";

        public static EmbedBuilder ServerStatusEmbed(ServerStatus smiteServerStatus, ServerStatus discordServerStatus)
        {
            var foundPC = smiteServerStatus.components.Find(x => x.name.ToLowerInvariant() == "smite pc");
            var foundXBO = smiteServerStatus.components.Find(x => x.name.ToLowerInvariant() == "smite xbox");
            var foundPS4 = smiteServerStatus.components.Find(x => x.name.ToLowerInvariant().Contains("smite ps4"));
            var foundSwi = smiteServerStatus.components.Find(x => x.name.ToLowerInvariant().Contains("smite switch"));
            var foundAPI = smiteServerStatus.components.Find(x => x.name.ToLowerInvariant().Contains("api"));

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("Server Status");
                author.WithUrl("http://status.hirezstudios.com/");
                author.WithIconUrl("https://i.imgur.com/8qNdxse.png");
            });
            if (foundPC.status.Contains("operational") &&
                foundPS4.status.Contains("operational") &&
                foundXBO.status.Contains("operational") &&
                foundSwi.status.Contains("operational"))
            {
                embed.WithColor(new Color(0, 255, 0));
            }
            else if (smiteServerStatus.incidents.Count >= 1)
            {
                for (int i = 0; i < smiteServerStatus.incidents.Count; i++)
                {
                    if (smiteServerStatus.incidents[i].name.ToLowerInvariant().Contains("smite"))
                    {
                        // Incident color
                        embed.WithColor(new Color(239, 167, 32));
                    }
                }
            }
            else if (smiteServerStatus.scheduled_maintenances.Count >= 1)
            {
                for (int i = 0; i < smiteServerStatus.scheduled_maintenances.Count; i++)
                {
                    if (smiteServerStatus.scheduled_maintenances[i].name.ToLowerInvariant().Contains("smite"))
                    {
                        // Maintenance color
                        embed.WithColor(new Color(52, 152, 219));
                    }
                }
            }
            string pcValue = foundPC.status.Contains("_") ? Text.ToTitleCase(foundPC.status.Replace("_", " ")) : Text.ToTitleCase(foundPC.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:PC:537746891610259467> " + foundPC.name; // PC
                field.Value = Text.StatusEmoji(foundPC.status) + pcValue;
            });
            string ps4Value = foundPS4.status.Contains("_") ? Text.ToTitleCase(foundPS4.status.Replace("_", " ")) : Text.ToTitleCase(foundPS4.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:PS4:537745670518472714> " + foundPS4.name; // PS4
                field.Value = Text.StatusEmoji(foundPS4.status) + ps4Value;
            });
            string xbValue = foundXBO.status.Contains("_") ? Text.ToTitleCase(foundXBO.status.Replace("_", " ")) : Text.ToTitleCase(foundXBO.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:XB:537749895029850112> " + foundXBO.name; // Xbox
                field.Value = Text.StatusEmoji(foundXBO.status) + xbValue;
            });
            string swValue = foundSwi.status.Contains("_") ? Text.ToTitleCase(foundSwi.status.Replace("_", " ")) : Text.ToTitleCase(foundSwi.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:SW:537752006719176714> " + foundSwi.name; // Switch
                field.Value = Text.StatusEmoji(foundSwi.status) + swValue;
            });
            string apiValue = foundAPI.status.Contains("_") ? Text.ToTitleCase(foundAPI.status.Replace("_", " ")) : Text.ToTitleCase(foundAPI.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = foundAPI.name; // Hi-Rez API
                field.Value = Text.StatusEmoji(foundAPI.status) + apiValue;
            });
            var foundDiscAPI = discordServerStatus.components.Find(x => x.name.ToLowerInvariant() == "api");
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Discord API"; // Status page link
                field.Value = Text.StatusEmoji(foundDiscAPI.status) + (foundDiscAPI.status.Contains("_") ? Text.ToTitleCase(foundDiscAPI.status.Replace("_", " ")) : Text.ToTitleCase(foundDiscAPI.status));
            });
            embed.WithFooter(x =>
            {
                x.Text = $"If you want to be notified for Server Status Updates use !!statusupdates #desired-channel";
            });

            return embed;
        }
        public static EmbedBuilder StatusIncidentEmbed(ServerStatus serverStatus)
        {
            var incidentEmbed = new EmbedBuilder();

            for (int n = 0; n < serverStatus.incidents.Count; n++)
            {
                if ((serverStatus.incidents[n].name.ToLowerInvariant().Contains("smite") ||
                         serverStatus.incidents[n].incident_updates[0].body.ToLowerInvariant().Contains("smite")) &&
                         !(serverStatus.incidents[n].name.ToLowerInvariant().Contains("blitz")))
                {
                    incidentEmbed.WithColor(new Color(239, 167, 32));
                    incidentEmbed.WithAuthor(author =>
                    {
                        author.WithName("Incident");
                        author.WithIconUrl("https://i.imgur.com/oTHjKkE.png");
                    });
                    string incidentValue = "";
                    for (int c = 0; c < serverStatus.incidents[n].incident_updates.Count; c++)
                    {
                        incidentValue += $"**[{Text.ToTitleCase(serverStatus.incidents[n].incident_updates[c].status)}]({serverStatus.incidents[n].shortlink})** - " +
                            $"{serverStatus.incidents[n].incident_updates[c].updated_at.ToUniversalTime().ToString("d MMM, HH:mm", CultureInfo.InvariantCulture)} UTC\n" +
                            $"{serverStatus.incidents[n].incident_updates[c].body}\n";
                    }
                    string incidentPlatIcons = "";

                    for (int z = 0; z < serverStatus.incidents[n].components.Count; z++) // cycle for platform icons
                    {
                        if (serverStatus.incidents[n].components[z].name.ToLowerInvariant().Contains("smite switch"))
                        {
                            incidentPlatIcons += "<:SW:537752006719176714> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.ToLowerInvariant().Contains("smite xbox"))
                        {
                            incidentPlatIcons += "<:XB:537749895029850112> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.ToLowerInvariant().Contains("smite ps4"))
                        {
                            incidentPlatIcons += "<:PS4:537745670518472714> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.ToLowerInvariant().Contains("smite pc"))
                        {
                            incidentPlatIcons += "<:PC:537746891610259467> ";
                        }
                    }

                    if (incidentValue.Length > 1024)
                    {
                        incidentEmbed.WithTitle($"{incidentPlatIcons} {serverStatus.incidents[n].name}");
                        incidentEmbed.WithDescription(incidentValue);
                    }
                    else
                    {
                        incidentEmbed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{incidentPlatIcons} {serverStatus.incidents[n].name}";
                            field.Value = incidentValue;
                        });
                    }

                    return incidentEmbed;
                }
            }

            return null;
        }
        public static EmbedBuilder StatusMaintenanceEmbed(ServerStatus serverStatus)
        {
            var embed = new EmbedBuilder();
            for (int i = 0; i < serverStatus.scheduled_maintenances.Count; i++)
            {
                if (serverStatus.scheduled_maintenances[i].name.ToLowerInvariant().Contains("smite") ||
                    serverStatus.scheduled_maintenances[i].incident_updates[0].body.ToLowerInvariant().Contains("smite"))
                {
                    embed.WithColor(new Color(52, 152, 219)); //maintenance color
                    embed.WithAuthor(author =>
                    {
                        author.WithName("Scheduled Maintenance");
                        author.WithIconUrl("https://i.imgur.com/qGjA3nY.png");
                    });
                    embed.WithFooter(footer =>
                    {
                        footer.Text = $"Current UTC: " + DateTime.UtcNow.ToString("dd MMM, HH:mm:ss", CultureInfo.InvariantCulture);
                    });

                    string platIcon = "";
                    string maintValue = "";
                    string expectedDtime = "";

                    if (serverStatus.scheduled_maintenances[i].incident_updates.Count > 1)
                    {
                        for (int k = 0; k < serverStatus.scheduled_maintenances[i].components.Count; k++)
                        {
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite switch"))
                            {
                                platIcon += "<:SW:537752006719176714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite xbox"))
                            {
                                platIcon += "<:XB:537749895029850112> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite ps4"))
                            {
                                platIcon += "<:PS4:537745670518472714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite pc"))
                            {
                                platIcon += "<:PC:537746891610259467> ";
                            }
                        }
                        TimeSpan expDwntime = serverStatus.scheduled_maintenances[i].scheduled_until - serverStatus.scheduled_maintenances[i].scheduled_for;

                        if (expDwntime.Hours != 0)
                        {
                            if (expDwntime.Hours == 1)
                            {
                                expectedDtime += $"{expDwntime.Hours} hour";
                            }
                            else
                            {
                                expectedDtime += $"{expDwntime.Hours} hours";
                            }
                        }
                        if (expDwntime.Minutes != 0)
                        {
                            expectedDtime += " and ";
                            if (expDwntime.Minutes == 1)
                            {
                                expectedDtime += $"{expDwntime.Minutes} minute";
                            }
                            else
                            {
                                expectedDtime += $"{expDwntime.Minutes} minutes";
                            }
                        }
                        if (expectedDtime == "")
                        {
                            expectedDtime = "n/a";
                        }

                        for (int j = 0; j < serverStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                        {
                            string maintStatus = serverStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status);

                            maintValue = maintValue + $"**[{maintStatus}]({serverStatus.scheduled_maintenances[i].shortlink})** - {serverStatus.scheduled_maintenances[i].incident_updates[j].created_at.ToString("d MMM, HH:mm:ss UTC", CultureInfo.InvariantCulture)}\n{serverStatus.scheduled_maintenances[i].incident_updates[j].body}\n";
                        }

                        embed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{platIcon}{serverStatus.scheduled_maintenances[i].name}";
                            field.Value = $"**__Expected downtime: {expectedDtime}__**, {serverStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {serverStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {serverStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n" + maintValue;
                        });
                    }

                    else
                    {
                        for (int k = 0; k < serverStatus.scheduled_maintenances[i].components.Count; k++)
                        {
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite switch"))
                            {
                                platIcon += "<:SW:537752006719176714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite xbox"))
                            {
                                platIcon += "<:XB:537749895029850112> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite ps4"))
                            {
                                platIcon += "<:PS4:537745670518472714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.ToLowerInvariant().Contains("smite pc"))
                            {
                                platIcon += "<:PC:537746891610259467> ";
                            }
                        }

                        for (int j = 0; j < serverStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                        {
                            string maintStatus = serverStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status);
                            TimeSpan expDwntime = serverStatus.scheduled_maintenances[i].scheduled_until - serverStatus.scheduled_maintenances[i].scheduled_for;
                            if (expDwntime.Hours != 0)
                            {
                                if (expDwntime.Hours == 1)
                                {
                                    expectedDtime += $"{expDwntime.Hours} hour";
                                }
                                else
                                {
                                    expectedDtime += $"{expDwntime.Hours} hours";
                                }
                            }
                            if (expDwntime.Minutes != 0)
                            {
                                expectedDtime += " and ";
                                if (expDwntime.Minutes == 1)
                                {
                                    expectedDtime += $"{expDwntime.Minutes} minute";
                                }
                                else
                                {
                                    expectedDtime += $"{expDwntime.Minutes} minutes";
                                }
                            }
                            if (expectedDtime == "")
                            {
                                expectedDtime = "n/a";
                            }

                            embed.AddField(field =>
                            {
                                field.IsInline = false;
                                field.Name = $"{platIcon}{serverStatus.scheduled_maintenances[i].name}";
                                field.Value = $"**[{maintStatus}]({serverStatus.scheduled_maintenances[i].shortlink})**\n__**Expected downtime: {expectedDtime}**__, {serverStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {serverStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {serverStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n{serverStatus.scheduled_maintenances[i].incident_updates[j].body}";
                            });
                        }
                    }
                }
            }

            return embed;
        }
        public static async Task<EmbedBuilder> PlayerStatsEmbed(string getplayerjson, string godranksjson, string achievementsjson, string playerstatusjson, string matchjson, int portal)
        {
            var playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerjson); // GetPlayer
            var godRanks = JsonConvert.DeserializeObject<List<GodRanks>>(godranksjson); //GodRanks
            var playerAchievements = JsonConvert.DeserializeObject<PlayerAchievements>(achievementsjson);
            var playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(playerstatusjson);
            var embed = new EmbedBuilder();

            string defaultEmoji = ""; //🔹 <:gems:443919192748589087>

            string rPlayerName = "";

            if (playerStats[0].Name.Contains("]"))
            {
                string[] splitName = playerStats[0].Name.Split(']');
                rPlayerName = splitName[1];
                if (playerStats[0].hz_player_name != null)
                {
                    rPlayerName = playerStats[0].hz_player_name + $", {splitName[0]}]{playerStats[0].Team_Name}";
                }
                else
                {
                    rPlayerName = playerStats[0].hz_gamer_tag + $", {splitName[0]}]{playerStats[0].Team_Name}";
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
            double rWinRate = 0;
            if (playerStats[0].Wins != 0 && playerStats[0].Losses != 0)
            {
                rWinRate = (double)playerStats[0].Wins * 100 / (playerStats[0].Wins + playerStats[0].Losses);
            }

            embed.WithAuthor(author =>
            {
                author
                    .WithName($"{rPlayerName}")
                    .WithUrl($"https://smite.guru/profile/{playerStats[0].ActivePlayerId}")
                    .WithIconUrl(Text.GetPortalIconLinks(portal.ToString()));
            });
            string embedTitle = await Text.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId.ToString());
            embed.WithTitle(embedTitle);
            if (playerStatus[0].status == 0)
            {
                embed.WithDescription($":eyes: **Last Login:** {(playerStats[0].Last_Login_Datetime != "" ? Text.PrettyDate(DateTime.Parse(playerStats[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}");
                embed.WithColor(new Color(220, 147, 4));
                defaultEmoji = ":small_orange_diamond:";
            }
            else
            {
                defaultEmoji = "🔹"; // 🔹 <:blank:570291209906552848>
                embed.WithColor(Constants.DefaultBlueColor);
                if (playerStatus[0].Match != 0)
                {
                    var matchPlayerDetails = JsonConvert.DeserializeObject<List<PlayerMatchDetails>>(matchjson);
                    for (int s = 0; s < matchPlayerDetails.Count; s++)
                    {
                        if (matchPlayerDetails[0].ret_msg == null)
                        {
                            if (Int32.Parse(matchPlayerDetails[s].playerId) == playerStats[0].ActivePlayerId)
                            {
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id)}**, playing as {matchPlayerDetails[s].GodName}");
                            }
                        }
                        else
                        {
                            embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id)}**");
                        }
                    }
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
                field.Name = $":trophy:**Wins** [{Math.Round(rWinRate, 2)}%]";
                field.Value = $"{defaultEmoji}{playerStats[0].Wins}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":flag_white:**Losses**";
                field.Value = $"{defaultEmoji}{playerStats[0].Losses}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":runner:**Leaves**";
                field.Value = $"{defaultEmoji}{playerStats[0].Leaves}";
            });
            // Ranked Modes check for PC or Console
            if (portal == 9 || portal == 10 || portal == 22)
            {
                // Consoles
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:conquesticon:528673820060418061>**Ranked Conquest**🎮";
                    field.Value = $"{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item2}**{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedConquestController.Wins}/{playerStats[0].RankedConquestController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedConquestController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedConquestController.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedConquestController.Rank_Variance, 2)}%";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>**Ranked Joust**🎮";
                    field.Value = $"{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item2}**{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedJoustController.Wins}/{playerStats[0].RankedJoustController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedJoustController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedJoustController.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedJoustController.Rank_Variance, 2)}%";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>**Ranked Duel**🎮";
                    field.Value = $"{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item2}**{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedDuelController.Wins}/{playerStats[0].RankedDuelController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedDuelController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedDuelController.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedDuelController.Rank_Variance, 2)}%";
                });
                // if the player plays on PC too
                if (playerStats[0].RankedConquest.Tier != 0 || playerStats[0].RankedJoust.Tier != 0 || playerStats[0].RankedDuel.Tier != 0)
                {
                    // PC
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:conquesticon:528673820060418061>**Ranked Conquest**";
                        field.Value = $"{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item2}**{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Conquest, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedConquest.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedConquest.Rank_Variance, 2)}%";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>**Ranked Joust**";
                        field.Value = $"{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item2}**{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Joust, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedJoust.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedJoust.Rank_Variance, 2)}%";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>**Ranked Duel**";
                        field.Value = $"{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item2}**{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Duel, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedDuel.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedDuel.Rank_Variance, 2)}%";
                    });
                }
            }
            else
            {
                // PC
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:conquesticon:528673820060418061>**Ranked Conquest**";
                    field.Value = $"{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item2}**{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Conquest, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedConquest.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedConquest.Rank_Variance, 2)}%";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>**Ranked Joust**";
                    field.Value = $"{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item2}**{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Joust, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedJoust.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedJoust.Rank_Variance, 2)}%";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>**Ranked Duel**";
                    field.Value = $"{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item2}**{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Duel, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedDuel.Points}\n" +
                    $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedDuel.Rank_Variance, 2)}%";
                });
                // If the player plays on consoles too
                if (playerStats[0].RankedConquestController.Tier != 0 || playerStats[0].RankedJoustController.Tier != 0 || playerStats[0].RankedDuelController.Tier != 0)
                {
                    // Consoles
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:conquesticon:528673820060418061>**Ranked Conquest**🎮";
                        field.Value = $"{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item2}**{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedConquestController.Wins}/{playerStats[0].RankedConquestController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedConquestController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedConquestController.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedConquestController.Rank_Variance, 2)}%";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>**Ranked Joust**🎮";
                        field.Value = $"{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item2}**{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedJoustController.Wins}/{playerStats[0].RankedJoustController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedJoustController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedJoustController.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedJoustController.Rank_Variance, 2)}%";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>**Ranked Duel**🎮";
                        field.Value = $"{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item2}**{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedDuelController.Wins}/{playerStats[0].RankedDuelController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedDuelController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedDuelController.Points}\n" +
                        $"{defaultEmoji}Variance: {Math.Round(playerStats[0].RankedDuelController.Rank_Variance, 2)}%";
                    });
                }
            }
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":video_game:**Playing SMITE since**";
                field.Value = $"{defaultEmoji}{(playerStats[0].Created_Datetime != "" ? Text.InvariantDate(DateTime.Parse(playerStats[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
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
                        field.Name = $":globe_with_meridians:**Region**";
                        field.Value = $":flag_us:{playerStats[0].Region}";
                    });
                    break;
                case "Brazil":
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $":globe_with_meridians:**Region**";
                        field.Value = $":flag_br:{playerStats[0].Region}";
                    });
                    break;
                case "Australia":
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $":globe_with_meridians:**Region**";
                        field.Value = $":flag_au:{playerStats[0].Region}";
                    });
                    break;
                case "":
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $":globe_with_meridians:**Region**";
                        field.Value = $"{defaultEmoji}n/a";
                    });
                    break;
                default:
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $":globe_with_meridians:**Region**";
                        x.Value = $"{defaultEmoji}{playerStats[0].Region}";
                    });
                    break;
            } // Region
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $":hourglass:**Playtime**";
                x.Value = $"{defaultEmoji}{playerStats[0].HoursPlayed.ToString()} hours";
            });
            // Top Gods
            if (godRanks.Count != 0)
            {
                switch (godRanks.Count)
                {
                    case 1:
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "<:Gods:567146088985919498>**Top Gods**";
                            field.Value = $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]\n";
                        });
                        break;
                    case 2:
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "<:Gods:567146088985919498>**Top Gods**";
                            field.Value = $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                            $":second_place:{godRanks[1].god} [{godRanks[1].Worshippers}]\n";
                        });
                        break;
                    default:
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "<:Gods:567146088985919498>**Top Gods**";
                            field.Value = $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                            $":second_place:{godRanks[1].god} [{godRanks[1].Worshippers}]\n" +
                            $":third_place:{godRanks[2].god} [{godRanks[2].Worshippers}]";
                        });
                        break;
                }
            } 
            // KDA
            if (playerAchievements.PlayerKills != 0 && playerAchievements.AssistedKills != 0 && playerAchievements.Deaths != 0)
            {
                double kda = (double)(playerAchievements.PlayerKills + (playerAchievements.AssistedKills / 2)) / playerAchievements.Deaths;
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":crossed_swords:**KDA** [{Math.Round(kda, 2).ToString(CultureInfo.InvariantCulture)}]");
                    field.Value = ($":dagger:Kills: {playerAchievements.PlayerKills}\n" +
                    $":skull_crossbones:Deaths: {playerAchievements.Deaths}\n" +
                    $":handshake:Assists: {playerAchievements.AssistedKills}");
                });
            }
            embed.WithFooter(footer =>
            {
                footer
                    .WithText(playerStats[0].Personal_Status_Message)
                    .WithIconUrl(playerStats[0].Avatar_URL == "" ? botIcon : playerStats[0].Avatar_URL);
            });

            // Saving to the database
            try
            {
                await Database.AddPlayerToDb(playerStats, portal).ConfigureAwait(false);//tuk butnahme nz
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Error: " + ex.Message);
            }

            return embed;
        }
        public static async Task<EmbedBuilder> LoadingStats(string username)
        {
            var embed = new EmbedBuilder();
            embed.Description = $"Loading {username}...";
            return embed;
        }
        public static async Task<EmbedBuilder> HiddenProfileEmbed(string username)
        {
            var embed = new EmbedBuilder();
            embed.Description = Text.UserIsHidden(username);
            return embed;
        }
        public static async Task<EmbedBuilder> ProfileNotFoundEmbed(string username)
        {
            var embed = new EmbedBuilder();
            embed.Description = Text.UserNotFound(username);
            return embed;
        }
        public static Task<Embed> BuildDescriptionEmbedAsync(string description)
        {
            var embed = new EmbedBuilder();
            embed.Description = description;
            return Task.FromResult(embed.Build());
        }
        public static async Task<EmbedBuilder> MultiplePlayers(List<SearchPlayers> players)
        {
            var embed = new EmbedBuilder();
            var sb = new StringBuilder();
            embed.WithAuthor(x =>
            {
                x.IconUrl = botIcon;
                x.Name = "Multiple players found";
            });//<:Hidden:591666971234402320>
            embed.WithColor(240, 71, 71);
            for (int i = 0; i < players.Count; i++)
            {
                string specialscheck = await Text.CheckSpecialsForPlayer(players[i].player_id.ToString());
                sb.Append($"{i+1}. {players[i].Name} {Text.GetPortalIcon(players[i].portal_id.ToString())} " +
                    $"{specialscheck}" +
                    $"{(players[i].privacy_flag == "y" ? "**Hidden Profile** <:Hidden:591666971234402320>" : "")}\n");
            }
            embed.WithFooter(x =>
            {
                x.Text = "You have 60 seconds to respond.";
            });
            embed.WithTitle("Please type the number of the player you would like to choose");
            embed.WithDescription(sb.ToString());
            return embed;
        }
        public static async Task<EmbedBuilder> LiveMatchEmbed(List<MatchPlayerDetails.PlayerMatchDetails> matchPlayerDetails)
        {
            var embed = new EmbedBuilder();

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(Int32.Parse(matchPlayerDetails[0].Queue))}");
                author.WithIconUrl(botIcon);
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchPlayerDetails[0].Match}";
            });

            var team1 = new List<PlayerMatchDetails>();
            var team2 = new List<PlayerMatchDetails>();
            foreach (var item in matchPlayerDetails)
            {
                if (item.taskForce == 1)
                {
                    team1.Add(item);
                }
                else
                {
                    team2.Add(item);
                }
            }
            string team1force = Text.SideEmoji(team1[0].taskForce);
            string team2force = Text.SideEmoji(team2[0].taskForce);
            var player1 = new StringBuilder();
            var player2 = new StringBuilder();
            string godemoji = "";
            for (int i = 0; i < team1.Count; i++)
            {
                if ((team1[0].Queue == "440") || (team1[0].Queue == "450") || (team1[0].Queue == "451"))
                {
                    if (team1[0].Queue == "451")
                    {
                        player1.Append($"{Text.GetRankedConquest(team1[i].Tier).Item2} {Text.GetRankedConquest(team1[i].Tier).Item1}\n");
                        player2.Append($"{Text.GetRankedConquest(team2[i].Tier).Item2} {Text.GetRankedConquest(team2[i].Tier).Item1}\n");
                    }
                    else if (team1[0].Queue == "450")
                    {
                        player1.Append($"{Text.GetRankedJoust(team1[i].Tier).Item2} {Text.GetRankedJoust(team1[i].Tier).Item1}\n");
                        player2.Append($"{Text.GetRankedJoust(team2[i].Tier).Item2} {Text.GetRankedJoust(team2[i].Tier).Item1}\n");
                    }
                    else
                    {
                        player1.Append($"{Text.GetRankedDuel(team1[i].Tier).Item2} {Text.GetRankedDuel(team1[i].Tier).Item1}\n");
                        player2.Append($"{Text.GetRankedDuel(team2[i].Tier).Item2} {Text.GetRankedDuel(team2[i].Tier).Item1}\n");
                    }
                    player1.Append($"🔹 W/L: {team1[i].tierWins}/{team1[i].tierLosses}\n" +
                        $"🔹 MMR: {Math.Round(team1[i].Rank_Stat, 0)}");
                    player2.Append($"🔸 W/L: {team2[i].tierWins}/{team2[i].tierLosses}\n" +
                        $"🔸 MMR: {Math.Round(team2[i].Rank_Stat, 0)}");
                }
                else
                {
                    player1.Append($":video_game:Account Created: \n{team1force}{(team1[i].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team1[i].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
                    player2.Append($":video_game:Account Created: \n{team2force}{(team2[i].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team2[i].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
                }
                godemoji = await Database.GetGodEmoji(team1[i].GodName);
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = $"{godemoji} {Text.HiddenProfileCheck(team1[i].playerName)}";// left
                    field.Value = player1.ToString();
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"**{team1[i].Account_Level} **<:level:529719212017451008>** {team2[i].Account_Level}**";
                    x.Value = $" {Text.AbbreviationRegions(team1[i].playerRegion)} 🌐 {Text.AbbreviationRegions(team2[i].playerRegion)}";
                });
                godemoji = await Database.GetGodEmoji(team2[i].GodName);
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = $"{godemoji} {Text.HiddenProfileCheck(team2[i].playerName)}";// loss
                    field.Value = player2.ToString();
                });

                player1.Clear();
                player2.Clear();
            }

            return embed;
        }
        public static async Task<EmbedBuilder> MatchDetailsEmbed(List<MatchDetails.MatchDetailsPlayer> matchdetailsList)
        {
            var embed = new EmbedBuilder();

            TimeSpan matchTime = TimeSpan.FromSeconds(matchdetailsList[0].Time_In_Match_Seconds);

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{matchdetailsList[0].name} | {matchTime.Minutes} mins");
                author.WithIconUrl(botIcon);
                author.WithUrl($"https://smite.guru/match/{matchdetailsList[0].Match}");
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchdetailsList[0].Match.ToString()}";
            });

            var winners = new List<MatchDetails.MatchDetailsPlayer>();
            var losers = new List<MatchDetails.MatchDetailsPlayer>();

            foreach (var player in matchdetailsList)
            {
                if (player.Win_Status.ToLowerInvariant() == "winner")
                {
                    winners.Add(player);
                }
                else if (player.Win_Status.ToLowerInvariant() == "loser")
                {
                    losers.Add(player);
                }
                else
                {
                    await ErrorTracker.SendError("**Yo, MatchDetails endpoint was probably changed, or the API is going wild..**");
                }
            }
            string team1emo = Text.SideEmoji(winners[0].TaskForce);
            string team2emo = Text.SideEmoji(losers[0].TaskForce);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $":green_circle: **{winners[0].Win_Status}** :green_circle:";
                x.Value = $"{team1emo}{Text.SideName(winners[0].TaskForce)}";
            });
            if (winners[0].Ban1.Length != 0)
            {
                var bans = new StringBuilder();
                bans.Append(await Database.GetGodEmoji(winners[0].Ban1));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban2));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban3));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban4));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban5));
                bans.Append("\n");
                bans.Append(await Database.GetGodEmoji(winners[0].Ban6));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban7));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban8));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban9));
                bans.Append(await Database.GetGodEmoji(winners[0].Ban10));
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "🚫**Bans**";
                    x.Value = bans.ToString();
                });
            }
            else
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "\u200b";
                    x.Value = "\u200b";
                });
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $":red_circle: **{losers[0].Win_Status}** :red_circle:";
                x.Value = $"{team2emo}{Text.SideName(losers[0].TaskForce)}";
            });
            if (matchdetailsList[0].Entry_Datetime != null)
            {
                embed.WithTimestamp(matchdetailsList[0].Entry_Datetime);
            }

            var player1 = new StringBuilder();
            var player2 = new StringBuilder();
            string godemoji = "";
            for (int i = 0; i < winners.Count; i++)
            {
                if ((winners[0].match_queue_id == 440) || (winners[0].match_queue_id == 450) || (winners[0].match_queue_id == 451))
                {
                    if (winners[0].match_queue_id == 451)
                    {
                        player1.Append($"{Text.GetRankedConquest(winners[i].Conquest_Tier).Item2}{Text.GetRankedConquest(winners[i].Conquest_Tier).Item1}\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Conquest_Wins}/{winners[i].Conquest_Losses}\n");
                        player1.Append($"{team1emo}MMR: {Math.Round(winners[i].Rank_Stat_Conquest, 0)}\n");
                        
                        player2.Append($"{Text.GetRankedConquest(losers[i].Conquest_Tier).Item2}{Text.GetRankedConquest(losers[i].Conquest_Tier).Item1}\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Conquest_Wins}/{losers[i].Conquest_Losses}\n");
                        player2.Append($"{team2emo}MMR: {Math.Round(losers[i].Rank_Stat_Conquest, 0)}\n");
                    }
                    else if (winners[0].match_queue_id == 450)
                    {
                        player1.Append($"{Text.GetRankedJoust(winners[i].Joust_Tier).Item2}{Text.GetRankedJoust(winners[i].Joust_Tier).Item1}\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Joust_Wins}/{winners[i].Joust_Losses}\n");
                        player1.Append($"{team1emo}MMR: {Math.Round(winners[i].Rank_Stat_Joust, 0)}\n");

                        player2.Append($"{Text.GetRankedJoust(losers[i].Joust_Tier).Item2}{Text.GetRankedJoust(losers[i].Joust_Tier).Item1}\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Joust_Wins}/{losers[i].Joust_Losses}\n");
                        player2.Append($"{team2emo}MMR: {Math.Round(losers[i].Rank_Stat_Joust, 0)}\n");
                    }
                    else
                    {
                        player1.Append($"{Text.GetRankedDuel(winners[i].Duel_Tier).Item2}{Text.GetRankedDuel(winners[i].Duel_Tier).Item1}\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Duel_Wins}/{winners[i].Duel_Losses}\n");
                        player1.Append($"{team1emo}MMR: {Math.Round(winners[i].Rank_Stat_Duel, 0)}\n");

                        player2.Append($"{Text.GetRankedDuel(losers[i].Duel_Tier).Item2}{Text.GetRankedDuel(losers[i].Duel_Tier).Item1}\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Duel_Wins}/{losers[i].Duel_Losses}\n");
                        player2.Append($"{team2emo}MMR: {Math.Round(losers[i].Rank_Stat_Duel, 0)}\n");
                    }
                }

                player1.Append($":crossed_swords:KDA: {winners[i].Kills_Player}/{winners[i].Deaths}/{winners[i].Assists}\n" +
                    $"🗡Damage: {winners[i].Damage_Player}");
                player2.Append($":crossed_swords:KDA: {losers[i].Kills_Player}/{losers[i].Deaths}/{losers[i].Assists}\n" +
                    $"🗡Damage: {losers[i].Damage_Player}");

                godemoji = await Database.GetGodEmoji(winners[i].Reference_Name);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"{godemoji} {(winners[i].hz_player_name == null || winners[i].hz_player_name == "" ? winners[i].hz_gamer_tag : winners[i].hz_player_name)}";
                    x.Value = player1.ToString();
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"**{winners[i].Account_Level} **<:level:529719212017451008>** {losers[i].Account_Level}**";
                    x.Value = $"{Text.AbbreviationRegions(winners[i].Region)} 🌐 {Text.AbbreviationRegions(losers[i].Region)}";
                });
                godemoji = await Database.GetGodEmoji(losers[i].Reference_Name);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"{godemoji} {(losers[i].hz_player_name == "" || losers[i].hz_player_name == null ? losers[i].hz_gamer_tag : losers[i].hz_player_name)}";
                    x.Value = player2.ToString();
                });

                player1.Clear();
                player2.Clear();
            }
            return embed;
        }
        public static async Task<Embed> BuildMatchHistoryEmbedAsync(List<MatchHistoryModel> matchHistory)
        {
            var embed = new EmbedBuilder();

            embed.WithAuthor(x =>
            {
                x.Name = $"Match History of {matchHistory[9].playerName}";
                x.Url = $"https://smite.guru/profile/{matchHistory[0].playerId}/matches";
                x.IconUrl = botIcon;
            });
            embed.WithColor(Constants.DefaultBlueColor);
            string godemoji = "";
            for (int i = 0; i < 5; i++)
            {
                godemoji = await Database.GetGodEmoji(matchHistory[i].God);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"{godemoji} **{matchHistory[i].Win_Status}**  {Text.GetQueueName(matchHistory[i].Match_Queue_Id)} - {matchHistory[i].Minutes} min - {Text.PrettyDate(matchHistory[i].Match_Time)} [{matchHistory[i].Match}]";
                    x.Value = $"⚔**KDA:** {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists} | 🗡Damage: {matchHistory[i].Damage}";
                });
            }
            return embed.Build();
        }
        public static async Task<Embed> BuildWorshippersEmbedAsync(List<GodRanks> ranks, SearchPlayers player)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.Name = $"{player.Name}'s masteries [{ranks.Count}]";
                x.IconUrl = Text.GetPortalIconLinks(player.portal_id.ToString());
                x.Url = $"https://smite.guru/profile/{player.player_id}";
            });
            int count = 0;
            int fieldsCounter = 1;
            for (int i = 0; i < ranks.Count; i++)
            {
                sb.AppendLine($"{Text.GetRankEmoji(ranks[i].Rank)} {ranks[i].god} [{ranks[i].Worshippers}]");
                count++;
                if (count == 20)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = Text.NumberEmojies(fieldsCounter);
                        x.Value = sb.ToString();
                    });
                    sb.Clear();
                    count = 0;
                    fieldsCounter++;
                }
            }
            if (sb.Length != 0)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = Text.NumberEmojies(fieldsCounter);
                    x.Value = sb.ToString();
                });
            }
            embed.WithFooter(x =>
            {
                x.Text = "Rank God [Worshippers]";
            });
            return await Task.FromResult(embed.Build());
        }
    }
}
