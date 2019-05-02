using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _commands;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _commands = new CommandService();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
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

            if ((msg.HasStringPrefix(Credentials.botConfig.prefix, ref argPos)
                || msg.HasStringPrefix(Database.GetPrefix(context.Guild)[0].prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) && !msg.Author.IsBot)
            {
                var result = await _commands.ExecuteAsync(context, argPos, null);
                if (result.IsSuccess || !result.IsSuccess)
                {
                    await ErrorTracker.SendSuccessCommands($"**Message: **{context.Message.Content}\n" +
                            $"**User: **{context.Message.Author}");
                }
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    if (result.ErrorReason.Contains("few parameters"))
                    {
                        await context.Channel.SendMessageAsync("Please check the command usage in **!!help**");
                    }
                    else if (result.ErrorReason.ToLower().Contains("user requires guild permission administrator"))
                    {
                        await context.Channel.SendMessageAsync("You must to have **Administrator** permission in this server to use this command.");
                    }
                    else if (result.ErrorReason.ToLower().Contains("channel not found"))
                    {
                        await context.Channel.SendMessageAsync("Channel not found.");
                    }
                    else if (result.ErrorReason.Contains("Command must be used in a guild channel"))
                    {
                        await context.Channel.SendMessageAsync("Command must be used in a guild channel.");
                    }
                    else if (result.ErrorReason.ToLower().Contains("50013"))
                    {
                        try
                        {
                            IUser user = Connection.Client.GetUser(context.User.Id);
                            await user.SendMessageAsync($"I don't have **Send Messages** or **Embed Links** permissions in #{context.Channel.Name}");
                        }
                        catch (Exception ex)
                        {
                            await ErrorTracker.SendError($"Error./n{ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine(result.Error + " " + result.ErrorReason);
                        await ErrorTracker.SendError($"**Message: **{context.Message.Content}\n" +
                            $"**User: **{context.Message.Author}\n" +
                            $"**Error: **{result.Error}\n" +
                            $"**Error Reason: **{result.ErrorReason}");
                        await context.Channel.SendMessageAsync("Something went wrong. Bot owner was notified about this error and will take care of it as soon as possible. Sorry for the inconvenience caused.");
                    }
                }
            }
        }
    }
}
