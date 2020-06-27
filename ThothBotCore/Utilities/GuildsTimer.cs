﻿using System;
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
        private static Timer HourTimer;
        static internal int joinedGuilds = 0;

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

        public static Task StartHourlyTimer()
        {
            // Hourly timer for DiscordLabs...
            HourTimer = new Timer()
            {
                AutoReset = false
            };
            HourTimer.Elapsed += HourTimer_Elapsed;
            HourTimer.Start();
            Console.WriteLine("[HourTimer.Enabled]" + HourTimer.Enabled);

            return Task.CompletedTask;
        }

        public static Task StopHourlyTimer()
        {
            HourTimer.Stop();
            Console.WriteLine("[HourTimer.Enabled]" + HourTimer.Enabled);
            return Task.CompletedTask;
        }

        internal static async void HourTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Connection.Client.CurrentUser.Id == 587623068461957121)
            {
                return;
            }

            Console.WriteLine("=== HourTimer Elapsed ===");
            //DiscordLabs
            try
            {
                using (var webclient = new HttpClient())
                using (var content = new StringContent(
                    $"{{ \"token\": \"{Credentials.botConfig.DiscordLabsAPI}\", " +
                    $"\"server_count\": \"{Connection.Client.Guilds.Count}\" }}", Encoding.UTF8, "application/json"))
                {
                    var response = await webclient.PostAsync($"https://bots.discordlabs.org/v2/bot/{Connection.Client.CurrentUser.Id}/stats", content);
                    Console.WriteLine($"===\nDiscordLabs: {response.ReasonPhrase}\n===\n");
                }
            }
            catch (Exception ex)
            {
                await Reporter.SendError("**DiscordLabs.**\n" +
                    $"**Error Message:** {ex.Message}");
            }

            int totalUsers = 0;
            foreach (var guild in Connection.Client.Guilds)
            {
                totalUsers += guild.Users.Count;
            }
            // StatCord
            try
            {

                using (var webclient = new HttpClient())
                using (var content = new StringContent(
                    $"{{ \"id\": \"{Connection.Client.CurrentUser.Id}\", " +
                    $"\"key\": \"{Credentials.botConfig.StatCordAPI}\", " +
                    $"\"servers\": \"{Connection.Client.Guilds.Count}\", " +
                    $"\"users\": \"{totalUsers}\", " +
                    $"\"active\": \"0\", " +
                    $"\"commands\": \"0\", " +
                    $"\"popular\": [] }}", Encoding.UTF8, "application/json"))
                {
                    var response = await webclient.PostAsync("https://statcord.com/mason/stats", content);
                    Console.WriteLine($"===\nStatCord: {response.ReasonPhrase}\n===\n");
                }
            }
            catch (Exception ex)
            {
                await Reporter.SendError("**StatCord.**\n" +
                    $"**Error Message:** {ex.Message}");
            }

            HourTimer.Interval = 3600000;
            HourTimer.Enabled = true;
        }

        internal static async void GuildCountTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (joinedGuilds != Connection.Client.Guilds.Count)
            {
                joinedGuilds = Connection.Client.Guilds.Count;
                await Connection.Client.SetGameAsync($"{Credentials.botConfig.setGame} | Servers: {joinedGuilds}");

                int totalUsers = 0;
                foreach (var guild in Connection.Client.Guilds)
                {
                    totalUsers += guild.Users.Count;
                }
                Console.WriteLine("Users: " + totalUsers);
                if (Connection.Client.CurrentUser.Id == 587623068461957121)
                {
                    GuildCountTimer.Interval = 60000;
                    GuildCountTimer.Enabled = true;
                    return;
                }

                //Top.GG
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.botsAPI);
                        await webclient.PostAsync("https://discordbots.org/api/bots/454145330347376651/stats", content);
                    }
                }
                catch (Exception ex)
                {
                    await Reporter.SendError("**DiscordBotsList.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                //BotsForDiscord.com
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.bfdAPI);
                        await webclient.PostAsync("https://botsfordiscord.com/api/bot/454145330347376651", content);
                    }
                }
                catch (Exception ex)
                {
                    await Reporter.SendError("**BotsForDiscord.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                //DiscordBotList.com
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"guilds\": {Connection.Client.Guilds.Count}, \"users\": {totalUsers} }}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Add("Authorization", $"Bot {Credentials.botConfig.dblAPI}");
                        await webclient.PostAsync("https://discordbotlist.com/api/bots/454145330347376651/stats", content);
                    }
                    
                }
                catch (Exception ex)
                {
                    await Reporter.SendError("**DiscordBotList.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                //Discord.Bots.GG
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"guildCount\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.dbggAPI);
                        webclient.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Connection.Client.CurrentUser.Username}-" +
                            $"{Connection.Client.CurrentUser.DiscriminatorValue}/1.0 (Discord.NET; +https://github.com/EasyThe/ThothBotCore) " +
                            $"DBots/{Connection.Client.CurrentUser.Id}");
                        await webclient.PostAsync("https://discord.bots.gg/api/v1/bots/454145330347376651/stats", content);
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
                        await webclient.PostAsync("https://bots.ondiscord.xyz/bot-api/bots/454145330347376651/guilds", content);
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
                        await webclient.PostAsync("https://api.discordservices.net/bot/454145330347376651/stats", content);
                    }
                }
                catch (Exception ex)
                {
                    await Reporter.SendError("**DiscordServices.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                Console.WriteLine($"{DateTime.Now:[HH:mm]} Guilds count updated! New count: {joinedGuilds}");
            }

            GuildCountTimer.Interval = 60000;
            GuildCountTimer.Enabled = true;
        }
    }
}
