using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private readonly DiscordShardedClient _client;
        private readonly DiscordLogger _logger;
        public static List<int> shardsConnected = new();
        public const int ShardCount = 3;
        
        public static DiscordShardedClient Client;
        public static DiscordLogger Logger;

        public Connection(DiscordLogger logger, DiscordShardedClient client)
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
            Logger = _logger;
            CommandHandler _handler = new();

            _client.ShardReady += ShardReady;
            _client.ShardConnected += ShardConnected;
            _client.ShardDisconnected += ShardDisconnected;

            if (_client.Shards.Count != 1)
            {
                await Task.Delay(30000);
            }
            await _logger.Log("i", "Starting Command Handler..");
            await _handler.InitializeAsync(_client);
            await _logger.Log("√", "Command Handler Started!");

            _client.JoinedGuild += JoinedNewGuildActions;
            _client.LeftGuild += ClientLeftGuildTask;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task ShardConnected(DiscordSocketClient arg)
        {
            shardsConnected.Add(arg.ShardId);
            return Task.CompletedTask;
        }

        private async Task ShardDisconnected(System.Exception arg1, DiscordSocketClient arg2)
        {
            await _logger.Log("X", $"Shard {arg2.ShardId} disconnected!");
            if (arg2.ShardId == 0)
            {
                shardsConnected.Remove(shardsConnected.Find(x=> x == 0));
                return;
            }
            var dsc = shardsConnected.Find(x => x == arg2.ShardId);
            if (dsc != 0)
            {
                shardsConnected.Remove(dsc);
            }
        }

        private async Task ShardReady(DiscordSocketClient arg)
        {
            await _logger.Log("√", $"Shard {arg.ShardId} Connected");
            if (!shardsConnected.Exists(x=> x == arg.ShardId))
            {
                shardsConnected.Add(arg.ShardId);
            }
            
            if (shardsConnected.Count == ShardCount || Connection.Client.CurrentUser.Id == 587623068461957121)
            {
                await _logger.Log("i", "Starting ServerStatusTimer & GuildsCountTimer");
                await StatusTimer.StartServerStatusTimer();
                await GuildsTimer.StartGuildsCountTimer();
            }
        }

        private async Task ClientLeftGuildTask(SocketGuild arg)
        {
            await Database.DeleteServerConfig(arg.Id);
            await Reporter.SendLeftServers(arg);
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            await Reporter.SendJoinedServerEmbedAsync(guild);
            await Database.SetGuild(guild.Id);
            var channel = guild.DefaultChannel;
            foreach (var chnl in guild.TextChannels)
            {
                if (chnl.Name.ToLowerInvariant().Contains("bot"))
                {
                    channel = chnl;
                    break;
                }
            }
            try
            {
                await channel.SendMessageAsync(Constants.JoinedMessage);
            }
            catch (System.Exception)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{Constants.FailedToSendJoinedMessage}\n{guild.Name}[{guild.Id}]", 255, 165);
                await Reporter.SendEmbedToBotLogsChannel(embed.ToEmbedBuilder());
            }
        }
    }
}
