using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Autocomplete;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using ThothBotCore.Utilities.Smite;

namespace ThothBotCore.Modules
{
    public class SlashSmite : InteractionModuleBase
    {
        static Random rnd = new();
        public HiRezAPIv2 HiRez { get; set; }

        [SlashCommand("gods", "Overall information about the gods & skins in the game and current free god rotation.")]
        public async Task SlashGodsCommand()
        {
            try
            {
                List<Gods.God> gods = MongoConnection.GetAllGods();
                StringBuilder sb = new();

                // Most skins
                if (Utilities.Constants.BotSettings.Skins != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        sb.AppendLine($"{Utilities.Constants.BotSettings.Skins[i].Emoji} " +
                            $"{Utilities.Constants.BotSettings.Skins[i].Name} " +
                            $"[{Utilities.Constants.BotSettings.Skins[i].Count}]");
                    }
                }

                // pantheon
                StringBuilder psb = new();
                if (Utilities.Constants.BotSettings.Pantheons != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        psb.AppendLine($"{Utilities.Constants.BotSettings.Pantheons[i].Emoji} " +
                            $"{Utilities.Constants.BotSettings.Pantheons[i].Name} " +
                            $"[{Utilities.Constants.BotSettings.Pantheons[i].Count}]");
                    }
                }

