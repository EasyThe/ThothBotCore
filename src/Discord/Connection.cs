using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public class Connection
    {
        private readonly DiscordShardedClient _client;
        private readonly DiscordLogger _logger;
        public static List<int> shardsConnected = new();
        public const int ShardCount = 5;
        
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
            try
            {
                await _logger.Log("√", $"Shard {arg.ShardId} Connected");
                if (!shardsConnected.Exists(x => x == arg.ShardId))
                {
                    shardsConnected.Add(arg.ShardId);
                }

                if (shardsConnected.Count == ShardCount || Client.CurrentUser.Id == 587623068461957121)
                {
                    await _logger.Log("i", "Starting ServerStatusTimer & GuildsCountTimer");
                    await StatusTimer.StartServerStatusTimer();
                    await GuildsTimer.StartGuildsCountTimer();

                    // Register commands
                    await RegisterSlashCommandsGlobally();
                }
            }
            catch (System.Exception ex)
            {
                await _logger.Log("X", $"ShardReady Exception: {ex.Message}");
            }
        }

        public async Task RegisterSlashCommandsGlobally()
        {
            try
            {
                if (Global.interactionService != null)
                {
                    // Register slash commands to test server
                    if (Credentials.botConfig.prefix != "??")
                    {
                        await Global.interactionService.RegisterCommandsGloballyAsync(true);
                        await _logger.Log("√", "Registered commands globally!");
                    }
                    else
                    {
                        await Global.interactionService.RegisterCommandsToGuildAsync(518408306415632384, true);
                        //await Global.interactionService.RegisterCommandsGloballyAsync(true);
                        await _logger.Log("√", "Registered commands to dev guild!");
                    }
                }
            }
            catch (System.Exception ex)
            {
                await _logger.Log("X", $"Registering commands failed. Exception: {ex.Message}");
                await Reporter.SendErrorAsync($"Registering commands failed. Exception:\n{ex.Message}");
            }
        }

        private async Task ClientLeftGuildTask(SocketGuild arg)
        {
            await MongoConnection.RemoveGuildSettings(arg.Id);

            await Reporter.SendLeftServers(arg);
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            await Reporter.SendJoinedServerEmbedAsync(guild);
        }
    }
}
