using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.MatchPlayerDetails;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Discord
{
    public static class EmbedHandler
    {
        static string botIcon = "https://i.imgur.com/8qNdxse.png";
        static HiRezAPI hirezAPI = new HiRezAPI();

        public static EmbedBuilder ServerStatusEmbed(ServerStatus serverStatus)
        {
            var foundPC = serverStatus.components.Find(x => x.name == "Smite PC");
            var foundXBO = serverStatus.components.Find(x => x.name == "Smite Xbox");
            var foundPS4 = serverStatus.components.Find(x => x.name.Contains("Smite PS4"));
            var foundSwi = serverStatus.components.Find(x => x.name.Contains("Smite Switch"));
            var foundAPI = serverStatus.components.Find(x => x.name.Contains("API"));

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
            else if (serverStatus.incidents.Count >= 1)
            {
                for (int i = 0; i < serverStatus.incidents.Count; i++)
                {
                    if (serverStatus.incidents[i].name.Contains("Smite"))
                    {
                        // Incident color
                        embed.WithColor(new Color(239, 167, 32));
                    }
                }
            }
            else if (serverStatus.scheduled_maintenances.Count >= 1)
            {
                for (int i = 0; i < serverStatus.scheduled_maintenances.Count; i++)
                {
                    if (serverStatus.scheduled_maintenances[i].name.Contains("Smite"))
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
                field.Name = "<:pcicon:537746891610259467> " + foundPC.name; // PC
                field.Value = $"{pcValue}";
            });
            string ps4Value = foundPS4.status.Contains("_") ? Text.ToTitleCase(foundPS4.status.Replace("_", " ")) : Text.ToTitleCase(foundPS4.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:playstationicon:537745670518472714> " + foundPS4.name; // PS4
                field.Value = $"{ps4Value}";
            });
            string xbValue = foundXBO.status.Contains("_") ? Text.ToTitleCase(foundXBO.status.Replace("_", " ")) : Text.ToTitleCase(foundXBO.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:xboxicon:537749895029850112> " + foundXBO.name; // Xbox
                field.Value = $"{xbValue}";
            });
            string swValue = foundSwi.status.Contains("_") ? Text.ToTitleCase(foundSwi.status.Replace("_", " ")) : Text.ToTitleCase(foundSwi.status);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "<:switchicon:537752006719176714> " + foundSwi.name; // Switch
                field.Value = $"{swValue}";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = foundAPI.name; // Hi-Rez API
                field.Value = foundAPI.status.Contains("_") ? Text.ToTitleCase(foundAPI.status.Replace("_", " ")) : Text.ToTitleCase(foundAPI.status);
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "\u2015\u2015\u2015\u2015\u2015\u2015"; // Status page link
                field.Value = "[Status Page](http://status.hirezstudios.com/)";
            });

            return embed;
        }

        public static EmbedBuilder StatusIncidentEmbed(ServerStatus serverStatus)
        {
            var incidentEmbed = new EmbedBuilder();

            for (int n = 0; n < serverStatus.incidents.Count; n++)
            {
                if (serverStatus.incidents[n].name.Contains("Smite"))
                {
                    incidentEmbed.WithColor(new Color(239, 167, 32));
                    incidentEmbed.WithAuthor(author =>
                    {
                        author.WithName("Incidents");
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
                        if (serverStatus.incidents[n].components[z].name.Contains("Switch"))
                        {
                            incidentPlatIcons += "<:switchicon:537752006719176714> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.Contains("Xbox"))
                        {
                            incidentPlatIcons += "<:xboxicon:537749895029850112> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.Contains("PS4"))
                        {
                            incidentPlatIcons += "<:playstationicon:537745670518472714> ";
                        }
                        if (serverStatus.incidents[n].components[z].name.Contains("PC"))
                        {
                            incidentPlatIcons += "<:pcicon:537746891610259467> ";
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
                if (serverStatus.scheduled_maintenances[i].name.Contains("Smite") ||
                    serverStatus.scheduled_maintenances[i].incident_updates[0].body.Contains("Smite"))
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
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("Switch"))
                            {
                                platIcon += "<:switchicon:537752006719176714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("Xbox"))
                            {
                                platIcon += "<:xboxicon:537749895029850112> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("PS4"))
                            {
                                platIcon += "<:playstationicon:537745670518472714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("PC"))
                            {
                                platIcon += "<:pcicon:537746891610259467> ";
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
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("Switch"))
                            {
                                platIcon += "<:switchicon:537752006719176714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("Xbox"))
                            {
                                platIcon += "<:xboxicon:537749895029850112> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("PS4"))
                            {
                                platIcon += "<:playstationicon:537745670518472714> ";
                            }
                            if (serverStatus.scheduled_maintenances[i].components[k].name.Contains("PC"))
                            {
                                platIcon += "<:pcicon:537746891610259467> ";
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

        public static async Task<EmbedBuilder> PlayerStatsEmbed(string json)
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

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName($"{rPlayerName}")
                    .WithUrl($"https://smite.guru/profile/{playerStats[0].ActivePlayerId}")
                    .WithIconUrl(botIcon);
            });
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
                    if (!playerStatus[0].status_string.ToLower().Contains("in game"))
                    {
                        embed.WithDescription($":eyes: In {playerStatus[0].status_string}");
                    }
                    else
                    {
                        embed.WithDescription($":eyes: {playerStatus[0].status_string}");
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
                field.Value = ($"{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item2}**{Text.GetRankedConquest(playerStats[0].Tier_Conquest).Item1}** [{playerStats[0].RankedConquest.Wins}/{playerStats[0].RankedConquest.Losses}]");
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"<:jousticon:528673820018737163>**Ranked Joust**");
                field.Value = ($"{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item2}**{Text.GetRankedJoust(playerStats[0].Tier_Joust).Item1}** [{playerStats[0].RankedJoust.Wins}/{playerStats[0].RankedJoust.Losses}]");
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ($"<:jousticon:528673820018737163>**Ranked Duel**");
                field.Value = ($"{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item2}**{Text.GetRankedDuel(playerStats[0].Tier_Duel).Item1}** [{playerStats[0].RankedDuel.Wins}/{playerStats[0].RankedDuel.Losses}]");
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

            // Saving the player to the database
            await Database.AddPlayerToDb(playerStats);

            return embed;
        }
    }
}
