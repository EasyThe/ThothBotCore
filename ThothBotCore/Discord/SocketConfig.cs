using Discord;
using Discord.Net.Providers.WS4Net;
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
                WebSocketProvider = WS4NetProvider.Instance
            };
        }

        public static DiscordSocketConfig GetNew()
        {
            return new DiscordSocketConfig();
        }
    }
}
