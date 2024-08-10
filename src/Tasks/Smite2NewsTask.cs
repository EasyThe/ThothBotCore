using Discord;
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
        private Task _timerTask;
        private readonly PeriodicTimer _timer;
        private readonly CancellationTokenSource _cts = new();

        public Smite2NewsTask(TimeSpan interval)
        {
            _timer = new PeriodicTimer(interval);
        }

        public async void Start()
        {
            _timerTask = DoSmite2NewsAsync();
        }

        private async Task DoSmite2NewsAsync()
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
                    var posts = await APIInteractions.FetchSmite2NewsAsync();
                    if (posts == null || posts.Count == 0)
                    {
                        await Reporter.SendMsgToBotLogsChannel("**Tasker**\nPosts is null or count = 0");
                        continue;
                    }
                    posts = [.. posts.OrderBy(x => x.attributes.publishedAt)];
                    foreach (var post in posts)
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

                            await Feeder.SendFeedWebhooks(emb.Build(), FeedType.SMITE2News, "SMITE2 News", post.attributes.slug);
                            // Save so we dont post it again
                            await MongoConnection.SaveFeedContentAsync(FeedType.SMITE2News, post.attributes.slug);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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
    }
}
