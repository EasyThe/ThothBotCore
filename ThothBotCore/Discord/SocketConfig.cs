using Discord;
using Discord.WebSocket;

namespace ThothBotCore.Discord
{
    public static class SocketConfig
    {
        public static DiscordSocketConfig GetDefault()
        {
            return new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRetryMode = RetryMode.AlwaysRetry
            };
        }

        public static DiscordSocketConfig GetNew()
        {
            return new DiscordSocketConfig();
        }
    }
}
