using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Feeds
{
    public static class Feeder
    {
        public static async Task SendFeedWebhooks(Embed embed, Models.GuildSettingsModel.FeedType feedType, string webhookUsername = "", string msg = "")
        {
            var allGuildSettings = MongoConnection.GetAllGuildsSettings();
            var feedGuilds = allGuildSettings.Where(x => x.Feeds.Any(z => z.Type == feedType)).ToList();

            SocketTextChannel channel = null;
            SocketGuild guild = null;
            int SuccessCount = 0;

            await FeedLogger(feedType, true, embed, feedGuilds.Count, msg: msg);

            for (int i = 0; i < feedGuilds.Count; i++)
            {
                var feed = feedGuilds[i].Feeds.Find(x => x.Type == feedType);
                try
                {
                    if (feed.WebhookID != 0)
                    {
                        // Sending via webhook
                        using var client = new DiscordWebhookClient(feed.WebhookID, feed.WebhookToken);

                        await client.SendMessageAsync(embeds: [embed],
                            username: webhookUsername, avatarUrl: "https://i.imgur.com/onR0CEh.png");
                    }
                    else
                    {
                        if (guild == null)
                        {
                            await MongoConnection.RemoveGuildSettings(feedGuilds[i]._id);
                            continue;
                        }
                        // Sending a normal message
                        guild = Connection.Client.GetGuild(feedGuilds[i]._id);
                        channel = guild.GetTextChannel(feed.ChannelID);
                        if (channel != null)
                        {
                            await channel.SendMessageAsync(embed: embed);
                        }
                        else
                        {
                            var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Removed {feedType} feed due to channel = null: {guild.Name}[{guild.Id}]");
                            await Reporter.SendEmbedToBotLogsChannelAsync(emb);

                            await MongoConnection.RemoveGuildSettings(feedGuilds[i]._id);
                        }
                    }
                    SuccessCount++;
                }
                catch (System.Exception ex)
                {
                    await Reporter.SendErrorAsync($"Couldn't send {feedType} feed to {feedGuilds[i]._id}, exception message: {ex.Message}");
                    if (ex.Message.Contains("Missing"))
                    {
                        IUser user = Connection.Client.GetUser(guild.OwnerId);
                        try
                        {
                            await user?.SendMessageAsync($":warning: Hey! I tried to send this {nameof(feedType)} to {channel?.Mention} " +
                                $"in the {guild?.Name} server but I am missing **Access** there.\n" +
                                $"Please make sure I have **View Channel, Manage Webhooks**, **Use External Emojis** and **Embed Links** permissions in {channel?.Mention}." +
                                $"You will get this message everytime I get an error by trying to send {nameof(feedType)} in {channel?.Mention}.\n" +
                                $"If you don't want to receive {nameof(feedType)} anymore, please disable the channel by using `/feeds` in one of the channels in your server.",
                                embed: embed); // maybe add a button under this message to disable the feed?
                        }
                        catch (System.Exception xx)
                        {
                            await Reporter.SendErrorAsync($"Feeder.cs sending a DM failed: {xx.Message} {guild?.Name}[{guild?.Id}]");
                            continue; // why is this here?
                        }
                    }
                    else if (ex.Message.Contains("500: Internal Server Error"))
                    {
                        await Reporter.SendErrorAsync($"Got error code 500 from Discord, trying to resend.\nGuildID: {feedGuilds[i]._id}");
                        // Trying again
                        // Not sure if the wrapper retries the 5xx calls hmmmm TODO
                        using var client = new DiscordWebhookClient(feed.WebhookID, feed.WebhookToken);

                        await client.SendMessageAsync(embeds: new[] { embed },
                            username: webhookUsername, avatarUrl: "https://i.imgur.com/onR0CEh.png");
                    }
                    else if (ex.Message.Contains("Could not find a webhook with the supplied credentials"))
                    {
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"Removed SMITE Status Feed due to not finding webhook with supplied credentials: {guild?.Name}[{guild?.Id}]");
                        await Reporter.SendEmbedToBotLogsChannelAsync(emb);

                        await MongoConnection.RemoveGuildSettings(feedGuilds[i]._id);
                    }
                    else
                    {
                        await Reporter.SendErrorAsync("Another Feeder error:\n" +
                            $"{ex.Message}\n" +
                            $"{ex.StackTrace}\n" +
                            $"ID: {feedGuilds[i]._id}");
                        continue; // why is this here?
                    }
                }
            }
            await FeedLogger(feedType, false, embed, feedGuilds.Count, SuccessCount, msg);
        }
        private static async Task FeedLogger(Models.GuildSettingsModel.FeedType feedType, bool isStarting, Embed embed, int count, int successCount = 0, string msg = "")
        {
            if (embed.Fields.Length != 0)
            {
                msg = embed?.Fields[0].Name.Split('>')[^1].Trim();
            }
            if (isStarting)
            {
                var e = await EmbedHandler.BuildDescriptionEmbedAsync($"**[Feeds | {feedType}] [{msg}]** Starting announcing to {count} servers.", Constants.SMITE2GoldColor);
                await Reporter.SendEmbedToBotLogsChannelAsync(e);
                await Connection.Logger.Log($"Feeds | {feedType}", $"[{msg}] Starting announcing to {count} servers.");
                return;
            }
            var em = await EmbedHandler.BuildDescriptionEmbedAsync($"**[Feeds | {feedType}] [{msg}]** " +
                    $"Success: {successCount}, Failed: {count - successCount}, out of {count}", Constants.SMITE2GoldColor);
            await Connection.Logger.Log($"Feeds | {feedType}", $"[{msg}] " +
                    $"Success: {successCount}, Failed: {count - successCount}, out of {count}");
            await Reporter.SendEmbedToBotLogsChannelAsync(em);
        }
    }
}
