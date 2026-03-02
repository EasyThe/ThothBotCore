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
    public class Smite2UpdateNotes
    {
        private readonly TimeSpan _interval;
        private Task? _timerTask;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        public Smite2UpdateNotes(TimeSpan interval)
        {
            _interval = interval;
        }

        public void Start()
        {
            // Clean up any previous instance
            if (_cts != null)
            {
                try
                {
                    _cts.Cancel();
                }
                catch { /* ignore */ }
                _cts.Dispose();
            }

            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(_interval);
            _timerTask = Task.Run(() => DoSmite2UpdateNotesAsync(_cts.Token));
        }

        private async Task DoSmite2UpdateNotesAsync(CancellationToken token)
        {
            try
            {
                while (_timer is not null && await _timer.WaitForNextTickAsync(token))
                {
                    try
                    {
                        if (Discord.Connection.Client?.LoginState.ToString() != "LoggedIn")
                        {
                            Text.WriteLine($"Tasker skipping because client is not logged in [{Discord.Connection.Client?.LoginState}]");
                            continue;
                        }

                        await BlogUpdateNotes();
                        // await BugFixes(); // dead
                    }
                    catch (OperationCanceledException)
                    {
                        // cancellation requested - exit loop cleanly
                        break;
                    }
                    catch (Exception ex)
                    {
                        SentrySdk.CaptureException(ex);
                        await Reporter.SendErrorAsync($"Smite2 UpdateNotes iteration failed:\n{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                await Reporter.SendErrorAsync($"Smite2 UpdateNotes Task crashed:\n{ex}");
            }
        }

        public async Task StopAsync()
        {
            if (_cts == null)
                return;

            try
            {
                _cts.Cancel();
            }
            catch { /* ignore */ }

            if (_timerTask != null)
            {
                try
                {
                    await _timerTask;
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }

            _timer?.Dispose();
            _cts?.Dispose();

            _timerTask = null;
            _timer = null;
            _cts = null;

            await Reporter.SendMsgToBotLogsChannel("Tasker stopped.");
        }

        private static async Task BlogUpdateNotes()
        {
            var posts = await APIInteractions.FetchSmite2NewsAsync();
            if (posts == null || posts.Count == 0)
            {
                await Reporter.SendMsgToBotLogsChannel("**Tasker**\nPosts is null or count = 0");
                return;
            }

            var sorted = posts.OrderBy(x => x.attributes.publishedAt).ToList();

            foreach (var post in sorted)
            {
                if (string.IsNullOrEmpty(post.attributes?.title) ||
                    !post.attributes.title.ToLowerInvariant().Contains("update"))
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
                bool exists = await MongoConnection.FeedContentExistsAsync(FeedType.SMITE2UpdateNotes, patch.shortLink);
                if (!exists)
                {
                    await Console.Out.WriteLineAsync($"[{exists}] {patch.name}");
                    var emb = new EmbedBuilder();
                    var desc = patch?.desc ?? string.Empty;
                    emb.WithTitle($"SMITE 2 Hotfix - {patch.name}");
                    emb.WithDescription(desc.Length > 4096 ? HotfixToucher(Text.Truncate(desc, 4096)) : HotfixToucher(desc));
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
