using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ThothBotCore.Utilities
{
    public static class ErrorTracker
    {
        private static SocketTextChannel reportsChannel = Discord.Connection.Client.GetGuild(518408306415632384).GetTextChannel(557974702941798410);
        private static SocketTextChannel joinsChannel = Discord.Connection.Client.GetGuild(518408306415632384).GetTextChannel(567495039622709268);
        private static SocketTextChannel commandsChannel = Discord.Connection.Client.GetGuild(518408306415632384).GetTextChannel(569710679796482068);
        private static IUser ownerUser = Discord.Connection.Client.GetUser(171675309177831424);

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
                    x.Name = $"Server #{Discord.Connection.Client.Guilds.Count}";
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
                    $"**Data: **{ex.Data}\n" +
                    $"**Stack Trace:** {ex.StackTrace}\n" +
                    $"**Source: **{ex.Source}");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public static async Task SendEmbedError(EmbedBuilder embed)
        {
            try
            {
                await reportsChannel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await reportsChannel.SendMessageAsync($"Error in SendEmbedError.\n{ex.Message}");
            }
        }
    }
}
