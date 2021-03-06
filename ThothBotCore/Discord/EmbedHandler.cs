﻿using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.MatchPlayerDetails;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Discord
{
    public class EmbedHandler
    {
        public static async Task<Embed> ServerStatusEmbedAsync(ServerStatus smiteStatus, ServerStatus discordStatus)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("SMITE Server Status");
                author.WithUrl("http://status.hirezstudios.com/");
                author.WithIconUrl(Constants.botIcon);
            });
            var smiteCat = smiteStatus.components.Find(x => x.id == "542zlqj9nwr6");
            if (smiteCat.status == "operational")
            {
                embed.WithColor(new Color(0, 255, 0));
            }
            else if (smiteCat.status == "under_maintenance")
            {
                embed.WithColor(new Color(52, 152, 219));
            }
            else
            {
                // Incident color
                embed.WithColor(new Color(239, 167, 32));
            }

            foreach (var item in smiteCat.components)
            {
                var comp = smiteStatus.components.Find(x => x.id == item);
                var sb = new StringBuilder();

                if (comp.name.ToLowerInvariant().Contains("pc"))
                {
                    sb.Append("<:PC:537746891610259467> ");
                }
                else if (comp.name.ToLowerInvariant().Contains("xbox"))
                {
                    sb.Append("<:XB:537749895029850112> ");
                }
                else if (comp.name.ToLowerInvariant().Contains("ps4"))
                {
                    sb.Append("<:PS4:537745670518472714> ");
                }
                else if (comp.name.ToLowerInvariant().Contains("switch"))
                {
                    sb.Append("<:SW:537752006719176714> ");
                }
                else if (comp.name.ToLowerInvariant().Contains("epic"))
                {
                    sb.Append("<:egs:705963938340274247> ");
                }
                sb.Append(comp.name);
                embed.AddField(x=>
                {
                    x.IsInline = true;
                    x.Name = sb.ToString();
                    x.Value = $"{Text.StatusEmoji(comp.status)}" +
                    $"{(comp.status.Contains("_") ? Text.ToTitleCase(comp.status.Replace("_", " ")) : Text.ToTitleCase(comp.status))}";
                });
            }
            var foundDiscAPI = discordStatus.components.Find(x => x.name.ToLowerInvariant() == "api");
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Discord API";
                field.Value = Text.StatusEmoji(foundDiscAPI.status) + (foundDiscAPI.status.Contains("_") ? Text.ToTitleCase(foundDiscAPI.status.Replace("_", " ")) : Text.ToTitleCase(foundDiscAPI.status));
            });
            embed.WithFooter(x =>
            {
                x.Text = $"If you want to be notified for Server Status Updates use !!statusupdates #desired-channel";
            });

            return await Task.FromResult(embed.Build());
        }
        public static async Task<EmbedBuilder> StatusIncidentEmbed(ServerStatus serverStatus)
        {
            var incidentEmbed = new EmbedBuilder();

            for (int n = 0; n < serverStatus.incidents.Count; n++)
            {
                if (serverStatus.incidents[n].name.ToLowerInvariant().Contains("smite") ||
                    serverStatus.incidents[n].incident_updates[0].body.ToLowerInvariant().Contains("smite"))
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
                    string incidentPlatIcons = await Utils.MaintenancePlatformsAsync(serverStatus.incidents[n].components);

                    if (incidentValue.Length > 1024)
                    {
                        incidentEmbed.WithTitle($"{incidentPlatIcons.ToString()} {serverStatus.incidents[n].name}");
                        incidentEmbed.WithDescription(incidentValue);
                    }
                    else
                    {
                        incidentEmbed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{incidentPlatIcons.ToString()} {serverStatus.incidents[n].name}";
                            field.Value = incidentValue;
                        });
                    }

                    return incidentEmbed;
                }
            }

            return null;
        }
        public static async Task<EmbedBuilder> StatusMaintenanceEmbed(ServerStatus serverStatus)
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
                        footer.Text = $"Current UTC: {DateTime.UtcNow.ToString("dd MMM, HH:mm:ss", CultureInfo.InvariantCulture)}";
                    });

                    var platIcon = "";
                    var maintValue = new StringBuilder();
                    string expectedDtime = "";

                    if (serverStatus.scheduled_maintenances[i].incident_updates.Count > 1)
                    {
                        platIcon = await Utils.MaintenancePlatformsAsync(serverStatus.scheduled_maintenances[i].components);
                        TimeSpan expDwntime = serverStatus.scheduled_maintenances[i].scheduled_until - serverStatus.scheduled_maintenances[i].scheduled_for;
                        expectedDtime = await Utils.ExpectedDowntimeAsync(expDwntime);

                        if (serverStatus.scheduled_maintenances[i].status.ToLowerInvariant().Contains("progress") ||
                            serverStatus.scheduled_maintenances[i].status.ToLowerInvariant().Contains("verifying"))
                        {
                            embed.Footer.Text += " | Ends ";
                            embed.WithTimestamp(serverStatus.scheduled_maintenances[i].scheduled_until);
                        }
                        else
                        {
                            embed.Footer.Text += " | Starts ";
                            embed.WithTimestamp(serverStatus.scheduled_maintenances[i].scheduled_for);
                        }

                        for (int j = 0; j < serverStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                        {
                            string maintStatus = serverStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status);

                            maintValue.Append($"**[{maintStatus}]({serverStatus.scheduled_maintenances[i].shortlink})** - " +
                                $"{serverStatus.scheduled_maintenances[i].incident_updates[j].created_at.ToString("d MMM, HH:mm:ss UTC", CultureInfo.InvariantCulture)}\n" +
                                $"{serverStatus.scheduled_maintenances[i].incident_updates[j].body}\n");
                        }

                        embed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{platIcon}{serverStatus.scheduled_maintenances[i].name}";
                            field.Value = $"**__Expected downtime: {expectedDtime}__**, {serverStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {serverStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {serverStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n" + maintValue.ToString();
                        });
                    }
                    else
                    {
                        platIcon = await Utils.MaintenancePlatformsAsync(serverStatus.scheduled_maintenances[i].components);

                        for (int j = 0; j < serverStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                        {
                            string maintStatus = serverStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status);
                            TimeSpan expDwntime = serverStatus.scheduled_maintenances[i].scheduled_until - serverStatus.scheduled_maintenances[i].scheduled_for;
                            expectedDtime = await Utils.ExpectedDowntimeAsync(expDwntime);

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
        public static async Task<EmbedBuilder> PlayerStatsEmbed(string getplayerjson, string godranksjson, string achievementsjson, string playerstatusjson, string matchjson)
        {
            var playerStats = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerjson); // GetPlayer
            var godRanks = JsonConvert.DeserializeObject<List<GodRanks>>(godranksjson); //GodRanks
            var playerAchievements = JsonConvert.DeserializeObject<PlayerAchievements>(achievementsjson);
            var playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(playerstatusjson);
            var embed = new EmbedBuilder();
            int portal = Text.GetPortalNumber(playerStats[0].Platform);

            string defaultEmoji = ""; 

            var rPlayerName = new StringBuilder();
            string[] clanName = Array.Empty<string>();

            // Checking if the player is in a clan
            if (playerStats[0].Name.Contains("]"))
            {
                clanName = playerStats[0].Name.Split(']');
            }

            if (playerStats[0].hz_player_name == null || playerStats[0].hz_player_name.Length == 0)
            {
                rPlayerName.Append(playerStats[0].hz_gamer_tag);
            }
            else if (playerStats[0].hz_player_name.Length != 0)
            {
                rPlayerName.Append(playerStats[0].hz_player_name);
            }
            else
            {
                rPlayerName.Append(playerStats[0].Name + " 🆘");
            }

            // Add clan to rPlayerName
            if (clanName.Length != 0)
            {
                rPlayerName.Append($", {clanName[0]}]{playerStats[0].Team_Name}");
            }

            double rWinRate = 0;
            if (playerStats[0].Wins != 0)
            {
                rWinRate = (double)playerStats[0].Wins * 100 / (playerStats[0].Wins + playerStats[0].Losses);
            }

            embed.WithAuthor(author =>
            {
                author
                    .WithName(rPlayerName.ToString())
                    .WithUrl($"https://smite.guru/profile/{playerStats[0].ActivePlayerId}")
                    .WithIconUrl(Text.GetPortalIconLinksByPortalName(playerStats[0].Platform));
            });
            string embedTitle = await Utils.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId, false);
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
                if (playerStatus[0].status != 5)
                {
                    embed.WithColor(Constants.DefaultBlueColor);
                }
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
                field.Name = ($"<:level:529719212017451008>Level");
                field.Value = ($"{defaultEmoji}{playerStats[0].Level}");
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"<:mastery:529719212076433418>Mastery Level");
                field.Value = ($"{defaultEmoji}{playerStats[0].MasteryLevel}");
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"<:wp:552579445475508229>Total Worshipers");
                field.Value = ($"{defaultEmoji}{playerStats[0].Total_Worshippers}");
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":trophy:Wins [{Math.Round(rWinRate, 2)}%]";
                field.Value = $"{defaultEmoji}{playerStats[0].Wins}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":flag_white:Losses";
                field.Value = $"{defaultEmoji}{playerStats[0].Losses}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":runner:Leaves";
                field.Value = $"{defaultEmoji}{playerStats[0].Leaves}";
            });
            // Ranked Modes check for PC or Console
            if (portal == 9 || portal == 10 || portal == 22)
            {
                // Consoles
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:conquesticon:528673820060418061>Ranked Conquest🎮";
                    field.Value = $"{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item2}**{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedConquestController.Wins}/{playerStats[0].RankedConquestController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedConquestController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedConquestController.Points}";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>Ranked Joust🎮";
                    field.Value = $"{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item2}**{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedJoustController.Wins}/{playerStats[0].RankedJoustController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedJoustController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedJoustController.Points}";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>Ranked Duel🎮";
                    field.Value = $"{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item2}**{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedDuelController.Wins}/{playerStats[0].RankedDuelController.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedDuelController.Rank_Stat, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedDuelController.Points}";
                });
                // if the player plays on PC too
                if (playerStats[0].RankedConquest.Tier != 0 || playerStats[0].RankedJoust.Tier != 0 || playerStats[0].RankedDuel.Tier != 0)
                {
                    // PC
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:conquesticon:528673820060418061>Ranked Conquest";
                        field.Value = $"{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item2}**{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Conquest, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedConquest.Points}";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>Ranked Joust";
                        field.Value = $"{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item2}**{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Joust, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedJoust.Points}";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>Ranked Duel";
                        field.Value = $"{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item2}**{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Duel, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedDuel.Points}";
                    });
                }
            }
            else
            {
                // PC
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:conquesticon:528673820060418061>Ranked Conquest";
                    field.Value = $"{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item2}**{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Conquest, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedConquest.Points}";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>Ranked Joust";
                    field.Value = $"{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item2}**{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Joust, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedJoust.Points}";
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:jousticon:528673820018737163>Ranked Duel";
                    field.Value = $"{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item2}**{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item1}**\n" +
                    $"{defaultEmoji}W/L: {playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}\n" +
                    $"{defaultEmoji}MMR: {Math.Round(playerStats[0].Rank_Stat_Duel, 0)}\n" +
                    $"{defaultEmoji}TP: {playerStats[0].RankedDuel.Points}";
                });
                // If the player plays on consoles too
                if (playerStats[0].RankedConquestController.Tier != 0 || playerStats[0].RankedJoustController.Tier != 0 || playerStats[0].RankedDuelController.Tier != 0)
                {
                    // Consoles
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:conquesticon:528673820060418061>Ranked Conquest🎮";
                        field.Value = $"{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item2}**{Text.GetRankedConquest(playerStats[0].RankedConquestController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedConquestController.Wins}/{playerStats[0].RankedConquestController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedConquestController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedConquestController.Points}";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>Ranked Joust🎮";
                        field.Value = $"{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item2}**{Text.GetRankedJoust(playerStats[0].RankedJoustController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedJoustController.Wins}/{playerStats[0].RankedJoustController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedJoustController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedJoustController.Points}";
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "<:jousticon:528673820018737163>Ranked Duel🎮";
                        field.Value = $"{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item2}**{Text.GetRankedDuel(playerStats[0].RankedDuelController.Tier).Item1}**\n" +
                        $"{defaultEmoji}W/L: {playerStats[0].RankedDuelController.Wins}/{playerStats[0].RankedDuelController.Losses}\n" +
                        $"{defaultEmoji}MMR: {Math.Round(playerStats[0].RankedDuelController.Rank_Stat, 0)}\n" +
                        $"{defaultEmoji}TP: {playerStats[0].RankedDuelController.Points}";
                    });
                }
            }
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $":video_game:Playing SMITE since";
                field.Value = $"{defaultEmoji}{(playerStats[0].Created_Datetime != "" ? Text.InvariantDate(DateTime.Parse(playerStats[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
            });
            string regionValue = playerStats[0].Region switch
            {
                "Europe" => $":flag_eu:{playerStats[0].Region}",
                "North America" => $":flag_us:{playerStats[0].Region}",
                "Brazil" => $":flag_br:{playerStats[0].Region}",
                "Australia" => $":flag_au:{playerStats[0].Region}",
                "" => $"{defaultEmoji}n/a",
                _ => $"{defaultEmoji}{playerStats[0].Region}",
            };
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ":globe_with_meridians:Region";
                field.Value = regionValue;
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $":hourglass:Playtime";
                x.Value = $"{defaultEmoji}{playerStats[0].HoursPlayed} hours";
            });
            // Top Gods
            if (godRanks.Count != 0)
            {
                string godRanksValue = godRanks.Count switch
                {
                    1 => $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]",
                    2 => $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                         $":second_place:{godRanks[1].god} [{godRanks[1].Worshippers}]",
                    _ => $":first_place:{godRanks[0].god} [{godRanks[0].Worshippers}]\n" +
                         $":second_place:{godRanks[1].god} [{godRanks[1].Worshippers}]\n" +
                         $":third_place:{godRanks[2].god} [{godRanks[2].Worshippers}]",
                };
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "<:Gods:567146088985919498>Top Gods";
                    field.Value = godRanksValue;
                });
            } 
            // KDA
            if (playerAchievements.PlayerKills != 0 && playerAchievements.AssistedKills != 0 && playerAchievements.Deaths != 0)
            {
                double kda = (double)(playerAchievements.PlayerKills + (playerAchievements.AssistedKills / 2)) / playerAchievements.Deaths;
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($":crossed_swords:KDA [{Math.Round(kda, 2).ToString(CultureInfo.InvariantCulture)}]");
                    field.Value = ($":dagger:Kills: {playerAchievements.PlayerKills}\n" +
                    $":skull_crossbones:Deaths: {playerAchievements.Deaths}\n" +
                    $":handshake:Assists: {playerAchievements.AssistedKills}");
                });
            }
            if (playerStats[0].Personal_Status_Message == "")
            {
                embed.WithFooter(x =>
                {
                    x.Text = Text.GetRandomTip();
                });
            }
            else
            {
                embed.WithFooter(footer =>
                {
                    footer
                        .WithText(playerStats[0].Personal_Status_Message)
                        .WithIconUrl(playerStats[0].Avatar_URL == "" ? Constants.botIcon : playerStats[0].Avatar_URL);
                });
            }
            return embed;
        }
        public static Task<EmbedBuilder> LoadingStats(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = $"<a:typing:393848431413559296> Loading {username}..."
            };
            embed.WithColor(Constants.DefaultBlueColor);
            return Task.FromResult(embed);
        }
        public static Task<EmbedBuilder> HiddenProfileEmbed(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = username.StartsWith('*') ? "<:Hidden:591666971234402320>Account is hidden" : Text.UserIsHidden(username),
                Color = new Color(254,255,255)
            };
            return Task.FromResult(embed);
        }
        public static Task<EmbedBuilder> ProfileNotFoundEmbed(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = Text.UserNotFound(username)
            };
            return Task.FromResult(embed);
        }
        public static Task<Embed> BuildDescriptionEmbedAsync(string description, int r = 0, int g = 0, int b = 0)
        {
            var embed = new EmbedBuilder
            {
                Description = description
            };
            if (r != 0 || g != 0 || b != 0)
            {
                embed.WithColor(new Color(r, g, b));
            }
            return Task.FromResult(embed.Build());
        }
        public static Task<Embed> BuildDescriptionEmbedAsync(string description, string footerText, int r = 0, int g = 0, int b = 0)
        {
            var embed = new EmbedBuilder
            {
                Description = description,
            };
            embed.WithFooter(footerText);
            if (r != 0 || g != 0 || b != 0)
            {
                embed.WithColor(new Color(r, g, b));
            }
            return Task.FromResult(embed.Build());
        }
        public static async Task<EmbedBuilder> MultiplePlayers(List<SearchPlayers> players)
        {
            var embed = new EmbedBuilder();
            var sb = new StringBuilder();
            embed.WithAuthor(x =>
            {
                x.IconUrl = Constants.botIcon;
                x.Name = "Multiple players found";
            });
            embed.WithColor(240, 71, 71);
            for (int i = 0; i < players.Count; i++)
            {
                string specialscheck = await Utils.CheckSpecialsForPlayer(players[i].player_id, true);
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
            var gods = MongoConnection.GetAllGods();

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(Int32.Parse(matchPlayerDetails[0].Queue))}");
                author.WithIconUrl(Constants.botIcon);
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchPlayerDetails[0].Match}";
            });

            if (matchPlayerDetails.Count == 1)
            {
                string ge = gods.Find(x => x.id == matchPlayerDetails[0].GodId).Emoji;
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"{ge} {Text.HiddenProfileCheck(matchPlayerDetails[0].playerName)}";
                    x.Value = $":video_game:Account Created: \n{(matchPlayerDetails[0].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(matchPlayerDetails[0].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}";
                });
                return embed;
            }
            else if (matchPlayerDetails.Count != 0 && matchPlayerDetails[0].mapGame.Contains("AI"))
            {
                foreach (var player in matchPlayerDetails)
                {
                    string ge = gods.Find(x => x.id == player.GodId).Emoji;
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{ge} {Text.HiddenProfileCheck(player.playerName)}";
                        x.Value = $":video_game:Account Created: \n{(player.playerCreated != "" ? Text.InvariantDate(DateTime.Parse(player.playerCreated, CultureInfo.InvariantCulture)) : "n/a")}";
                    });
                }
            }

            // Averages

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
            int nz = (team1.Count + team2.Count) / 2;
            for (int i = 0; i < nz; i++)
            {
                if ((team1[0].Queue == "440") || 
                    (team1[0].Queue == "450") || 
                    (team1[0].Queue == "451") ||
                    (team1[0].Queue == "502") ||
                    (team1[0].Queue == "503") ||
                    (team1[0].Queue == "504"))
                {
                    if (team1[0].Queue == "451" || team1[0].Queue == "504")
                    {
                        player1.Append($"{Text.GetRankedConquest(team1[i].Tier).Item2} {Text.GetRankedConquest(team1[i].Tier).Item1}\n");
                        player2.Append($"{Text.GetRankedConquest(team2[i].Tier).Item2} {Text.GetRankedConquest(team2[i].Tier).Item1}\n");
                    }
                    else if (team1[0].Queue == "450" || team1[0].Queue == "503")
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
                godemoji = gods.Find(x => x.id == team1[i].GodId).Emoji;
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
                godemoji = gods.Find(x => x.id == team2[i].GodId).Emoji;
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
            string godemoji = "";
            var gods = MongoConnection.GetAllGods();

            if (matchdetailsList.Count == 1 && matchdetailsList[0].ActivePlayerId == "0")
            {
                embed.WithTitle("Hi-Rez API error:");
                embed.WithDescription(matchdetailsList[0].ret_msg.ToString());
                return embed;
            }

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(matchdetailsList[0].match_queue_id)} | {matchdetailsList[0].Minutes} mins");
                author.WithIconUrl(Constants.botIcon);
                author.WithUrl($"https://smite.guru/match/{matchdetailsList[0].Match}");
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchdetailsList[0].Match}";
            });

            if (matchdetailsList[0].name.Contains("AI"))
            {
                foreach (var player in matchdetailsList)
                {
                    godemoji = gods.Find(x => x.id == player.GodId).Emoji;
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{godemoji} {(player.hz_player_name == null || player.hz_player_name == "" ? player.hz_gamer_tag : player.hz_player_name)}";
                        x.Value = $":crossed_swords:KDA: {player.Kills_Player}/{player.Deaths}/{player.Assists}\n" +
                    $"🗡Damage: {player.Damage_Player}";
                    });
                }
                return embed;
            }

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
                    await Reporter.SendError("**Yo, MatchDetails endpoint was probably changed, or the API is going wild..**");
                }
            }
            // If the match is vs bots
            if (winners.Count == 0 || losers.Count == 0)
            {
                foreach (var player in matchdetailsList)
                {
                    godemoji = gods.Find(x => x.id == player.GodId).Emoji;
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{godemoji} {(player.hz_player_name == null || player.hz_player_name == "" ? player.hz_gamer_tag : player.hz_player_name)}\n" +
                        $"{Utils.GetItemsBuiltAsync(player)}";
                        x.Value = $":crossed_swords:KDA: {player.Kills_Player}/{player.Deaths}/{player.Assists}\n" +
                    $"🗡Damage: {player.Damage_Player}";
                    });
                }
                return embed;
            }
            string team1emo = Text.SideEmoji(winners[0].TaskForce);
            string team2emo = Text.SideEmoji(losers[0].TaskForce);
            double winnersAverageMMR = 0;
            double losersAverageMMR = 0;
            if (winners[0].match_queue_id == 451 || winners[0].match_queue_id == 504) // Conquest
            {
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Conquest != 0).Average(x => x.Rank_Stat_Conquest);
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Conquest != 0).Average(x => x.Rank_Stat_Conquest);
            }
            else if (winners[0].match_queue_id == 450 || winners[0].match_queue_id == 503) // Joust
            {
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Joust != 0).Average(x => x.Rank_Stat_Joust);
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Joust != 0).Average(x => x.Rank_Stat_Joust);
            }
            else if (winners[0].match_queue_id == 440 || winners[0].match_queue_id == 502) // Duel
            {
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Duel != 0).Average(x => x.Rank_Stat_Duel);
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Duel != 0).Average(x => x.Rank_Stat_Duel);
            }
            // Winners Main Info
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"🏆 **{winners[0].Win_Status}** 🏆";
                x.Value = $":crossed_swords:Team KDA: {winners.Sum(x => x.Kills_Player)}/{winners.Sum(x => x.Deaths)}/{winners.Sum(x => x.Assists)}\n" +
                $"{team1emo}**Averages:**{team1emo}\n" +
                $"{(winnersAverageMMR != 0 ? $"{team1emo}MMR: {Math.Round(winnersAverageMMR)}\n" : "")}" +
                $"🗡Damage: {Math.Round(winners.Average(x => x.Damage_Player))}\n";
            });
            if (winners[0].Ban1.Length != 0)
            {
                var bans = new StringBuilder();
                bans.Append(gods.Find(x => x.id == winners[0].Ban1Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban2Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban3Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban4Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban5Id)?.Emoji);
                bans.Append('\n');
                bans.Append(gods.Find(x => x.id == winners[0].Ban6Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban7Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban8Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban9Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban10Id)?.Emoji);
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
            // Losers Main Info
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $":red_circle: **{losers[0].Win_Status}** :red_circle:";
                x.Value = $":crossed_swords:Team KDA: {losers.Sum(x => x.Kills_Player)}/{losers.Sum(x => x.Deaths)}/{losers.Sum(x => x.Assists)}\n" +
                $"{team2emo}**Averages:**{team2emo}\n" +
                $"{(losersAverageMMR != 0 ? $"{team2emo}MMR: {Math.Round(losersAverageMMR)}\n" : "")}" +
                $"🗡Damage: {Math.Round(losers.Average(x => x.Damage_Player))}\n";
            });
            if (matchdetailsList[0].Entry_Datetime != null)
            {
                embed.WithTimestamp(matchdetailsList[0].Entry_Datetime);
            }

            var player1 = new StringBuilder();
            var player2 = new StringBuilder();
            
            for (int i = 0; i < winners.Count; i++)
            {
                if ((winners[0].match_queue_id == 440) || 
                    (winners[0].match_queue_id == 450) || 
                    (winners[0].match_queue_id == 451) ||
                    (winners[0].match_queue_id == 502) ||
                    (winners[0].match_queue_id == 503) ||
                    (winners[0].match_queue_id == 504))
                {
                    if (winners[0].match_queue_id == 451 || winners[0].match_queue_id == 504) // Conquest
                    {
                        player1.Append($"{Text.GetRankedConquest(winners[i].Conquest_Tier).Item2}{Text.GetRankedConquest(winners[i].Conquest_Tier).Item1}\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Conquest_Wins}/{winners[i].Conquest_Losses}\n");
                        player1.Append($"{team1emo}MMR: {Math.Round(winners[i].Rank_Stat_Conquest, 0)}\n");
                        
                        player2.Append($"{Text.GetRankedConquest(losers[i].Conquest_Tier).Item2}{Text.GetRankedConquest(losers[i].Conquest_Tier).Item1}\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Conquest_Wins}/{losers[i].Conquest_Losses}\n");
                        player2.Append($"{team2emo}MMR: {Math.Round(losers[i].Rank_Stat_Conquest, 0)}\n");
                    }
                    else if (winners[0].match_queue_id == 450 || winners[0].match_queue_id == 503) // Joust
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

                player1.Append($"{await Utils.GetItemsBuiltAsync(winners[i])}\n" +
                    $":crossed_swords:KDA: {winners[i].Kills_Player}/{winners[i].Deaths}/{winners[i].Assists}\n" +
                    $"🗡Damage: {winners[i].Damage_Player}");
                player2.Append($"{await Utils.GetItemsBuiltAsync(losers[i])}\n" +
                    $":crossed_swords:KDA: {losers[i].Kills_Player}/{losers[i].Deaths}/{losers[i].Assists}\n" +
                    $"🗡Damage: {losers[i].Damage_Player}");

                godemoji = gods.Find(x => x.id == winners[i].GodId).Emoji;
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
                godemoji = gods.Find(x => x.id == losers[i].GodId).Emoji;
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
            if (matchHistory.Count == 1 && matchHistory[0].playerId == 0)
            {
                var emb = await BuildDescriptionEmbedAsync("SMITE API Error: " + matchHistory[0].ret_msg.ToString(), 255);
                return emb;
            }
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = $"Match History of {matchHistory[0].playerName}";
                x.Url = $"https://smite.guru/profile/{matchHistory[0].playerId}/matches";
                x.IconUrl = Constants.botIcon;
            });
            embed.WithColor(Constants.DefaultBlueColor);
            string godemoji = "";
            int i = 0;
            var gods = MongoConnection.GetAllGods();
            foreach (var match in matchHistory)
            {
                if (i != 6)
                {
                    godemoji = gods.Find(x => x.id == matchHistory[i].GodId).Emoji;
                    embed.AddField(async x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{godemoji} `{matchHistory[i].Win_Status}` {Text.GetQueueName(matchHistory[i].Match_Queue_Id)} - {matchHistory[i].Minutes} min - {Text.PrettyDate(Convert.ToDateTime(matchHistory[i].Match_Time, CultureInfo.InvariantCulture))} `[{matchHistory[i].Match}]`";
                        x.Value = $"⚔**KDA:** {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists} | " +
                        $"🗡Damage: {matchHistory[i].Damage} | {await Utils.GetItemsBuiltAsync(match)}";
                    });
                }
                else
                {
                    break;
                }
                i++;
            }
            embed.WithFooter(x =>
            {
                x.Text = Text.GetRandomTip();
            });
            return embed.Build();
        }
        public static async Task<Embed> BuildWorshipersEmbedAsync(List<GodRanks> ranks, PlayerStats player)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.Name = $"{(player.hz_player_name ?? player.hz_gamer_tag)}'s masteries [{ranks.Count}]";
                x.IconUrl = Text.GetPortalIconLinksByPortalName(player.Platform);
                x.Url = $"https://smite.guru/profile/{player.ActivePlayerId}/champions";
            });
            int count = 0;
            for (int i = 0; i < ranks.Count; i++)
            {
                sb.AppendLine($"{Text.GetRankEmoji(ranks[i].Rank)} {ranks[i].god} [{ranks[i].Worshippers}]");
                count++;
                if (count == 20)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "\u200b";
                        x.Value = sb.ToString();
                    });
                    sb.Clear();
                    count = 0;
                }
            }
            if (ranks.Count == 0)
            {
                embed.WithTitle($"{(player.hz_player_name ?? player.hz_gamer_tag)} has not played any gods.");
            }
            /*
            embed.WithDescription($"**{ranks.Where(x => x.Rank == 10).Count()}** gods at rank {Text.GetRankEmoji(10)}, " +
                $"**{ranks.Where(x => x.Rank == 10).OrderByDescending(x => x.Wins).FirstOrDefault().god}**");
            var nz = ranks.GroupBy(x => x.Rank == 10);
            int x = 0;
            
            foreach (var r in ranks)
            {
                if (r.Rank == 10)
                {
                    x++;
                }
            }
            */
            if (sb.Length != 0)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "\u200b";
                    x.Value = sb.ToString();
                });
            }
            embed.WithFooter(x =>
            {
                x.Text = "Rank God [Worshipers]";
            });
            return await Task.FromResult(embed.Build());
        }
        public static async Task<Embed> BuildWinRatesEmbedAsync(List<GodRanks> ranks, PlayerStats player)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.Name = $"{(player.hz_player_name ?? player.hz_gamer_tag)}'s God Win Rates";
                x.IconUrl = Text.GetPortalIconLinksByPortalName(player.Platform);
                x.Url = $"https://smite.guru/profile/{player.ActivePlayerId}/champions";
            });
            int count = 0;
            var gods = MongoConnection.GetAllGods();
            foreach (var god in ranks)
            {
                god.WinRate = (double)god.Wins * 100 / (god.Wins + god.Losses);
            }
            var sortedRanksByWinRate = ranks.OrderByDescending(x=>x.WinRate).ToList();
            for (int i = 0; i < sortedRanksByWinRate.Count; i++)
            {
                string godEmoji = gods.Find(x => x.id.ToString() == sortedRanksByWinRate[i].god_id).Emoji;
                sb.AppendLine($"{godEmoji} {sortedRanksByWinRate[i].god} [**{Math.Round(sortedRanksByWinRate[i].WinRate, 2)}%**]");
                count++;
                if (count == 19)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "\u200b";
                        x.Value = sb.ToString();
                    });
                    sb.Clear();
                    count = 0;
                }
            }
            if (ranks.Count == 0)
            {
                embed.WithTitle($"{(player.hz_player_name ?? player.hz_gamer_tag)} has not played any gods.");
            }
            if (sb.Length != 0)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "\u200b";
                    x.Value = sb.ToString();
                });
            }
            embed.WithFooter(x =>
            {
                x.Text = "God [Win Rate]";
            });
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildPatchNotesEmbedAsync(WebAPIPostModel patchPost, string description, string imageURL, string slug)
        {
            var embed = new EmbedBuilder();
            embed.WithDescription(description);
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle(patchPost.title);
            embed.WithUrl($"https://www.smitegame.com/news/{slug}");
            embed.WithImageUrl(imageURL);
            embed.WithAuthor(x =>
            {
                x.Name = "SMITE";
                x.IconUrl = "https://pbs.twimg.com/profile_images/1210997178786832384/ScZXulZQ_400x400.jpg";
            });
            return await Task.FromResult(embed.Build());
        }
    }
}
