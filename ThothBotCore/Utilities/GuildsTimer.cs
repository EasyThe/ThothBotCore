using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Utilities
{
    public static class GuildsTimer
    {
        private static Timer GuildCountTimer;
        static internal int joinedGuilds = 0;
        static ObservableGauge<int> _guildsCounter = null;
        static ObservableGauge<int> _usersCounter = null;

        public static Task StartGuildsCountTimer()
        {
            GuildCountTimer = new Timer() // Timer for Guilds Count
            {
                AutoReset = false
            };
            GuildCountTimer.Elapsed += GuildCountTimer_Elapsed;
            GuildCountTimer.Start();

            return Task.CompletedTask;
        }

        internal static async void GuildCountTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Connection.Client.LoginState.ToString() == "LoggedIn" && Connection.Client.CurrentUser != null)
            {
                int totalUsers = 0;
                foreach (var guild in Connection.Client.Guilds)
                {
                    totalUsers += guild.MemberCount;
                }
                if (_guildsCounter == null && Global.Metrics != null)
                {
                    _guildsCounter = Global.Metrics.CreateObservableGauge("guilds", () => Connection.Client.Guilds.Count);
                    _usersCounter = Global.Metrics.CreateObservableGauge("users", () => totalUsers);
                }
                
                if (joinedGuilds != Connection.Client.Guilds.Count && Connection.Client.CurrentUser.Id != 587623068461957121)
                {
                    joinedGuilds = Connection.Client.Guilds.Count;
                    if (Connection.Client.CurrentUser.Activities.Count == 1)
                    {
                        await Connection.Client.SetGameAsync($"{Credentials.botConfig.setGame} | Servers: {joinedGuilds}");
                    }

                    Text.WriteLine("Users: " + totalUsers);

                    //Top.GG
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}," +
                            $"\"shard_count\": {Connection.Client.Shards.Count}}}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.botsAPI);
                            var topggResponse = await webclient.PostAsync("https://top.gg/api/bots/454145330347376651/stats", content);
                            await BotListCallResponse("Top.GG", topggResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**TopGG.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    //DiscordBotList.com
                    // Disabled because they don't care about the list anymore, API throwing 5xx codes
                    //try
                    //{
                    //    using (var webclient = new HttpClient())
                    //    using (var content = new StringContent($"{{ \"guilds\": {Connection.Client.Guilds.Count}, \"users\": {totalUsers} }}", Encoding.UTF8, "application/json"))
                    //    {
                    //        webclient.DefaultRequestHeaders.Add("Authorization", Credentials.botConfig.dblAPI);
                    //        var dblcomResponse = await webclient.PostAsync("https://discordbotlist.com/api/v1/bots/454145330347376651/stats", content);
                    //        await BotListCallResponse("DiscordBotList.com", dblcomResponse);
                    //    }

                    //}
                    //catch (Exception ex)
                    //{
                    //    await Reporter.SendError("**DiscordBotList.**\n" +
                    //        $"**Error Message:** {ex.Message}");
                    //}

                    //Discord.Bots.GG
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"guildCount\": {Connection.Client.Guilds.Count}," +
                            $"\"shardCount\": {Connection.Client.Shards.Count}}}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.dbggAPI);
                            webclient.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Connection.Client.CurrentUser.Username}-" +
                                $"{Connection.Client.CurrentUser.DiscriminatorValue}/1.0 (Discord.NET; +https://github.com/EasyThe/ThothBotCore) " +
                                $"DBots/{Connection.Client.CurrentUser.Id}");
                            var dbggResponse = await webclient.PostAsync("https://discord.bots.gg/api/v1/bots/454145330347376651/stats", content);
                            await BotListCallResponse("Discord.Bots.GG", dbggResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**Discord.Bots.GG.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    //BotsOnDiscord
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"guildCount\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.BotsOnDiscordAPI);
                            var response = await webclient.PostAsync("https://bots.ondiscord.xyz/bot-api/bots/454145330347376651/guilds", content);
                            await BotListCallResponse("BotsOnDiscord", response);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**BotsOnDiscord.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    //DiscordServices
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"servers\": {Connection.Client.Guilds.Count} }}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.DiscordServicesAPI);
                            var response = await webclient.PostAsync("https://api.discordservices.net/bot/454145330347376651/stats", content);
                            await BotListCallResponse("DiscordServices", response);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**DiscordServices.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    //DiscordLabs
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent(
                            $"{{ \"token\": \"{Credentials.botConfig.DiscordLabsAPI}\", " +
                            $"\"server_count\": \"{Connection.Client.Guilds.Count}\"," +
                            $"\"shard_count\": {Connection.Client.Shards.Count} }}", Encoding.UTF8, "application/json"))
                        {
                            var response = await webclient.PostAsync($"https://bots.discordlabs.org/v2/bot/{Connection.Client.CurrentUser.Id}/stats", content);
                            await BotListCallResponse("DiscordLabs", response);
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**DiscordLabs.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    await Connection.Logger.Log("i", $"Guilds count updated! New count: {joinedGuilds}");
                }

                double cpuPercentage = await GetCpuUsageForProcess();
                // StatCord
                try
                {
                    long linkedPlayers = await Storage.Implementations.MongoConnection.LinkedPlayersCount();
                    using var webclient = new HttpClient();
                    using var content = new StringContent(
                        $"{{ \"id\": \"{Connection.Client.CurrentUser.Id}\", " +
                        $"\"key\": \"{Credentials.botConfig.StatCordAPI}\", " +
                        $"\"servers\": \"{Connection.Client.Guilds.Count}\", " +
                        $"\"users\": \"{totalUsers}\", " +
                        $"\"active\": [], " +
                        $"\"commands\": \"0\", " +
                        $"\"popular\": []," +
                        $"\"memactive\": \"{GetMemoryUsageForProcess()}\"," +
                        $"\"memload\": \"0\"," +
                        $"\"cpuload\": \"{cpuPercentage}\"," +
                        $"\"bandwidth\": \"0\"," +
                        $"\"custom1\": \"{linkedPlayers}\"," +
                        $"\"custom2\": \"{Storage.Database.CountOfStatusUpdatesActivatedInDB()[0]}\" }}", Encoding.UTF8, "application/json");
                    var response = await webclient.PostAsync("https://api.statcord.com/v3/stats", content);
                    await BotListCallResponse("StatCord", response);
                }
                catch (Exception ex)
                {
                    await Reporter.SendError("**StatCord.**\n" +
                        $"**Error Message:** {ex.Message}");
                }
            }

            GuildCountTimer.Interval = 60000;
            GuildCountTimer.Enabled = true;
        }

        private static async Task BotListCallResponse(string listName, HttpResponseMessage response)
        {
            if ((response.StatusCode != System.Net.HttpStatusCode.OK && listName != "BotsOnDiscord") ||
                (listName == "BotsOnDiscord" && response.StatusCode != System.Net.HttpStatusCode.NoContent))
            {
                var str = await response.Content.ReadAsStringAsync();
                await Connection.Logger.Log(listName, $"{response.ReasonPhrase}\n{str}");
                return;
            }
            return;
        }
        private static async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
        private static long GetMemoryUsageForProcess()
        {
            return Process.GetCurrentProcess().PrivateMemorySize64;
        }
    }
}
