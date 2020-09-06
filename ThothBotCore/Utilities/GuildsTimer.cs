using Discord;
using System;
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
            if (Connection.Client.LoginState.ToString() == "LoggedIn")
            {
                int totalUsers = 0;
                foreach (var guild in Connection.Client.Guilds)
                {
                    totalUsers += guild.Users.Count;
                }
                if (joinedGuilds != Connection.Client.Guilds.Count)
                {
                    joinedGuilds = Connection.Client.Guilds.Count;
                    await Connection.Client.SetGameAsync($"{Credentials.botConfig.setGame} | Servers: {joinedGuilds}");

                    Console.WriteLine("Users: " + totalUsers);

                    if (Connection.Client.CurrentUser.Id == 587623068461957121)
                    {
                        Console.WriteLine(Connection.Client.CurrentUser.Username + " is logged in, guild count update timer disabled.");
                        GuildCountTimer.Interval = 60000;
                        GuildCountTimer.Enabled = false;
                        return;
                    }

                    var sb = new StringBuilder();

                    //Top.GG
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.botsAPI);
                            var topggResponse = await webclient.PostAsync("https://discordbots.org/api/bots/454145330347376651/stats", content);
                            sb.AppendLine($"Top.GG -- {topggResponse.StatusCode} {topggResponse.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**TopGG.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    //BotsForDiscord.com
                    try
                    {
                        using (var webclient = new HttpClient())
                        using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                        {
                            webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.bfdAPI);
                            var bfdcomResponse = await webclient.PostAsync("https://botsfordiscord.com/api/bot/454145330347376651", content);
                            sb.AppendLine($"BotsForDiscord.com -- {bfdcomResponse.StatusCode} {bfdcomResponse.ReasonPhrase}");
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
                            webclient.DefaultRequestHeaders.Add("Authorization", Credentials.botConfig.dblAPI);
                            var dblcomResponse = await webclient.PostAsync("https://discordbotlist.com/api/v1/bots/454145330347376651/stats", content);
                            sb.AppendLine($"DiscordBotList.com -- {dblcomResponse.StatusCode} {dblcomResponse.ReasonPhrase}");
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
                            var dbggResponse = await webclient.PostAsync("https://discord.bots.gg/api/v1/bots/454145330347376651/stats", content);
                            sb.AppendLine($"Discord.Bots.GG -- {dbggResponse.StatusCode} {dbggResponse.ReasonPhrase}");
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
                            sb.AppendLine($"BotsOnDiscord -- {response.StatusCode} {response.ReasonPhrase}");
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
                            sb.AppendLine($"DiscordServices.com -- {response.StatusCode} {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Reporter.SendError("**DiscordServices.**\n" +
                            $"**Error Message:** {ex.Message}");
                    }

                    sb.AppendLine($"{DateTime.Now:[HH:mm]} Guilds count updated! New count: {joinedGuilds}");
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync(sb.ToString(), 107, 70, 147);
                    await Reporter.SendEmbedToBotLogsChannel(em.ToEmbedBuilder());
                }

                if (Connection.Client.CurrentUser.Id == 587623068461957121)
                {
                    return;
                }

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
                        $"\"popular\": []," +
                        $"\"memactive\": \"0\"," +
                        $"\"memload\": \"0\"," +
                        $"\"cpuload\": \"0\"," +
                        $"\"bandwidth\": \"0\" }}", Encoding.UTF8, "application/json"))
                    {
                        var response = await webclient.PostAsync("https://statcord.com/logan/stats", content);
                        Console.WriteLine($"===\nStatCord: {response.ReasonPhrase}\n===\n");
                    }
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
    }
}
