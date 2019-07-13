using Discord;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Notifications;
using ThothBotCore.Storage;

namespace ThothBotCore.Utilities
{
    public static class StatusTimer
    {
        private static Timer ServerStatusTimer;
        //private static Timer ActiveStatusTimer;

        public static Task StartServerStatusTimer()
        {
            ServerStatusTimer = new Timer() // Timer for SMITE Server Status
            {
                AutoReset = false
            };
            ServerStatusTimer.Elapsed += ServerStatusTimer_Elapsed;
            ServerStatusTimer.Start();

            return Task.CompletedTask;
        }

        public static async Task StopServerStatusTimer(string message)
        {
            ServerStatusTimer.Enabled = false;
            Console.WriteLine(message);
            await ErrorTracker.SendError(message);
        }

        private static async void ServerStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Discord.Connection.Client.LoginState.ToString() == "LoggedIn")
                {
                    await StatusPage.GetStatusSummary();
                    ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(StatusPage.statusSummary);
                    //ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(File.ReadAllText("test.json")); // Debugging

                    if (ServerStatus.incidents.Count >= 1) // Incidents
                    {
                        for (int i = 0; i < ServerStatus.incidents.Count; i++)
                        {
                            if (ServerStatus.incidents[i].name.Contains("Smite") ||
                                ServerStatus.incidents[i].incident_updates[0].body.Contains("Smite"))
                            {
                                var incidentEmbed = new EmbedBuilder();
                                if (Database.GetServerStatusUpdates(ServerStatus.incidents[i].incident_updates[0].id)[0] == "0")
                                {
                                    string json = JsonConvert.SerializeObject(ServerStatus, Formatting.Indented);
                                    File.WriteAllText($"Status/{ServerStatus.incidents[i].id}.json", json);

                                    incidentEmbed.WithColor(new Color(239, 167, 32));
                                    string incidentValue = "";
                                    for (int c = 0; c < ServerStatus.incidents[i].incident_updates.Count; c++)
                                    {
                                        incidentValue += $"**[{Text.ToTitleCase(ServerStatus.incidents[i].incident_updates[c].status)}]({ServerStatus.incidents[i].shortlink})** - " +
                                            $"{ServerStatus.incidents[i].incident_updates[c].updated_at.ToUniversalTime().ToString("d MMM, HH:mm", CultureInfo.InvariantCulture)} UTC\n" +
                                            $"{ServerStatus.incidents[i].incident_updates[c].body}\n";
                                    }
                                    string incidentPlatIcons = "";

                                    for (int z = 0; z < ServerStatus.incidents[i].components.Count; z++) // cycle for platform icons
                                    {
                                        if (ServerStatus.incidents[i].components[z].name.Contains("Switch"))
                                        {
                                            incidentPlatIcons += "<:switchicon:537752006719176714> ";
                                        }
                                        if (ServerStatus.incidents[i].components[z].name.Contains("Xbox"))
                                        {
                                            incidentPlatIcons += "<:xboxicon:537749895029850112> ";
                                        }
                                        if (ServerStatus.incidents[i].components[z].name.Contains("PS4"))
                                        {
                                            incidentPlatIcons += "<:playstationicon:537745670518472714> ";
                                        }
                                        if (ServerStatus.incidents[i].components[z].name.Contains("PC"))
                                        {
                                            incidentPlatIcons += "<:pcicon:537746891610259467> ";
                                        }
                                    }

                                    if (incidentValue.Length > 1024)
                                    {
                                        incidentEmbed.WithTitle($"{incidentPlatIcons} {ServerStatus.incidents[i].name}");
                                        incidentEmbed.WithDescription(incidentValue);
                                    }
                                    else
                                    {
                                        incidentEmbed.AddField(field =>
                                        {
                                            field.IsInline = false;
                                            field.Name = $"{incidentPlatIcons} {ServerStatus.incidents[i].name}";
                                            field.Value = incidentValue;
                                        });
                                    }

                                    // Sending the Incident to the servers
                                    await StatusNotifier.SendServerStatus(incidentEmbed);
                                    //await ErrorTracker.SendEmbedError(incidentEmbed); // Debugging

                                    // Saving to DB
                                    try
                                    {
                                        await Database.InsertServerStatusUpdates(ServerStatus.incidents[i].incident_updates[0].id,
                                        ServerStatus.incidents[i].incident_updates[0].incident_id,
                                        ServerStatus.incidents[i].impact,
                                        ServerStatus.incidents[i].status,
                                        ServerStatus.incidents[i].name,
                                        ServerStatus.incidents[i].incident_updates[0].body,
                                        ServerStatus.incidents[i].incident_updates[0].created_at.ToString(CultureInfo.InvariantCulture));
                                    }
                                    catch (Exception ex)
                                    {
                                        await ErrorTracker.SendError(":warning:**Exception in StatusTimer (DB SAVE):** \n" +
                                            $"**Message: **{ex.Message}\n" +
                                            $"**StackTrace: **`{ex.StackTrace}`");
                                        ServerStatusTimer.Enabled = false;
                                    }
                                }
                            }
                        }
                    }

                    if (ServerStatus.scheduled_maintenances.Count >= 1) // Maintenances
                    {
                        var embed = new EmbedBuilder();
                        for (int i = 0; i < ServerStatus.scheduled_maintenances.Count; i++)
                        {
                            if (ServerStatus.scheduled_maintenances[i].name.Contains("Smite") ||
                                ServerStatus.scheduled_maintenances[i].incident_updates[0].body.Contains("Smite"))
                            {
                                embed.WithColor(new Color(52, 152, 219)); //maintenance color
                                embed.WithFooter(footer =>
                                {
                                    footer.Text = $"Current UTC: " + DateTime.UtcNow.ToString("dd MMM, HH:mm:ss", CultureInfo.InvariantCulture);
                                });

                                if (ServerStatus.scheduled_maintenances[i].incident_updates.Count > 1 &&
                                    Database.GetServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[0].id)[0] == "0")
                                {
                                    string json = JsonConvert.SerializeObject(ServerStatus, Formatting.Indented);
                                    File.WriteAllText($"Status/{ServerStatus.scheduled_maintenances[i].id}.json", json);
                                    string platIcon = "";
                                    string maintValue = "";
                                    string expectedDtime = "";

                                    for (int k = 0; k < ServerStatus.scheduled_maintenances[i].components.Count; k++)
                                    {
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("Switch"))
                                        {
                                            platIcon += "<:switchicon:537752006719176714> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("Xbox"))
                                        {
                                            platIcon += "<:xboxicon:537749895029850112> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PS4"))
                                        {
                                            platIcon += "<:playstationicon:537745670518472714> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PC"))
                                        {
                                            platIcon += "<:pcicon:537746891610259467> ";
                                        }
                                    }

                                    TimeSpan expDwntime = ServerStatus.scheduled_maintenances[i].scheduled_until - ServerStatus.scheduled_maintenances[i].scheduled_for;

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
                                    if (expDwntime.Minutes != 0 || expDwntime.Hours != 0)
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

                                    string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Contains("_") ? Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Replace("_", " ")) : Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status);
                                    maintValue = maintValue + $"**[{maintStatus}]({ServerStatus.scheduled_maintenances[i].shortlink})** - {ServerStatus.scheduled_maintenances[i].incident_updates[0].created_at.ToString("d MMM, HH:mm:ss UTC", CultureInfo.InvariantCulture)}\n{ServerStatus.scheduled_maintenances[i].incident_updates[0].body}\n";

