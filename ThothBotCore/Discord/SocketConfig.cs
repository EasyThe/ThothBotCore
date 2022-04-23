using Discord;
using Discord.WebSocket;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Discord
{
    public static class SocketConfig
    {
        public static DiscordSocketConfig GetDefault()
        {
            return new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogGatewayIntentWarnings = false,
                TotalShards = Credentials.botConfig.prefix == "??" ? 1 : Connection.ShardCount,
                GatewayIntents = GatewayIntents.AllUnprivileged
            };
        }

        public static DiscordSocketConfig GetNew()
        {
            return new DiscordSocketConfig();
        }
    }
}
