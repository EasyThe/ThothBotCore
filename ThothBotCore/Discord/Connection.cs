using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordLogger _logger;

        public static DiscordSocketClient Client;
        private GuildsTimer guildsTimer = new GuildsTimer();

        public Connection(DiscordLogger logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        internal async Task ConnectAsync()
        {
            _client.Log += _logger.Log;

            //await _client.SetGameAsync($"{Credentials.botConfig.prefix}help");

            await _client.LoginAsync(TokenType.Bot, Credentials.botConfig.Token);
            await _client.StartAsync();
            Client = _client;
            CommandHandler _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);

            _client.Ready += ClientReadyTask;
            _client.JoinedGuild += JoinedNewGuildActions;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task ClientReadyTask()
        {
            StatusTimer.StartServerStatusTimer();
            guildsTimer.StartGuildsCountTimer();

            return Task.CompletedTask;
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            await ErrorTracker.SendDMtoOwner($":tada: I joined {guild.Name} :tada:\n**Owner**: {guild.Owner}");
            var channel = guild.DefaultChannel;

            await channel.SendMessageAsync(":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can set a custom prefix for your server with !!prefix `your-prefix-here`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`");
        }
    }
}
