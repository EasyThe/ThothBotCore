using Discord;
using Newtonsoft.Json;
using System;
using System.Globalization;
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

        private static async void ServerStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await StatusPage.GetStatusSummary();
                ServerStatus ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(StatusPage.statusSummary);

                if (ServerStatus.incidents.Count >= 1)
                {
                    for (int i = 0; i < ServerStatus.incidents.Count; i++)
                    {
                        if (ServerStatus.incidents[i].name.Contains("Smite") ||
                            ServerStatus.incidents[i].incident_updates[0].body.Contains("Smite"))
                        {
                            for (int x = 0; x < ServerStatus.incidents[i].incident_updates.Count; x++)
                            {
                                if (Database.GetServerStatusUpdates(ServerStatus.incidents[i].incident_updates[x].id)[0] == "0")
                                {
                                    await StatusNotifier.SendNotifs(ServerStatus);
                                }

                                await Database.InsertServerStatusUpdates(ServerStatus.incidents[i].incident_updates[x].id,
                                    ServerStatus.incidents[i].incident_updates[x].incident_id,
                                    ServerStatus.incidents[i].name,
                                    ServerStatus.incidents[i].incident_updates[x].created_at.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }

                if (ServerStatus.scheduled_maintenances.Count >= 1)
                {
                    for (int i = 0; i < ServerStatus.scheduled_maintenances.Count; i++)
                    {
                        if (ServerStatus.scheduled_maintenances[i].name.Contains("Smite") ||
                            ServerStatus.scheduled_maintenances[i].incident_updates[0].body.Contains("Smite"))
                        {
                            var embed = new EmbedBuilder();
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

                            if (ServerStatus.scheduled_maintenances[i].incident_updates.Count > 1 &&
                                Database.GetServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[0].id)[0] == "0")
                            {
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

                                for (int j = 0; j < ServerStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                                {
                                    string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status);

                                    maintValue = maintValue + $"**[{maintStatus}]({ServerStatus.scheduled_maintenances[i].shortlink})** - {ServerStatus.scheduled_maintenances[i].incident_updates[j].created_at.ToString("d MMM, HH:mm:ss UTC", CultureInfo.InvariantCulture)}\n{ServerStatus.scheduled_maintenances[i].incident_updates[j].body}\n";
                                }
                                embed.AddField(field =>
                                {
                                    field.IsInline = false;
                                    field.Name = $"{platIcon}{ServerStatus.scheduled_maintenances[i].name}";
                                    field.Value = $"**__Expected downtime: {expectedDtime}__**, {ServerStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {ServerStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {ServerStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n" + maintValue;
                                });

                                await StatusNotifier.SendServerStatus(embed);
                                //await ErrorTracker.SendEmbedError(embed);
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

                                await StatusNotifier.SendServerStatus(embed);
                                //await ErrorTracker.SendEmbedError(embed);
                            }

                            // saving to DB
                            for (int c = 0; c < ServerStatus.scheduled_maintenances[i].incident_updates.Count; c++)
                            {
                                await Database.InsertServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[c].id,
                                    ServerStatus.scheduled_maintenances[i].incident_updates[c].incident_id,
                                    ServerStatus.scheduled_maintenances[i].name,
                                    ServerStatus.scheduled_maintenances[i].incident_updates[c].created_at.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }
                else if (ServerStatus.scheduled_maintenances.Count == 0)
                {

                }
            }
            catch (Exception ex)
            {
                await ErrorTracker.SendError(":warning:**Exception in StatusTimer:** \n" +
                    $"{ex.Message}\n" +
                    $"StackTrace: `{ex.StackTrace}`");
            }
            //await channel.SendMessageAsync(result);

            ServerStatusTimer.Interval = 60000;
            ServerStatusTimer.AutoReset = true;
            ServerStatusTimer.Enabled = true;
        }
    }
}
