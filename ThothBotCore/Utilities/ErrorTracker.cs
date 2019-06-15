using Discord;
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
                embed.WithTitle($":new:{guild.Name}\n" +
                $":id:**Server ID:** {guild.Id}\n" +
                $":bust_in_silhouette:**Owner:** {guild.Owner}\n");
                embed.AddField(x =>
                {
                    x.Name = $":busts_in_silhouette:**Users:** {guild.MemberCount}\n" +
                $":speech_balloon:**Channels:** {guild.Channels.Count - guild.CategoryChannels.Count}\n" +
                $":alarm_clock:**Joined at:** {DateTime.Now.ToString("HH:mm dd.MM.yyyy")}";
                    x.Value = "\u200b";
                });
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
                embed.WithTitle($":small_red_triangle_down:{guild.Name}\n" +
                $":id:**Server ID:** {guild.Id}\n" +
                $":bust_in_silhouette:**Owner:** {guild.Owner}");
                embed.AddField(x =>
                {
                    x.Name = $":busts_in_silhouette:**Users:** {guild.MemberCount}\n" +
                $":speech_balloon:**Channels:** {guild.Channels.Count - guild.CategoryChannels.Count}\n" +
                $":alarm_clock:**Left at:** {DateTime.Now.ToString("HH:mm dd.MM.yyyy")}";
                    x.Value = "\u200b";
                });

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
