using DiscordBotsList.Api;
using System.Threading.Tasks;
using System.Timers;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Utilities
{
    public class GuildsTimer
    {
        private static Timer GuildCountTimer;
        AuthDiscordBotListApi DblApi = new AuthDiscordBotListApi(454145330347376651, "token");
        internal int joinedGuilds = 0;

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
                System.Console.WriteLine("done");
            }

            bool nz = false;
            if (nz == true)
            {
                try
                {
                    var me = await DblApi.GetMeAsync();
                    // Update stats           guildCount
                    await me.UpdateStatsAsync(Connection.Client.Guilds.Count);
                }
                catch (System.Exception ex)
                {
                    await ErrorTracker.SendError($"Error from DiscordServersAPI thingy../n{ex.Message}");
                }
            }


            GuildCountTimer.Interval = 60000;
            GuildCountTimer.AutoReset = true;
            GuildCountTimer.Enabled = true;
        }
    }
}
