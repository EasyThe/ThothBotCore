using Discord;
using Discord.WebSocket;
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

            await _client.SetGameAsync($"{Credentials.botConfig.prefix}help");

            await _client.LoginAsync(TokenType.Bot, Credentials.botConfig.Token);
            await _client.StartAsync();

            _client.JoinedGuild += JoinedGuildMessage; // Send message to default channel of joined guild

            await Task.Delay(-1);
        }

        private async Task JoinedGuildMessage(SocketGuild arg)
        {
            var channel = arg.DefaultChannel;

            await channel.SendMessageAsync(":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`");
        }
    }
}
