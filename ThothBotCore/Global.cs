
using Discord.WebSocket;

namespace ThothBotCore
{
    public static class Global
    {
        public static int CommandsRun { get; set; } = 1;
        public static DiscordSocketClient Client { get; set; }
    }
}
