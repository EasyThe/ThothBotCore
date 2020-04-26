using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Utilities;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Notifications
{
    public static class StatusNotifier
    {
        private static SocketTextChannel channel;

        public static async Task SendServerStatus(EmbedBuilder embed)
        {
            var notifChannels = await GetNotifChannels();

            for (int i = 0; i < notifChannels.Count; i++)
            {
                try
                {
                    channel = Connection.Client.GetGuild(notifChannels[i].serverID).GetTextChannel(notifChannels[i].statusChannel);
                    await channel.SendMessageAsync(embed: embed.Build());
                    System.Console.WriteLine($"Sent Status Updates to: {notifChannels[i].serverName}[{notifChannels[i].serverID}]");
                }
                catch (System.Exception ex)
                {
                    if (ex.Message.Contains("Missing"))
                    {
                        SocketGuild guild = Connection.Client.GetGuild(notifChannels[i].serverID);
                        IUser user = Connection.Client.GetUser(guild.OwnerId);
                        await user.SendMessageAsync($":warning: Hey! I tried to send this status update to {channel.Mention} but I am missing **Access** there.\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}." +
                        $"You will get this message everytime I get an error by trying to send Server Status Updates in {channel.Mention}.\n" +
                        $"If you don't want to receive Server Status Updates anymore, please use **!!stopstatusupdates** in one of your servers channels.",
                        embed: embed.Build());
                    }
                    else if (ex.Message.Contains("Object reference not set to an instance of an object."))
                    {
                        await StopNotifs(notifChannels[i].serverID);
                    }
                    else
                    {
                        await ErrorTracker.SendError("Another StatusNotifier error:\n" +
                            $"{ex.Message}\n" +
                            $"{ex.TargetSite}\n" +
                            $"{ex.Data}\n" +
                            $"{notifChannels[i].serverName} [{notifChannels[i].serverID}]");
                    }
                }
            }
        }
    }
}
