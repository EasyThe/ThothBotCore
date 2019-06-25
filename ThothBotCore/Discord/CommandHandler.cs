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
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;

            if ((msg.HasStringPrefix(Credentials.botConfig.prefix, ref argPos)
                || msg.HasStringPrefix(Database.GetServerConfig(context.Guild).Result[0].prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) && !msg.Author.IsBot)
            {
                var result = await _commands.ExecuteAsync(context, argPos, null);
                if (result.IsSuccess)
                {
                    Global.CommandsRun++;
                }
                if ((result.IsSuccess || !result.IsSuccess) && context.Guild.Id != 518408306415632384 && context.Message.Author.Id != 171675309177831424)
                {
                    await ErrorTracker.SendSuccessCommands("**Result: **" +
                            (result.IsSuccess ? ":white_check_mark:" : ":negative_squared_cross_mark:") +
                            $"\n**Message: **{context.Message.Content}\n" +
                            $"**User: **{context.Message.Author}\n" +
                            $"**Server: **{context.Guild.Name} **[**{context.Guild.Id}**] **(**{context.Guild.Users.Count}**)");
                }
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await ErrorHandler(result, context).ConfigureAwait(false);
                }
            }
        }

        private async Task ErrorHandler(IResult result, SocketCommandContext context)
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
                    await context.Channel.SendMessageAsync($"I don't have **Send Messages** or **Embed Links** permissions in #{context.Channel.Name}");
                }
                catch (Exception)
                {
                    IUser user = Connection.Client.GetUser(context.User.Id);
                    await user.SendMessageAsync($"I don't have **Send Messages** or **Embed Links** permissions in #{context.Channel.Name}");
                }
            }
            else if (result.ErrorReason.Contains("1024"))
            {
                await context.Channel.SendMessageAsync("Oops. Because your custom prefix is too long, I was not able to fit the help command in one field.");
            }
            else if (result.ErrorReason.Contains("Command can only be run by the owner of the bot."))
            {
                return;
            }
            else
            {
                Console.WriteLine(result.Error + " " + result.ErrorReason);
                await ErrorTracker.SendError($"**Message: **{context.Message.Content}\n" +
                    $"**User: **{context.Message.Author}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Error: **{result.Error}\n" +
                    $"**Error Reason: **{result.ErrorReason}");
                await context.Channel.SendMessageAsync("Something went wrong. Bot owner was notified about this error and will take care of it as soon as possible. Sorry for the inconvenience caused.");
            }
        }
    }
}
