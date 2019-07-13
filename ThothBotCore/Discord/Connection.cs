﻿using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
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
            await _handler.InitializeAsync(_client);

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
                await channel.SendMessageAsync(":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can set a custom prefix for your server with {Credentials.botConfig.prefix}prefix `your-prefix-here`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`\n" +
                $":small_orange_diamond:Please make sure I have **Send Messages**, **Read Messages**, **Embed Links** and **Use External Emojis** in the channels you would like me to react to your commands.");
            }
            catch (System.Exception)
            {
                System.Console.WriteLine("Couldn't send JoinedMessage to the Guild.");
            }
        }
    }
}
