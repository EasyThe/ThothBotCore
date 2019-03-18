using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordLogger _logger;

        public static DiscordSocketClient Client;

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
            Client = _client;
            CommandHandler _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);

            _client.JoinedGuild += JoinedNewGuildActions; // Send message to default channel of joined guild and add to DB.

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            var channel = guild.DefaultChannel;

            await channel.SendMessageAsync(":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`");
        }
    }
}
