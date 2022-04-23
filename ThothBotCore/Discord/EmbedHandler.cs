using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id)}**, " +
                                    $"playing as {matchPlayerDetails[s].GodName}");
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
                                embed.WithDescription($":eyes: {playerStatus[0].status_string}: **{Text.GetQueueName(playerStatus[0].match_queue_id)}**, " +
                                    $"playing as {matchPlayerDetails[s].GodName}");
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
        public static Task<Embed> BuildNotLinkedEmbedAsync()
        {
            var embed = new EmbedBuilder
            {
                Description = "This command works without a provided `PlayerName` only if you've linked your Discord and SMITE accounts in Thoth's database.\nTo link, use `/link`.",
                Color = Constants.FeedbackColor
            };
            
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
                    player1.Append($":video_game:Account Created: \n{team1force}{(team1[i].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team1[i].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
                    player2.Append($":video_game:Account Created: \n{team2force}{(team2[i].playerCreated != "" ? Text.InvariantDate(DateTime.Parse(team2[i].playerCreated, CultureInfo.InvariantCulture)) : "n/a")}");
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
                author.WithName($"{Text.GetQueueName(matchdetailsList[0].match_queue_id, matchdetailsList[0].name)} | {matchdetailsList[0].Minutes} mins");
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
                    await Reporter.SendError("**Yo, MatchDetails endpoint was probably changed, or the API is going wild..**");
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
                    godemoji = Utils.FindGodEmoji(gods, matchHistory[i].GodId);
                    embed.AddField(async x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{godemoji} `{matchHistory[i].Win_Status}` {Text.GetQueueName(matchHistory[i].Match_Queue_Id, matchHistory[i].Queue)} - " +
                        $"{matchHistory[i].Minutes} min - {Text.RelativeTimestamp(Convert.ToDateTime(matchHistory[i].Match_Time, CultureInfo.InvariantCulture))} " +
                        $"`[{matchHistory[i].Match}]`";
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
            int count = 0;
            var gods = MongoConnection.GetAllGods();
            foreach (var god in ranks)
            {
                god.WinRate = (double)god.Wins * 100 / (god.Wins + god.Losses);
            }
            var sortedRanksByWinRate = ranks.OrderByDescending(x=>x.WinRate).ToList();
            for (int i = 0; i < sortedRanksByWinRate.Count; i++)
            {
                string godEmoji = Utils.FindGodEmoji(gods, Int32.Parse(sortedRanksByWinRate[i].god_id));
                if (godEmoji.Length + 
                    sortedRanksByWinRate[i].god.Length + 
                    Math.Round(sortedRanksByWinRate[i].WinRate, 2).ToString().Length + 
                    sb.Length + embed.Length > 6000)
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

            if (sb.Length != 0 && embed.Length !> 5500)
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

        public static async Task<Embed> BuildRandomAssaultTeamsEmbedAsync()
        {
            var embed = new EmbedBuilder();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            bool hasHealer = false;
            bool firstHasHealer = false;
            int healerTier = 0;
            Dictionary<int, int> healersDict = new()
            {
                // godid, tier
                { 1698, 2 }, // ra
                { 1718, 1 }, // hel
                { 1763, 2 }, // guan yu
                { 1778, 3 }, // cupid
                { 1898, 1 }, // aphrodite
                { 1921, 2 }, // change
                { 2030, 2 }, // sylvanus
                { 2147, 2 }, // terra
                { 3518, 3 }, // baron
                { 3611, 2 }, // horus
                { 3664, 3 }, // olorun
                { 3811, 1 }  // yemoja
            };
            var gods = MongoConnection.GetAllGods();

            // First team
            for (int i = 0; i < 5; i++)
            {
                var current = gods[rnd.Next(gods.Count)];
                while (hasHealer && healersDict.ContainsKey(current.id))
                {
                    current = gods[rnd.Next(gods.Count)];
                }
                if (healersDict.ContainsKey(current.id))
                {
                    hasHealer = true;
                    firstHasHealer = true;
                    healerTier = healersDict[current.id];
                }

                sb1.AppendLine($"{current.Emoji} {current.Name}");
                gods.Remove(current);
            }

            // Second team
            gods = MongoConnection.GetAllGods();
            hasHealer = false;
            var healersDictTeam2 = new Dictionary<int, int>();
            // if first team has a healer and we havent got a healer in the second
            if (firstHasHealer && !hasHealer)
            {
                foreach (var z in healersDict)
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
                while (hasHealer && healersDict.ContainsKey(current.id))
                {
                    current = gods[rnd.Next(gods.Count)];
                }

                // if first team has a healer & the current random god is healer, mark that second team has healer
                if (healersDict.ContainsKey(current.id) && firstHasHealer && !hasHealer)
                {
                    if (!healersDictTeam2.ContainsKey(current.id))
                    {
                        while (!healersDictTeam2.ContainsKey(current.id))
                        {
                            current = gods[rnd.Next(gods.Count)];
                        }
                    }
                    hasHealer = true;
                }
                // if current god is healer & first team doesn't have a healer, reroll until we get a non-healer god
                else if (healersDict.ContainsKey(current.id) && !firstHasHealer)
                {
                    while (healersDict.ContainsKey(current.id))
                    {
                        current = gods[rnd.Next(gods.Count)];
                    }
                }

                // if first team has healer & we don't have healer in this team yet by the 5th random god, we force it to get a healer
                while (firstHasHealer && !hasHealer && i == 4 && !healersDictTeam2.ContainsKey(current.id))
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
        {
            var embed = new EmbedBuilder();
            embed.WithDescription(description);
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle(patchPost.title);
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
                x.Name = $"SPL Schedule - {schedule.phases[^1].title} [{schedule.date_range}]";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smiteproleague.com/schedule";
            });
            embed.WithFooter(x =>
            {
                x.Text = "This command is still in beta. It may break at any point.";
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
                x.Name = "SPL Standings";
                x.IconUrl = Constants.SmiteBolt;
                x.Url = "https://www.smiteproleague.com/standings";
            });
            embed.WithFooter(x =>
            {
                x.Text = "This command is still in beta. It may break at any point.";
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

        public static async Task<Embed> BuildDefaultFeedsPage(string serverStString, bool hasTooManyChannels)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("️📃 Feeds");
            embed.WithThumbnailUrl("https://i.imgur.com/tba1Sjh.png");
            embed.WithColor(Constants.FeedsColor);
            embed.WithDescription("**What are \"feeds\"?**\n> *Feeds are a new name for the various content that you can \"subscribe\" to to get " +
                "notified about. Currently only SMITE Server status feeds are available, but in the upcoming updates content like SMITE Update Notes, Hotfixes, " +
                "SmiteGame blog posts, SmiteDatamining posts etc. will be available for you.*\n" +
                $"{(hasTooManyChannels ? "__If you don't see the channel you want to set, run the command in that channel and it will be the first option.__" : "")}");

            embed.AddField("🔹 SMITE Status Feeds", (serverStString.Length == 0 ? "Channel not set" : $"Set to {serverStString}"), true);

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
                          $"**{(god.Type.Contains("<:Physical_Power:961778316627550228> Physical") ? "Physical" : "<:Magical_Power:961778316451409970> Magical")} Power:** " +
                          $"{(god.Type.Contains("Physical") ? god.PhysicalPower : god.MagicalPower)}" +
                          $"(+{(god.Type.Contains("Physical") ? god.PhysicalPowerPerLevel : god.MagicalPowerPerLevel)})";
            });

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
    }
}
