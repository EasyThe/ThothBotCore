using Discord;
using Discord.WebSocket;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordLogger _logger;

        public Connection(DiscordLogger logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        internal async Task ConnectAsync()
        {
            _client.Log += _logger.Log;

            await _client.LoginAsync(TokenType.Bot, Credentials.botConfig.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
