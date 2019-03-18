using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;

namespace ThothBotCore.Discord
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg))
            {
                return;
            }

            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;

            if (msg.HasStringPrefix(Credentials.botConfig.prefix, ref argPos)
                || msg.HasStringPrefix(Database.GetPrefix(context.Guild)[0].prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos) && msg.Author.IsBot == false)
            {
                var result = await _service.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}
