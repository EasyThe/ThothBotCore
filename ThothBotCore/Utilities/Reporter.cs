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
        private static IUser ownerUser = Connection.Client.GetUser(Constants.OwnerID);

        public static async Task SendDMtoOwner(string message)
        {
            try
            {
                await UserExtensions.SendMessageAsync(ownerUser, message);
            }
            catch (Exception ex)
            {

                await SendError($"Error in SendDMtoOwner\n**Message**: {ex.Message}").ConfigureAwait(false);
            }
        }

        public static async Task SendJoinedServers(SocketGuild guild)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(255, 255, 255));
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
                embed.WithColor(new Color(255, 0, 0));
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

        public static async Task SendSuccessCommands(string message)
        {
            try
            {
                await commandsChannel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {

                await SendError($"Error in SendSuccessCommands\n**Message**: {ex.Message}").ConfigureAwait(false);
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
        public static async Task SendException(Exception ex, SocketCommandContext context)
        {
            try
            {
                await reportsChannel.SendMessageAsync($"**Message: **{context.Message.Content}\n" +
                    $"**User: **{context.Message.Author}\n" +
                    $"**Server and Channel: **{context.Guild.Id}[{context.Channel.Id}]\n" +
                    $"**Exception Message: **{ex.Message}\n" +
                    $"**Stack Trace:** {ex.StackTrace}");
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
            }
        }

        public static async Task SendEmbedError(EmbedBuilder embed)
        {
            try
            {
                await reportsChannel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await reportsChannel.SendMessageAsync($"Error in SendEmbedError().\n{ex.Message}");
            }
        }
        public static async Task<Embed> RespondToCommandOnErrorAsync(Exception ex, SocketCommandContext context)
        {
            var sb = new StringBuilder();
            if (ex.Message.ToLowerInvariant().Contains("the api is unavailable"))
            {
                sb.Append("Sorry, the Hi-Rez API is unavailable right now. Please try again later.");
            }
            else
            {
                sb.Append("An unexpected error has occured. Please try again later.\nIf the error persists, don't hesitate to contact the bot owner for further assistance.");
                await SendException(ex, context);
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
                x.Name = $"{user.Username}#{user.DiscriminatorValue}";
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
