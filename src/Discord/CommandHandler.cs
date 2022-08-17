using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    class CommandHandler
    {
        DiscordShardedClient _client;
        CommandService _commands;
        InteractionService _interactionService;
        HiRezAPIv2 _HiRez;
        Meter _metrics;
        List<Counter<int>> _slashCounters = new();
        List<Counter<int>> _normalCounters = new();
        List<Counter<int>> _guildCounters = new();

        public IServiceProvider _services;
        public async Task InitializeAsync(DiscordShardedClient client)
        {
            _client = client;
            _services = ConfigureServices();
            _commands = _services.GetRequiredService<CommandService>();
            _interactionService = _services.GetRequiredService<InteractionService>();
            _HiRez = _services.GetRequiredService<HiRezAPIv2>();
            _metrics = _services.GetRequiredService<Meter>();

            Global.commandService = _commands;
            Global.Metrics = _metrics;
            Global.interactionService = _interactionService;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await RegisterMetrics();

            _client.MessageReceived += HandleCommandAsync;
            _client.InteractionCreated += HandleInteractionAsync;

            _commands.CommandExecuted += CommandExecuted;
            _interactionService.InteractionExecuted += InteractionExecuted;
        }

        private Task RegisterMetrics()
        {
            foreach (var module in _interactionService.Modules)
            {
                foreach (var command in module.SlashCommands)
                {
                    _slashCounters.Add(_metrics.CreateCounter<int>($"slash_{command.Name.Replace("*", "")}"));
                }
                foreach (var command in module.ComponentCommands)
                {
                    _slashCounters.Add(_metrics.CreateCounter<int>($"comp_{command.Name.Replace("*", "")}"));
                }
                foreach (var command in module.ModalCommands)
                {
                    _slashCounters.Add(_metrics.CreateCounter<int>($"modal_{command.Name.Replace("*", "")}"));
                }
            }
            foreach (var module in _commands.Modules)
            {
                foreach (var command in module.Commands)
                {
                    _normalCounters.Add(_metrics.CreateCounter<int>($"old_{command.Name}"));
                }
            }
            return Task.CompletedTask;
        }

        private Task InteractionExecuted(ICommandInfo arg1, IInteractionContext arg2, global::Discord.Interactions.IResult arg3)
        {
            var name = arg1.Name.Replace("*", "");
            if (Connection.Client.CurrentUser.Id != 587623068461957121 && 
                arg2.User.Id == 171675309177831424)
            {
                return Task.CompletedTask;
            }
            if (arg2.Interaction.Type == InteractionType.ApplicationCommand)
            {
                _slashCounters.Find(x => x.Name == $"slash_{name}").Add(1);
            }
            else if (arg2.Interaction.Type == InteractionType.MessageComponent)
            {
                _slashCounters.Find(x => x.Name == $"comp_{name}").Add(1);
            }
            else if (arg2.Interaction.Type == InteractionType.ModalSubmit)
            {
                _slashCounters.Find(x => x.Name == $"modal_{name}").Add(1);
            }
            // guild counter
            if (arg2.Guild.Id != 518408306415632384)
            {
                try
                {
                    _guildCounters.Find(x => x.Name == $"g_{arg2.Guild.Id}").Add(1);
                }
                catch
                {
                    _guildCounters.Add(_metrics.CreateCounter<int>($"g_{arg2.Guild.Id}"));
                }
            }
            if (arg3.IsSuccess)
            {
                Global.CommandsRun++;
            }

            return Task.CompletedTask;
        }

        private Task CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, global::Discord.Commands.IResult arg3)
        {
            if (arg1.IsSpecified)
            {
                _normalCounters.Find(x => x.Name == $"old_{arg1.Value.Name}").Add(1);
            }
            return Task.CompletedTask;
        }

        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            try
            {
                if (arg.Type == InteractionType.MessageComponent)
                {
                    var cntxt = new ShardedInteractionContext<SocketMessageComponent>(_client, (SocketMessageComponent)arg);
                    await _interactionService.ExecuteCommandAsync(cntxt, _services);
                    return;
                }
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new ShardedInteractionContext(_client, arg);
                await _interactionService.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HandleInteractionAsync (CommandHandler.cs) - " + ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<InteractiveService>()
                .AddSingleton(new InteractionService(_client, new InteractionServiceConfig { UseCompiledLambda = true }))
                .AddSingleton(new HiRezAPIv2())
                .AddSingleton(new Meter("ThothBotMetrics"))
                .BuildServiceProvider();
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (s is not SocketUserMessage msg || msg.Author.IsBot)
            {
                return;
            }

            var context = new ShardedCommandContext(_client, msg);
            int argPos = 0;

            try
            {
                var serverConfig = new Models.ServerConfig { prefix = Credentials.botConfig.prefix };
                if (context.Guild != null)
                {
                    var db = await Database.GetServerConfig(context.Guild.Id);
                    if (db.FirstOrDefault() != null)
                    {
                        serverConfig = db.FirstOrDefault();
                    }
                }

                if ((msg.HasStringPrefix(Credentials.botConfig.prefix, ref argPos)
                || msg.HasStringPrefix(serverConfig.prefix, ref argPos)
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
                    if ((result.IsSuccess || !result.IsSuccess) && context.Message.Author.Id != Utilities.Constants.OwnerID)
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
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Text.WriteLine($"Guild: {context.Guild.Name} [{context.Guild.Id}]\nMessage: {context.Message.Content}");
                await ErrorHandler(context, null, ex);
            }
        }

        public static async Task ErrorHandler(SocketCommandContext context, global::Discord.Commands.IResult result = null, Exception exception = null)
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

            if (errorString.ToLowerInvariant().Contains("user requires guild permission administrator") ||
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
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"Oops. The content of this message was too long. If you think this was an error, please [contact]({Utilities.Constants.SupportServerInvite}) my developer!", 254);
                await context.Channel.SendMessageAsync(embed: embed);
            }
            else if (errorString.ToLowerInvariant().Contains("command can only be run by the owner of the bot.") ||
                     errorString.ToLowerInvariant().Contains("few parameters"))
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
