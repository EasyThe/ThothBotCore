﻿using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace ThothBotCore.Utilities
{
    public static class ErrorTracker
    {
        private static SocketTextChannel channel = Discord.Connection.Client.GetGuild(518408306415632384).GetTextChannel(557974702941798410);
        private static IUser ownerUser = Discord.Connection.Client.GetUser(171675309177831424);

        public static async Task SendDMtoOwner(string message)
        {
            try
            {
                await UserExtensions.SendMessageAsync(ownerUser, message);
            }
            catch (System.Exception ex)
            {

                await SendError($"Error in SendDMtoOwner\n**Message**: {ex.Message}");
            }
        }

        public static async Task SendError(string message)
        {
            try
            {
                await channel.SendMessageAsync(message);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public static async Task SendEmbedError(EmbedBuilder embed)
        {
            try
            {
                await channel.SendMessageAsync("", false, embed.Build());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}
