﻿using Discord;
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

                await SendError($"Error in SendDMtoOwner\n**Message**: {ex.Message}");
            }
        }

        public static async Task SendJoinedServers(SocketGuild guild)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithColor(new Color(255, 255, 255));
                embed.WithTitle($":new:**Server name:** {guild.Name}\n" +
                $":id:**Server ID:** {guild.Id}\n" +
                $":bust_in_silhouette:**Owner:** {guild.Owner}\n" +
                $":busts_in_silhouette:**Users:** {guild.MemberCount}\n" +
                $":speech_balloon:**Channels:** {guild.Channels.Count - guild.CategoryChannels.Count}\n" +
                $":alarm_clock:**Joined at:** {DateTime.Now.ToString("HH:mm dd.MM.yyyy")}");
                if (guild.IconUrl != null || guild.IconUrl == "")
                {
                    embed.ImageUrl = guild.IconUrl;
                }
                await joinsChannel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await SendError($"Error in SendJoinedServers\n**Message**: {ex.Message}\n" +
                    $"**StackTrace: **`{ex.StackTrace}`");
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

                await SendError($"Error in SendJoinedServers\n**Message**: {ex.Message}");
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
                Console.WriteLine(ex.Message);
            }
        }
    }
}
