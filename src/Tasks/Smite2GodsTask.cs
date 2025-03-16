using Discord;
using MongoDB.Driver;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tasks
{
    public class Smite2GodsTask(TimeSpan interval)
    {
        private Task _timerTask;
        private readonly PeriodicTimer _timer = new(interval);
        private readonly CancellationTokenSource _cts = new();
        bool running = false;

        public async void Start()
        {
            _timerTask = DoSmite2GodsAsync();
        }

        private async Task DoSmite2GodsAsync()
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
                    //await Reporter.SendMsgToBotLogsChannel("<@171675309177831424> started");
                    if (running)
                    {
                        return;
                    }
                    running = true;
                    var webGods = await APIInteractions.FetchSmite2GodsAsync();
                    int godsFromAPI = webGods.data.Length;
                    var localGods = new List<Gods.God>();
                    foreach (var god in webGods.data)
                    {
                        var skins = new List<GodSkinModel>();
                        // skins
                        foreach (var skin in god.attributes.Skin)
                        {
                            skins.Add(new()
                            {
                                god_name = god.attributes.Name,
                                godSkin_URL = skin.Image.data.attributes.url,
                                god_id = god.id,
                                skin_name = skin.Name,
                                skin_id1 = skin.id,
                                imageFormats = skin.Image.data.attributes.formats
                            });
                        }

                        var ab1 = god.attributes.Ability.Where(x => x.Slot == "Passive").ToList().FirstOrDefault();
                        var ab2 = god.attributes.Ability.Where(x => x.Slot == "Position 1").ToList().FirstOrDefault();
                        var ab3 = god.attributes.Ability.Where(x => x.Slot == "Position 2").ToList().FirstOrDefault();
                        var ab4 = god.attributes.Ability.Where(x => x.Slot == "Position 3").ToList().FirstOrDefault();
                        var ab5 = god.attributes.Ability.Where(x => x.Slot == "Position 4").ToList().FirstOrDefault();
                        localGods.Add(new()
                        {
                            id = god.id,
                            Name = god?.attributes?.Name,
                            Title = god?.attributes?.Subtitle,
                            latestGod = god.attributes.isNew ? "y" : "n",
                            Lore = Text.RemoveHtmlEntities(god?.attributes?.Lore),
                            godHeader_URL = god?.attributes?.HeaderImage?.data?.attributes?.url,
                            godCard_URL = god?.attributes?.Portrait?.data?.attributes?.url,
                            Roles = string.Join(", ", god?.attributes?.roles?.data?.Select(x => x.attributes?.Name)),
                            Pantheon = god?.attributes?.pantheon?.data?.attributes?.Name,
                            ret_msg = god.attributes.slug,
                            Skins = skins,
                            Ability_1 = new()
                            {
                                Id = ab1.id,
                                Summary = ab1?.Name,
                                Description = new()
                                {
                                    itemDescription = new()
                                    {
                                        description = Text.RemoveHtmlEntities(
                                            ab1?.Description
                                            .Replace("<p>", "")
                                            .Replace("</p>", "\n")
                                            .Replace("<br>", "\n")
                                            .Replace("</li>", "")
                                            .Replace("<ul>", "")
                                            .Replace("<li>", "\n* ")
                                            .Replace("</ul>", "")),
                                    }
                                },
                                Video = ab1?.YouTubeLink,
                                URL = ab1?.Icon?.data?.attributes?.url
                            },
                            Ability_2 = new()
                            {
                                Id = ab2.id,
                                Summary = ab2?.Name,
                                Description = new()
                                {
                                    itemDescription = new()
                                    {
                                        description = Text.RemoveHtmlEntities(
                                            ab2?.Description
                                            .Replace("<p>", "")
                                            .Replace("</p>", "\n")
                                            .Replace("<br>", "\n")
                                            .Replace("</li>", "")
                                            .Replace("<ul>", "")
                                            .Replace("<li>", "\n* ")
                                            .Replace("</ul>", "")),
                                    }
                                },
                                Video = ab2?.YouTubeLink,
                                URL = ab2?.Icon?.data?.attributes?.url
                            },
                            Ability_3 = new()
                            {
                                Id = ab3.id,
                                Summary = ab3?.Name,
                                Description = new()
                                {
                                    itemDescription = new()
                                    {
                                        description = Text.RemoveHtmlEntities(
                                            ab3?.Description
                                            .Replace("<p>", "")
                                            .Replace("</p>", "\n")
                                            .Replace("<br>", "\n")
                                            .Replace("</li>", "")
                                            .Replace("<ul>", "")
                                            .Replace("<li>", "\n* ")
                                            .Replace("</ul>", "")),
                                    }
                                },
                                Video = ab3?.YouTubeLink,
                                URL = ab3?.Icon?.data?.attributes?.url
                            },
                            Ability_4 = new()
                            {
                                Id = ab4.id,
                                Summary = ab4?.Name,
                                Description = new()
                                {
                                    itemDescription = new()
                                    {
                                        description = Text.RemoveHtmlEntities(
                                            ab4?.Description
                                            .Replace("<p>", "")
                                            .Replace("</p>", "\n")
                                            .Replace("<br>", "\n")
                                            .Replace("</li>", "")
                                            .Replace("<ul>", "")
                                            .Replace("<li>", "\n* ")
                                            .Replace("</ul>", "")),
                                    }
                                },
                                Video = ab4?.YouTubeLink,
                                URL = ab4?.Icon?.data?.attributes?.url
                            },
                            Ability_5 = new()
                            {
                                Id = ab5.id,
                                Summary = ab5?.Name,
                                Description = new()
                                {
                                    itemDescription = new()
                                    {
                                        description = Text.RemoveHtmlEntities(
                                            ab5?.Description
                                            .Replace("<p>", "")
                                            .Replace("</p>", "\n")
                                            .Replace("<br>", "\n")
                                            .Replace("</li>", "")
                                            .Replace("<ul>", "")
                                            .Replace("<li>", "\n* ")
                                            .Replace("</ul>", "")),
                                    }
                                },
                                Video = ab5?.YouTubeLink,
                                URL = ab5?.Icon?.data?.attributes?.url
                            }
                        });
                    }

                    // check locals first
                    var dbGods = MongoConnection.GetAllGods(true);
                    int godsInDBcount = dbGods.Count;
                    foreach (var god in localGods)
                    {
                        var found = dbGods.Find(x => x.Name == god.Name);
                        if (found != null)
                        {
                            god.DomColor = found.DomColor;
                            god.Ability_1.DomColor = found.Ability_1.DomColor;
                            god.Ability_1.Emoji = found.Ability_1.Emoji;

                            god.Ability_2.DomColor = found.Ability_2.DomColor;
                            god.Ability_2.Emoji = found.Ability_2.Emoji;

                            god.Ability_3.DomColor = found.Ability_3.DomColor;
                            god.Ability_3.Emoji = found.Ability_3.Emoji;

                            god.Ability_4.DomColor = found.Ability_4.DomColor;
                            god.Ability_4.Emoji = found.Ability_4.Emoji;

                            god.Ability_5.DomColor = found.Ability_5.DomColor;
                            god.Ability_5.Emoji = found.Ability_5.Emoji;
                        }
                    }

                    // do emojis
                    foreach (var god in localGods)
                    {
                        // dom color
                        if (god?.DomColor == 0 || god?.Ability_1.DomColor == 0)
                        {
                            Text.WriteLine($"Missing dom color for {god.Name}");
                            god.DomColor = await GetDomColorAsync(god?.godHeader_URL);
                            god.Ability_1.DomColor = await GetDomColorAsync(god.Ability_1.URL);
                            god.Ability_2.DomColor = await GetDomColorAsync(god.Ability_2.URL);
                            god.Ability_3.DomColor = await GetDomColorAsync(god.Ability_3.URL);
                            god.Ability_4.DomColor = await GetDomColorAsync(god.Ability_4.URL);
                            god.Ability_5.DomColor = await GetDomColorAsync(god.Ability_5.URL);
                        }

                        // emojis
                        if ((god.Ability_1?.Emoji == null || god.Ability_1.Emoji?.Length == 0) ||
                            (god.Ability_2?.Emoji == null || god.Ability_2.Emoji?.Length == 0) ||
                            (god.Ability_3?.Emoji == null || god.Ability_3.Emoji?.Length == 0) ||
                            (god.Ability_4?.Emoji == null || god.Ability_4.Emoji?.Length == 0) ||
                            (god.Ability_5?.Emoji == null || god.Ability_5.Emoji?.Length == 0))
                        {
                            Text.WriteLine($"Missing emojis for {god.Name}");
                            var emojis = await Utils.AddMissingAbilityEmojiAsync(god, true);
                            if (emojis.Length == 5)
                            {
                                god.Ability_1.Emoji = emojis[0];
                                god.Ability_2.Emoji = emojis[1];
                                god.Ability_3.Emoji = emojis[2];
                                god.Ability_4.Emoji = emojis[3];
                                god.Ability_5.Emoji = emojis[4];
                            }
                        }

                        // save to db
                        await MongoConnection.SaveGodAsync(god, true);
                    }
                    if (godsFromAPI > godsInDBcount)
                    {
                        await Reporter.SendMsgToBotLogsChannel($"{localGods.Count} gods for SMITE 2 were saved to db.");
                    }
                    Constants.ReloadConstants();
                    if (webGods == null)
                    {
                        await Reporter.SendMsgToBotLogsChannel("**Tasker**\nGods is null or count = 0");
                        continue;
                    }
                    running = false;
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

        // Helpers
        private static async Task<int> GetDomColorAsync(string URL)
        {
            try
            {
                var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(URL);
                var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                    colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                    colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                return (int)clr.RawValue;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"GetDomColorAsync {ex.Message}");
                return 0;
            }
        }
    }
}
