using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tasks
{
    public class AdvertiserTask(TimeSpan interval)
    {
        private Task _timerTask;
        private readonly PeriodicTimer _timer = new(interval);
        private readonly CancellationTokenSource _cts = new();
        private protected List<TipsModel> tips = [];

        public async void Start()
        {
            _timerTask = DoAdvertiserAsync();
        }

        private async Task DoAdvertiserAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (Discord.Connection.Client?.LoginState.ToString() != "LoggedIn")
                    {
                        Text.WriteLine($"Tasker skipping because client is not logged in [{Discord.Connection.Client?.LoginState}]");
                        continue;
                    }
                    if (tips == null || tips.Count == 0)
                    {
                        tips = MongoConnection.GetAllTips();
                    }
                    int r = Random.Shared.Next(0, tips.Count - 1);

                    while (tips[r].TipText.Length > 120)
                    {
                        tips.RemoveAt(r);
                        r = Random.Shared.Next(0, tips.Count - 1);
                    }
                    await Discord.Connection.Client?.SetCustomStatusAsync($"{tips[r].TipText}");
                    tips.RemoveAt(r);
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
