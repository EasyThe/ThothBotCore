using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Discord;

namespace ThothBotCore.Utilities
{
    public static class Reporter
    {
        private static SocketTextChannel reportsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(557974702941798410);
        private static SocketTextChannel joinsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(567495039622709268);
        private static SocketTextChannel commandsChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(569710679796482068);
        private static SocketTextChannel feedbackChannel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(713183236238344193);
        private static SocketTextChannel botlogs = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(734987439353102426);
        private static IUser ownerUser = Connection.Client.GetUser(Constants.OwnerID);

        public static async Task SendJoinedServerEmbedAsync(SocketGuild guild)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(33, 222, 124));
                embed.WithAuthor(x =>
                {
                    x.Name = $"Server #{Connection.Client.Guilds.Count}";
                });
                string result = $"🆕{guild.Name}\n" +
                $"🆔**Server ID:** {guild.Id}\n" +
                $"👤**Owner:** {guild.Owner}\n" +
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
                await SendError($"Error in SendJoinedServers\n**Message**: {ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`").ConfigureAwait(false);
            }
        }

        public static async Task SendLeftServers(SocketGuild guild)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(254, 0, 0));
                string result = $"🔻{guild.Name}\n" +
                $"🆔**Server ID:** {guild.Id}\n" +
                $"👤**Owner:** {guild.Owner}\n" +
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
                await SendError($"Error in SendLeftServers\n" +
                    $"**Message**: {ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`").ConfigureAwait(false);
            }
        }

        public static async Task SendSuccessCommands(SocketCommandContext context, IResult result)
        {
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
                if (result.IsSuccess)
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
                await SendError($"Error in SendSuccessCommands\n**Message**: {ex.Message}");
            }
        }

        public static async Task SendError(string message)
        {
            try
            {
                await reportsChannel.SendMessageAsync(message + $"\n{DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task SendException(Exception ex, SocketCommandContext context, string errorMessage = "")
        {
            try
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"**Message: **{context.Message.Content}\n" +
                    $"**User: **{context.Message.Author}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Exception Message: **{(ex != null ? ex.Message : errorMessage)}\n" +
                    $"```csharp\n{(ex != null ? ex.StackTrace : errorMessage)}```", 254);
                await reportsChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception exc)
            {
                Console.WriteLine("\t===" +
                    "\n\tCouldn't send error to reports channnel." +
                    "\n" +
                    $"\t\tMessage: **{context.Message.Content}\n" +
                    $"\t\tUser: **{context.Message.Author}\n" +
                    $"\t\tServer and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"\t\tException Message: **{ex.Message}\n" +
                    $"\t\tData: **{ex.Data}\n" +
                    $"\t\tStack Trace:** {ex.StackTrace}\n" +
                    $"\t\tSource: {ex.Source}" +
                    "\n\t===\n" + exc.Message);
                await reportsChannel.SendMessageAsync($"{ownerUser.Mention} **Check the console for an error!**");
            }
        }

        public static async Task SendEmbedToBotLogsChannel(EmbedBuilder embed)
        {
            try
            {
                await botlogs.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await reportsChannel.SendMessageAsync($"Error in SendEmbedError().\n{ex.Message}");
            }
        }
        public static async Task<Embed> RespondToCommandOnErrorAsync(Exception ex, SocketCommandContext context, string errorMessage = "")
        {
            var sb = new StringBuilder();
            if (ex != null && ex.Message.ToLowerInvariant().Contains("the api is unavailable"))
            {
                sb.Append("Sorry, the Hi-Rez API is unavailable right now. Please try again later.");
            }
            else if (ex != null && !(ex.Message.ToLowerInvariant().Contains("database")))
            {
                sb.Append($"An unexpected error has occured. Please try again later.\nIf the error persists, don't hesitate to [contact]({Constants.SupportServerInvite}) the bot owner for further assistance.");
                await SendException(ex, context, errorMessage);
            }
            if (Global.ErrorMessageByOwner != null || Global.ErrorMessageByOwner != "")
            {
                sb.Append("\n" + Global.ErrorMessageByOwner);
            }
            return await EmbedHandler.BuildDescriptionEmbedAsync(sb.ToString(), 183, 0, 0);
        }

        public static async Task SendFeedback(string message, SocketUser user)
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = $"{user}";
                x.IconUrl = user.GetAvatarUrl();
            });
            embed.WithColor(Constants.FeedbackColor);
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = $"ID: {user.Id} | First Mutual Guild: {user.MutualGuilds.First().Id}";
            });
            await feedbackChannel.SendMessageAsync(embed: embed.Build());
        }
    }
}
