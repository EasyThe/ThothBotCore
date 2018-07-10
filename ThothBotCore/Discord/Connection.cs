using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private DiscordSocketClient _client;
        private DiscordLogger _logger;

        public Connection(DiscordLogger logger)
        {
            _logger = logger;
        }

        internal async Task ConnectAsync(ThothBotConfig config)
        {
            _client = new DiscordSocketClient(config.SocketConfig);

            _client.Log += _logger.Log;

            // TODO: Continue
        }
    }
}