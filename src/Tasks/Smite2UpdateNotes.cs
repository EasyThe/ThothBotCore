using Discord;
using Sentry;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Feeds;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using static ThothBotCore.Models.GuildSettingsModel;

namespace ThothBotCore.Tasks
{
    public class Smite2UpdateNotes(TimeSpan interval)
    {
        private Task _timerTask;
        private readonly PeriodicTimer _timer = new PeriodicTimer(interval);
        private readonly CancellationTokenSource _cts = new();

        public async void Start()
        {
            _timerTask = DoSmite2UpdateNotesAsync();
        }
        
        private async Task DoSmite2UpdateNotesAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (Discord.Connection.Client.LoginState.ToString() != "LoggedIn")
                    {
                        Text.WriteLine($"Tasker skipping because client is not logged in [{Discord.Connection.Client?.LoginState}]");
                        continue;
                    }
                    await BlogUpdateNotes();
                    await BugFixes();
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                if (ex.Message != "Operation is not valid due to the current state of the object.")
                {
                    await Reporter.SendErrorAsync($"<@171675309177831424> **Error:** {ex.Message}\n```csharp\n{ex.StackTrace}```");
                    await StopAsync();
                    Start();
                }
                else
                {
                    await Reporter.SendErrorAsync($"# <@171675309177831424> **__You need to restart the timer!__**\n**Error:** {ex.Message}\n```csharp\n{ex.StackTrace}```");
                }
            }
        }

        public async Task StopAsync()
        {
            if (_timerTask is null)
            {
                return;
            }

            _cts.Cancel();
            await _timerTask;
            _cts.Dispose();
            await Console.Out.WriteLineAsync("Tasker was stopped.");
            await Reporter.SendMsgToBotLogsChannel($"Tasker was stopped.");
        }

        private static async Task BlogUpdateNotes()
        {
            var posts = await APIInteractions.FetchSmite2NewsAsync();
            if (posts == null || posts.Count == 0)
            {
                await Reporter.SendMsgToBotLogsChannel("**Tasker**\nPosts is null or count = 0");
                return;
            }
            posts = [.. posts.OrderBy(x => x.attributes.publishedAt)];
            foreach (var post in posts)
            {
                if (!post.attributes.title.ToLowerInvariant().Contains("update notes"))
                {
                    continue;
                }
                bool exists = await MongoConnection.FeedContentExistsAsync(FeedType.SMITE2UpdateNotes, post.attributes.slug);
                if (!exists)
                {
                    await Console.Out.WriteLineAsync($"[{exists}] {post.attributes.slug}");
                    var emb = new EmbedBuilder();
                    emb.WithTitle(post.attributes.title);
                    emb.WithImageUrl(post.attributes?.header_image?.data?.attributes?.url);
                    emb.WithDescription(Text.RemoveHtmlEntities(post.attributes.content));
                    emb.WithUrl($"https://smite2.com/news/{post.attributes.slug}");
                    emb.WithColor(Constants.SMITE2GoldColor);
                    //emb.WithThumbnailUrl("https://i.imgur.com/TR9dSLn.png");
                    emb.WithTimestamp(post.attributes.publishedAt);
                    emb.WithAuthor(x =>
                    {
                        x.Url = "https://www.smite2.com/";
                        x.Name = "SMITE 2 Update Notes";
                        x.IconUrl = "https://i.imgur.com/VjciMdI.png";
                    });

                    await Feeder.SendFeedWebhooks(emb.Build(), FeedType.SMITE2UpdateNotes, Text.SplitCamelCase(FeedType.SMITE2UpdateNotes.ToString()), post.attributes.slug);
                    // Save so we dont post it again
                    await MongoConnection.SaveFeedContentAsync(FeedType.SMITE2UpdateNotes, post.attributes.slug);
                }
            }
        }

        private static async Task BugFixes()
        {
            var trello = await APIInteractions.GetSMITE2TrelloCards();
            var hotfixes = trello.FindAll(x => x.idList == "66eae28467dff5302630ab44");

            if (hotfixes == null || hotfixes.Count == 0)
            {
                await Reporter.SendMsgToBotLogsChannel("**Tasker**\nHotfixes is null or count = 0");
                return;
            }

            foreach (var patch in hotfixes)
            {
                bool exists = await MongoConnection.FeedContentExistsAsync(FeedType.SMITE2UpdateNotes, patch.shortLink); // shortlink is kinda slug
                if (!exists)
                {
                    await Console.Out.WriteLineAsync($"[{exists}] {patch.name}");
                    var emb = new EmbedBuilder();
                    emb.WithTitle($"SMITE 2 Hotfix - {patch.name}");
                    emb.WithDescription(patch?.desc.Length > 4096 ? HotfixToucher(Text.Truncate(patch.desc, 4096)) : HotfixToucher(patch?.desc));
                    emb.WithUrl(patch.url);
                    emb.WithColor(Constants.SMITE2GoldColor);
                    //emb.WithThumbnailUrl("https://i.imgur.com/TR9dSLn.png");
                    emb.WithTimestamp(patch.dateLastActivity);
                    emb.WithAuthor(x =>
                    {
                        x.Url = "https://trello.com/b/mrW6CEFO";
                        x.Name = "SMITE 2 Update Notes";
                        x.IconUrl = "https://i.imgur.com/VjciMdI.png";
                    });

                    await Feeder.SendFeedWebhooks(emb.Build(), FeedType.SMITE2UpdateNotes, Text.SplitCamelCase(FeedType.SMITE2UpdateNotes.ToString()), patch.shortLink);
                    // Save so we dont post it again
                    await MongoConnection.SaveFeedContentAsync(FeedType.SMITE2UpdateNotes, patch.shortLink);
                }
            }
        }

        private static string HotfixToucher(string desc)
        {
            return desc.Replace("\n\n", "\n")
                       .Replace("\n\n\n", "\n")
                       .Replace("Gods", "**Gods**")
                       .Replace("General", "**General**")
                       .Replace("Gamemodes", "**Gamemodes**")
                       .Replace("God Balance", "**God Balance**")
                       .Replace("Item Balance", "**Item Balance**");
        }
    }
}
