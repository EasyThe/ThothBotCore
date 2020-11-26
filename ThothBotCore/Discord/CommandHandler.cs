using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using System;
using System.Linq;
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
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (result.IsSuccess)
                    {
                        Global.CommandsRun++;
                    }
                    else if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) && msg.Content.ToLowerInvariant().Contains("love"))
                    {
                        await context.Channel.SendMessageAsync($"I love you too, {msg.Author.Mention} :heart:");
                        await Reporter.SendSuccessCommands(context, null);
                    }
                    if ((result.IsSuccess || !result.IsSuccess) && context.Message.Author.Id != Constants.OwnerID)
                    {
                        await Reporter.SendSuccessCommands(context, result);
                    }
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        await ErrorHandler(context, result);
                    }
                }
                else if (msg.Content == $"<@!{_client.CurrentUser.Id}>")
                {
                    await context.Channel.SendMessageAsync($"{context.Message.Author.Mention}, my prefix is `{Credentials.botConfig.prefix}`");
                }
                else if (msg.MentionedUsers.Any(x=> x.Id == Connection.Client.CurrentUser.Id) && msg.Content.ToLowerInvariant().Contains("love"))
                {
                    await context.Channel.SendMessageAsync($"I love you too, {msg.Author.Mention} :heart:");
                    await Reporter.SendSuccessCommands(context, null);
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Text.WriteLine($"Guild: {context.Guild.Name} [{context.Guild.Id}]\nMessage: {context.Message.Content}");
                await ErrorHandler(context, null, ex);
            }
        }

        public static async Task ErrorHandler(SocketCommandContext context, IResult result = null, Exception exception = null)
        {
            string errorString;
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
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"Please check the command usage with **{Credentials.botConfig.prefix}help**", 254);
                await context.Channel.SendMessageAsync(embed: embed);
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
            else if (errorString.ToLowerInvariant().Contains("missing access"))
            {
                IUser user = Connection.Client.GetUser(context.User.Id);
                await user.SendMessageAsync($"Hey! 👋\nPlease make sure I have **Read Messages**, **Send Messages**, **Embed Links** and **Use External Emojis** permissions in #{context.Channel.Name} and try again.");
            }
            else if (errorString.ToLowerInvariant().Contains("1024"))
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"Oops. The content of this message was too long. If you think this was an error, please [contact]({Constants.SupportServerInvite}) my developer!", 254);
                await context.Channel.SendMessageAsync(embed: embed);
            }
            else if (errorString.ToLowerInvariant().Contains("command can only be run by the owner of the bot."))
            {
                return;
            }
            else if (errorString.ToLowerInvariant().Contains("failed to parse int32"))
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Please provide a valid number!", 254);
                await context.Channel.SendMessageAsync(embed: embed);
            }
            else if (errorString.Contains("permission"))
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync(errorString, 255);
                await context.Channel.SendMessageAsync(embed: embed);
            }
            else
            {
                Text.WriteLine($"{DateTime.Now:[HH:mm]}\tError Tracker(CommandHandler): " + errorString);
                await Reporter.RespondToCommandOnErrorAsync(exception, context, errorString);
            }
        }
    }
}
