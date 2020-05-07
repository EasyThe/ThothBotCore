using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
        public IServiceProvider _services;
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _services = ConfigureServices();
            _commands = _services.GetRequiredService<CommandService>();
            Global.commandService = _commands;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _client.MessageReceived += HandleCommandAsync;
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;

            try
            {
                if ((msg.HasStringPrefix(Credentials.botConfig.prefix, ref argPos)
                || msg.HasStringPrefix(Database.GetServerConfig(context.Guild.Id).Result[0].prefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) && !msg.Author.IsBot)
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (result.IsSuccess)
                    {
                        Global.CommandsRun++;
                    }
                    else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) && msg.Content.ToLowerInvariant().Contains("love"))
                    {
                        await context.Channel.SendMessageAsync($"I love you too, {msg.Author.Mention} :heart:");
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
                        await ErrorHandler(context, result).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Guild: {context.Guild.Name} [{context.Guild.Id}]\nMessage: {context.Message.Content}");
                await ErrorHandler(context, null, ex);
            }
        }

        public static async Task ErrorHandler(SocketCommandContext context, IResult result = null, Exception exception = null)
        {
            string errorString = "";
            if (result == null)
            {
                errorString = exception.Message;
            }
            else
            {
                errorString = result.ErrorReason;
            }

            if (errorString.ToLowerInvariant().Contains("few parameters"))
            {
                await context.Channel.SendMessageAsync($"Please check the command usage with **{Credentials.botConfig.prefix}help**");
            }
            else if (errorString.ToLowerInvariant().Contains("user requires guild permission administrator") ||
                errorString.ToLowerInvariant().Contains("group owner failed"))
            {
                await context.Channel.SendMessageAsync("You must to have **Administrator** permission in this server to use this command.");
            }
            else if (errorString.ToLowerInvariant().Contains("channel not found"))
            {
                await context.Channel.SendMessageAsync("Channel not found.");
            }
            else if (errorString.ToLowerInvariant().Contains("command must be used in a guild channel"))
            {
                await context.Channel.SendMessageAsync("Command must be used in a server channel.");
            }
            else if (errorString.ToLowerInvariant().Contains("50013") || errorString.ToLowerInvariant().Contains("embedlinks"))
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
            else if (errorString.ToLowerInvariant().Contains("1024"))
            {
                await context.Channel.SendMessageAsync("Oops. The text is too long so, I was not able to fit the help command in one field.");
            }
            else if (errorString.ToLowerInvariant().Contains("command can only be run by the owner of the bot."))
            {
                return;
            }
            else
            {
                Console.WriteLine("Error Tracker: " + errorString);
                await ErrorTracker.SendError($"**Message: **{context.Message.Content}\n" +
                    $"**User: **{context.Message.Author}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Error: **{result.Error}\n" +
                    $"**Error Reason: **{result.ErrorReason}\n" +
                    $"{(exception != null ? $"**Stack Trace:** {exception.StackTrace}\n**Source: **{exception.Source}" : "")}");
                await context.Channel.SendMessageAsync("Something went wrong. Bot owner was notified about this error and will take care of it as soon as possible. Sorry for the inconvenience caused.");
            }
        }
    }
}