                if (gods.Count != 0)
                {
                    StringBuilder onRotation = new();
                    var embed = new EmbedBuilder();
                    var latestGod = gods.Find(x => x.latestGod == "y");
                    var mages = gods.FindAll(x => x.Roles.Contains("Mage"));
                    var hunters = gods.FindAll(x => x.Roles.Contains("Hunter"));
                    var guardians = gods.FindAll(x => x.Roles.Contains("Guardian"));
                    var assassins = gods.FindAll(x => x.Roles.Contains("Assassin"));
                    var warriors = gods.FindAll(x => x.Roles.Contains("Warrior"));

                    var onRot = gods.FindAll(x => x.OnFreeRotation == "true");
                    for (int i = 0; i < onRot.Count; i++)
                    {
                        onRotation.AppendLine($"{onRot[i].Emoji} {onRot[i].Name}");
                    }
                    embed.WithColor(Utilities.Constants.DefaultBlueColor);
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"Gods: {gods.Count}";
                        x.Value = $"<:Mage:607990144380698625>Mages: {mages.Count}\n" +
                        $"<:Hunter:607990144271646740>Hunters: {hunters.Count}\n" +
                        $"<:Guardian:607990144385024000>Guardians: {guardians.Count}\n" +
                        $"<:Assassin:607990143915261983>Assassins: {assassins.Count}\n" +
                        $"<:Warrior:607990144338886658>Warriors: {warriors.Count}";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "On Free Rotation";
                        x.Value = onRotation.ToString();
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Latest God";
                        x.Value = $"{latestGod.Emoji} {latestGod.Name}\n🔹 {latestGod.Title}\n🔹 {latestGod.Type}, {latestGod.Roles}";
                    });
                    if (sb.Length != 0)
                    {
                        embed.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = "Most Skins";
                            x.Value = sb.ToString();
                        });
                    }
                    if (psb.Length != 0)
                    {
                        embed.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = "Most Gods per Pantheon";
                            x.Value = psb.ToString();
                        });
                    }
                    embed.WithFooter(x =>
                    {
                        x.Text = $"For God specific information use /god";
                    });

                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    await RespondAsync($"Something is not right... Please report this in my [support server]({Utilities.Constants.SupportServerInvite}).");
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("god", "Provides information about the requested god.")]
        public async Task SlashGodInfoCommand([Summary("GodName", "Start typing the name of the god and you will get recommendations.")]
            [Autocomplete(typeof(GodNameAutocompleteHandler))]string GodName)
        {
            try
            {
                string titleCaseGod = Text.ToTitleCase(GodName);
                Gods.God gods = await MongoConnection.GetGodByNameAsync(titleCaseGod);

                if (gods == null)
                {
                    await RespondAsync($"{titleCaseGod} was not found.", allowedMentions: AllowedMentions.None);
                }
                else
                {
                    var embed = await EmbedHandler.BuildMainGodPageEmbedAsync(gods);

                    var buttons = await ComponentsHandler.GodsInfoButtonsAsync(gods.id);

                    await RespondAsync(embed: embed,
                    allowedMentions: AllowedMentions.None,
                    components: buttons);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("rgod", "Gives you a random God and randomised build.")]
        public async Task SlashRangomGodCommand([Summary("godClass", "What class should the random god be?")][Choice("Mage","mage"), 
                                                 Choice("Warrior", "warrior"), 
                                                 Choice("Hunter", "hunter"),
                                                 Choice("Guardian", "guardian"),
                                                 Choice("Assassin", "assassin")] string godClass = "")
        {
            try
            {
                List<Gods.God> godsF = MongoConnection.GetAllGods();
                List<Gods.God> gods = null;
                if (godClass != "")
                {
                    godClass = godClass.ToLowerInvariant().Trim();
                    gods = godClass switch
                    {
                        "mage" => godsF.Where(x => x.Roles.Contains("Mage")).ToList(),
                        "warrior" => godsF.Where(x => x.Roles.Contains("Warrior")).ToList(),
                        "hunter" => godsF.Where(x => x.Roles.Contains("Hunter")).ToList(),
                        "guardian" => godsF.Where(x => x.Roles.Contains("Guardian")).ToList(),
                        "assassin" => godsF.Where(x => x.Roles.Contains("Assassin")).ToList(),
                        _ => godsF,
                    };
                }
                else
                {
                    gods = godsF;
                }
                int rr = rnd.Next(gods.Count);
                string rbuild = await Utils.RandomBuilderAsync(gods[rr]);

                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods[rr].Name);
                    author.WithIconUrl(gods[rr].godIcon_URL);
                });
                embed.WithTitle(gods[rr].Title);
                embed.WithThumbnailUrl(gods[rr].godIcon_URL);
                if (gods[0].DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods[rr].DomColor));
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Role";
                    field.Value = gods[rr].Roles;
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Type";
                    field.Value = gods[rr].Type;
                });
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Random Build";
                    x.Value = rbuild;
                });
                embed.WithFooter(x =>
                {
                    x.Text = Text.GetRandomTip();
                });
                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("rteam", "Gives you random Gods with randomised builds for them.")]
        public async Task SlashRandomTeamCommand([Summary("option", "How many gods would you like? Or you want randomized gods respecting the in-game Assault rules?")]
                                                 [Choice("2 gods", 2),
                                                  Choice("3 gods", 3),
                                                  Choice("4 gods", 4),
                                                  Choice("5 gods", 5),
                                                  Choice("Assault (2 teams)", 6)]int option)
        {
            try
            {
                // Assault
                if (option == 6)
                {
                    var em = await EmbedHandler.BuildRandomAssaultTeamsEmbedAsync();
                    await RespondAsync(embed: em);
                    return;
                }

                var embed = new EmbedBuilder();
                var gods = MongoConnection.GetAllGods();

                embed.WithColor(Utilities.Constants.DefaultBlueColor);

                for (int i = 0; i < option; i++)
                {
                    int rr = rnd.Next(gods.Count);
                    string rbuild = await Utils.RandomBuilderAsync(gods[rr]);

                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{gods[rr].Emoji} {gods[rr].Name}";
                        x.Value = rbuild;
                    });
                    gods.RemoveAt(rr);
                }
                embed.WithFooter(x =>
                {
                    x.Text = Text.GetRandomTip();
                });

                await RespondAsync($"Team of {option} for you!", embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("rbuild", "Gives you a random build for the requested god")] 
        public async Task SlashRandomBuildCommand([Summary("godName", "Start typing the name of the god and you will get recommendations.")]
            [Autocomplete(typeof(GodNameAutocompleteHandler))]string godName)
        {
            try
            {
                string titleCaseGod = Text.ToTitleCase(godName);
                Gods.God god = await MongoConnection.GetGodByNameAsync(titleCaseGod);

                if (god == null)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync($"{titleCaseGod} was not found.");
                    await RespondAsync(embed: em);
                }
                else
                {
                    string rbuild = await Utils.RandomBuilderAsync(god);

                    var embed = new EmbedBuilder();
                    embed.WithAuthor(author =>
                    {
                        author.WithName(god.Name);
                        author.WithIconUrl(god.godIcon_URL);
                    });
                    embed.WithTitle(god.Title);
                    embed.WithThumbnailUrl(god.godIcon_URL);
                    if (god.DomColor != 0)
                    {
                        embed.WithColor(new Color((uint)god.DomColor));
                    }
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "Role";
                        field.Value = god.Roles;
                    });
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = "Type";
                        field.Value = god.Type;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Random Build";
                        x.Value = rbuild;
                    });
                    embed.WithFooter(x =>
                    {
                        x.Text = Text.GetRandomTip();
                    });
                    await RespondAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("rank", "Gives you random SMITE Ranked division.")]
        public async Task SlashRandomRankCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                int n;
                n = rnd.Next(0, 28);
                embed.WithColor(rnd.Next(255), rnd.Next(255), rnd.Next(255));
                embed.WithAuthor(x =>
                {
                    x.IconUrl = Context.User.GetAvatarUrl();
                    x.Name = $"{Context.User.Username}'s rank is:";
                });
                embed.WithTitle($"{Text.GetRankedConquest(n).Item2} {Text.GetRankedConquest(n).Item1}");
                embed.WithFooter(x =>
                {
                    x.Text = Text.GetRandomTip();
                });

                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("item", "Provides information about the requested item.")]
        public async Task SlashItemCommand([Summary("ItemName", "Start typing the name of the item and you will get recommendations.")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))]string ItemName)
        {
            try
            {
                int index = 0;
                var item = await MongoConnection.GetSpecificItemAsync(ItemName);
                if (item.Count != 0)
                {
                    if (item.Any(x => x.DeviceName.ToLowerInvariant() == ItemName.ToLowerInvariant()))
                    {
                        index = item.FindIndex(x => x.DeviceName.ToLowerInvariant() == ItemName.ToLowerInvariant());
                    }
                    var embed = await EmbedHandler.BuildItemInfoEmbedAsync(item[index]);

                    // related items
                    var allitems = MongoConnection.GetAllActiveItems();
                    var relatedButtons = await ComponentsHandler.RelatedItemsSelectMenuAsync(allitems, item[index]);

                    await RespondAsync(embed: embed, components: relatedButtons != null && relatedButtons.Components.Count != 0 ? relatedButtons : null);
                }
                else
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync($"Sorry, I couldn't find `{ItemName}`.");
                    await RespondAsync(embed: em);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("itemstarters", "Provides a list with all starting items.")]
        public async Task SlashItemStartersCommand()
        {
            var items = MongoConnection.GetAllActiveItems();
            var starters = items.FindAll(x => x.StartingItem && x.ActiveFlag == "y");
            int maxCount = starters.FindAll(x => x.ItemTier == 2).Count / 2;
            int counter = 0;
            StringBuilder sb21 = new();
            StringBuilder sb22 = new();
            StringBuilder sb1 = new();
            foreach (var item in starters)
            {
                if (item.ItemTier == 2)
                {
                    if (counter! > maxCount - 1)
                    {
                        sb22.AppendLine($"{item.Emoji} {item.DeviceName}");
                    }
                    else
                    {
                        sb21.AppendLine($"{item.Emoji} {item.DeviceName}");
                    }
                    counter++;
                }
                else
                {
                    sb1.AppendLine($"{item.Emoji} {item.DeviceName}");
                }
            }
            EmbedBuilder embed = new();
            embed.WithColor(Utilities.Constants.DefaultBlueColor);
            embed.WithTitle($"List of all {starters.Count} starting items");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Tier 1 Starters";
                x.Value = sb1.ToString();
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Tier 2 Starters [1/2]";
                x.Value = sb21.ToString();
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Tier 2 Starters [2/2]";
                x.Value = sb22.ToString();
            });
            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("status", "Check the status of SMITE Servers.")]
        public async Task SlashServerStatusCommand()
        {
            await DeferAsync();

            try
            {
                var smiteServerStatus = JsonConvert.DeserializeObject<ServerStatus>(await APIInteractions.GetStatusSummary());
                var hirezServerStatus = await HiRez.GetHiRezServerStatusAsync();

                var statusEmbed = await EmbedHandler.ServerStatusEmbedAsync(smiteServerStatus, hirezServerStatus);
                List<Embed> embeds = new();
                embeds.Add(statusEmbed);

                bool maint = false;
                bool inci = false;
                // Incidents
                if (smiteServerStatus.incidents.Count >= 1)
                {
                    for (int i = 0; i < smiteServerStatus.incidents.Count; i++)
                    {
                        if ((smiteServerStatus.incidents[i].name.ToLowerInvariant().Contains("smite") ||
                             smiteServerStatus.incidents[i].incident_updates[0].body.ToLowerInvariant().Contains("smite")) &&
                             !(smiteServerStatus.incidents[i].name.ToLowerInvariant().Contains("blitz")))
                        {
                            inci = true;
                        }
                    }
                }
                if (inci)
                {
                    var inciEmbed = await EmbedHandler.StatusIncidentEmbed(smiteServerStatus);
                    embeds.Add(inciEmbed.Build());
                }
                // Scheduled Maintenances
                if (smiteServerStatus.scheduled_maintenances.Count >= 1)
                {
                    for (int c = 0; c < smiteServerStatus.scheduled_maintenances.Count; c++)
                    {
                        if (smiteServerStatus.scheduled_maintenances[c].name.ToLowerInvariant().Contains("smite"))
                        {
                            maint = true;
                        }
                    }
                }
                if (maint)
                {
                    var maintEmbed = await EmbedHandler.StatusMaintenanceEmbed(smiteServerStatus);
                    embeds.Add(maintEmbed.Build());
                }

                await FollowupAsync(embeds: embeds.ToArray<Embed>());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("updatenotes", "Sends the last two update notes posted on the SMITEgame.com website")]
        public async Task SlashUpdateNotesCommand()
        {
            try
            {
                await DeferAsync();
                List<Embed> embs = new();
                var posts = await APIInteractions.FetchPostsAsync();
                var foundPost = posts.FindAll(x => x.real_categories.ToLowerInvariant().Contains("notes"));
                for (int i = 0; i < 2; i++)
                {
                    var actualPost = await APIInteractions.GetPostBySlugAsync(foundPost[i].slug);
                    string description = await PatchPageReader.ReadPatch(actualPost);
                    Embed embed = await EmbedHandler.BuildPatchNotesEmbedAsync(actualPost, description, foundPost[i]?.large_image, foundPost[i].slug);
                    embs.Add(embed);
                }
                await FollowupAsync(embeds: embs.ToArray());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("events", "Shows if there are any events currently available in-game.")]
        public async Task SlashEventsCommand()
        {
            try
            {
                await DeferAsync();
                StringBuilder sb = new();
                var result = await APIInteractions.GetLandingPanel();
                var embed = new EmbedBuilder();
                embed.WithColor(Utilities.Constants.FeedbackColor);
                embed.WithAuthor(x =>
                {
                    x.Name = "SMITE Events";
                    x.IconUrl = Utilities.Constants.SmiteBolt;
                    x.Url = "https://www.smitegame.com/news/";
                });
                if (result.events.content.Count == 0)
                {
                    if (result.singlePanel.visible == "true")
                    {
                        var first = result.singlePanel.content.FirstOrDefault(x => x.locationId == 702 && x.isStandard == "true" && x.endDate.HasValue && x.endDate.Value.AddHours(2) > DateTime.UtcNow);
                        if (first != null && first.isStandard == "true")
                        {
                            embed.WithTitle($"{first.header?.@default} | Ends <t:{Text.DateTimeToUnix(first.endDate.Value.AddHours(2))}:R>");
                            embed.WithImageUrl(first?.imageUrl.INT);
                            await FollowupAsync(embed: embed.Build());
                            return;
                        }
                    }
                    embed.WithTitle("There are no events at the moment.");
                    await FollowupAsync(embed: embed.Build());
                    return;
                }
                var header = result.events.content.FirstOrDefault().eventList.Find(x => x.header != null);
                if (header != null)
                {
                    embed.WithTitle(header.header.@default);
                }
                embed.WithImageUrl(result.events.content.FirstOrDefault()?.imageUrl);
                embed.WithColor(Utilities.Constants.FeedbackColor);
                foreach (var item in result.events.content[0].eventList)
                {
                    sb.AppendLine($"🔹 " + (item.desc.@default.Contains("Today") ? $"**{item.desc.@default}**" : $"{item.desc.@default}"));
                }
                embed.WithDescription(sb.ToString());
                await FollowupAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("bugs", "Checks the SMITE Community Issues Trello Board for current and fixed issues.")]
        public async Task SlashTrelloBoardCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                var result = await APIInteractions.GetTrelloCards();

                StringBuilder topIssues = new();
                StringBuilder hotfixNotes = new();
                StringBuilder incominghotfix = new();
                StringBuilder alreadyFixedInLIVE = new();

                int patchCount = 0;
                // Appending the issues
                foreach (var item in result)
                {
                    // Top Issues
                    if (item.idList == "5c740d7d4e18c107890167ea")
                    {
                        topIssues.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Hotfix PatchNotes
                    if (item.idList == "5c740da2ff81b93a4039da81" && hotfixNotes.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024 && patchCount < 7)
                    {
                        hotfixNotes.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                        patchCount++;
                    }
                    // Incoming hotfix
                    if (item.idList == "5c804623d75e55500472cf9a" && incominghotfix.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024)
                    {
                        incominghotfix.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Fixed in LIVE
                    if (item.idList == "5c7e9e5e30dbfd27cb7c4442" && alreadyFixedInLIVE.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024)
                    {
                        alreadyFixedInLIVE.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                }

                embed.WithAuthor(x =>
                {
                    x.Name = "SMITE Community Issues Trello Board";
                    x.Url = "https://trello.com/b/d4fJtBlo/smite-community-issues";
                    x.IconUrl = "https://cdn3.iconfinder.com/data/icons/popular-services-brands-vol-2/512/trello-512.png";
                });

                // Incoming Hotfix
                if (incominghotfix.Length != 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Incoming Hotfix";
                        x.Value = incominghotfix.ToString();
                    });
                }

                // Already in LIVE
                if (alreadyFixedInLIVE.Length != 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Already fixed";
                        x.Value = alreadyFixedInLIVE.ToString();
                    });
                }

                // Hotfix Patch Notes
                if (hotfixNotes.ToString() != "")
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Hotfix Patch Notes";
                        x.Value = hotfixNotes.ToString();
                    });
                }

                embed.WithColor(Utilities.Constants.FeedbackColor);
                embed.WithTitle("All Platforms Top Issues");
                if (!(topIssues.ToString().Length > 2048))
                {
                    embed.WithDescription(topIssues.ToString());
                }
                else
                {
                    // Its longer than 2048
                    embed.WithDescription(Text.Truncate(topIssues.ToString(), 2048));
                }

                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the Trello API is down. Try visiting the [website](https://trello.com/b/d4fJtBlo/smite-community-issues) instead.");
                await RespondAsync(embed: embed);
                await Reporter.SendErrorAsync($"**Trello Error: **\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        // HIREZ API COMMANDS

        [SlashCommand("motd", "Information about upcoming MOTDs in the game.")]
        public async Task SlashMotdCommand()
        {
            try
            {
                var motdList = await HiRez.GetMOTD();
                string desc = "";
                string embedValue = "";
                bool addedTodaysMotd = false;

                var embed = new EmbedBuilder();

                embed.WithColor(Utilities.Constants.DefaultBlueColor);
                embed.WithAuthor(x =>
                {
                    x.Name = "Current & Upcoming Matches Of The Day";
                    x.IconUrl = Utilities.Constants.botIcon;
                });
                embed.WithFooter(x =>
                {
                    x.IconUrl = Utilities.Constants.botIcon;
                    x.Text = Text.GetRandomTip();
                });

                Motd motdDay = new();

                for (int i = 0; i < 5; i++)
                {
                    string[] finalDesc = Array.Empty<string>();
                    motdDay = motdList.Find(x => x.startDateTime.Date == DateTime.Today.AddDays(i));

                    // Checking if the motd for the day has actually changed or not
                    if (motdDay != null && DateTime.UtcNow.Hour < motdDay.startDateTime.Hour && !addedTodaysMotd)
                    {
                        motdDay = motdList.Find(x => x.startDateTime.Date == DateTime.Today.AddDays(-1));
                        addedTodaysMotd = true;
                        i -= 1;
                    }

                    if (motdDay == null)
                    {
                        break;
                    }
                    desc = Text.ReFormatMOTDText(motdDay.description);
                    if (desc.Contains("**Map"))
                    {
                        finalDesc = desc.Split("**Map");
                        embedValue = "**Map" + finalDesc[1];
                    }
                    else
                    {
                        embedValue = desc;
                    }
                    embed.AddField(x =>
                    {
                        x.Name = $":large_blue_diamond: **{motdDay.title}** - {Text.ShortDateTimeTimestamp(motdDay.startDateTime)}";
                        x.Value = $"{embedValue}";
                    });
                }

                if (embed.Fields?.Count == 0)
                {
                    embed.WithDescription("No data available. Please try again later.");
                }
                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("stats", "Display stats for the requested PlayerName.")]
        public async Task SlashPlayerStatsCommand([Summary("PlayerName", "The in-game name of a SMITE player")]string PlayerName = "")
        {
            try
            {
                await DeferAsync();
                var isAlive = await IsSmiteApiAlive(HiRez);
                
                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }

                if ((string)player[0].ret_msg == "apidown" || !isAlive)
                {
                    var embed = await Reporter.SlashRespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    EmbedBuilder embed;
                    string mostplayed = await CalculateTopGameModesForStatsAsync(HiRez, player[0].player_id.ToString());

                    // Doing the stuff
                    var playerStatus = await HiRez.GetPlayerStatusAsync(player[0].player_id.ToString());
                    List<MatchPlayerDetails.PlayerMatchDetails> match = new();
                    if (playerStatus != null && playerStatus[0].Match != 0)
                    {
                        match = await HiRez.GetMatchPlayerDetailsAsync(playerStatus[0].Match.ToString());
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());
                    var godRanks = await HiRez.GetGodRanksAsync(player[0].player_id.ToString());

                    // Generating the embed and sending to channel
                    embed = await EmbedHandler.PlayerStatsEmbed(
                        getPlayer,
                        godRanks,
                        await HiRez.GetPlayerAchievementsAsync(player[0].player_id.ToString()),
                        playerStatus,
                        match);

                    // Buttons
                    var comps = await ComponentsHandler.RichStatsButtonsAsync(player[0].player_id.ToString(), 0, playerStatus[0].Match != 0);

                    // Add Most played matches
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = $"<:matches:579604410569850891>Most Played Modes";
                        field.Value = mostplayed;
                    });

                    await FollowupAsync(embed: embed.Build(), components: comps);

                    // Saving player to DB
                    try
                    {
                        await MongoConnection.SavePlayerAsync(getPlayer[0]).ConfigureAwait(false);
                        await MongoConnection.SavePlayerGodRanksAsync(godRanks).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Text.WriteLine(ex.Message, ConsoleColor.Red, ConsoleColor.White);
                    }
                }
                else
                {
                    // A select menu event will handle it somewhere else...
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "stats");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                if (Context.Interaction.HasResponded)
                {
                    await FollowupAsync(embed: embed);
                }
                else
                {
                    await FollowupAsync(embed: embed);
                }
            }
        }

        [SlashCommand("wp", "Shows all masteries for gods played for the requested PlayerName.")]
        public async Task SlashWorshipersCommand([Summary("PlayerName", "The in-game name of a SMITE player")] string PlayerName = "")
        {
            try
            {
                await DeferAsync();

                var isAlive = await IsSmiteApiAlive(HiRez);
                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }

                if ((string)player[0].ret_msg == "apidown" || !isAlive)
                {
                    var embed = await Reporter.RespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    var getGodRanks = await HiRez.GetGodRanksAsync(player[0].player_id.ToString());
                    // Generating the embed and sending to channel
                    var embed = await EmbedHandler.BuildWorshipersEmbedAsync(getGodRanks, getPlayer[0]);

                    await FollowupAsync(embed: embed);
                }
                else
                {
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "wp");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("wr", "Shows win rate percentage for gods played for the requested PlayerName.")]
        public async Task SlashGodWinratesCommand([Summary("PlayerName", "The in-game name of a SMITE player")] string PlayerName = "")
        {
            try
            {
                await DeferAsync();

                var isAlive = await IsSmiteApiAlive(HiRez);
                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }

                if ((string)player[0].ret_msg == "apidown" || !isAlive)
                {
                    var embed = await Reporter.RespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    var getGodRanks = await HiRez.GetGodRanksAsync(player[0].player_id.ToString());
                    // Generating the embed and sending to channel
                    var embed = await EmbedHandler.BuildWinRatesEmbedAsync(getGodRanks, getPlayer[0]);

                    await FollowupAsync(embed: embed);
                }
                else
                {
                    // A select menu event will handle it somewhere else...
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "wr");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("history", "Latest match history for the requested PlayerName.")]
        public async Task SlashMatchHistoryCommand([Summary("PlayerName", "The in-game name of a SMITE player")] string PlayerName = "")
        {
            try
            {
                await DeferAsync();

                var isAlive = await IsSmiteApiAlive(HiRez);
                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }

                if ((string)player[0].ret_msg == "apidown" || !isAlive)
                {
                    var embed = await Reporter.SlashRespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    var matchHistory = await HiRez.GetMatchHistoryAsync(player[0].player_id.ToString());

                    if (matchHistory.Count == 0)
                    {
                        var em = await EmbedHandler.BuildDescriptionEmbedAsync(Utilities.Constants.APIEmptyResponse, 254, 0, 0);
                        await FollowupAsync(embed: em);
                        return;
                    }
                    if (matchHistory[0].ret_msg != null && matchHistory[0].ret_msg.ToString().ToLowerInvariant().Contains("no match history"))
                    {
                        var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{(PlayerName.Length != 0 ? PlayerName : player[0].Name)} has no recent matches.");
                        await FollowupAsync(embed: emb);
                        return;
                    }

                    //if (matchHistory.Count >= 2)
                    //{
                    //    var distinctMh = matchHistory.DistinctBy(x => x.Match).ToList();
                    //    var emb = await EmbedHandler.BuildMatchHistoryEmbedAsync(distinctMh);
                    //
                    //    await FollowupAsync(embed: emb);
                    //    return;
                    //}
                    
                    // Match details select menu
                    List<SelectMenuOptionBuilder> options = new();
                    for (int i = 0; i < matchHistory.Count; i++)
                    {
                        if (i == 24)
                        {
                            break;
                        }
                        string godemoji = Utils.FindGodEmoji(Utilities.Constants.GodsHashSet.ToList(), matchHistory[i].GodId);
                        options.Add(new SelectMenuOptionBuilder()
                        {
                            Label = $"[{matchHistory[i].Win_Status}] {Text.GetQueueName(matchHistory[i].Match_Queue_Id, matchHistory[i].Queue)} - ID: {matchHistory[i].Match}",
                            Description = $"KDA: {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists}",
                            Emote = Emote.Parse(godemoji),
                            Value = matchHistory[i].Match.ToString()
                        });
                    }
                    var comp = new ComponentBuilder().WithSelectMenu("mdselect", placeholder: "Show match details", options: options);

                    var embed = await EmbedHandler.BuildMatchHistoryEmbedAsync(matchHistory);

                    await FollowupAsync(embed: embed, components: comp.Build());
                }
                else
                {
                    // A select menu event will handle it somewhere else...
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "mh");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("md", "Match details for the requested MatchID.")]
        public async Task SlashMatchDetailsMatchIDCommand(uint MatchID)
        {
            try
            {
                await DeferAsync();

                var isAlive = await IsSmiteApiAlive(HiRez);
                int mID = Convert.ToInt32(MatchID);

                if (!isAlive)
                {
                    var embed = await Reporter.SlashRespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }

                // We have a match ID, we go for it
                var matchDetails = await HiRez.GetMatchDetailsAsync(mID.ToString());
                if (matchDetails.Count == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the API sent an empty response which most of the time means the match" +
                        " is not available anymore.\n" +
                        "You can try Smite.Guru instead", $"MatchID: {mID}");
                    await FollowupAsync(embed: embed);
                    return;
                }

                if (matchDetails.Count == 1 && matchDetails[0].ret_msg != null)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync(matchDetails[0].ret_msg.ToString(), $"MatchID: {mID}", 255);
                    await FollowupAsync(embed: embed);
                    return;
                }

                //var finalembed = await EmbedHandler.MatchDetailsEmbed(matchDetails);
                //await FollowupAsync(embed: finalembed.Build());

                // new md kek
                var finalembed = await EmbedHandler.BuildMatchDetailsEmbedAsync(matchDetails);
                await FollowupAsync(embed: finalembed);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("livemd", "Live match details for the requested PlayerName if they are currently in a match.")]
        public async Task SlashLiveMatchCommand([Summary("PlayerName", "The in-game name of a SMITE player")] string PlayerName = "")
        {
            try
            {
                await DeferAsync();

                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }

                if ((string)player[0].ret_msg == "apidown")
                {
                    var embed = await Reporter.RespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    var playerstatus = await HiRez.GetPlayerStatusAsync(player[0].player_id.ToString());

                    // Checking if the player is online and is in match
                    if (playerstatus[0].Match == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{getPlayer[0].hz_player_name ?? getPlayer[0].hz_gamer_tag} is not in a match. [{playerstatus[0].status_string}]");
                        await FollowupAsync(embed: embed);
                        return;
                    }

                    var matchPlayerDetails = await HiRez.GetMatchPlayerDetailsAsync(playerstatus[0].Match.ToString());

                    if (matchPlayerDetails == null || matchPlayerDetails.Count == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but this match seems to be unavailable to show.");
                        await FollowupAsync(embed: embed);
                        return;
                    }
                    if (matchPlayerDetails[0].ret_msg == null)
                    {
                        var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else if (matchPlayerDetails.Count > 1 && matchPlayerDetails[0].ret_msg != null)
                    {
                        await FollowupAsync(matchPlayerDetails[0].ret_msg.ToString());
                        await Reporter.SlashRespondToCommandOnErrorAsync(null, Context, matchPlayerDetails[0].ret_msg.ToString());
                    }
                }
                else
                {
                    // A select menu event will handle it somewhere else...
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "livemd");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("mdlast", "Match details for last match of the requested PlayerName.")]
        public async Task SlashLastMatchDetailsCommand([Summary("PlayerName", "The in-game name of a SMITE player")] string PlayerName = "")
        {
            try
            {
                await DeferAsync();

                int mID = 0;
                var player = await GetPlayerIDsByUsername(Context, HiRez, PlayerName);

                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(PlayerName);
                    await Context.Interaction.FollowupAsync(embed: embed.Build());
                    return;
                }
                if ((string)player[0].ret_msg == "apidown")
                {
                    var embed = await Reporter.RespondToCommandOnErrorAsync(null, null, "apidown");
                    await Context.Interaction.FollowupAsync(embed: embed);
                    return;
                }
                else if ((string)player[0].ret_msg == "notlinked")
                {
                    var embed = await EmbedHandler.BuildNotLinkedEmbedAsync();
                    await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    if (player[0].privacy_flag == "y")
                    {
                        return;
                    }

                    var getPlayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    var matchHistory = await HiRez.GetMatchHistoryAsync(player[0].player_id.ToString());
                    if (matchHistory.Count == 0)
                    {
                        var em = await EmbedHandler.BuildDescriptionEmbedAsync(Utilities.Constants.APIEmptyResponse, 254, 0, 0);
                        await FollowupAsync(embed: em);
                        return;
                    }
                    mID = matchHistory[0].Match;

                    if (mID == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"There are no recent matches in record.");
                        await FollowupAsync(embed: embed);
                        return;
                    }
                    // We have a match ID, we go for it
                    var matchDetails = await HiRez.GetMatchDetailsAsync(mID.ToString());

                    if (matchDetails.Count == 0)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but this match seems to be unavailable.");
                        await FollowupAsync(embed: embed);
                    }

                    if (matchDetails.Count == 1 && matchDetails[0].ret_msg != null)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync(matchDetails[0].ret_msg.ToString(), $"MatchID: {mID}", 255);
                        await FollowupAsync(embed: embed);
                        return;
                    }

                    var finalembed = await EmbedHandler.BuildMatchDetailsEmbedAsync(matchDetails);
                    await FollowupAsync(embed: finalembed);
                }
                else
                {
                    // A select menu event will handle it somewhere else...
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "mdlast");
                    await FollowupAsync("Multiple players found, please select a player via the select menu:", components: comps);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await FollowupAsync(embed: embed);
            }
        }

        [SlashCommand("link", "Link your Discord and SMITE accounts in Thoth's database.")]
        public async Task SlashLinkingInfoCommand()
        {
            try
            {
                var button = new ComponentBuilder();
                var userInDB = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Interaction.User.Id);
                // if the user running the command is already linked
                if (userInDB != null)
                {
                    var getplayer = await HiRez.GetPlayerAsync(userInDB._id.ToString());
                    var getplayerstatus = await HiRez.GetPlayerStatusAsync(userInDB._id.ToString());
                    string statusString = $":eyes: **{getplayerstatus[0].status_string}**";

                    var em = await EmbedHandler.BuildAlreadyLinkedEmbedAsync(getplayer, getplayerstatus);
                    button.WithButton($"Unlink", "unlink", ButtonStyle.Danger, Emoji.Parse("✖️"));
                    await RespondAsync(embed: em, components: button.Build(), ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder();
                embed.WithAuthor(x =>
                {
                    x.Name = "Thoth Account Linking";
                    x.IconUrl = Utilities.Constants.botIcon;
                });
                embed.WithDescription($"Thoth Account Linking will link your Discord and SMITE account in our database which will " +
                    $"allow executing commands without providing PlayerName. \nCommands using this feature are:\n\n" +
                    $"`/stats`, `/livemd`, `/mdlast`, `/history`, `/wp`, `/wr`\n\n" +
                    $"❗**Before starting the linking process, make sure your account in SMITE is NOT hidden! " +
                    $"Linking requires changing your Personal Status Message in-game to verify that the account you're trying to link is yours.** " +
                    $"You can keep using your previous status message after linking is completed." +
                    $"\n__To start the linking process press the \"Start linking\" button and follow the instructions.__");
                embed.WithColor(Utilities.Constants.DefaultBlueColor);
                embed.WithFooter(x => x.Text = "This is not an official Hi-Rez linking!");

                button.WithButton("Start linking", "startlinking", ButtonStyle.Primary, Emoji.Parse("🔗"));
                await RespondAsync(embed: embed.Build(), components: button.Build());
            }
            catch (Exception ex)
            {
                var embedd = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embedd);
            }            
        }

        [SlashCommand("feeds", "Set a channel to get server status notifications and more (soon™).")]
        [CustomRequireContext(Discord.ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels | GuildPermission.ManageGuild)]
        public async Task SlashFeedsCommand()
        {
            try
            {
                var guildSettings = await MongoConnection.GetGuildSettingsAsync(Context.Guild.Id);
                var textChannels = await Context.Guild.GetTextChannelsAsync();
                string serverStString = "";

                if (guildSettings != null)
                {
                    var statusFeeds = guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus);
                    if (statusFeeds.WebhookID != 0)
                    {
                        var webhook = await Context.Guild.GetWebhookAsync(statusFeeds.WebhookID);
                        var channel = await Context.Guild.GetTextChannelAsync(webhook.ChannelId);
                        if (webhook != null)
                        {
                            serverStString = channel.Mention.ToString();
                            if (channel.Id != statusFeeds.ChannelID)
                            {
                                statusFeeds.ChannelID = channel.Id;
                                await MongoConnection.SaveGuildSettingsAsync(guildSettings);
                            }
                        }
                    }
                }

                var embed = await EmbedHandler.BuildDefaultFeedsPage(serverStString, textChannels.Count > 25);

                var buttons = await ComponentsHandler.FeedsSelectMenuAsync(guildSettings, Context);
                await RespondAsync(embed: embed, components: buttons);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("about", "Check out bot statistics, invite link, support server or send feedback.")]
        public async Task SlashThothAboutCommand()
        {
            try
            {
                await DeferAsync();
                int totalUsers = 0;
                foreach (var guild in Connection.Client.Guilds)
                {
                    totalUsers += guild.MemberCount;
                }
                string patch = "n/a";
                var patchInfo = await HiRez.GetPatchInfoAsync();
                patch = patchInfo.Version_string;
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("About Thoth Bot")
                        .WithIconUrl(Utilities.Constants.botIcon);
                });
                embed.WithDescription($"<:Developer:747217301006319737> Owner & Developer: EasyThe#2836 - <@171675309177831424>\n" +
                    $"⚖ Data provided by Hi-Rez. © {DateTime.Now.Year} [Hi-Rez Studios](https://www.hirezstudios.com/), Inc. All rights reserved.");
                embed.WithColor(Utilities.Constants.DefaultBlueColor);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Statistics";
                    x.Value = $":stopwatch: **Uptime**: {GetUptime()}\n" +
                    $"⛓ **Shards Connected: **{Connection.shardsConnected.Count}\n" +
                    $":chart_with_upwards_trend: **Servers**: {Connection.Client.Guilds.Count}\n" +
                    $":busts_in_silhouette: **Users**: {totalUsers}\n" +
                    $":1234: **Commands Run**: {Global.CommandsRun}\n" +
                    $"⏳ **Discord Latency**: {Connection.Client.Latency}ms";
                });
                long playersCount = await MongoConnection.PlayersCount();
                long linkedCount = await MongoConnection.LinkedPlayersCount();
                var feedsStatusCount = MongoConnection.GetFeedGuildsAsync(GuildSettingsModel.FeedType.ServerStatus);
                var statusCount = feedsStatusCount.Where(x => x.Feeds.Exists(z => z.Type == GuildSettingsModel.FeedType.ServerStatus && z.WebhookID != 0)).ToList().Count;
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Thoth's Database";
                    x.Value = $":video_game: **Players**: {playersCount}\n" +
                    $":link: **Linked Players**: {linkedCount}\n" +
                    $":loudspeaker: **SMITE Status Subs**: {statusCount}\n" +
                    $"<:Gods:567146088985919498> **SMITE Version**: {patch}";
                });
                var settings = MongoConnection.GetBotSettings();
                string links = "";
                int counter = 0;
                foreach (var link in settings.AboutLinks)
                {
                    if (counter == 2)
                    {
                        counter = 0;
                        links += "\n";
                    }
                    links += $"[{link.Key}]({link.Value})";
                    if (counter == 0)
                    {
                        links += " | ";
                    }
                    counter++;
                }
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Links";
                    x.Value = links;
                });
                embed.WithFooter(x =>
                {
                    x.Text = $"Discord.NET {DiscordConfig.Version} | {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
                });

                // BUTTONS

                var buttons = await ComponentsHandler.AboutThothButtonsAsync(Context.User.Id == Utilities.Constants.OwnerID, 0);
                await FollowupAsync(embed: embed.Build(), components: buttons);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                var buttons = await ComponentsHandler.AboutThothButtonsAsync(Context.User.Id == Utilities.Constants.OwnerID, 0);
                await FollowupAsync(embed: embed, components: buttons);
            }
        }

        // Player handler
        public static async Task<List<SearchPlayers>> GetPlayerIDsByUsername(IInteractionContext context, HiRezAPIv2 hiRez, string Username = "")
        {
            var actualPlayers = new List<SearchPlayers>();

            if (Username.Length == 0)
            {
                var playerSpec = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(context.User.Id);
                if (playerSpec != null)
                {
                    actualPlayers.Add(new SearchPlayers() { player_id = playerSpec._id });
                    return actualPlayers;
                }
                else
                {
                    actualPlayers.Add(new SearchPlayers() { ret_msg = "notlinked" });
                    return actualPlayers;
                }
            }

            var searchPlayer = await hiRez.SearchPlayersAsync(Username);

            if (searchPlayer.Count == 0)
            {
                return searchPlayer;
            }

            // if api down
            if ((string)searchPlayer[0].ret_msg == "apidown")
            {
                // maybe get the player from db if exists?
                return searchPlayer;
            }

            if (searchPlayer.Count != 0)
            {
                // Finding all occurrences of provided username and adding them in a list
                foreach (var player in searchPlayer)
                {
                    if (player.Name.ToLowerInvariant() == Username.ToLowerInvariant())
                    {
                        actualPlayers.Add(player);
                    }
                }
            }

            if (actualPlayers.Count == 1 && actualPlayers[0].privacy_flag != "n")
            {
                var embed = await EmbedHandler.HiddenProfileEmbed(Username);
                await context.Interaction.FollowupAsync(embed: embed.Build());
                return actualPlayers;
            }

            return actualPlayers;
        }
        public static async Task<string> CalculateTopGameModesForStatsAsync(HiRezAPIv2 hiRez, string playerId)
        {
            try
            {
                var allQueue = new List<AllQueueStats>();
                for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                {
                    int matches = 0;
                    var queueStats = await hiRez.GetQueueStats(playerId, Text.LegitQueueIDs()[i]);
                    if (queueStats.Count != 0 && (string)queueStats[0]?.ret_msg != "apidown")
                    {
                        for (int c = 0; c < queueStats.Count; c++)
                        {
                            if (queueStats[c].Matches != 0)
                            {
                                matches += queueStats[c].Matches;
                            }
                        }
                        allQueue.Add(new AllQueueStats { queueName = queueStats[0].Queue, matches = matches });
                    }
                }
                var orderedQueues = allQueue.OrderByDescending(x => x.matches).ToList();
                if (orderedQueues.Count != 0)
                {
                    return orderedQueues.Count switch
                    {
                        1 => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]",
                        2 => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                             $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]",
                        _ => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                             $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]\n" +
                             $":third_place:{orderedQueues[2].queueName} [{orderedQueues[2].matches}]",
                    };
                }
                return "No match data was found.";
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Console.WriteLine($"CalculateTopGameModesForStatsAsync[CapturedBySentry] - {ex.Message}");
                return "n/a";
            }
        }
        public static async Task<bool> IsSmiteApiAlive(HiRezAPIv2 api)
        {
            var status = await api.GetPlayerStatusAsync("2615245");
            return status.Count != 0;
        }
        public static string GetUptime()
        {
            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "";

            if (time.Days != 0)
            {
                str += $"{time.Days}d ";
            }

            if (time.Hours != 0)
            {
                str += $"{time.Hours}h ";
            }

            if (time.Minutes != 0)
            {
                str += $"{time.Minutes}m ";
            }

            if (time.Seconds != 0 && time.Hours! >= 0 && time.Days! <= 0)
            {
                str += $"{time.Seconds}s";
            }

            return str;
        }
    }
}
