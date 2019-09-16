using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Tournament;
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

            await _client.LoginAsync(TokenType.Bot, Credentials.botConfig.Token);
            await _client.StartAsync();
            Client = _client;
            CommandHandler _handler = new CommandHandler();
            SignupReader _signupReader = new SignupReader();
            await _handler.InitializeAsync(_client);
            await _signupReader.InitializeSignupReaderAsync(_client);

            _client.Ready += ClientReadyTask;
            _client.JoinedGuild += JoinedNewGuildActions;
            _client.LeftGuild += ClientLeftGuildTask;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task ClientLeftGuildTask(SocketGuild arg)
        {
            await Database.DeleteServerConfig(arg.Id);
            await ErrorTracker.SendLeftServers(arg);
        }

        private Task ClientReadyTask()
        {
            StatusTimer.StartServerStatusTimer();
            guildsTimer.StartGuildsCountTimer();
            _client.DownloadUsersAsync(Client.Guilds);

            return Task.CompletedTask;
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            await ErrorTracker.SendJoinedServers(guild);
            await Database.SetGuild(guild.Id, guild.Name);
            var channel = guild.DefaultChannel;
            foreach (var chnl in guild.TextChannels)
            {
                if (chnl.Name.ToLowerInvariant().Contains("bot"))
                {
                    channel = chnl;
                }
            }
            try
            {
                await channel.SendMessageAsync(Constants.JoinedMessage);
            }
            catch (System.Exception)
            {
                System.Console.WriteLine(Constants.FailedToSendJoinedMessage);
            }
        }
    }
}
