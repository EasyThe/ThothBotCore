using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Feeds
{
    public static class Feeder
    {
        public static async Task SendServerStatus(EmbedBuilder embed)
        {
            var notifChannels = await GetNotifChannels();
            SocketTextChannel channel = null;
            SocketGuild guild = null;
            int SuccessCount = 0;

            await Connection.Logger.Log("Feeds|ServerStatus", $"[{embed.Fields[0].Name.Split('>')[^1].Trim()}] Starting announcing to {notifChannels.Count} servers.");

            for (int i = 0; i < notifChannels.Count; i++)
            {
                try
                {
                    guild = Connection.Client.GetGuild(notifChannels[i]._id);
                    channel = guild.GetTextChannel(notifChannels[i].statusChannel);
                    if (channel != null)
                    {
                        await channel.SendMessageAsync(embed: embed.Build());
                        SuccessCount++;
                    }
                    else
                    {
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Removed status update sub: {guild.Name}[{guild.Id}]");
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
                            await user?.SendMessageAsync($":warning: Hey! I tried to send this status update to {channel?.Mention} in the {guild?.Name} server but I am missing **Access** there.\n" +
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
            await Connection.Logger.Log("Feeds|ServerStatus", $"[{embed.Fields[0].Name.Split('>')[^1].Trim()}] " +
                $"Success: {SuccessCount}, Failed: {notifChannels.Count - SuccessCount}, out of {notifChannels.Count}");
        }
        public static async Task SendServerStatusWebhooks(Embed embed, Models.GuildSettingsModel.FeedType feedType, string webhookUsername = "")
        {
            var allGuildSettings = MongoConnection.GetAllGuildsSettings();
            var feedGuilds = allGuildSettings.Where(x => x.Feeds.Any(z => z.Type == feedType)).ToList();

            SocketTextChannel channel = null;
            SocketGuild guild = null;
            int SuccessCount = 0;

            await FeedLogger(feedType, true, embed, feedGuilds.Count);

            for (int i = 0; i < feedGuilds.Count; i++)
            {
                try
                {
                    var feed = feedGuilds[i].Feeds.Find(x => x.Type == feedType);
                    guild = Connection.Client.GetGuild(feedGuilds[i]._id);
                    channel = guild.GetTextChannel(feed.ChannelID);
                    if (channel != null)
                    {
                        if (feed.WebhookID != 0)
                        {
                            using var client = new DiscordWebhookClient($"https://discord.com/api/webhooks/{feed.WebhookID}/{feed.WebhookToken}");
                            
                            await client.SendMessageAsync(embeds: new[] { embed },
                                username: webhookUsername, avatarUrl: "https://i.imgur.com/onR0CEh.png");
                        }
                        else
                        {
                            await channel.SendMessageAsync(embed: embed);
                        }
                        SuccessCount++;
                    }
                    else
                    {
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Removed SMITE Status Feed due to channel = null: {guild.Name}[{guild.Id}]");
                        await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());

                        feed.WebhookToken = null;
                        feed.WebhookID = 0;
                        feed.ChannelID = 0;
                        await MongoConnection.SaveGuildSettingsAsync(feedGuilds[i]);
                    }
                }
                catch (System.Exception ex)
                {
                    await Reporter.SendError($"Couldn't send {feedType} feed to {feedGuilds[i]._id}");
                    if (ex.Message.Contains("Missing"))
                    {
                        IUser user = Connection.Client.GetUser(guild.OwnerId);
                        try
                        {
                            await user?.SendMessageAsync($":warning: Hey! I tried to send this status update to {channel?.Mention} in the {guild?.Name} server but I am missing **Access** there.\n" +
                                $"Please make sure I have **View Channel, Manage Webhooks**, **Use External Emojis** and **Embed Links** permissions in {channel?.Mention}." +
                                $"You will get this message everytime I get an error by trying to send SMITE Status Feeds in {channel?.Mention}.\n" +
                                $"If you don't want to receive SMITE Status Feeds anymore, please disable the channel by using `/feeds` in one of the channels in your server.",
                                embed: embed);
                        }
                        catch (System.Exception xx)
                        {
                            await Reporter.SendError($"Feeder.cs sending a DM failed: {xx.Message} {guild?.Name}[{guild?.Id}]");
                            continue;
                        }
                    }
                    else
                    {
                        await Reporter.SendError("Another Feeder error:\n" +
                            $"{ex.Message}\n" +
                            $"{ex.StackTrace}\n" +
                            $"ID: {feedGuilds[i]._id}");
                        continue;
                    }
                }
            }
            await FeedLogger(feedType, false, embed, feedGuilds.Count, SuccessCount);
        }
        private static async Task FeedLogger(Models.GuildSettingsModel.FeedType feedType, bool isStarting, Embed embed, int count, int successCount = 0)
        {
            string msg = "";
            if (embed.Fields.Length != 0)
            {
                msg = embed?.Fields[0].Name.Split('>')[^1].Trim();
            }
            if (isStarting)
            {
                switch (feedType)
                {
                    case Models.GuildSettingsModel.FeedType.ServerStatus:
                        await Connection.Logger.Log($"Feeds|{feedType}", $"[{msg}] Starting announcing to {count} servers.");
                        return;
                    case Models.GuildSettingsModel.FeedType.UpdateNotes:
                        return;
                    case Models.GuildSettingsModel.FeedType.BlogPosts:
                        return;
                    case Models.GuildSettingsModel.FeedType.Datamining:
                        return;
                    case Models.GuildSettingsModel.FeedType.GameTwitter:
                        return;
                    case Models.GuildSettingsModel.FeedType.GameYouTube:
                        return;
                    case Models.GuildSettingsModel.FeedType.ProTwitter:
                        return;
                    case Models.GuildSettingsModel.FeedType.ProBlogPosts:
                        return;
                    default:
                        return;
                }
            }
            switch (feedType)
            {
                case Models.GuildSettingsModel.FeedType.ServerStatus:
                    await Connection.Logger.Log($"Feeds|{feedType}", $"[{msg}] " +
                    $"Success: {successCount}, Failed: {count - successCount}, out of {count}");
                    return;
                case Models.GuildSettingsModel.FeedType.UpdateNotes:
                    return;
                case Models.GuildSettingsModel.FeedType.BlogPosts:
                    return;
                case Models.GuildSettingsModel.FeedType.Datamining:
                    return;
                case Models.GuildSettingsModel.FeedType.GameTwitter:
                    return;
                case Models.GuildSettingsModel.FeedType.GameYouTube:
                    return;
                case Models.GuildSettingsModel.FeedType.ProTwitter:
                    return;
                case Models.GuildSettingsModel.FeedType.ProBlogPosts:
                    return;
                default:
                    return;
            }
        }
    }
}
