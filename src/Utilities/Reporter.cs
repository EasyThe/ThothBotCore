using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Sentry;
using System;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Discord;

namespace ThothBotCore.Utilities
{
    public static class Reporter
    {
        private static SocketTextChannel reportsChannel;
        private static SocketTextChannel joinsChannel;
        private static SocketTextChannel commandsChannel;
        private static SocketTextChannel feedbackChannel;
        private static SocketTextChannel botlogs;
        private static SocketTextChannel changelogChannel;
        private static IUser ownerUser;

        private static Task ChannelLoader()
        {
            reportsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(557974702941798410);
            joinsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(567495039622709268);
            commandsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(569710679796482068);
            feedbackChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(713183236238344193);
            botlogs = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(734987439353102426);
            ownerUser = Connection.Client.GetUser(Constants.OwnerID);
            changelogChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(567192879026536448);

            return Task.CompletedTask;
        }
        private static Task ChannelChecker(SocketTextChannel textChannel)
        {
            if (textChannel == null)
            {
                ChannelLoader();
            }
            return Task.CompletedTask;
        }
        public static async Task SendJoinedServerEmbedAsync(SocketGuild guild, bool couldntSend = true)
        {
            await ChannelChecker(joinsChannel);
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(33, 222, 124));
                embed.WithAuthor(x =>
                {
                    x.Name = $"Server #{Connection.Client.Guilds.Count}";
                });
                string result = $"{(!couldntSend ? "🚫" : "")}🆕{guild.Name}\n" +
                $"🆔**Server ID:** {guild.Id}\n" +
                $"👤**Owner:** {guild.Owner} [{guild.OwnerId}]\n" +
                $"👥**Users:** {guild.MemberCount}\n" +
                $"💬**Channels:** {guild.Channels.Count - guild.CategoryChannels.Count}";
                if (result.Length >= 256)
                {
                    embed.WithDescription(result);
                }
                else
                {
                    embed.WithTitle(result);
                }

