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
        public List<int> shardsConnected = new();

        public static DiscordShardedClient Client;

        public Connection(DiscordLogger logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
        }

        internal async Task ConnectAsync()
        {
            _client.Log += _logger.Log;

            await _client.LoginAsync(TokenType.Bot, Credentials.botConfig.Token);
            await _client.StartAsync().ConfigureAwait(false);
            Client = _client;
            CommandHandler _handler = new();
            await _handler.InitializeAsync(_client);

            _client.ShardReady += ShardReady;
            _client.ShardDisconnected += ShardDisconnected;
            _client.JoinedGuild += JoinedNewGuildActions;
            _client.LeftGuild += ClientLeftGuildTask;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task ShardDisconnected(System.Exception arg1, DiscordSocketClient arg2)
        {
            if (arg2.ShardId == 0)
            {
                shardsConnected.Remove(shardsConnected.Find(x=> x == 0));
                return Task.CompletedTask;
            }
            var dsc = shardsConnected.Find(x => x == arg2.ShardId);
            if (dsc != 0)
            {
                shardsConnected.Remove(dsc);
            }
            return Task.CompletedTask;
        }

        private Task ShardReady(DiscordSocketClient arg)
        {
            Text.WriteLine($"Shard Connected: {arg.ShardId}", System.ConsoleColor.Green, System.ConsoleColor.Black);
            if (!shardsConnected.Exists(x=> x == arg.ShardId))
            {
                shardsConnected.Add(arg.ShardId);
            }
            
            if (shardsConnected.Count == 3)
            {
                StatusTimer.StartServerStatusTimer();
                GuildsTimer.StartGuildsCountTimer();
            }

            return Task.CompletedTask;
        }

        private async Task ClientLeftGuildTask(SocketGuild arg)
        {
            await Database.DeleteServerConfig(arg.Id);
            await Reporter.SendLeftServers(arg);
        }

        private async Task JoinedNewGuildActions(SocketGuild guild)
        {
            await Reporter.SendJoinedServerEmbedAsync(guild);
            await Database.SetGuild(guild.Id, guild.Name);
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
