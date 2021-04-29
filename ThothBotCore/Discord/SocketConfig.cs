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
                LogLevel = LogSeverity.Verbose,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                ExclusiveBulkDelete = true,
                TotalShards = Credentials.botConfig.prefix == "??" ? 1 : Connection.ShardCount
            };
        }

        public static DiscordSocketConfig GetNew()
        {
            return new DiscordSocketConfig();
        }
    }
}
