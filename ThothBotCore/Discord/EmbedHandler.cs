using Discord;
using System;
using System.Globalization;
using ThothBotCore.Connections.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public static class EmbedHandler
    {
        //readonly string botIcon = "https://i.imgur.com/AgNocjS.png";

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

                    if (serverStatus.incidents[n].name.Contains("Switch"))
                    {
                        incidentPlatIcons += "<:switchicon:537752006719176714> ";
                    }
                    if (serverStatus.incidents[n].name.Contains("Xbox"))
                    {
                        incidentPlatIcons += "<:xboxicon:537749895029850112> ";
                    }
                    if (serverStatus.incidents[n].name.Contains("PS4"))
                    {
                        incidentPlatIcons += "<:playstationicon:537745670518472714> ";
                    }
                    if (serverStatus.incidents[n].name.Contains("PC"))
                    {
                        incidentPlatIcons += "<:pcicon:537746891610259467> ";
                    }

                    incidentEmbed.AddField(field =>
                    {
                        field.IsInline = false;
                        field.Name = $"{incidentPlatIcons} {serverStatus.incidents[n].name}";
                        field.Value = incidentValue;
                    });

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
    }
}
