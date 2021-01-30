using Discord;
using Discord.WebSocket;
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
                    channel = Connection.Client.GetGuild(notifChannels[i]._id).GetTextChannel(notifChannels[i].statusChannel);
                    await channel.SendMessageAsync(embed: embed.Build());
                    Text.WriteLine($"Sent Status Updates to: {notifChannels[i]._id}]");
                }
                catch (System.Exception ex)
                {
                    await Reporter.SendError($"Couldn't send status update to {notifChannels[i]._id}");
                    if (ex.Message.Contains("Missing"))
                    {
                        SocketGuild guild = Connection.Client.GetGuild(notifChannels[i]._id);
                        IUser user = Connection.Client.GetUser(guild.OwnerId);
                        try
                        {
                            await user?.SendMessageAsync($":warning: Hey! I tried to send this status update to {channel?.Mention} ({guild?.Name}) but I am missing **Access** there.\n" +
                                $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel?.Mention}." +
                                $"You will get this message everytime I get an error by trying to send Server Status Updates in {channel?.Mention}.\n" +
                                $"If you don't want to receive Server Status Updates anymore, please use **!!stopstatusupdates** in one of your servers channels.",
                                embed: embed.Build());
                        }
                        catch (System.Exception xx)
                        {
                            await Reporter.SendError($"StatusNotifier.cs sending a DM failed: {xx.Message} {guild?.Name}[{guild?.Id}]");
                            continue;
                        }
                    }
                    else
                    {
                        await Reporter.SendError("Another StatusNotifier error:\n" +
                            $"{ex.Message}\n" +
                            $"{ex.TargetSite}\n" +
                            $"{ex.Data}\n" +
                            $"ID: {notifChannels[i]._id}");
                        continue;
                    }
                }
            }
        }
    }
}
