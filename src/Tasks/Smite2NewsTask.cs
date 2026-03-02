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
    public class Smite2NewsTask
    {
        private readonly TimeSpan _interval;
        private Task? _timerTask;
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        public Smite2NewsTask(TimeSpan interval)
        {
            _interval = interval;
        }

        public void Start()
        {
            // Ensure previous instances are cleaned up if Start is called again
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
            _timerTask = Task.Run(() => DoSmite2NewsAsync(_cts.Token));
        }

        private async Task DoSmite2NewsAsync(CancellationToken token)
        {
            try
            {
                // loop until cancelled
                while (_timer is not null && await _timer.WaitForNextTickAsync(token))
                {
                    try
                    {
                        if (Discord.Connection.Client.LoginState.ToString() != "LoggedIn")
                        {
                            Text.WriteLine($"Task skipping, not logged in.");
                            continue;
                        }

                        var posts = await APIInteractions.FetchSmite2NewsAsync();
                        if (posts == null || posts.Count == 0)
                        {
                            await Reporter.SendMsgToBotLogsChannel("**Tasker**: No posts.");
                            continue;
                        }

                        foreach (var post in posts.OrderBy(x => x.attributes.publishedAt))
                        {
                            bool exists = await MongoConnection.FeedContentExistsAsync(FeedType.SMITE2News, post.attributes.slug);
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
                                    x.Name = "SMITE 2 News";
                                    x.IconUrl = "https://i.imgur.com/VjciMdI.png";
                                });

                                await Feeder.SendFeedWebhooks(emb.Build(), FeedType.SMITE2News, Text.SplitCamelCase(FeedType.SMITE2News.ToString()), post.attributes.slug);
                                // Save so we dont post it again
                                await MongoConnection.SaveFeedContentAsync(FeedType.SMITE2News, post.attributes.slug);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // cancellation requested - break out cleanly
                        break;
                    }
                    catch (Exception ex)
                    {
                        // log *per-iteration* exceptions
                        SentrySdk.CaptureException(ex);
                        await Reporter.SendErrorAsync($"Smite2 news iteration failed:\n{ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // expected when stopping
            }
            catch (Exception ex)
            {
                // log unexpected crash
                SentrySdk.CaptureException(ex);
                await Reporter.SendErrorAsync($"Smite2 Task crashed:\n{ex}");
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
                catch (OperationCanceledException) { /* expected */ }
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
    }
}
