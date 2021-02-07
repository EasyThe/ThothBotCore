using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Notifications
{
    public static class StatusNotifier
    {
        public static async Task SendServerStatus(EmbedBuilder embed)
        {
            var notifChannels = await GetNotifChannels();
            SocketTextChannel channel = null;
            SocketGuild guild = null;

            for (int i = 0; i < notifChannels.Count; i++)
            {
                try
                {
                    guild = Connection.Client.GetGuild(notifChannels[i]._id);
                    channel = guild.GetTextChannel(notifChannels[i].statusChannel);
                    if (channel != null)
                    {
                        await channel.SendMessageAsync(embed: embed.Build());
                        Text.WriteLine($"Sent Status Updates to: {notifChannels[i]._id}]");
                    }
                    else
                    {
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Removed {guild.Name}[{guild.Id}]");
                        await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());
                        await Database.StopNotifs(guild.Id);
                    }
                }
                catch (System.Exception ex)
                {
                    await Reporter.SendError($"Couldn't send status update to {notifChannels[i]._id}");
                    if (ex.Message.Contains("Missing"))
                    {
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
                            $"{ex.StackTrace}\n" +
                            $"ID: {notifChannels[i]._id}");
                        continue;
                    }
                }
            }
        }
    }
}