                if (guild.IconUrl != null || guild.IconUrl == "")
                {
                    embed.ImageUrl = guild.IconUrl;
                }
                await joinsChannel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorAsync($"Error in SendJoinedServers\n**Message**: {ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`").ConfigureAwait(false);
            }
        }

        public static async Task SendLeftServers(SocketGuild guild)
        {
            await ChannelChecker(joinsChannel);
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(254, 0, 0));
                string result = $"🔻{guild.Name}\n" +
                $"🆔**Server ID:** {guild.Id}\n" +
                $"👤**Owner:** {guild.OwnerId}\n" +
                $"👥**Users:** {guild.MemberCount}\n" +
                $"💬**Channels:** {guild.Channels.Count - guild.CategoryChannels.Count}";
                if (result.Length >= 256)
                {
                    embed.WithDescription(result);
                }
                else
                {
                    embed.WithTitle(result);
                }

                if (guild.IconUrl != null || guild.IconUrl == "")
                {
                    embed.ImageUrl = guild.IconUrl;
                }
                await joinsChannel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await SendErrorAsync($"Error in SendLeftServers\n" +
                    $"**Message**: {ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`").ConfigureAwait(false);
            }
        }

        public static async Task SendSuccessCommands(SocketCommandContext context, IResult result)
        {
            await ChannelChecker(commandsChannel);
            try
            {
                string message;
                Embed embed;
                if (result == null)
                {
                    message = $"{context.Message.Author.Username}#{context.Message.Author.DiscriminatorValue} loves me ♥";
                }
                else
                {
                    message = $"\n**Message: **{context.Message.Content}\n" +
                                $"**User: **{context.Message.Author} [{context.Message.Author.Id}]\n" +
                                $"**Server: **{context.Guild.Name} [{context.Guild.Id}]\n" +
                                $"**Channel: **{context.Channel.Name} [{context.Channel.Id}]";
                }
                if (result != null && result.IsSuccess)
                {
                    embed = await EmbedHandler.BuildDescriptionEmbedAsync(message, 33, 222, 124);
                }
                else
                {
                    embed = await EmbedHandler.BuildDescriptionEmbedAsync(message, 254);
                }
                await commandsChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                await SendException(ex, context, ex.Message);
                await SendErrorAsync($"Error in SendSuccessCommands\n**Message**: {ex.Message}");
            }
        }

        public static async Task SendErrorAsync(string message)
        {
            await ChannelChecker(reportsChannel);
            try
            {
                var em = await EmbedHandler.BuildDescriptionEmbedAsync($"SendErrorAsync()\n{message}");
                await reportsChannel.SendMessageAsync(embed: em);
            }
            catch (Exception ex)
            {
                Text.WriteLine($"{ex.Message}\n{message}");
            }
        }
        public static async Task SendException(Exception ex, SocketCommandContext context, string errorMessage = "")
        {
            // Discontinued due to switch to slash commands
            return;
            await ChannelChecker(reportsChannel);
            try
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"**Message: **{context.Message.Content}\n" +
                    $"**User: **{context.Message.Author} [{context.Message.Author.Id}]\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Exception Message: **{(ex != null ? ex.Message : errorMessage)}\n" +
                    $"```csharp\n{(ex != null ? ex.StackTrace : errorMessage)}```", 254);
                await reportsChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception exc)
            {
                Text.WriteLine("\t=== Couldn't send error to reports channnel." +
                                "\n" +
                                $"\t\tMessage: {context.Message.Content}\n" +
                                $"\t\tUser: {context.Message.Author}\n" +
                                $"\t\tServer and Channel: {context.Guild.Id}[{context.Channel.Id}]\n" +
                                $"\t\tException Message: {ex.Message}\n" +
                                $"\t\tData: {ex.Data}\n" +
                                $"\t\tStack Trace: {ex.StackTrace}\n" +
                                $"\t\tInnerException: {ex.InnerException}" +
                                "\n\t===\n" + exc.Message);
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"**Message: **{context.Message.Content}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Inner Exception Message: **{(ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception")}\n" +
                    $"```csharp\n{(ex != null ? ex.StackTrace : errorMessage)}```", 254);
                await reportsChannel.SendMessageAsync(embed: embed);
                await reportsChannel.SendMessageAsync($"{ownerUser.Mention} **Check the console for an error!**");
            }
        }

        public static async Task SlashSendException(Exception ex, IInteractionContext context, string errorMessage = "")
        {
            await ChannelChecker(reportsChannel);
            try
            {
                StringBuilder sb = new();
                if (context?.Interaction.Data is SocketSlashCommandData data)
                {
                    sb.Append($"**SlashCommand:** {data.Name}");
                    if (data.Options != null && data.Options.Count != 0)
                    {
                        foreach (var item in data.Options)
                        {
                            sb.Append($" `{item.Name}: {item.Value}`");
                        }
                    }
                }
                else if (context?.Interaction.Data is SocketMessageComponentData compData)
                {
                    sb.Append($"**Component:** {compData.CustomId}");
                    if (compData.Value != null && compData.Value.Length != 0)
                    {
                        sb.Append($" `{compData.Value}`");
                    }
                    else if (compData.Values != null && compData.Values.Count != 0)
                    {
                        foreach (var item in compData.Values)
                        {
                            sb.Append($" `{item}`");
                        }
                    }
                } // any other?
                else if (context?.Interaction.Data is IModalInteractionData modalData)
                {
                    sb.Append($"**Modal:** {modalData.CustomId}");
                }
                else
                {
                    sb.Append(context?.Interaction.Data);
                    
                }
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{sb}\n" +
                    $"**User: **{context?.Interaction.User} [{context?.Interaction.User.Id}]\n" +
                    $"**Server and Channel: **{context?.Guild.Id}[{context?.Interaction.ChannelId}]\n" +
                    $"**Exception Message: **{(ex != null ? ex.Message : errorMessage)}\n" +
                    $"```csharp\n{(ex != null ? ex.StackTrace : errorMessage)}```", 200);
                await reportsChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception exc)
            {
                Text.WriteLine("\t=== Couldn't send error to reports channnel." +
                                "\n" +
                                $"\t\tInteraction type & Data: {context.Interaction.Type} {context.Interaction.Data}\n" +
                                $"\t\tUser: {context.Interaction.User}\n" +
                                $"\t\tServer and Channel: {context.Guild.Id}[{context.Channel.Id}]\n" +
                                $"\t\tException Message: {ex.Message}\n" +
                                $"\t\tData: {ex.Data}\n" +
                                $"\t\tStack Trace: {ex.StackTrace}\n" +
                                $"\t\tInnerException: {ex.InnerException}" +
                                "\n\t===\n" + exc.Message);
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{context.Interaction.Type} {context.Interaction.Data}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Inner Exception Message: **{(ex.InnerException != null ? ex.InnerException.Message : "No Inner Exception")}\n" +
                    $"```csharp\n{(ex != null ? ex.StackTrace : errorMessage)}```", 200);
                await reportsChannel.SendMessageAsync(embed: embed);
                await reportsChannel.SendMessageAsync($"{ownerUser.Mention} **Check the console for an error!**");
            }
        }

        public static async Task SendEmbedToBotLogsChannelAsync(EmbedBuilder embed) =>
            await SendEmbedToBotLogsChannelAsync(embed.Build());
        public static async Task SendEmbedToBotLogsChannelAsync(Embed embed)
        {
            await ChannelChecker(botlogs);
            try
            {
                await botlogs.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                await reportsChannel.SendMessageAsync($"Error in SendEmbedToBotLogsChannel().\n{ex.Message}");
            }
        }
        public static async Task SendMsgToBotLogsChannel(string message)
        {
            await ChannelChecker(botlogs);
            try
            {
                await botlogs.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await reportsChannel.SendMessageAsync($"Error in SendMsgToBotLogsChannel().\n{ex.Message}");
            }
        }
        public static async Task<Embed> RespondToCommandOnErrorAsync(Exception ex, SocketCommandContext context, string errorMessage = "")
        {
            return await EmbedHandler.BuildDescriptionEmbedAsync(
                $"Normal commands have been discontinued and support for using them won't be provided. " +
                $"Please use [slash commands](https://discord.com/blog/welcome-to-the-new-era-of-discord-apps)." +
                $"\nVisual representation of all available `/` slash commands can be found [here](https://easythe.github.io/ThothWeb/)", 255, 0, 0);
            // delete all this
            var sb = new StringBuilder();
            if (errorMessage == "apidown" || (ex != null && ex.Message.ToLowerInvariant().Contains("the api is unavailable")))
            {
                sb.Append("Sorry, the Hi-Rez API is unavailable right now. Please try again later.");
            }
            else if (ex != null && ex.Message.Contains("Value was either too large or too small for an Int32"))
            {
                sb.Append($"The value was either too large or too small.\n\nIf you believe that this is wrong, please [contact]({Constants.SupportServerInvite}) the bot developer for further assistance.");
            }
            else if (ex != null && !(ex.Message.ToLowerInvariant().Contains("database")))
            {
                sb.Append($"An unexpected error has occured. Normal commands have been discontinued and support for using them won't be provided. Please use [slash commands](https://discord.com/blog/welcome-to-the-new-era-of-discord-apps).");
                //await SendException(ex, context, errorMessage);
                //SentrySdk.CaptureException(ex);
            }
            else if (ex == null && errorMessage != "")
            {
                sb.Append($"An unexpected error has occured.");
                await SendErrorAsync(errorMessage);
            }
            if (Global.ErrorMessageByOwner != null || Global.ErrorMessageByOwner != "")
            {
                sb.Append("\n" + Global.ErrorMessageByOwner);
            }
            return await EmbedHandler.BuildDescriptionEmbedAsync(sb.ToString(), 183, 0, 0);
        }

        public static async Task<Embed> SlashRespondToCommandOnErrorAsync(Exception ex, IInteractionContext context, string errorMessage = "")
        {
            var sb = new StringBuilder();
            if (errorMessage == "apidown" || (ex != null && ex.Message.ToLowerInvariant().Contains("the api is unavailable")))
            {
                sb.Append("Sorry, the Hi-Rez API is unavailable right now. Please try again later.");
            }
            else if (ex != null && !(ex.Message.ToLowerInvariant().Contains("database")))
            {
                sb.Append($"{(Global.ErrorMessageByOwner != null && Global.ErrorMessageByOwner != "" ? Global.ErrorMessageByOwner : "An unexpected error has occured. Please try again later.")}" +
                    $"\nIf the error persists, don't hesitate to [contact]({Constants.SupportServerInvite}) the bot developer for further assistance.");
                await SlashSendException(ex, context, errorMessage);
                SentrySdk.CaptureException(ex);
            }
            else if (ex == null && errorMessage != "")
            {
                sb.Append($"An unexpected error has occured.");
                await SendErrorAsync(errorMessage);
            }
            if ((Global.ErrorMessageByOwner != null || Global.ErrorMessageByOwner != "") && !sb.ToString().StartsWith(Global.ErrorMessageByOwner))
            {
                sb.Append("\n" + Global.ErrorMessageByOwner);
            }
            return await EmbedHandler.BuildDescriptionEmbedAsync(sb.ToString(), 183, 0, 0);
        }

        public static async Task SendFeedback(string message, SocketCommandContext context)
        {
            await ChannelChecker(feedbackChannel);
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = $"{context.User}";
                x.IconUrl = context.User.GetAvatarUrl();
            });
            embed.WithColor(Constants.FeedbackColor);
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = $"{context.User.Id} {context.Guild.Id} {context.Channel.Id}";
            });
            await feedbackChannel.SendMessageAsync(embed: embed.Build());
        }
        public static async Task SendFeedback(string message, IInteractionContext context)
        {
            await ChannelChecker(feedbackChannel);
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = $"{context.User}";
                x.IconUrl = context.User.GetAvatarUrl();
            });
            embed.WithColor(Constants.FeedbackColor);
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = $"{context.Interaction.User.Id} {context.Guild?.Id} {context.Channel.Id}";
            });
            await feedbackChannel.SendMessageAsync(embed: embed.Build());
        }
        public static async Task SendChangelog(string message)
        {
            await ChannelChecker(changelogChannel);
            await changelogChannel.SendMessageAsync(message);
        }
    }
}
