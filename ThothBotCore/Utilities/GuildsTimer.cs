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
    public class GuildsTimer
    {
        private static Timer GuildCountTimer;
        internal int joinedGuilds = 0;

        DiscordBotList dblcom = new DiscordBotList();

        public Task StartGuildsCountTimer()
        {
            GuildCountTimer = new Timer() // Timer for Guilds Count
            {
                AutoReset = false
            };
            GuildCountTimer.Elapsed += GuildCountTimer_Elapsed;
            GuildCountTimer.Start();

            return Task.CompletedTask;
        }

        internal async void GuildCountTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (joinedGuilds != Connection.Client.Guilds.Count)
            {
                joinedGuilds = Connection.Client.Guilds.Count;
                await Connection.Client.SetGameAsync($"{Credentials.botConfig.setGame} | Servers: {joinedGuilds}");

                int totalUsers = 0;
                foreach (var guild in Connection.Client.Guilds)
                {
                    totalUsers = totalUsers + guild.Users.Count;
                }

                //DiscordBots.org
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.botsAPI);
                        HttpResponseMessage response = await webclient.PostAsync("https://discordbots.org/api/bots/454145330347376651/stats", content);
                    }
                }
                catch (Exception ex)
                {
                    await ErrorTracker.SendError("**Something happened when I tried to update guilds count for DiscordBotsList.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                //BotsForDiscord.com
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"server_count\": {Connection.Client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Credentials.botConfig.bfdAPI);
                        HttpResponseMessage response = await webclient.PostAsync("https://botsfordiscord.com/api/bot/454145330347376651", content);
                    }
                }
                catch (Exception ex)
                {
                    await ErrorTracker.SendError("**Something happened when I tried to update guilds count for BotsForDiscord.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                //DiscordBotList.com
                try
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent($"{{ \"guilds\": {Connection.Client.Guilds.Count}, \"users\": {totalUsers} }}", Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Add("Authorization", $"Bot {Credentials.botConfig.dblAPI}");
                        HttpResponseMessage response = await webclient.PostAsync("https://discordbotlist.com/api/bots/454145330347376651/stats", content);
                    }
                    
                }
                catch (Exception ex)
                {
                    await ErrorTracker.SendError("**Something happened when I tried to update guilds count for DiscordBotList.**\n" +
                        $"**Error Message:** {ex.Message}");
                }

                
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm]")} Guilds count updated! New count: {joinedGuilds}");
            }

            GuildCountTimer.Interval = 60000;
            GuildCountTimer.AutoReset = true;
            GuildCountTimer.Enabled = true;
        }

        internal class DiscordBotList
        {
            internal int guilds { get; set; }
            internal int users { get; set; }
        }
    }
}
