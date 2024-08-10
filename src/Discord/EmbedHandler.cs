using Discord;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public class EmbedHandler
    {
        static Random rnd = new();
        public static async Task<Embed> ServerStatusEmbedAsync(ServerStatus smiteStatus, List<HiRezServerStatus> hiRezServerStatus)
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

            var apiPc = hiRezServerStatus.Find(x=> x.Platform == "pc" && x.Environment == "live");
            var apiXb = hiRezServerStatus.Find(x => x.Platform == "xbox" && x.Environment == "live");
            var apiPs = hiRezServerStatus.Find(x => x.Platform == "ps4" && x.Environment == "live");
            var apiSw = hiRezServerStatus.Find(x => x.Platform == "switch" && x.Environment == "live");
            var apiPTS = hiRezServerStatus?.Find(x => x.Environment == "pts" && x.Platform == "pc");

            foreach (var item in smiteCat.components)
            {
                var comp = smiteStatus.components.Find(x => x.id == item);
                var sb = new StringBuilder();
                string apiInfo = "";

                if (comp.name.ToLowerInvariant().Contains("pc"))
                {
                    sb.Append("<:PC:537746891610259467> ");
                    apiInfo = $"{Text.StatusEmoji(apiPc != null && apiPc.Limited_access ? "limited_access" : Text.EmptyStringCheck(apiPc?.Status.ToLowerInvariant()))}" +
                        (apiPc != null && apiPc.Limited_access ? "Limited Access" :  Text.EmptyStringCheck(apiPc?.Status));
                    
                }
                else if (comp.name.ToLowerInvariant().Contains("xbox"))
                {
                    sb.Append("<:XB:537749895029850112> ");
                    apiInfo = $"{Text.StatusEmoji(apiXb != null && apiXb.Limited_access ? "limited_access" : Text.EmptyStringCheck(apiXb?.Status.ToLowerInvariant()))}" +
                        (apiXb != null && apiXb.Limited_access ? "Limited Access" : Text.EmptyStringCheck(apiXb?.Status));
                }
                else if (comp.name.ToLowerInvariant().Contains("ps4"))
                {
                    sb.Append("<:PS4:537745670518472714> ");
                    apiInfo = $"{Text.StatusEmoji(apiPs != null && apiPs.Limited_access ? "limited_access" : Text.EmptyStringCheck(apiPs?.Status.ToLowerInvariant()))}" +
                        (apiPs != null && apiPs.Limited_access ? "Limited Access" : Text.EmptyStringCheck(apiPs?.Status));
                }
                else if (comp.name.ToLowerInvariant().Contains("switch"))
                {
                    sb.Append("<:SW:537752006719176714> ");
                    apiInfo = $"{Text.StatusEmoji(apiSw != null && apiSw.Limited_access ? "limited_access" : Text.EmptyStringCheck(apiSw?.Status.ToLowerInvariant()))}" +
                        (apiSw != null && apiSw.Limited_access ? "Limited Access" :  Text.EmptyStringCheck(apiSw?.Status));
                }
                else if (comp.name.ToLowerInvariant().Contains("epic"))
                {
                    sb.Append("<:egs:705963938340274247> ");
                    apiInfo = $"{Text.StatusEmoji(apiPc != null && apiPc.Limited_access ? "limited_access" : Text.EmptyStringCheck(apiPc?.Status.ToLowerInvariant()))}" +
                        (apiPc != null && apiPc.Limited_access ? "Limited Access" : Text.EmptyStringCheck(apiPc?.Status));
                }
                sb.Append(comp.name);
                embed.AddField(x=>
                {
                    x.IsInline = true;
                    x.Name = sb.ToString();
                    x.Value = $"{Text.StatusEmoji(comp.status)}" +
                    $"{(comp.status.Contains("_") ? Text.ToTitleCase(comp.status.Replace("_", " ")) : Text.ToTitleCase(comp.status))}\n" +
                    $"{apiInfo}";
                });
            }
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:PC:537746891610259467> SMITE PTS";
                field.Value = $"{Text.StatusEmoji(Text.EmptyStringCheck(apiPTS?.Status.ToLowerInvariant()))}{Text.EmptyStringCheck(apiPTS?.Status)}\nVersion: {Text.EmptyStringCheck(apiPTS?.Version)}";
            });
            embed.WithFooter(x =>
            {
                x.Text = $"If you want to be notified for SMITE Status changes use /feeds";
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
                            $"{Text.RelativeTimestamp(serverStatus.incidents[n].incident_updates[c].updated_at.ToUniversalTime())}\n" +
                            $"{serverStatus.incidents[n].incident_updates[c].body}\n";
                    }
                    string incidentPlatIcons = await Utils.MaintenancePlatformsAsync(serverStatus.incidents[n].components);

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

                    var platIcon = "";
                    var maintValue = new StringBuilder();
                    string expectedDtime = "";

                    if (serverStatus.scheduled_maintenances[i].incident_updates.Count > 1)
                    {
                        platIcon = await Utils.MaintenancePlatformsAsync(serverStatus.scheduled_maintenances[i].components);
                        TimeSpan expDwntime = serverStatus.scheduled_maintenances[i].scheduled_until - serverStatus.scheduled_maintenances[i].scheduled_for;
                        expectedDtime = await Utils.ExpectedDowntimeAsync(expDwntime);

                        for (int j = 0; j < serverStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                        {
                            string maintStatus = serverStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(serverStatus.scheduled_maintenances[i].incident_updates[j].status);

                            maintValue.Append($"**[{maintStatus}]({serverStatus.scheduled_maintenances[i].shortlink})** - " +
                                $"{Text.RelativeTimestamp(serverStatus.scheduled_maintenances[i].incident_updates[j].created_at.ToUniversalTime())}\n" +
                                $"{serverStatus.scheduled_maintenances[i].incident_updates[j].body}\n");
                        }

                        embed.AddField(field =>
                        {
                            field.IsInline = false;
                            field.Name = $"{platIcon}{serverStatus.scheduled_maintenances[i].name}";
                            field.Value = $"**__Expected downtime: {expectedDtime}__**, {Text.ShortDateTimeTimestamp(serverStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime())}" +
                            $" - {Text.ShortTimeTimestamp(serverStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime())}\n" + maintValue.ToString();
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
                                field.Value = $"**[{maintStatus}]({serverStatus.scheduled_maintenances[i].shortlink})**\n" +
                                $"__**Expected downtime: {expectedDtime}**__, {Text.ShortDateTimeTimestamp(serverStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime())} - " +
                                $"{Text.ShortTimeTimestamp(serverStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime())}" +
                                $"\n{serverStatus.scheduled_maintenances[i].incident_updates[j].body}";
                            });
                        }
                    }
                }
            }

            return embed;
        }
        public static async Task<EmbedBuilder> PlayerStatsEmbed(string getplayerjson, string godranksjson, string achievementsjson, string playerstatusjson, string matchjson)
        {
            var playerStats = JsonConvert.DeserializeObject<List<Player.PlayerStats>>(getplayerjson); // GetPlayer
            var godRanks = JsonConvert.DeserializeObject<List<GodRanks>>(godranksjson); //GodRanks
            var playerAchievements = JsonConvert.DeserializeObject<PlayerAchievements>(achievementsjson);
            var playerStatus = JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(playerstatusjson);
            var embed = new EmbedBuilder();
            int portal = Text.GetPortalNumber(playerStats[0].Platform);

            string defaultEmoji = ""; 

            var rPlayerName = new StringBuilder();
            string[] clanName = Array.Empty<string>();

            // Checking if the player is in a clan
            if (playerStats[0].Name.Contains(']'))
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
            string embedTitle = await Utils.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId, 1);
            embed.WithTitle(embedTitle);
            if (playerStatus[0].status == 0)
            {
                embed.WithDescription($":eyes: **Last Login:** " +
                    $"{(playerStats[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(playerStats[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}");
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
                    var matchPlayerDetails = JsonConvert.DeserializeObject<List<MatchPlayerDetails.PlayerMatchDetails>>(matchjson);
                    for (int s = 0; s < matchPlayerDetails.Count; s++)
                    {
                        if (matchPlayerDetails[0].ret_msg == null)
                        {
                            if (int.Parse(matchPlayerDetails[s].playerId) == playerStats[0].ActivePlayerId)
                            {
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**, " +
                                    $"playing as {matchPlayerDetails[s].GodName}");
                            }
                        }
                        else
                        {
                            embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**");
                        }
                    }
                }
                else
                {
                    embed.WithDescription($":eyes: {playerStatus[0].status_string}");
                }
            }
            if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
            {
                // April Fools
                defaultEmoji = "<:kek:785917111884972032> ";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
                field.Value = $"{defaultEmoji}" +
                $"{(playerStats[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(playerStats[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
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
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"⚔️KDA [{Math.Round(Text.CalculateKDA(playerAchievements.PlayerKills, playerAchievements.Deaths, playerAchievements.AssistedKills), 2).ToString(CultureInfo.InvariantCulture)}]");
                field.Value = ($":dagger:Kills: {playerAchievements.PlayerKills}\n" +
                $":skull_crossbones:Deaths: {playerAchievements.Deaths}\n" +
                $":handshake:Assists: {playerAchievements.AssistedKills}");
            });
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
        public static async Task<EmbedBuilder> PlayerStatsEmbed(List<Player.PlayerStats> playerStats, 
                                                                List<GodRanks> godRanks,
                                                                PlayerAchievements playerAchievements,
                                                                List<Player.PlayerStatus> playerStatus,
                                                                List<MatchPlayerDetails.PlayerMatchDetails> matchPlayerDetails)
        {
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
            string embedTitle = await Utils.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId, 1);
            embed.WithTitle(embedTitle);
            if (playerStatus[0].status == 0)
            {
                embed.WithDescription($":eyes: **Last Login:** " +
                    $"{(playerStats[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(playerStats[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}");
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
                    for (int s = 0; s < matchPlayerDetails.Count; s++)
                    {
                        if (matchPlayerDetails[0].ret_msg == null)
                        {
                            if (Int32.Parse(matchPlayerDetails[s].playerId) == playerStats[0].ActivePlayerId)
                            {
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**, " +
                                    $"playing as {matchPlayerDetails[s].GodName}");
                            }
                        }
                        else
                        {
                            embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**");
                        }
                    }
                }
                else
                {
                    embed.WithDescription($":eyes: {playerStatus[0].status_string}");
                }
            }
            if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
            {
                // April Fools
                defaultEmoji = "<a:danceCat:854122666298179604> ";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
                field.Value = $"{defaultEmoji}" +
                $"{(playerStats[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(playerStats[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
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
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"⚔️KDA [{Math.Round(Text.CalculateKDA(playerAchievements.PlayerKills, playerAchievements.Deaths, playerAchievements.AssistedKills), 2).ToString(CultureInfo.InvariantCulture)}]");
                field.Value = ($":dagger:Kills: {playerAchievements.PlayerKills}\n" +
                $":skull_crossbones:Deaths: {playerAchievements.Deaths}\n" +
                $":handshake:Assists: {playerAchievements.AssistedKills}");
            });
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
        public static async Task<EmbedBuilder> NewPlayerStatsEmbed(List<Player.PlayerStats> playerStats,
                                                                List<GodRanks> godRanks,
                                                                PlayerAchievements playerAchievements,
                                                                List<Player.PlayerStatus> playerStatus,
                                                                List<MatchPlayerDetails.PlayerMatchDetails> matchPlayerDetails)
        {
            var embed = new EmbedBuilder();
            int portal = Text.GetPortalNumber(playerStats[0].Platform);

            string defaultEmoji = "";

            var rPlayerName = new StringBuilder();
            string[] clanName = Array.Empty<string>();

            // Checking if the player is in a clan
            if (playerStats[0].Name.Contains(']'))
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
            string embedTitle = await Utils.CheckSpecialsForPlayer(playerStats[0].ActivePlayerId, 1);
            embed.WithTitle(embedTitle);
            if (playerStatus[0].status == 0)
            {
                embed.WithDescription($":eyes: **Last Login:** " +
                    $"{(playerStats[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(playerStats[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}");
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
                    for (int s = 0; s < matchPlayerDetails.Count; s++)
                    {
                        if (matchPlayerDetails[0].ret_msg == null)
                        {
                            if (Int32.Parse(matchPlayerDetails[s].playerId) == playerStats[0].ActivePlayerId)
                            {
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**, " +
                                    $"playing as {matchPlayerDetails[s].GodName}");
                            }
                        }
                        else
                        {
                            embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id, playerStatus[0].Match.ToString())}**");
                        }
                    }
                }
                else
                {
                    embed.WithDescription($":eyes: {playerStatus[0].status_string}");
                }
            }
            if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
            {
                // April Fools
                defaultEmoji = "<a:danceCat:854122666298179604> ";
            }
            string regionEmoji = playerStats[0].Region switch
            {
                "Europe" => $":flag_eu:",
                "North America" => $":flag_us:",
                "Brazil" => $":flag_br:",
                "Australia" => $":flag_au:",
                _ => $":globe_with_meridians:",
            };

            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"<:level:529719212017451008>Level: {playerStats[0].Level}";
                field.Value = $"<:mastery:529719212076433418>Masteries: {playerStats[0].MasteryLevel}\n" +
                $"<:wp:552579445475508229>Worshipers: {playerStats[0].Total_Worshippers}\n" +
                $"{regionEmoji}Region: {playerStats[0].Region}\n" +
                $"👶{(playerStats[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(playerStats[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
            });
            // invisible character \u200b
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"<:matches:579604410569850891>Games: {playerStats[0].Wins + playerStats[0].Losses + playerStats[0].Leaves}";
                field.Value = $":trophy:Wins: {playerStats[0].Wins} [{Math.Round(rWinRate, 2)}%]\n" +
                $":flag_white:Losses: {playerStats[0].Losses}\n" +
                $":runner:Leaves: {playerStats[0].Leaves}\n" +
                $":hourglass:Playtime: {playerStats[0].HoursPlayed} hrs";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"<:fill:862261456756801537>Most Played Classes";
                field.Value = $"lol";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                    field.Name = "<:Joust:1141068132522393640>Ranked Joust";
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
                        field.Name = "<:Joust:1141068132522393640>Ranked Joust🎮";
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
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"⚔️KDA [{Math.Round(Text.CalculateKDA(playerAchievements.PlayerKills, playerAchievements.Deaths, playerAchievements.AssistedKills), 2).ToString(CultureInfo.InvariantCulture)}]");
                field.Value = ($":dagger:Kills: {playerAchievements.PlayerKills}\n" +
                $":skull_crossbones:Deaths: {playerAchievements.Deaths}\n" +
                $":handshake:Assists: {playerAchievements.AssistedKills}");
            });
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
        public static async Task<EmbedBuilder> LoadingStats(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = $"<a:typing:393848431413559296> Loading {username}...",
                Color = Constants.DefaultBlueColor
            };
            return await Task.FromResult(embed);
        }
        public static async Task<EmbedBuilder> HiddenProfileEmbed(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = username.StartsWith('*') ? "<:Hidden:591666971234402320>Account is hidden" : Text.UserIsHidden(username),
                Color = new Color(254,255,255)
            };
            return await Task.FromResult(embed);
        }
        public static async Task<EmbedBuilder> ProfileNotFoundEmbed(string username)
        {
            var embed = new EmbedBuilder
            {
                Description = Text.UserNotFound(username),
                Color = Constants.ErrorColor
            };
            return await Task.FromResult(embed);
        }
        public static async Task<Embed> BuildClanEmbedAsync(List<TeamDetailsModel> clandetails, List<TeamPlayersModel.TeamPlayer> players)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var player in players)
            {
                sb.AppendLine($" {player.AccountLevel} {Text.HiddenProfileCheck(player.Name)} " +
                    $"[{Text.RelativeTimestamp(DateTime.Parse(player.JoinedDatetime, CultureInfo.InvariantCulture))}] " +
                    $"[{Text.RelativeTimestamp(DateTime.Parse(player.LastLoginDatetime, CultureInfo.InvariantCulture))}]");
            }
            var embed = new EmbedBuilder
            {
                Description = $"### [{clandetails[0].Tag}] {clandetails[0].Name}\n" +
                $"👤 **Owner:** {clandetails[0].Founder}\n" +
                $"👥 **Players:** {clandetails[0].Players}\n" +
                $"🏆 **Wins:** {clandetails[0].Wins}\n" +
                $"🏳 **Losses:** {clandetails[0].Losses}\n" +
                $"⭐ **Rating:** {clandetails[0].Rating}",
                Color = Constants.DefaultBlueColor,
                ThumbnailUrl = clandetails[0].AvatarURL
            };
            embed.AddField(x =>
            {
                x.Name = "Players";
                x.Value = $"**<:level:529719212017451008> IGN [Clan Joined] [Last Login]**\n" +
                $"{sb}";
            });

            return await Task.FromResult(embed.Build());
        }
        public static async Task<Embed> BuildNotLinkedEmbedAsync()
        {
            var embed = new EmbedBuilder
            {
                Description = "Please provide a 'PlayerName' or </link:969453246819209299> your SMITE account in " +
                "Thoth's database to view your SMITE stats without providing your `PlayerName` " +
                "every time you run this command.",
                Color = Constants.DefaultBlueColor
            };
            
            return await Task.FromResult(embed.Build());
        }
        public static Task<Embed> BuildAlreadyLinkedEmbedAsync(List<Player.PlayerStats> getplayer, List<Player.PlayerStatus> getplayerstatus)
        {
            var embed = new EmbedBuilder();
            string statusString = $":eyes: **{getplayerstatus[0].status_string}**";

            if (getplayerstatus[0].status == 0)
            {
                statusString = $":eyes: **Last Login:** " +
                    $"{(getplayer[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(getplayer[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
            }
            embed.WithColor(Constants.ErrorColor);
            embed.WithThumbnailUrl(getplayer[0].Avatar_URL);
            embed.WithTitle("You have already linked your Discord account with this SMITE account. ⏬");
            embed.WithDescription($"**{getplayer[0].hz_player_name + " " + getplayer[0].hz_gamer_tag}**\n" +
                    $"<:level:529719212017451008>**Level**: {getplayer[0].Level}\n" +
                    $"📅 **Account Created**: " +
                    $"{(getplayer[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(getplayer[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                    $"💭 **Personal Status Message:** {getplayer[0].Personal_Status_Message}\n" +
                    $"⌛ **Playtime:** {getplayer[0].HoursPlayed} hours\n" +
                    $"{statusString}\n\n" +
                    $"**If you would like to link it to another SMITE account, please unlink the accounts by pressing the \"Unlink\" button under this message and then run the `/link` command again.**");
            return Task.FromResult(embed.Build());
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
        public static Task<Embed> BuildDescriptionEmbedAsync(string description, Color color)
        {
            var embed = new EmbedBuilder
            {
                Description = description
            };
            if (color.RawValue != 0)
            {
                embed.WithColor(color);
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
                string specialscheck = await Utils.CheckSpecialsForPlayer(players[i].player_id, 0);
                sb.Append($"{i+1}. {players[i].Name} {Text.GetPortalEmoji(players[i].portal_id.ToString())} " +
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
            var gods = Constants.GodsHashSet.ToList();

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(Int32.Parse(matchPlayerDetails[0].Queue), matchPlayerDetails[0].Match.ToString())}");
                author.WithIconUrl(Constants.botIcon);
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchPlayerDetails[0].Match}";
            });

            if (matchPlayerDetails.Count == 1)
            {
                string ge = Utils.FindGodEmoji(gods, matchPlayerDetails[0].GodId);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"{ge} {Text.HiddenProfileCheck(matchPlayerDetails[0].playerName)}";
                    x.Value = $":video_game:Account Created: \n{(matchPlayerDetails[0].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(matchPlayerDetails[0].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}";
                });
                return embed;
            }
            else if (matchPlayerDetails.Count != 0 && (matchPlayerDetails[0].mapGame.Contains("AI") || matchPlayerDetails[0].mapGame.Contains("Practice")))
            {
                foreach (var player in matchPlayerDetails)
                {
                    string ge = Utils.FindGodEmoji(gods, player.GodId);
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{ge} {Text.HiddenProfileCheck(player.playerName)}, <:level:529719212017451008>{player.Account_Level}";
                        x.Value = $":video_game:Account Created: \n{(player.playerCreated != "" ? Text.InvariantDate(DateTime.Parse(player.playerCreated, CultureInfo.InvariantCulture)) : "n/a")}";
                    });
                }
                return embed;
            }

            var team1 = new List<MatchPlayerDetails.PlayerMatchDetails>();
            var team2 = new List<MatchPlayerDetails.PlayerMatchDetails>();

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
            bool isRanked = Utils.IsRanked(team1[0].Queue);
            // 0 = Duel, 1 = Conquest, 2 = Joust
            byte queueMode = 0;
            // Averages
            if (isRanked)
            {
                string team1AvgTier = "";
                string team2AvgTier = "";
                if (team1[0].Queue == "451" || team1[0].Queue == "504")
                {
                    // Conquest
                    queueMode = 1;
                    var tuple = Text.GetRankedConquest(Convert.ToInt32(team1.Where(x => x.Tier != -1).Average(x => x.Tier)));
                    team1AvgTier = $"{tuple.Item2}{tuple.Item1}";
                    tuple = Text.GetRankedConquest(Convert.ToInt32(team2.Where(x => x.Tier != -1).Average(x => x.Tier)));
                    team2AvgTier = $"{tuple.Item2}{tuple.Item1}";
                }
                else if (team1[0].Queue == "450" || team1[0].Queue == "503")
                {
                    // Joust
                    queueMode = 2;
                    var tuple = Text.GetRankedJoust(Convert.ToInt32(team1.Where(x => x.Tier != -1).Average(x => x.Tier)));
                    team1AvgTier = $"{tuple.Item2}{tuple.Item1}";
                    tuple = Text.GetRankedJoust(Convert.ToInt32(team2.Where(x => x.Tier != -1).Average(x => x.Tier)));
                    team2AvgTier = $"{tuple.Item2}{tuple.Item1}";
                }
                if (team1AvgTier.Length != 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{team1force}**Averages:**{team1force}";
                        x.Value = $"{team1AvgTier} [{Math.Round(team1.Where(x => x.Rank_Stat != 0).Average(x => x.Rank_Stat), 2)}]";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "\u200b";
                        x.Value = "\u200b";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{team2force}**Averages:**{team2force}";
                        x.Value = $"{team2AvgTier} [{Math.Round(team2.Where(x => x.Rank_Stat != 0).Average(x => x.Rank_Stat), 2)}]";
                    });
                }
            }

            var player1 = new StringBuilder();
            var player2 = new StringBuilder();
            string godemoji = "";
            int nz = (team1.Count + team2.Count) / 2;
            for (int i = 0; i < nz; i++)
            {
                if (isRanked)
                {
                    if (queueMode == 1)
                    {
                        player1.Append($"{Text.GetRankedConquest(team1[i].Tier).Item2}{Text.GetRankedConquest(team1[i].Tier).Item1} [{Math.Round(team1[i].Rank_Stat, 0)}]\n");
                        player2.Append($"{Text.GetRankedConquest(team2[i].Tier).Item2}{Text.GetRankedConquest(team2[i].Tier).Item1} [{Math.Round(team2[i].Rank_Stat, 0)}]\n");
                    }
                    else if (queueMode == 2)
                    {
                        player1.Append($"{Text.GetRankedJoust(team1[i].Tier).Item2}{Text.GetRankedJoust(team1[i].Tier).Item1} [{Math.Round(team1[i].Rank_Stat, 0)}]\n");
                        player2.Append($"{Text.GetRankedJoust(team2[i].Tier).Item2}{Text.GetRankedJoust(team2[i].Tier).Item1} [{Math.Round(team2[i].Rank_Stat, 0)}]\n");
                    }
                    else
                    {
                        player1.Append($"{Text.GetRankedDuel(team1[i].Tier).Item2}{Text.GetRankedDuel(team1[i].Tier).Item1} [{Math.Round(team1[i].Rank_Stat, 0)}]\n");
                        player2.Append($"{Text.GetRankedDuel(team2[i].Tier).Item2}{Text.GetRankedDuel(team2[i].Tier).Item1} [{Math.Round(team2[i].Rank_Stat, 0)}]\n");
                    }
                    player1.Append($"🔹W/L: {team1[i].tierWins}/{team1[i].tierLosses}<:blank:570291209906552848>{team1[i].tierPoints} TP");
                    player2.Append($"🔸W/L: {team2[i].tierWins}/{team2[i].tierLosses}<:blank:570291209906552848>{team2[i].tierPoints} TP");
                }
                else
                {
                    player1.Append($":video_game:Account Created: \n{team1force}{(team1[i]?.playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team1[i]?.playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
                    player2.Append($":video_game:Account Created: \n{team2force}{(team2[i]?.playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team2[i]?.playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
                }
                godemoji = Utils.FindGodEmoji(gods, team1[i].GodId);
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
                godemoji = Utils.FindGodEmoji(gods, team2[i].GodId);
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = $"{godemoji} {Text.HiddenProfileCheck(team2[i].playerName)}";// right
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
            var gods = Constants.GodsHashSet.ToList();

            if (matchdetailsList.Count == 1 && matchdetailsList[0].ActivePlayerId == "0")
            {
                embed.WithTitle("Hi-Rez API error:");
                embed.WithDescription(matchdetailsList[0].ret_msg.ToString());
                return embed;
            }

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(matchdetailsList[0].match_queue_id, matchdetailsList[0].Match.ToString(), matchdetailsList[0].name)} | {matchdetailsList[0].Minutes} mins");
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
                    godemoji = Utils.FindGodEmoji(gods, player.GodId);
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{godemoji} {Text.HiddenProfileCheck(player.playerName, player.hz_player_name, player.hz_gamer_tag, player.ret_msg)}";
                        x.Value = $"⚔️KDA: {player.Kills_Player}/{player.Deaths}/{player.Assists}\n" +
                    $"🗡Damage: {player.Damage_Player}";
                    });
                }
                return embed;
            }

            var winners = new List<MatchDetails.MatchDetailsPlayer>();
            var losers = new List<MatchDetails.MatchDetailsPlayer>();
            var parties = new Dictionary<int, List<MatchDetails.MatchDetailsPlayer>>();

            foreach (var player in matchdetailsList)
            {
                // Parties
                if (player.PartyId != 0)
                {
                    if (parties.ContainsKey(player.PartyId))
                    {
                        parties[player.PartyId].Add(player);
                    }
                    else
                    {
                        parties.Add(player.PartyId, new List<MatchDetails.MatchDetailsPlayer> { player });
                    }
                }

                // Winners / Losers
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
                    await Reporter.SendErrorAsync("**Yo, MatchDetails endpoint was probably changed, or the API is going wild..**");
                }
            }
            // Sorting the parties for easier printing after
            var sortedParties = parties.OrderByDescending(x => x.Value.Count).ToList();

            // If the match is vs bots
            // Yo, does this even run at any point?
            if (winners.Count == 0 || losers.Count == 0)
            {
                foreach (var player in matchdetailsList)
                {
                    godemoji = Utils.FindGodEmoji(gods, player.GodId);
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{godemoji} {Text.HiddenProfileCheck(player.playerName, player.hz_player_name, player.hz_gamer_tag, player.ret_msg)}\n" +
                        $"{Utils.GetItemsBuiltAsync(player)}";
                        x.Value = $"⚔️KDA: {player.Kills_Player}/{player.Deaths}/{player.Assists}\n" +
                    $"🗡Damage: {player.Damage_Player}";
                    });
                }
                return embed;
            }
            string team1emo = Text.SideEmoji(winners[0].TaskForce);
            string team2emo = Text.SideEmoji(losers[0].TaskForce);
            Tuple<string,string> tuple;
            double winnersAverageMMR = 0;
            string winnersAverages = "";
            string losersAverages = "";
            double losersAverageMMR = 0;
            if (winners[0].match_queue_id == 451 || winners[0].match_queue_id == 504) // Conquest
            {
                tuple = Text.GetRankedConquest(Convert.ToInt32(winners.Where(x => x.Conquest_Tier != -1).Average(x => x.Conquest_Tier)));
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Conquest != 0).Average(x => x.Rank_Stat_Conquest);
                winnersAverages = $"{tuple.Item2}{tuple.Item1} {(winnersAverageMMR != 0 ? $"[{Math.Round(winnersAverageMMR)}]\n" : "")}";

                tuple = Text.GetRankedConquest(Convert.ToInt32(losers.Where(x => x.Conquest_Tier != -1).Average(x => x.Conquest_Tier)));
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Conquest != 0).Average(x => x.Rank_Stat_Conquest);
                losersAverages = $"{tuple.Item2}{tuple.Item1} {(losersAverageMMR != 0 ? $"[{Math.Round(losersAverageMMR)}]\n" : "")}";
            }
            else if (winners[0].match_queue_id == 450 || winners[0].match_queue_id == 503) // Joust
            {
                tuple = Text.GetRankedJoust(Convert.ToInt32(winners.Where(x => x.Joust_Tier != -1).Average(x => x.Joust_Tier)));
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Joust != 0).Average(x => x.Rank_Stat_Joust);
                winnersAverages = $"{tuple.Item2}{tuple.Item1} {(winnersAverageMMR != 0 ? $"[{Math.Round(winnersAverageMMR)}]\n" : "")}";

                tuple = Text.GetRankedJoust(Convert.ToInt32(losers.Where(x => x.Joust_Tier != -1).Average(x => x.Joust_Tier)));
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Joust != 0).Average(x => x.Rank_Stat_Joust);
                losersAverages = $"{tuple.Item2}{tuple.Item1} {(losersAverageMMR != 0 ? $"[{Math.Round(losersAverageMMR)}]\n" : "")}";
            }
            else if (winners[0].match_queue_id == 440 || winners[0].match_queue_id == 502) // Duel
            {
                tuple = Text.GetRankedDuel(Convert.ToInt32(winners.Where(x => x.Duel_Tier != -1).Average(x => x.Duel_Tier)));
                winnersAverageMMR = winners.Where(x => x.Rank_Stat_Duel != 0).Average(x => x.Rank_Stat_Duel);
                winnersAverages = $"{tuple.Item2}{tuple.Item1} {(winnersAverageMMR != 0 ? $"[{Math.Round(winnersAverageMMR)}]\n" : "")}";

                tuple = Text.GetRankedDuel(Convert.ToInt32(losers.Where(x => x.Duel_Tier != -1).Average(x => x.Duel_Tier)));
                losersAverageMMR = losers.Where(x => x.Rank_Stat_Duel != 0).Average(x => x.Rank_Stat_Duel);
                losersAverages = $"{tuple.Item2}{tuple.Item1} {(losersAverageMMR != 0 ? $"[{Math.Round(losersAverageMMR)}]\n" : "")}";
            }
            // Winners Main Info
            string winnersVal = "";
            if (winners.Count != 1)
            {
                winnersVal = $"⚔️Team KDA: {winners.Sum(x => x.Kills_Player)}/{winners.Sum(x => x.Deaths)}/{winners.Sum(x => x.Assists)}\n" +
                    $"{team1emo}**Averages:**{team1emo}\n" +
                    $"{winnersAverages}" +
                    $"🗡Damage: {Math.Round(winners.Average(x => x.Damage_Player))}\n";
            }
            else
            {
                winnersVal = $"{team1emo}{Text.SideName(winners[0].TaskForce)}";
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"🏆 **{winners[0].Win_Status}** 🏆";
                x.Value = winnersVal;
            });
            if (winners[0].Ban1.Length != 0 || winners[0].Ban2.Length != 0)
            {
                var bans = new StringBuilder();
                bans.Append(gods.Find(x => x.id == winners[0].Ban1Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban2Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban3Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban4Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban5Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban6Id)?.Emoji);
                bans.Append('\n');
                bans.Append(gods.Find(x => x.id == winners[0].Ban7Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban8Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban9Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban10Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban11Id)?.Emoji);
                bans.Append(gods.Find(x => x.id == winners[0].Ban12Id)?.Emoji);
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
            string losersVal = "";
            if (winners.Count != 1)
            {
                losersVal = $"⚔️Team KDA: {losers.Sum(x => x.Kills_Player)}/{losers.Sum(x => x.Deaths)}/{losers.Sum(x => x.Assists)}\n" +
                $"{team2emo}**Averages:**{team2emo}\n" +
                $"{losersAverages}" +
                $"🗡Damage: {Math.Round(losers.Average(x => x.Damage_Player))}\n";
            }
            else
            {
                losersVal = $"{team2emo}{Text.SideName(losers[0].TaskForce)}";
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"🔴 **{losers[0].Win_Status}** 🔴";
                x.Value = losersVal;
            });
            if (matchdetailsList[0].Entry_Datetime.HasValue)
            {
                embed.WithTimestamp(matchdetailsList[0].Entry_Datetime.Value);
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
                        tuple = Text.GetRankedConquest(winners[i].Conquest_Tier);
                        player1.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(winners[i].Rank_Stat_Conquest, 0)}]\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Conquest_Wins}/{winners[i].Conquest_Losses}<:blank:570291209906552848>{winners[i].Conquest_Points} TP\n");

                        tuple = Text.GetRankedConquest(losers[i].Conquest_Tier);
                        player2.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(losers[i].Rank_Stat_Conquest, 0)}]\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Conquest_Wins}/{losers[i].Conquest_Losses}<:blank:570291209906552848>{losers[i].Conquest_Points} TP\n");
                    }
                    else if (winners[0].match_queue_id == 450 || winners[0].match_queue_id == 503) // Joust
                    {
                        tuple = Text.GetRankedJoust(winners[i].Joust_Tier);
                        player1.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(winners[i].Rank_Stat_Joust, 0)}]\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Joust_Wins}/{winners[i].Joust_Losses}<:blank:570291209906552848>{winners[i].Joust_Points} TP\n");

                        tuple = Text.GetRankedJoust(losers[i].Joust_Tier);
                        player2.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(losers[i].Rank_Stat_Joust, 0)}]\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Joust_Wins}/{losers[i].Joust_Losses}<:blank:570291209906552848>{losers[i].Joust_Points} TP\n");
                    }
                    else
                    {
                        tuple = Text.GetRankedDuel(winners[i].Duel_Tier);
                        player1.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(winners[i].Rank_Stat_Duel, 0)}]\n");
                        player1.Append($"{team1emo}W/L: {winners[i].Duel_Wins}/{winners[i].Duel_Losses}<:blank:570291209906552848>{winners[i].Duel_Points} TP\n");

                        tuple = Text.GetRankedDuel(losers[i].Duel_Tier);
                        player2.Append($"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(losers[i].Rank_Stat_Duel, 0)}]\n");
                        player2.Append($"{team2emo}W/L: {losers[i].Duel_Wins}/{losers[i].Duel_Losses}<:blank:570291209906552848>{losers[i].Duel_Points} TP\n");
                    }
                }

                // Party Checker
                var foundWinP = sortedParties.Find(x => x.Key == winners[i].PartyId);
                KeyValuePair<int, List<MatchDetails.MatchDetailsPlayer>> foundLosP = new();
                if (i < losers.Count)
                {
                    foundLosP = sortedParties.Find(x => x.Key == losers[i].PartyId);
                }

                player1.Append($"{await Utils.GetItemsBuiltAsync(winners[i])}\n" +
                    $"⚔️KDA: {winners[i].Kills_Player}/{winners[i].Deaths}/{winners[i].Assists}\n" +
                    $"🗡Damage: {winners[i].Damage_Player}");

                if (i < losers.Count)
                {
                    player2.Append($"{await Utils.GetItemsBuiltAsync(losers[i])}\n" +
                    $"⚔️KDA: {losers[i].Kills_Player}/{losers[i].Deaths}/{losers[i].Assists}\n" +
                    $"🗡Damage: {losers[i].Damage_Player}");
                }
                else
                {
                    player2.Append($"\u200b");
                }

                godemoji = Utils.FindGodEmoji(gods, winners[i].GodId);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"{godemoji} {Text.HiddenProfileCheck(winners[i].playerName, winners[i].hz_player_name, winners[i].hz_gamer_tag, winners[i].ret_msg)}";
                    x.Value = player1.ToString();
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"**{winners[i].Account_Level} **<:level:529719212017451008>** {losers[i].Account_Level}**\n" +
                    $"{Text.AbbreviationRegions(winners[i].Region)} 🌐 {Text.AbbreviationRegions(losers[i].Region)}";
                    x.Value = $"{Text.GetPortalEmoji(winners[i].playerPortalId)} <:blank:570291209906552848> {Text.GetPortalEmoji(losers[i].playerPortalId)}\n" +
                    $"{(winners[i].PartyId != 0 && foundWinP.Value.Count > 1 ? Text.GetPartyEmoji(sortedParties.IndexOf(foundWinP) + 1) : Text.GetPartyEmoji(0))} <:blank:570291209906552848> " +
                    $"{(losers[i].PartyId != 0 && foundLosP.Value.Count > 1 ? Text.GetPartyEmoji(sortedParties.IndexOf(foundLosP) + 1) : Text.GetPartyEmoji(0))}";
                });
                godemoji = Utils.FindGodEmoji(gods, losers[i].GodId);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"{godemoji} {Text.HiddenProfileCheck(losers[i].playerName, losers[i].hz_player_name, losers[i].hz_gamer_tag, losers[i].ret_msg)}";
                    x.Value = player2.ToString();
                });

                player1.Clear();
                player2.Clear();
            }
            return embed;
        }
        
        public static async Task<Embed> BuildMatchDetailsEmbedAsync(List<MatchDetails.MatchDetailsPlayer> matchDetails)
        {
            var embed = new EmbedBuilder();
            var gods = Constants.GodsHashSet.ToList();

            if (matchDetails.Count == 1 && matchDetails[0].ActivePlayerId == "0")
            {
                embed.WithTitle("Hi-Rez API error:");
                embed.WithDescription(matchDetails[0].ret_msg.ToString());
                return embed.Build();
            }

            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(author =>
            {
                author.WithName($"{Text.GetQueueName(matchDetails[0].match_queue_id, matchDetails[0].Match.ToString(), matchDetails[0].name)} | {matchDetails[0].Minutes} mins");
                author.WithIconUrl(Constants.botIcon);
                author.WithUrl($"https://smite.guru/match/{matchDetails[0].Match}");
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Match ID: {matchDetails[0].Match}";
            });
            if (matchDetails[0].Entry_Datetime.HasValue)
            {
                embed.WithTimestamp(matchDetails[0].Entry_Datetime.Value);
            }

            List<MatchDetails.MatchDetailsPlayer> taskForce1, taskForce2 = new();
            List<KeyValuePair<int, List<MatchDetails.MatchDetailsPlayer>>> parties = new();

            if (matchDetails.Count > 10)
            {
                taskForce1 = matchDetails.Where(x => x.TaskForce == 1).ToList().DistinctBy(x => x.GodId).ToList();
                taskForce2 = matchDetails.Where(x => x.TaskForce == 2).ToList().DistinctBy(x => x.GodId).ToList();
                // Group parties by partyid and orderbydescending of count of players in a party
                parties = matchDetails.GroupBy(x => x.PartyId)
                    .ToDictionary(g => g.Key, g => g.DistinctBy(x => x.GodId)
                    .ToList())
                    .OrderByDescending(x => x.Value.Count)
                    .ToList();
            }
            else
            {
                taskForce1 = matchDetails.Where(x => x.TaskForce == 1).ToList();
                taskForce2 = matchDetails.Where(x => x.TaskForce == 2).ToList();
                // Group parties by partyid and orderbydescending of count of players in a party
                parties = matchDetails.GroupBy(x => x.PartyId)
                    .ToDictionary(g => g.Key, g => g
                    .ToList())
                    .OrderByDescending(x => x.Value.Count)
                    .ToList();
            }
            bool isRanked = Utils.IsRanked(matchDetails[0].match_queue_id.ToString());

            // Left Summary
            string winStatusEmoji = "🏆";
            if (taskForce1[0].Winning_TaskForce == 2)
            {
                winStatusEmoji = "🔴";
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"{winStatusEmoji} **{taskForce1[0].Win_Status}** {winStatusEmoji}";
                x.Value = Text.MatchDetailsTaskForceSummary(taskForce1, isRanked);
            });
            // End Left Summary
            // Bans
            var bans = Text.CheckMatchBans(matchDetails.FirstOrDefault(), gods);
            if (bans.Length != 0)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "🚫 **Bans**";
                    x.Value = bans;
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
            // End Bans
            // Right Summary
            winStatusEmoji = "🏆";
            if (taskForce2[0].Winning_TaskForce == 1)
            {
                winStatusEmoji = "🔴";
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"{winStatusEmoji} **{taskForce2[0].Win_Status}** {winStatusEmoji}";
                x.Value = Text.MatchDetailsTaskForceSummary(taskForce2, isRanked);
            });
            // End Right Summary

            // Players
            bool isMid = false; // if true, we add the mid part between players with the party icon, region and levels
            int count = taskForce1.Count > taskForce2.Count ? taskForce1.Count : taskForce2.Count;
            
            for (int i = 0; i < count; i++)
            {
                // add left, mid and right
                // left TF1
                // looking for party # 
                KeyValuePair<int, List<MatchDetails.MatchDetailsPlayer>> taskFor1Party = new();
                KeyValuePair<int, List<MatchDetails.MatchDetailsPlayer>> taskFor2Party = new();
                if (taskForce1.ElementAtOrDefault(i) != null)
                {
                    taskFor1Party = parties.Find(x => x.Key == taskForce1[i].PartyId);
                }
                if (taskForce2.ElementAtOrDefault(i) != null)
                {
                    taskFor2Party = parties.Find(x => x.Key == taskForce2[i].PartyId);
                }
                
                if (taskForce1.ElementAtOrDefault(i) != null)
                {
                    string value = "";

                    // ranked data
                    if (isRanked)
                    {
                        Tuple<string, string> tuple = null;
                        if (taskForce1[i].match_queue_id == 451 || taskForce1[i].match_queue_id == 504) // Conquest
                        {
                            tuple = Text.GetRankedConquest(taskForce1[i].Conquest_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce1[i].Rank_Stat_Conquest, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce1[i].TaskForce)}W/L: {taskForce1[i].Conquest_Wins}/{taskForce1[i].Conquest_Losses}" +
                            $"<:blank:570291209906552848>{taskForce1[i].Conquest_Points} TP\n";
                        }
                        else if (taskForce1[i].match_queue_id == 450 || taskForce1[i].match_queue_id == 503) // Joust
                        {
                            tuple = Text.GetRankedJoust(taskForce1[i].Joust_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce1[i].Rank_Stat_Joust, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce1[i].TaskForce)}W/L: {taskForce1[i].Joust_Wins}/{taskForce1[i].Joust_Losses}" +
                            $"<:blank:570291209906552848>{taskForce1[i].Joust_Points} TP\n";
                        }
                        else // Duel
                        {
                            tuple = Text.GetRankedDuel(taskForce1[i].Duel_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce1[i].Rank_Stat_Duel, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce1[i].TaskForce)}W/L: {taskForce1[i].Duel_Wins}/{taskForce1[i].Duel_Losses}" +
                            $"<:blank:570291209906552848>{taskForce1[i].Duel_Points} TP\n";
                        }
                    }
                    // end ranked data

                    value += $"{await Utils.GetItemsBuiltAsync(taskForce1[i])}\n" +
                    $"⚔️KDA: {taskForce1[i].Kills_Player}/{taskForce1[i].Deaths}/{taskForce1[i].Assists}\n" +
                    $"🗡Damage: {taskForce1[i].Damage_Player}";

                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = Utils.FindGodEmoji(gods, taskForce1[i].GodId) +
                        $" {Text.HiddenProfileCheck(taskForce1[i].playerName, taskForce1[i].hz_player_name, taskForce1[i].hz_gamer_tag, taskForce1[i].ret_msg)}";
                        x.Value = value;
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
                bool leftIsNull = taskForce1.ElementAtOrDefault(i) != null;
                bool rightIsNull = taskForce2.ElementAtOrDefault(i) != null;
                // mid
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"**{(leftIsNull ? taskForce1[i].Account_Level : "<:blank:570291209906552848>")} " +
                    $"**<:level:529719212017451008>** " +
                    $"{(rightIsNull ? taskForce2[i].Account_Level : "<:blank:570291209906552848>")}**\n" +
                    $"{(leftIsNull ? Text.AbbreviationRegions(taskForce1[i].Region) : "<:blank:570291209906552848>")} 🌐 " +
                    $"{(rightIsNull ? Text.AbbreviationRegions(taskForce2[i].Region) : "<:blank:570291209906552848>")}";
                    x.Value = $"{(taskForce1.ElementAtOrDefault(i) != null ? Text.GetPortalEmoji(taskForce1[i].playerPortalId) : "<:blank:570291209906552848>")} " +
                    $"<:blank:570291209906552848> " +
                    $"{(rightIsNull ? Text.GetPortalEmoji(taskForce2[i].playerPortalId) : "\u200b")}\n" +
                    $"{(leftIsNull && taskForce1[i]?.PartyId != 0 && taskFor1Party.Value != null && taskFor1Party.Value.Count > 1 ? Text.GetPartyEmoji(parties.IndexOf(taskFor1Party) + 1) : Text.GetPartyEmoji(0))} " +
                    $"<:blank:570291209906552848> " +
                    $"{(rightIsNull && taskForce2[i]?.PartyId != 0 && taskFor2Party.Value != null && taskFor2Party.Value.Count > 1 ? Text.GetPartyEmoji(parties.IndexOf(taskFor2Party) + 1) : Text.GetPartyEmoji(0))}";
                });
                // right TF2
                if (taskForce2.ElementAtOrDefault(i) != null)
                {
                    string value = "";

                    // ranked data
                    if (isRanked)
                    {
                        Tuple<string, string> tuple = null;
                        if (taskForce2[i].match_queue_id == 451 || taskForce2[i].match_queue_id == 504) // Conquest
                        {
                            tuple = Text.GetRankedConquest(taskForce2[i].Conquest_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce2[i].Rank_Stat_Conquest, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce2[i].TaskForce)}W/L: {taskForce2[i].Conquest_Wins}/{taskForce2[i].Conquest_Losses}" +
                            $"<:blank:570291209906552848>{taskForce2[i].Conquest_Points} TP\n";
                        }
                        else if (taskForce2[i].match_queue_id == 450 || taskForce2[i].match_queue_id == 503) // Joust
                        {
                            tuple = Text.GetRankedJoust(taskForce2[i].Joust_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce2[i].Rank_Stat_Joust, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce2[i].TaskForce)}W/L: {taskForce2[i].Joust_Wins}/{taskForce2[i].Joust_Losses}" +
                            $"<:blank:570291209906552848>{taskForce2[i].Joust_Points} TP\n";
                        }
                        else // Duel
                        {
                            tuple = Text.GetRankedDuel(taskForce2[i].Duel_Tier);
                            value = $"{tuple.Item2}{tuple.Item1} " +
                            $"[{Math.Round(taskForce2[i].Rank_Stat_Duel, 0)}]\n" +
                            $"{Text.SideEmoji(taskForce2[i].TaskForce)}W/L: {taskForce2[i].Duel_Wins}/{taskForce2[i].Duel_Losses}" +
                            $"<:blank:570291209906552848>{taskForce2[i].Duel_Points} TP\n";
                        }
                    }
                    // end ranked data

                    value += $"{await Utils.GetItemsBuiltAsync(taskForce2[i])}\n" +
                    $"⚔️KDA: {taskForce2[i].Kills_Player}/{taskForce2[i].Deaths}/{taskForce2[i].Assists}\n" +
                    $"🗡Damage: {taskForce2[i].Damage_Player}";

                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = Utils.FindGodEmoji(gods, taskForce2[i].GodId) +
                        $" {Text.HiddenProfileCheck(taskForce2[i].playerName, taskForce2[i].hz_player_name, taskForce2[i].hz_gamer_tag, taskForce2[i].ret_msg)}";
                        x.Value = value;
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
            }
            // END Players

            return await Task.FromResult(embed.Build());
        }
        
        public static async Task<Embed> BuildMatchHistoryEmbedAsync(List<MatchHistoryModel> matchHistory, List<Player.PlayerStats> getPlayer)
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
                x.IconUrl = Text.GetPortalIconLinksByPortalName(getPlayer.FirstOrDefault()?.Platform);
            });
            embed.WithColor(Constants.DefaultBlueColor);
            string godemoji = "";
            int i = 0;
            var gods = Constants.GodsHashSet.ToList();
            foreach (var match in matchHistory)
            {
                if (i != 6)
                {
                    godemoji = Utils.FindGodEmoji(gods, matchHistory[i].GodId);
                    embed.AddField(async x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{godemoji} `{matchHistory[i].Win_Status}` {Text.GetQueueName(matchHistory[i].Match_Queue_Id, matchHistory[i].Match.ToString(), matchHistory[i].Queue)} - " +
                        $"{matchHistory[i].Minutes} min - {Text.RelativeTimestamp(Convert.ToDateTime(matchHistory[i].Match_Time, CultureInfo.InvariantCulture))} " +
                        $"`[{matchHistory[i].Match}]`";
                        x.Value = $":crossed_swords:**KDA:** {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists} | " +
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
        
        public static async Task<Embed> BuildWorshipersEmbedAsync(List<GodRanks> ranks, Player.PlayerStats player)
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
            else
            {
                embed.WithDescription($"**{ranks.Count(x=> x.Rank == 10)}** diamond, " +
                                        $"**{(ranks.Any(x => x.Rank < 10 && x.Rank >= 5) ? ranks.Count(x => x.Rank < 10 && x.Rank >= 5) : "0")}** legendary and " +
                                        $"**{ranks.Count(x => x.Rank < 5 && x.Rank >= 1)}** golden.");
            }

            // Add the rest of the gods
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
        
        public static async Task<Embed> BuildWinRatesEmbedAsync(List<GodRanks> ranks, Player.PlayerStats player)
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
            embed.WithFooter(x =>
            {
                x.Text = "God [Win Rate]";
            });
            int count = 0;
            var gods = Constants.GodsHashSet.ToList();
            foreach (var god in ranks)
            {
                god.WinRate = (double)god.Wins * 100 / (god.Wins + god.Losses);
            }
            var sortedRanksByWinRate = ranks.OrderByDescending(x=>x.WinRate).ToList();
            for (int i = 0; i < sortedRanksByWinRate.Count; i++)
            {
                string godEmoji = Utils.FindGodEmoji(gods, Int32.Parse(sortedRanksByWinRate[i]._id.god_id));
                if (godEmoji.Length + 
                    sortedRanksByWinRate[i].god.Length + 
                    Math.Round(sortedRanksByWinRate[i].WinRate, 2).ToString().Length + 
                    sb.Length + embed.Length + 13 > 6000)
                {
                    break;
                }
                sb.Append($"{godEmoji} {sortedRanksByWinRate[i].god} [**{Math.Round(sortedRanksByWinRate[i].WinRate, 2)}%**]\n");
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

            if (sb.Length != 0 && embed.Length + sb.Length <= 6000)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "\u200b";
                    x.Value = sb.ToString();
                });
            }
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildQueueStatsEmbedAsync(List<QueueStats> queueStats, Player.PlayerStats player, string queueID)
        {
            var sb = new StringBuilder();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.Name = $"{player.hz_player_name ?? player.hz_gamer_tag}'s Queue Stats";
                x.IconUrl = Text.GetPortalIconLinksByPortalName(player.Platform);
                x.Url = $"https://smite.guru/profile/{player.ActivePlayerId}";
            });
            embed.WithFooter(x =>
            {
                x.Text = Text.GetRandomTip();
            });
            embed.WithTitle($"{Text.GetQueueEmoji(int.Parse(queueID))} {queueStats[0].Queue}");

            // Get the roles, kda and winratio
            foreach (var god in queueStats)
            {
                if (god.GodId == 2071) // Zeus [Redesign]
                {
                    var realZus = queueStats.Find(x => x.GodId == 1672);
                    if (realZus != null)
                    {
                        realZus.WinRatio = ((realZus.WinRatio) + ((double)god.Wins * 100 / (god.Wins + god.Losses))) / 2;
                        realZus.KDA = (realZus.KDA + Text.CalculateKDA(god.Kills, god.Deaths, god.Assists)) / 2;
                    }
                }
                else
                {
                    god.GodRole = Constants.GodsHashSet.Where(x => x.id == god.GodId).FirstOrDefault().Roles;
                    god.WinRatio = (double)god.Wins * 100 / (god.Wins + god.Losses);
                    god.KDA = Text.CalculateKDA(god.Kills, god.Deaths, god.Assists);
                }
            }
            if (queueStats.Any(x => x.GodId == 2071))
            {
                var fakezeus = queueStats.Find(x => x.GodId == 2071);
                queueStats.Remove(fakezeus);
            }

            // top role
            var roleStatistics = queueStats
                .GroupBy(qs => qs.GodRole)
                .Select(group => new RoleStatistics
                {
                    Role = group.Key,
                    MatchCount = group.Sum(qs => qs.Matches),
                    AverageKDA = group.Average(qs => qs.KDA),
                    AverageWinRatio = group.Average(qs => qs.WinRatio)
                })
                .OrderByDescending(stats => stats.MatchCount)
                .Take(5)
                .ToList();

            int totalMatches = roleStatistics.Sum(x => x.MatchCount);

            sb.AppendLine($"🔹Total Kills / Deaths / Assists: {queueStats.Select(x => x.Kills).Sum()} / {queueStats.Select(x => x.Deaths).Sum()} / " +
                $"{queueStats.Select(x => x.Assists).Sum()}\n");
            foreach (var role in roleStatistics)
            {
                sb.AppendLine($"{Text.GetGodRoleEmoji(role.Role)} **{role.MatchCount}** game{(role.MatchCount > 1 ? "s" : "")} " +
                    $"[{Math.Round(((double)role.MatchCount / (double)totalMatches) * 100, 2)}%], " +
                    $"Wins: {Math.Round(role.AverageWinRatio, 2)}%, " +
                    $"KDA: {Math.Round(role.AverageKDA, 2)}");
            }
            embed.WithDescription(sb.ToString());
            sb.Clear();

            // Top
            for (int i = 0; i < (queueStats.Count < 10 ? queueStats.Count : 10); i++)
            {
                sb.AppendLine($"{Constants.GodsHashSet.First(x => x.Name == queueStats[i].God)?.Emoji} **[{queueStats[i].Matches}]** " +
                    $"Wins: {Math.Round(queueStats[i].WinRatio, 2)}%, " +
                    $"KDA: {Math.Round(queueStats[i].KDA, 2)}");
            }

            embed.AddField(x =>
            {
                x.Name = "<:Gods:567146088985919498>Most Played Gods";
                x.Value = sb.ToString();
                x.IsInline = true;
            });

            // Least
            var last = queueStats.TakeLast(10).ToList();
            sb.Clear();
            for (int i = 0; i < (last.Count < 10 ? last.Count : 10); i++)
            {
                string emoji = "";
                if (last[i].GodId == 2071) // Zeus [Redesign]
                {
                    emoji = Constants.GodsHashSet.First(x => x.Name == "Zeus")?.Emoji;
                }
                sb.AppendLine($"{(emoji == "" ? Constants.GodsHashSet.First(x => x.Name == last[i].God)?.Emoji : emoji)} **[{last[i].Matches}]** " +
                    $"Wins: {Math.Round(last[i].WinRatio, 2)}%, " +
                    $"KDA: {Math.Round(last[i].KDA, 2)}");
            }

            embed.AddField(x =>
            {
                x.Name = "<:Gods:567146088985919498>Least Played Gods";
                x.Value = sb.ToString();
                x.IsInline = true;
            });

            // Highest KDA

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildRandomAssaultTeamsEmbedAsync()
        {
            var embed = new EmbedBuilder();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            bool hasHealer = false;
            bool firstHasHealer = false;
            string healerTier = "0";
            var healers = MongoConnection.GetBotSettings().Healers;
            var gods = Constants.GodsHashSet.ToList();

            // First team
            for (int i = 0; i < 5; i++)
            {
                var current = gods[rnd.Next(gods.Count)];
                while (hasHealer && healers.ContainsKey(current.id.ToString()))
                {
                    current = gods[rnd.Next(gods.Count)];
                }
                if (healers.ContainsKey(current.id.ToString()))
                {
                    hasHealer = true;
                    firstHasHealer = true;
                    healerTier = healers[current.id.ToString()];
                }

                sb1.AppendLine($"{current.Emoji} {current.Name}");
                gods.Remove(current);
            }

            // Second team
            gods = Constants.GodsHashSet.ToList();
            hasHealer = false;
            var healersDictTeam2 = new Dictionary<string, string>();
            // if first team has a healer and we havent got a healer in the second
            if (firstHasHealer && !hasHealer)
            {
                foreach (var z in healers)
                {
                    if (z.Value == healerTier)
                    {
                        healersDictTeam2.Add(z.Key, z.Value);
                    }
                }
            }
            // get the gods
            for (int i = 0; i < 5; i++)
            {
                var current = gods[rnd.Next(gods.Count)];

                // if team already has a healer, reroll until we don't
                while (hasHealer && healers.ContainsKey(current.id.ToString()))
                {
                    current = gods[rnd.Next(gods.Count)];
                }

                // if first team has a healer & the current random god is healer, mark that second team has healer
                if (healers.ContainsKey(current.id.ToString()) && firstHasHealer && !hasHealer)
                {
                    if (!healersDictTeam2.ContainsKey(current.id.ToString()))
                    {
                        while (!healersDictTeam2.ContainsKey(current.id.ToString()))
                        {
                            current = gods[rnd.Next(gods.Count)];
                        }
                    }
                    hasHealer = true;
                }
                // if current god is healer & first team doesn't have a healer, reroll until we get a non-healer god
                else if (healers.ContainsKey(current.id.ToString()) && !firstHasHealer)
                {
                    while (healers.ContainsKey(current.id.ToString()))
                    {
                        current = gods[rnd.Next(gods.Count)];
                    }
                }

                // if first team has healer & we don't have healer in this team yet by the 5th random god, we force it to get a healer
                while (firstHasHealer && !hasHealer && i == 4 && !healersDictTeam2.ContainsKey(current.id.ToString()))
                {
                    current = gods[rnd.Next(gods.Count)];
                }

                sb2.AppendLine($"{current.Emoji} {current.Name}");
                gods.Remove(current);
            }

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Team 1";
                x.Value = sb1.ToString();
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Team 2";
                x.Value = sb2.ToString();
            });
            embed.WithAuthor(x =>
            {
                x.Name = "Two Random Assault Teams";
                x.IconUrl = Constants.botIcon;
            });
            embed.WithColor(Constants.DefaultBlueColor);

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildPatchNotesEmbedAsync(WebAPIPostModel patchPost, string description, string imageURL, string slug)
            => await BuildPatchNotesEmbedAsync(patchPost.title, description, imageURL, slug);

        public static async Task<Embed> BuildPatchNotesEmbedAsync(string title, string description, string imageURL, string slug)
        {
            var embed = new EmbedBuilder();
            embed.WithDescription(description);
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle(title);
            embed.WithUrl($"https://www.smitegame.com/news/{slug}");
            if (imageURL != null)
            {
                embed.WithImageUrl(imageURL);
            }
            embed.WithAuthor(x =>
            {
                x.Name = "SMITE Update Notes";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smitegame.com/news/";
            });
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildEsportsScheduleEmbedAsync(SPLSchedule schedule, GoogleCalendarModel calendar)
        {
            var unixToday = Text.DateTimeToUnix(DateTime.UtcNow.AddDays(-5));
            StringBuilder sb = new();
            var embed = new EmbedBuilder();
            embed.WithDescription("<:Twitch:579125715874742280> [Twitch.Tv/SmiteGame](https://www.twitch.tv/smitegame)");
            embed.WithColor(Constants.SPLColor);
            embed.WithAuthor(x =>
            {
                x.Name = $"Smite Pro League Schedule - {schedule.phases[^1].title} [{schedule.date_range}]";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smiteproleague.com/schedule";
            });
            embed.WithFooter(x =>
            {
                x.Text = "This command is still in beta. It may break at any point or show outdated or wrong data.";
            });
            foreach (var item in schedule.schedule)
            {
                if (item.date > unixToday)
                {
                    if (embed.Fields.Count == 6)
                    {
                        break;
                    }
                    foreach (var match in item.matches)
                    {
                        if (calendar.items.Any(x => x.start.dateTime.Day == Text.UnixToDateTime(match.time).Day && x.summary.ToLowerInvariant().Contains("scc")))
                        {
                            var calendMatches = calendar.items.Find(x => x.start.dateTime.Day == Text.UnixToDateTime(match.time).Day && x.summary.ToLowerInvariant().Contains("scc"));
                            if (calendMatches != null && calendMatches.summary != null && calendMatches.summary.Length != 0)
                            {
                                if (calendMatches.start.dateTime.Hour > Text.UnixToDateTime(match.time).Hour)
                                {
                                    sb.AppendLine($"{calendMatches.summary.Replace("VS", "`VS`")} " +
                                        $"{Text.ShortTimeTimestamp(calendMatches.start.dateTime.AddHours(-3))}");
                                }
                            }
                        }
                        sb.AppendLine($"{Text.GetEsportsTeamEmoji(match.team_1_shortname)}" +
                            $"{match.team_1_name} `VS` {match.team_2_name} {Text.GetEsportsTeamEmoji(match.team_2_shortname)} " +
                            $"{(match.playlist_url != null && match.playlist_url.Length > 1 ? $"[VOD]({match.playlist_url})" : $"{Text.ShortTimeTimestamp(Text.UnixToDateTime(match.time))}")}");
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = Text.LongDateTimestamp(Text.UnixToDateTime(item.date));
                        x.Value = sb.ToString();
                    });
                    sb.Clear();
                }
            }

            if (embed.Fields.Count == 0)
            {
                embed.WithDescription("No recent matches found.");
            }
            return await Task.FromResult(embed.Build());
        }
        
        public static async Task<Embed> BuildEsportsStandingsEmbedAsync(List<SPLStandings> standings)
        {
            var unixToday = Text.DateTimeToUnix(DateTime.UtcNow.AddDays(-5));
            StringBuilder sb = new();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.SPLColor);
            embed.WithAuthor(x =>
            {
                x.Name = "Smite Pro League Standings";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smiteproleague.com/standings";
            });
            embed.WithFooter(x =>
            {
                x.Text = "This command is still in beta. It may break at any point or show outdated or wrong data.";
            });
            for (int i = 0; i < standings.Count; i++)
            {
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"{i + 1}. {Text.GetEsportsTeamEmoji(standings[i].team_shortname)} {standings[i].team_name}";
                    x.Value = $"**{standings[i].matches}** matches, **{standings[i].wins}** wins, **{standings[i].losses}** losses, **{standings[i].win_percent}** win ratio";
                });
            }
            if (embed.Fields?.Count == 0)
            {
                embed.WithDescription("No standings found. Try again later.");
            }
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildEsportsStatsEmbedAsync(SPLStats stats)
        {
            StringBuilder sb = new();
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.SPLColor);
            embed.WithAuthor(x =>
            {
                x.Name = "Smite Pro League Statistics";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smiteproleague.com/stats";
            });
            embed.WithFooter(x =>
            {
                x.Text = "This command is still in beta. It may break at any point or show outdated or wrong data.";
            });

            // POTW
            for (int i = 0; i < stats.potw.Length; i++)
            {
                sb.AppendLine($"{Text.GetRoleIcon(stats.potw[i].role)} [{stats.potw[i].name}](https://www.smiteproleague.com/players/{stats.potw[i].name.ToLowerInvariant()})");
            }

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = $"Players of the Week";
                x.Value = sb.ToString();
            });

            sb.Clear();
            //Trending KDA
            sb.AppendLine($"{Text.GetRoleIcon(stats.trending[0].role)} [{stats.trending[0].name}](https://www.smiteproleague.com/players/{stats.trending[0].name.ToLowerInvariant()})\n" +
                $"{Text.GetEsportsTeamEmoji(stats.trending[0].short_name)} [[{stats.trending[0].team}](https://www.smiteproleague.com/teams/{stats.trending[0].short_name.ToLowerInvariant()})]\n" +
                $"⚔ KDA [{stats.trending[0].stats[0].value}]\n" +
                $"🗡 Kills: {stats.trending[0].stats[1].value}\n" +
                $"☠ Deaths: {stats.trending[0].stats[2].value}\n" +
                $"🤝 Assists: {stats.trending[0].stats[3].value}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Highest Player KDA";
                x.Value = sb.ToString();
            });

            sb.Clear();
            // Trending Gold
            sb.AppendLine($"{Text.GetRoleIcon(stats.trending[1].role)} [{stats.trending[1].name}](https://www.smiteproleague.com/players/{stats.trending[1].name.ToLowerInvariant()})\n" +
                $"{Text.GetEsportsTeamEmoji(stats.trending[1].short_name)}[[{stats.trending[1].team}](https://www.smiteproleague.com/teams/{stats.trending[1].short_name.ToLowerInvariant()})]\n" +
                $"<:coins:590942235474919464> Gold: {stats.trending[1].stats[0].value}\n" +
                $"⌚ GPM: {stats.trending[1].stats[1].value}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Highest Player Gold";
                x.Value = sb.ToString();
            });

            sb.Clear();
            // Trending Most Damage
            sb.AppendLine($"{Text.GetRoleIcon(stats.trending[2].role)} [{stats.trending[2].name}](https://www.smiteproleague.com/players/{stats.trending[2].name.ToLowerInvariant()})\n" +
                $"{Text.GetEsportsTeamEmoji(stats.trending[2].short_name)} [[{stats.trending[2].team}](https://www.smiteproleague.com/teams/{stats.trending[2].short_name.ToLowerInvariant()})]\n" +
                $"🩸 Damage: {stats.trending[2].stats[0].value}\n" +
                $"🗡 Kills: {stats.trending[2].stats[1].value}\n" +
                $"🤝 Assists: {stats.trending[2].stats[2].value}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Highest Player Damage";
                x.Value = sb.ToString();
            });

            sb.Clear();
            //KDA Team
            sb.AppendLine($"{Text.GetEsportsTeamEmoji(stats.team_leaders[0].short_name)} " +
                $"[{stats.team_leaders[0].team}](https://www.smiteproleague.com/teams/{stats.team_leaders[0].short_name.ToLowerInvariant()})\n" +
                $"⚔ K/D/A: {stats.team_leaders[0].stats[1].value} [{stats.team_leaders[0].stats[0].value}]");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Highest Team KDA";
                x.Value = sb.ToString();
            });

            sb.Clear();
            //Gold Team
            sb.AppendLine($"{Text.GetEsportsTeamEmoji(stats.team_leaders[1].short_name)} " +
                $"[{stats.team_leaders[1].team}](https://www.smiteproleague.com/teams/{stats.team_leaders[1].short_name.ToLowerInvariant()})\n" +
                $"<:coins:590942235474919464> Gold: {stats.team_leaders[1].stats[0].value}\n" +
                $"⌚ GPM: {stats.team_leaders[1].stats[1].value}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Highest Team Gold";
                x.Value = sb.ToString();
            });

            sb.Clear();
            //Most Objectives
            sb.AppendLine($"{Text.GetEsportsTeamEmoji(stats.team_leaders[2].short_name)} " +
                $"[{stats.team_leaders[2].team}](https://www.smiteproleague.com/teams/{stats.team_leaders[2].short_name.ToLowerInvariant()})\n" +
                $"👱‍♀️ {stats.team_leaders[2].stats[0].title}: {stats.team_leaders[2].stats[0].value}\n" +
                $"🔥 {stats.team_leaders[2].stats[1].title}: {stats.team_leaders[2].stats[1].value}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Most Objectives by Team";
                x.Value = sb.ToString();
            });

            if (embed.Fields?.Count == 0)
            {
                embed.WithDescription("No stats found. Try again later.");
            }
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildPlayerLookupEmbedAsync(PlayerSpecial playerSpecial, List<Player.PlayerStats> getplayer, List<Player.PlayerStatus> getplayerstatus)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.IconUrl = getplayer[0].Avatar_URL == "" ? Constants.botIcon : getplayer[0].Avatar_URL;
                x.Name = getplayer[0].Name;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Smite Account";
                x.Value = $"🆔 ID: {getplayer[0].ActivePlayerId}\n" +
                $"<:level:529719212017451008> Level: {getplayer[0].Level}\n" +
                $"👀 Last Login: {(getplayer[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(getplayer[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                $"🎮 Account Created: {(getplayer[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(getplayer[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                $"🔹 Platform: {getplayer[0].Platform}\n" +
                $"🔹 Personal Status Message: {getplayerstatus[0].personal_status_message}";
            });
            if (playerSpecial != null)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Discord ID";
                    x.Value = playerSpecial.discordID;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Streamer?";
                    x.Value = $"{playerSpecial.streamer_bool}\n{playerSpecial.streamer_link}";
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Pro?";
                    x.Value = playerSpecial.pro_bool;
                });
                var badge = await MongoConnection.GetBadgeAsync(playerSpecial.special);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Special Badge";
                    x.Value = $"```\n{playerSpecial.special}```{(badge != null ? $"{badge.Emote} {badge.Title}" : "**The badge is not set in the database.**")}";
                });
            }
            return await Task.FromResult(embed.Build());
        }
        
        public static async Task<Embed> BuildPlayerLookupEmbedAsync(PlayerSpecial playerSpecial)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            if (playerSpecial != null)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "ID";
                    x.Value = playerSpecial._id;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Discord ID";
                    x.Value = playerSpecial.discordID;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Streamer?";
                    x.Value = $"{playerSpecial.streamer_bool}\n{playerSpecial.streamer_link}";
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Pro?";
                    x.Value = playerSpecial.pro_bool;
                });
                var badge = await MongoConnection.GetBadgeAsync(playerSpecial.special);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Special Badge";
                    x.Value = $"```\n{playerSpecial.special}```{(badge != null ? $"{badge.Emote} {badge.Title}" : "**The badge is not set in the database.**")}";
                });
            }
            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildDefaultFeedsPage(GuildSettingsModel guildSettings, bool hasTooManyChannels)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("️📃 Feeds");
            embed.WithThumbnailUrl("https://i.imgur.com/tba1Sjh.png");
            embed.WithColor(Constants.FeedsColor);
            embed.WithDescription("**What are \"feeds\"?**\n> *Feeds is a name for the various content that you can \"subscribe\" to to get " +
                "notified about.*\n" +
                $"{(hasTooManyChannels ? "__If you don't see the channel you want to set, run the command in that channel and it will be the first option.__" : "")}");
            var serverStatus = guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.SMITEServerStatus);
            var smite2News = guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.SMITE2News);

            embed.AddField($"🔹 {Text.SplitCamelCase(GuildSettingsModel.FeedType.SMITEServerStatus.ToString())}", 
                (serverStatus == null || serverStatus?.ChannelID == 0 ? "Channel not set" : $"Set to <#{serverStatus.ChannelID}>"), true);
            embed.AddField($"🔹 {Text.SplitCamelCase(GuildSettingsModel.FeedType.SMITE2News.ToString())}", 
                (smite2News == null || smite2News?.ChannelID == 0 ? "Channel not set" : $"Set to <#{smite2News.ChannelID}>"), true);

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildMainGodPageEmbedAsync(Gods.God god)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godIcon_URL);
                author.WithUrl($"https://www.smitegame.com/gods/{Text.URLifyGodName(god.Name)}");
            });
            embed.WithTitle(god.Title);
            embed.WithThumbnailUrl(god.godIcon_URL);
            embed.WithDescription($"{Text.GetPantheonEmoji(god.Pantheon.ToLowerInvariant())} **Pantheon:** {god.Pantheon}\n" +
                $"{(god.Type.Contains("Magical") ? Text.GetGodTypeEmoji("magical") : Text.GetGodTypeEmoji("physical"))} **Type:** {god.Type}\n" +
                $"{Text.GetGodRoleEmoji(god.Roles)} **Role:** {god.Roles}\n" +
                $"{(god.Pros != null || god.Pros != " " || god.Pros != "" ? $"🔹 **Pros:** {god.Pros}\n" : "")}");
            if (god.DomColor != 0)
            {
                embed.WithColor(new Color((uint)god.DomColor));
            }

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Stats";
                x.Value = $"<:Health:961778316577210418> **Health:** {god.Health} (+{god.HealthPerLevel})\n" +
                          $"<:Mana:961778316195553292> **Mana:** {god.Mana} (+{god.ManaPerLevel})\n" +
                          $"<:Speed:961778316388495400> **Speed:** {god.Speed}\n" +
                          $"<:Physical_Protection:961778316430409798> **Physical Protection:** {god.PhysicalProtection} (+{god.PhysicalProtectionPerLevel})\n" +
                          $"<:Magical_Protection:961778316120035379> **Magical Protection:** {god.MagicProtection} (+{god.MagicProtectionPerLevel})\n" +
                          $"<:HP5:961779566580490270> **HP5:** {god.HealthPerFive} (+{god.HP5PerLevel})\n" +
                          $"<:MP5:961779566567911424> **MP5:** {god.ManaPerFive} (+{god.MP5PerLevel})";
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "\u200b";
                x.Value = $"<:Basic_Attack_Damage:961778316354928650> **Basic Attack {god.basicAttack.itemDescription.menuitems[0].description}** {god.basicAttack.itemDescription.menuitems[0].value}\n" +
                          $"<:Basic_Attack_Damage:961778316354928650> **{god.basicAttack.itemDescription.menuitems[1]?.description}** {god.basicAttack.itemDescription.menuitems[1].value}\n" +
                          $"<:Attack_Speed:961778316300390450> **Attack Speed:** {god.AttackSpeed} (+{god.AttackSpeedPerLevel})\n" +
                          $"**{(god.Type.Contains("Physical") ? "<:Physical_Power:961778316627550228> Physical" : "<:Magical_Power:961778316451409970> Magical")} Power:** " +
                          $"{(god.Type.Contains("Physical") ? god.PhysicalPower : god.MagicalPower)}" +
                          $"(+{(god.Type.Contains("Physical") ? god.PhysicalPowerPerLevel : god.MagicalPowerPerLevel)})";
            });

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildGodLorePageEmbedAsync(Gods.God god)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godIcon_URL);
                author.WithUrl($"https://www.smitegame.com/gods/{Text.URLifyGodName(god.Name)}");
            });
            embed.WithTitle($"Lore of {god.Name}, {god.Title}");
            embed.WithThumbnailUrl(god.godIcon_URL);
            if (god.Lore.Length < 5900)
            {
                embed.WithDescription(god.Lore.Replace("\\n", "\n"));
            }
            else
            {
                embed.WithDescription("Oops, the lore is too long to display here.");
                await Reporter.SendErrorAsync("Lore for " + god.Name + " is too long to display in a single embed.");
            }
            if (god.DomColor != 0)
            {
                embed.WithColor(new Color((uint)god.DomColor));
            }

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildItemInfoEmbedAsync(GetItems.Item item)
        {
            var embed = new EmbedBuilder();

            string secondaryDesc = Text.ReformatSecondaryItemDescription(item.ItemDescription.SecondaryDescription);

            embed.WithAuthor(x =>
            {
                x.Name = item.DeviceName;
                x.IconUrl = item.itemIcon_URL;
            });
            embed.WithColor(new Color((uint)item.DomColor));
            embed.WithThumbnailUrl(item.itemIcon_URL);
            StringBuilder itemBenefits = new();
            foreach (var benefit in item.ItemDescription.Menuitems)
            {
                itemBenefits.AppendLine($"{benefit.Value} {benefit.Description}");
            }
            embed.WithTitle(itemBenefits.ToString());
            embed.WithDescription($"{(item.StartingItem ? "**Starting Item**" : "")}" +
                $"{(item.ItemDescription?.Description?.Length != 0 ? $"\n{item.ItemDescription.Description}" : "")}\n\n{secondaryDesc}");

            // calculating price
            var firstChildPrice = new List<GetItems.Item>();
            var secondChildPrice = new List<GetItems.Item>();
            var thirdChildPrice = new List<GetItems.Item>();
            int totalPrice = 0;

            if (item.ChildItemId != 0)
            {
                firstChildPrice = await MongoConnection.GetSpecificItemByIDAsync(item.ChildItemId);//tier 3
                if (firstChildPrice != null && firstChildPrice.Count != 0)
                {
                    if (firstChildPrice[0].ChildItemId != 0)
                    {
                        secondChildPrice = await MongoConnection.GetSpecificItemByIDAsync(firstChildPrice[0].ChildItemId);// tier 2
                        totalPrice = secondChildPrice[0].Price;

                        if (secondChildPrice[0].ChildItemId != 0)
                        {
                            thirdChildPrice = await MongoConnection.GetSpecificItemByIDAsync(secondChildPrice[0].ChildItemId);// tier 1
                            totalPrice += thirdChildPrice[0].Price;
                        }
                    }
                    totalPrice = totalPrice + item.Price + firstChildPrice[0].Price;
                }
            }
            else
            {
                totalPrice = item.Price;
            }

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Total Price (Price)";
                x.Value = $"<:coins:590942235474919464>{totalPrice} ({item.Price})";
            });
            if (!item.StartingItem || item.Type != "Consumable")
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Tier";
                    x.Value = item.ItemTier;
                });
            }
            return await Task.FromResult(embed.Build());
        }

        // SMITE 2

        public static async Task<Embed> BuildMainSMITE2GodPageEmbedAsync(Gods.God god)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godHeader_URL);
                author.WithUrl($"https://www.smite2.com/gods/{god.ret_msg}"); // ret msg is slug
            });
            embed.WithTitle(god.Title);
            embed.WithThumbnailUrl("https://i.imgur.com/TR9dSLn.png");
            embed.WithDescription($"{Text.GetSMITE2PantheonEmoji(god.Pantheon)} **Pantheon:** {god.Pantheon}\n" +
                $"{Text.GetSMITE2RoleIcon(god.Roles)} **Role{(god.Roles.Split(" ").Count() > 1 ? "s" : "")}:** {god.Roles}");
            embed.WithImageUrl(god.godHeader_URL);
            if (god.DomColor != 0)
            {
                embed.WithColor(new Color((uint)god.DomColor));
            }
            //embed.AddField(x =>
            //{
            //    x.IsInline = true;
            //    x.Name = "Stats";
            //    x.Value = $"<:Health:961778316577210418> **Health:** {god.Health} (+{god.HealthPerLevel})\n" +
            //              $"<:Mana:961778316195553292> **Mana:** {god.Mana} (+{god.ManaPerLevel})\n" +
            //              $"<:Speed:961778316388495400> **Speed:** {god.Speed}\n" +
            //              $"<:Physical_Protection:961778316430409798> **Physical Protection:** {god.PhysicalProtection} (+{god.PhysicalProtectionPerLevel})\n" +
            //              $"<:Magical_Protection:961778316120035379> **Magical Protection:** {god.MagicProtection} (+{god.MagicProtectionPerLevel})\n" +
            //              $"<:HP5:961779566580490270> **HP5:** {god.HealthPerFive} (+{god.HP5PerLevel})\n" +
            //              $"<:MP5:961779566567911424> **MP5:** {god.ManaPerFive} (+{god.MP5PerLevel})";
            //});
            //embed.AddField(x =>
            //{
            //    x.IsInline = true;
            //    x.Name = "\u200b";
            //    x.Value = $"<:Basic_Attack_Damage:961778316354928650> **Basic Attack {god.basicAttack.itemDescription.menuitems[0].description}** {god.basicAttack.itemDescription.menuitems[0].value}\n" +
            //              $"<:Basic_Attack_Damage:961778316354928650> **{god.basicAttack.itemDescription.menuitems[1]?.description}** {god.basicAttack.itemDescription.menuitems[1].value}\n" +
            //              $"<:Attack_Speed:961778316300390450> **Attack Speed:** {god.AttackSpeed} (+{god.AttackSpeedPerLevel})\n" +
            //              $"**{(god.Type.Contains("Physical") ? "<:Physical_Power:961778316627550228> Physical" : "<:Magical_Power:961778316451409970> Magical")} Power:** " +
            //              $"{(god.Type.Contains("Physical") ? god.PhysicalPower : god.MagicalPower)}" +
            //              $"(+{(god.Type.Contains("Physical") ? god.PhysicalPowerPerLevel : god.MagicalPowerPerLevel)})";
            //});

            return await Task.FromResult(embed.Build());
        }

        public static async Task<Embed> BuildGodLorePage2EmbedAsync(Gods.God god)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godHeader_URL);
                author.WithUrl($"https://www.smite2.com/gods/{god.ret_msg}");
            });
            embed.WithTitle($"Lore of {god.Name}, {god.Title}");
            embed.WithThumbnailUrl(god.godHeader_URL);
            if (god.Lore.Length < 5900)
            {
                embed.WithDescription(
                    god.Lore
                    .Replace("\\n", "\n")
                    .Replace("<p>", "\n")
                    .Replace("</p>", ""));
            }
            else
            {
                embed.WithDescription("Oops, the lore is too long to display here.");
                await Reporter.SendErrorAsync("Lore for " + god.Name + " is too long to display in a single embed.");
            }
            if (god.DomColor != 0)
            {
                embed.WithColor(new Color((uint)god.DomColor));
            }

            return await Task.FromResult(embed.Build());
        }
    }
}
