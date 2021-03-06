﻿using Discord;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;
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
        public static async Task StopServerStatusTimer(string message)
        {
            ServerStatusTimer.Enabled = false;
            Text.WriteLine(message);
            await Reporter.SendError(message);
        }
        private static async void ServerStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Discord.Connection.Client.LoginState.ToString() == "LoggedIn")
                {
                    if (!Directory.Exists("Status"))
                    {
                        Directory.CreateDirectory("Status");
                    }

                    await StatusPage.GetStatusSummary();
                    var ServerStatus = JsonConvert.DeserializeObject<ServerStatus>(await StatusPage.GetStatusSummary());

                    if (ServerStatus != null)
                    {
                        if (ServerStatus.incidents.Count >= 1) // Incidents
                        {
                            var incidentEmbed = new EmbedBuilder();
                            for (int i = 0; i < ServerStatus.incidents.Count; i++)
                            {
                                if ((ServerStatus.incidents[i].name.ToLowerInvariant().Contains("smite") ||
                                    ServerStatus.incidents[i].incident_updates[0].body.ToLowerInvariant().Contains("smite")) && !(ServerStatus.incidents[i].name.ToLowerInvariant().Contains("blitz")))
                                {
                                    if (Database.GetServerStatusUpdates(ServerStatus.incidents[i].incident_updates[0].id)[0] == "0")
                                    {
                                        incidentEmbed.WithColor(new Color(239, 167, 32));

                                        string json = JsonConvert.SerializeObject(ServerStatus, Formatting.Indented);
                                        await File.WriteAllTextAsync($"Status/{ServerStatus.incidents[i].id}.json", json);

                                        var incidentValue = new StringBuilder();
                                        for (int c = 0; c < ServerStatus.incidents[i].incident_updates.Count; c++)
                                        {
                                            incidentValue.Append($"**[{Text.ToTitleCase(ServerStatus.incidents[i].incident_updates[c].status)}]({ServerStatus.incidents[i].shortlink})** - " +
                                                $"{ServerStatus.incidents[i].incident_updates[c].updated_at.ToUniversalTime().ToString("d MMM, HH:mm", CultureInfo.InvariantCulture)} UTC\n" +
                                                $"{ServerStatus.incidents[i].incident_updates[c].body}\n");
                                        }
                                        string incidentPlatIcons = await Utils.MaintenancePlatformsAsync(ServerStatus.incidents[i].components);

                                        if (incidentValue.Length > 1024)
                                        {
                                            incidentEmbed.WithTitle($"{incidentPlatIcons} {ServerStatus.incidents[i].name}");
                                            incidentEmbed.WithDescription(incidentValue.ToString());
                                        }
                                        else
                                        {
                                            incidentEmbed.AddField(field =>
                                            {
                                                field.IsInline = false;
                                                field.Name = $"{incidentPlatIcons} {ServerStatus.incidents[i].name}";
                                                field.Value = incidentValue.ToString();
                                            });
                                        }
                                        incidentEmbed.WithFooter(x => x.Text = "Since");
                                        incidentEmbed.WithTimestamp(ServerStatus.incidents[i].created_at);

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
                                            await Reporter.SendError(":warning:**Exception in StatusTimer (DB SAVE):** \n" +
                                                $"**Message: **{ex.Message}\n" +
                                                $"**StackTrace: **`{ex.StackTrace}`");
                                            ServerStatusTimer.Enabled = false;
                                        }
                                    }
                                }
                            }
                            if (incidentEmbed.Length != 0)
                            {
                                // Sending the Incident to the servers
                                await StatusNotifier.SendServerStatus(incidentEmbed);
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
                                        await File.WriteAllTextAsync($"Status/{ServerStatus.scheduled_maintenances[i].id}.json", json);
                                        string platIcon = await Utils.MaintenancePlatformsAsync(ServerStatus.scheduled_maintenances[i].components);
                                        string maintValue = "";

                                        TimeSpan expDwntime = ServerStatus.scheduled_maintenances[i].scheduled_until - ServerStatus.scheduled_maintenances[i].scheduled_for;
                                        string expectedDtime = await Utils.ExpectedDowntimeAsync(expDwntime);

                                        if (ServerStatus.scheduled_maintenances[i].status.ToLowerInvariant().Contains("progress") ||
                                            ServerStatus.scheduled_maintenances[i].status.ToLowerInvariant().Contains("verifying"))
                                        {
                                            embed.Footer.Text += " | Ends ";
                                            embed.WithTimestamp(ServerStatus.scheduled_maintenances[i].scheduled_until);
                                        }
                                        else
                                        {
                                            embed.Footer.Text += " | Starts ";
                                            embed.WithTimestamp(ServerStatus.scheduled_maintenances[i].scheduled_for);
                                        }

                                        string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Contains("_") ? Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status.Replace("_", " ")) : Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[0].status);
                                        maintValue += $"**[{maintStatus}]({ServerStatus.scheduled_maintenances[i].shortlink})** - {ServerStatus.scheduled_maintenances[i].incident_updates[0].created_at.ToString("d MMM, HH:mm:ss UTC", CultureInfo.InvariantCulture)}\n{ServerStatus.scheduled_maintenances[i].incident_updates[0].body}\n";

                                        embed.AddField(field =>
                                        {
                                            field.IsInline = false;
                                            field.Name = $"{platIcon}{ServerStatus.scheduled_maintenances[i].name}";
                                            field.Value = $"**__Expected downtime: {expectedDtime}__**, {ServerStatus.scheduled_maintenances[i].scheduled_until.ToString("d MMM", CultureInfo.InvariantCulture)}, {ServerStatus.scheduled_maintenances[i].scheduled_for.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} - {ServerStatus.scheduled_maintenances[i].scheduled_until.ToUniversalTime().ToString("t", CultureInfo.InvariantCulture)} UTC\n" + maintValue;
                                        });
                                    }
                                    else if (Database.GetServerStatusUpdates(ServerStatus.scheduled_maintenances[i].incident_updates[0].id)[0] == "0")
                                    {
                                        string platIcon = await Utils.MaintenancePlatformsAsync(ServerStatus.scheduled_maintenances[i].components);

                                        for (int j = 0; j < ServerStatus.scheduled_maintenances[i].incident_updates.Count; j++)
                                        {
                                            string maintStatus = ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Contains("_") ? Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status.Replace("_", " ")) : Text.ToTitleCase(ServerStatus.scheduled_maintenances[i].incident_updates[j].status);
                                            TimeSpan expDwntime = ServerStatus.scheduled_maintenances[i].scheduled_until - ServerStatus.scheduled_maintenances[i].scheduled_for;
                                            string expectedDtime = await Utils.ExpectedDowntimeAsync(expDwntime);

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
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Reporter.SendError($"StatusTimer.cs Line 209 Error:\n{ex.Message}\n{ex.StackTrace}");
                Text.WriteLine($"\nStatusTimer\n{ex.Message}\n{ex.StackTrace}\n");
            }

            ServerStatusTimer.Interval = 60000;
            ServerStatusTimer.Enabled = true;
        }
    }
}