                                    embed.AddField(field =>
                                    {
                                        field.IsInline = false;
                                        field.Name = $"{platIcon}{ServerStatus.scheduled_maintenances[i].name}";
                                        field.Value = $"**__Expected downtime: {expectedDtime}__**, {ServerStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {ServerStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {ServerStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n" + maintValue;
                                    });
                                }
                                else if (Database.GetServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[0].id)[0] == "0")
                                {
                                    string platIcon = "";

                                    for (int k = 0; k < ServerStatus.scheduled_maintenances[i].components.Count; k++)
                                    {
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("Switch"))
                                        {
                                            platIcon += "<:switchicon:537752006719176714> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("Xbox"))
                                        {
                                            platIcon += "<:xboxicon:537749895029850112> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PS4"))
                                        {
                                            platIcon += "<:playstationicon:537745670518472714> ";
                                        }
                                        if (ServerStatus.scheduled_maintenances[i].components[k].name.Contains("PC"))
                                        {
                                            platIcon += "<:pcicon:537746891610259467> ";
                                        }
                                    }

                                    for (int j = 0; j < ServerStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                                    {
                                        string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status);
                                        TimeSpan expDwntime = ServerStatus.scheduled_maintenances[i].scheduled_until - ServerStatus.scheduled_maintenances[i].scheduled_for;
                                        string expectedDtime = "";
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
                                            field.Name = $"{platIcon}{ServerStatus.scheduled_maintenances[i].name}";
                                            field.Value = $"**[{maintStatus}]({ServerStatus.scheduled_maintenances[i].shortlink})**\n__**Expected downtime: {expectedDtime}**__, {ServerStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {ServerStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {ServerStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n{ServerStatus.scheduled_maintenances[i].incident_updates[j].body}";
                                        });
                                    }
                                }

                                // saving to DB
                                for (int c = 0; c < ServerStatus.scheduled_maintenances[i].incident_updates.Count; c++)
                                {
                                    await Database.InsertServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[c].id,
                                        ServerStatus.scheduled_maintenances[i].incident_updates[c].incident_id,
                                        ServerStatus.scheduled_maintenances[i].impact,
                                        ServerStatus.scheduled_maintenances[i].status,
                                        ServerStatus.scheduled_maintenances[i].name,
                                        ServerStatus.scheduled_maintenances[i].incident_updates[c].body,
                                        ServerStatus.scheduled_maintenances[i].incident_updates[c].created_at.ToString(CultureInfo.InvariantCulture));
                                }
                            }
                        }
                        //Sending maintenance embed
                        if (embed.Fields.Count != 0)
                        {
                            await StatusNotifier.SendServerStatus(embed);
                            //await ErrorTracker.SendEmbedError(embed); // Debugging
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ErrorTracker.SendError(":warning:**Exception in StatusTimer:** \n" +
                    $"**Message: **{ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`");
                Console.WriteLine("StatusTimer\n" + ex.Message);
            }
            //await channel.SendMessageAsync(result);

            ServerStatusTimer.Interval = 60000;
            ServerStatusTimer.AutoReset = true;
            ServerStatusTimer.Enabled = true;
        }

        private static async void ActiveStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await StatusPage.GetStatusSummary();
            ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(StatusPage.statusSummary);
        }
    }
}
