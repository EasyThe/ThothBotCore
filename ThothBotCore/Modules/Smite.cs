using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using ThothBotCore.Utilities.Smite;

namespace ThothBotCore.Modules
{
    [RequireBotPermission(ChannelPermission.UseExternalEmojis)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    [RequireBotPermission(ChannelPermission.ViewChannel)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [Name("SMITE")]
    public class Smite : ModuleBase<SocketCommandContext>
    {
        static Random rnd = new();
        public InteractiveService Interactive { get; set; }
        public HiRezAPIv2 HiRez { get; set; }

        HiRezAPI hirezAPI = new();
        private const string slash = "⚠Thoth is switching to Slash Commands! Please use ";

        [Command("stats", true, RunMode = RunMode.Async)] // DONE
        [Summary("Display stats for the provided `PlayerName`.")]
        [Alias("stat", "pc", "st", "stata", "ст", "статс", "ns", "smitestats")]
        public async Task Stats([Remainder] string PlayerName = "")
        {
            try
            {
                var playerHandler = await PlayerHandler(PlayerName, Context);
                if (playerHandler.playerID == 0)
                {
                    return;
                }
                int playerID = playerHandler.playerID;
                var sentMessage = playerHandler.userMessage;
                EmbedBuilder finalEmbed;

                // Doing the stuff
                if (sentMessage == null)
                {
                    await Context.Channel.TriggerTypingAsync();
                }
                string statusJson = await hirezAPI.GetPlayerStatus(playerID);
                var playerStatus = JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(statusJson);
                string matchJson = "";
                if (playerStatus[0].Match != 0)
                {
                    matchJson = await hirezAPI.GetMatchPlayerDetails(playerStatus[0].Match);
                }

                var getPlayerJson = await hirezAPI.GetPlayer(playerID.ToString());
                if (getPlayerJson.ToLowerInvariant().Contains("privacy flag"))
                {
                    var embed = await EmbedHandler.HiddenProfileEmbed("*");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
                // Generating the embed and sending to channel
                finalEmbed = await EmbedHandler.PlayerStatsEmbed(
                    getPlayerJson,
                    await hirezAPI.GetGodRanks(playerID),
                    await hirezAPI.GetPlayerAchievements(playerID),
                    await hirezAPI.GetPlayerStatus(playerID),
                    matchJson);
                finalEmbed.WithFooter(x => x.Text = slash + "/stats");
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = finalEmbed.Build();
                    });
                }
                else
                {
                    sentMessage = await Context.Channel.SendMessageAsync(embed: finalEmbed.Build());
                }

                // Getting the top queues
                try
                {
                    var allQueue = new List<AllQueueStats>();
                    for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
                    {
                        int matches = 0;
                        var queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(playerID, Text.LegitQueueIDs()[i]));
                        if (queueStats.Count != 0)
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
                        finalEmbed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = $"<:matches:579604410569850891>Most Played Modes";
                            field.Value = orderedQueues.Count switch
                            {
                                1 => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]",
                                2 => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                     $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]",
                                _ => $":first_place:{orderedQueues[0].queueName} [{orderedQueues[0].matches}]\n" +
                                     $":second_place:{orderedQueues[1].queueName} [{orderedQueues[1].matches}]\n" +
                                     $":third_place:{orderedQueues[2].queueName} [{orderedQueues[2].matches}]",
                            };
                        });

                        await sentMessage.ModifyAsync(x =>
                        {
                            x.Embed = finalEmbed.Build();
                        });
                    }
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("10008"))
                    {
                        await Reporter.SendError($"Error in topmatches\n{ex.Message}\nStack Trace: {ex.StackTrace}");
                    }
                }

                // Saving player to DB
                try
                {
                    var getPlayer = JsonConvert.DeserializeObject<List<Player.PlayerStats>>(getPlayerJson);
                    await MongoConnection.SavePlayerAsync(getPlayer[0]).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Text.WriteLine(ex.Message, ConsoleColor.Red, ConsoleColor.White);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("queues", RunMode = RunMode.Async)]
        public async Task StatsTwo([Remainder] string PlayerName = "")
        {
            var playerHandler = await PlayerHandler(PlayerName, Context);
            if (playerHandler.playerID == 0)
            {
                return;
            }
            await Context.Channel.TriggerTypingAsync();
            int id = playerHandler.playerID;
            int allmatchescount = 0;
            double totalminutes = 0;
            var embed = new EmbedBuilder
            {
                Title = $"{id}"
            };
            await Context.Channel.TriggerTypingAsync();
            for (int i = 0; i < Text.LegitQueueIDs().Count; i++)
            {
                int matches = 0, kills = 0, deaths = 0, assists = 0, minutes = 0, wins = 0, losses = 0;
                var queueStats = JsonConvert.DeserializeObject<List<QueueStats>>(await hirezAPI.GetQueueStats(id, Text.LegitQueueIDs()[i]));
                if (queueStats.Count != 0)
                {
                    for (int c = 0; c < queueStats.Count; c++)
                    {
                        matches += queueStats[c].Matches;
                        allmatchescount += queueStats[c].Matches;
                        totalminutes += queueStats[c].Minutes;
                        kills += queueStats[c].Kills;
                        deaths += queueStats[c].Deaths;
                        assists += queueStats[c].Assists;
                        minutes += queueStats[c].Minutes;
                        wins += queueStats[c].Wins;
                        losses += queueStats[c].Losses;
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = queueStats[0].Queue;
                        x.Value = $"**Matches: **{matches}\n" +
                        $"**Kills: **{kills}\n" +
                        $"**Deaths: **{deaths}\n" +
                        $"**Assists: **{assists}\n" +
                        $"**Minutes: **{minutes}\n" +
                        $"**Wins: ** {wins}\n" +
                        $"**Losses: **{losses}";
                    });
                }
            }
            embed.WithDescription($"Total Matches: {allmatchescount}\n" +
                                  $"Total Minutes: {totalminutes}");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("gods", true)] //DONE
        [Summary("Overall information about the gods in the game and current free god rotation.")]
        [Alias("годс")]
        public async Task GodsCommand()
        {
            List<Gods.God> gods = MongoConnection.GetAllGods();

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

                foreach (var god in gods)
                {
                    if (god.OnFreeRotation == "true")
                    {
                        onRotation.Append($"{god.Emoji} {god.Name}\n");
                    }
                }
                embed.WithColor(Constants.DefaultBlueColor);
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
                embed.WithFooter(x =>
                {
                    x.Text = $"{slash}/gods";
                });

                await ReplyAsync("", false, embed.Build());

            }
            else
            {
                await ReplyAsync("Something is not right... Please report this in my support server.");
            }
        }

        [Command("god", true)] // DONE
        [Summary("Provides information about GodName.")]
        [Alias("g")]
        public async Task GodInfo([Remainder] string GodName)
        {
            string titleCaseGod = Text.ToTitleCase(GodName);
            Gods.God gods = await MongoConnection.GetGodByNameAsync(titleCaseGod);

            if (gods == null)
            {
                await ReplyAsync($"{titleCaseGod} was not found.", allowedMentions: AllowedMentions.None);
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods.Name);
                    author.WithIconUrl(gods.godIcon_URL);
                });
                embed.WithTitle(gods.Title);
                embed.WithThumbnailUrl(gods.godIcon_URL);
                if (gods.DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods.DomColor));
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Pantheon";
                    field.Value = gods.Pantheon;
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Type";
                    field.Value = gods.Type;
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = "Role";
                    field.Value = gods.Roles;
                });
                if (gods.Pros != null || gods.Pros != " " || gods.Pros != "")
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = false;
                        field.Name = "Pros";
                        field.Value = gods.Pros + "\u200b";
                    });
                }
                embed.WithFooter(x =>
                {
                    x.IconUrl = Constants.botIcon;
                    x.Text = $"{slash}/god";
                });

                await ReplyAsync(embed: embed.Build(),
                    allowedMentions: AllowedMentions.None);
            }
        }

        [Command("rgod", true, RunMode = RunMode.Async)] // DONE
        [Summary("Gives you a random God and randomised build.")]
        [Remarks("`m` or `mage` for **mage**, `w` or `warrior` for **warrior**, `h` or `hunter` for **hunter**, `g`, `tank` or `guardian` for **guardian**, `a`, `ass` or `assassin` for **assassin**")]
        [Alias("rg", "randomgod", "random")]
        public async Task RandomGod([Remainder]string godClass = "")
        {
            List<Gods.God> godsF = MongoConnection.GetAllGods();
            List<Gods.God> gods = null;
            if (godClass != "")
            {
                godClass = godClass.ToLowerInvariant().Trim();
                gods = godClass switch
                {
                    "m" or "mage" => godsF.Where(x => x.Roles.Contains("Mage")).ToList(),
                    "w" or "warrior" => godsF.Where(x => x.Roles.Contains("Warrior")).ToList(),
                    "h" or "hunter" => godsF.Where(x => x.Roles.Contains("Hunter")).ToList(),
                    "g" or "tank" or "guardian" => godsF.Where(x => x.Roles.Contains("Guardian")).ToList(),
                    "a" or "ass" or "assassin" => godsF.Where(x => x.Roles.Contains("Assassin")).ToList(),
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
                x.Text = $"{slash}/rgod";
            });
            await ReplyAsync($"{Context.Message.Author.Mention}, your random god is:", false, embed.Build());
        }

        [Command("rbuild", true)] // DONE
        [Summary("Gives you a random build for the requested god")]
        [Alias("rb", "randombuild")]
        public async Task RandomBuildCommand([Remainder][Name("GodName")] string godName)
        {
            string titleCaseGod = Text.ToTitleCase(godName);
            Gods.God god = await MongoConnection.GetGodByNameAsync(titleCaseGod);

            if (god == null)
            {
                var em = await EmbedHandler.BuildDescriptionEmbedAsync($"{titleCaseGod} was not found.");
                await ReplyAsync(embed: em);
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
                    x.Text = $"{slash}/rbuild";
                });
                await ReplyAsync($"{Context.Message.Author.Mention}, your random build for {god.Name} is:", embed: embed.Build());
            }
        }

        [Command("rteam", true)] // DONE
        [Summary("Gives you `number` random Gods with randomised builds for them.")]
        [Alias("team", "ртеам", "теам", "rt")]
        public async Task RandomTeam(int number)
        {
            if (!(number > 5))
            {
                var embed = new EmbedBuilder();
                var gods = MongoConnection.GetAllGods();

                embed.WithColor(Constants.DefaultBlueColor);

                for (int i = 0; i < number; i++)
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
                    x.Text = $"{slash}/rteam";
                });

                await ReplyAsync($"Team of {number} for you, {Context.Message.Author.Mention}!", false, embed.Build());
            }
            else
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("That's not a proper number for a team, don't ya' think?", 254);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("ritem", true)] // NO
        [Alias("ri")]
        public async Task RandomItemCommand()
        {
            var item = MongoConnection.GetAllItems().FindAll(x => x.ActiveFlag == "y" && x.ItemTier == 3 && x.Type == "Item");
            int r = rnd.Next(item.Count);
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = item[r].DeviceName;
                x.IconUrl = item[r].itemIcon_URL;
            });
            embed.WithColor(new Color((uint)item[r].DomColor));
            embed.WithThumbnailUrl(item[r].itemIcon_URL);
            await ReplyAsync(
                embed: embed.Build(),
                messageReference: new MessageReference(Context.Message.Id,Context.Channel.Id,Context.Guild.Id),
                allowedMentions: new AllowedMentions(){ MentionRepliedUser = false });
        }

        [Command("status", true, RunMode = RunMode.Async)] // DONE
        [Summary("Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.")]
        [Alias("статус", "statis", "s", "с", "server", "servers", "se", "се", "serverstatus")]
        public async Task ServerStatusCheck()
        {
            List<HiRezServerStatus> hirezServerStatus = new();

            await Context.Channel.TriggerTypingAsync();
            var smiteServerStatus = JsonConvert.DeserializeObject<ServerStatus>(await APIInteractions.GetStatusSummary());
            var hirezStatusString = await hirezAPI.GetHiRezServerStatus();
            if (!hirezStatusString.Contains("<html>"))
            {
                hirezServerStatus = JsonConvert.DeserializeObject<List<HiRezServerStatus>>(hirezStatusString);
            }
            else
            {
                hirezServerStatus.Add(new() { Platform = "pc", Status = "API Unavailable", Limited_access = false, Environment = "live" });
                hirezServerStatus.Add(new() { Platform = "xbox", Status = "API Unavailable", Limited_access = false, Environment = "live" });
                hirezServerStatus.Add(new() { Platform = "ps4", Status = "API Unavailable", Limited_access = false, Environment = "live" });
                hirezServerStatus.Add(new() { Platform = "switch", Status = "API Unavailable", Limited_access = false, Environment = "live" });
                hirezServerStatus.Add(new() { Platform = "pc", Status = "API Unavailable", Limited_access = false, Environment = "pts" });
            }

            var statusEmbed = await EmbedHandler.ServerStatusEmbedAsync(smiteServerStatus, hirezServerStatus);
            await ReplyAsync(embed: statusEmbed);

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
                var embed = await EmbedHandler.StatusIncidentEmbed(smiteServerStatus);
                if (embed != null)
                {
                    await ReplyAsync(embed: embed.Build());
                }
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
                var embed = await EmbedHandler.StatusMaintenanceEmbed(smiteServerStatus);
                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("statusupdates", true)] // FEEDS DONE
        [Summary("When SMITE incidents and scheduled maintenances appear in the status page they will be sent to #channel")]
        [Alias("statusupd", "su")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task SetStatusUpdatesChannel()
        {
            await ReplyAsync("Please use `/feeds` to set or unset a channel.");
        }

        [Command("item")] // DONE
        [Summary("Provides information about `ItemName`.")]
        [Alias("i")]
        public async Task ItemInfoCommand([Remainder] string ItemName)
        {
            int index = 0;
            var item = await MongoConnection.GetSpecificItemAsync(ItemName);
            if (item.Count != 0)
            {
                if (item.Any(x=> x.DeviceName.ToLowerInvariant() == ItemName.ToLowerInvariant()))
                {
                    index = item.FindIndex(x => x.DeviceName.ToLowerInvariant() == ItemName.ToLowerInvariant());
                }
                string secondaryDesc = Text.ReformatSecondaryItemDescription(item[index].ItemDescription.SecondaryDescription);
                var embed = new EmbedBuilder();

                embed.WithAuthor(x =>
                {
                    x.Name = item[index].DeviceName;
                    x.IconUrl = item[index].itemIcon_URL;
                });
                embed.WithColor(new Color((uint)item[index].DomColor));
                embed.WithThumbnailUrl(item[index].itemIcon_URL);
                StringBuilder itemBenefits = new();
                foreach (var benefit in item[index].ItemDescription.Menuitems)
                {
                    itemBenefits.AppendLine($"{benefit.Value} {benefit.Description}");
                }
                embed.WithTitle(itemBenefits.ToString());
                embed.WithDescription($"{(item[index].StartingItem ? "**Starting Item**" : "")}" +
                    $"{(item[index].ItemDescription?.Description?.Length != 0 ? $"\n{item[index].ItemDescription.Description}" : "")}\n\n{secondaryDesc}");

                // calculating price
                var itemsForPrice = new List<GetItems.Item>();
                var itemsForPriceTwo = new List<GetItems.Item>();
                int itemPrice = 0;

                if (item[index].ChildItemId != 0)
                {
                    itemsForPrice = await MongoConnection.GetSpecificItemByIDAsync(item[index].ChildItemId);
                    if (itemsForPrice != null && itemsForPrice.Count != 0)
                    {
                        if (itemsForPrice[0].ChildItemId != 0)
                        {
                            itemsForPriceTwo = await MongoConnection.GetSpecificItemByIDAsync(itemsForPrice[0].ChildItemId);
                            itemPrice = itemsForPriceTwo[0].Price;
                        }
                        itemPrice = itemPrice + item[index].Price + itemsForPrice[0].Price;
                    }
                }
                else
                {
                    itemPrice = item[index].Price;
                }

                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Total Price (Price)";
                    x.Value = $"<:coins:590942235474919464>{itemPrice} ({item[index].Price})";
                });
                if (!item[index].StartingItem || item[index].Type != "Consumable")
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Tier";
                        x.Value = $"{item[index].ItemTier}";
                    });
                }

                // related items
                try
                {
                    var allitems = MongoConnection.GetAllItems();
                    allitems.RemoveAll(x => x.ActiveFlag != "y");
                    List<GetItems.Item> relatedItems = new();
                    relatedItems.AddRange(allitems.FindAll(x => x.ChildItemId == item[index].ItemId));
                    if (item[index].ChildItemId != 0)
                    {
                        relatedItems.Add(allitems.Find(x => x.ItemId == item[index].ChildItemId));
                    }
                    if (item[index].RootItemId != 0 && !relatedItems.Any(x => x.ItemId == item[index].RootItemId))
                    {
                        relatedItems.Add(allitems.Find(x => x.ItemId == item[index].RootItemId));
                    }
                    // Remove duplicate items 
                    if (relatedItems.Any(x=> x.ItemId == item[index].ItemId))
                    {
                        relatedItems.Remove(relatedItems.Find(x=> x.ItemId == item[index].ItemId));
                    }
                    if (relatedItems.Count != 0)
                    {
                        itemBenefits.Clear();
                        foreach (var relitem in relatedItems)
                        {
                            itemBenefits.AppendLine($"{relitem.Emoji} {relitem.DeviceName}");
                        }
                        embed.AddField(x =>
                        {
                            x.IsInline = false;
                            x.Name = "Related Items";
                            x.Value = itemBenefits.ToString();
                        });
                    }
                }
                catch (Exception ex)
                {
                    await Reporter.SendException(null, Context, $"Item command got an error on related items\n{ex.Message}\nOn item: {item[index].DeviceName}");
                }
                // SLASH
                embed.WithFooter(x => x.Text = $"{slash}/item");

                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                var em = await EmbedHandler.BuildDescriptionEmbedAsync($"Sorry, I couldn't find `{ItemName}`.");
                await ReplyAsync(embed: em);
            }
        }
        
        [Command("itemstarters", true)] // DONE
        [Summary("Provides a list with all starting items.")]
        [Alias("starters")]
        public async Task ItemStartersCommand()
        {
            var items = MongoConnection.GetAllItems();
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
                    if (counter !> maxCount - 1)
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
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle($"List of all {starters.Count} starting items");
            embed.AddField(x=> 
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
            embed.WithFooter(x => x.Text = $"{slash}/starters");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("trello", true, RunMode = RunMode.Async)] // DONE
        [Summary("Checks the [SMITE Community Issues Trello Board](https://trello.com/b/d4fJtBlo/smite-community-issues).")]
        [Alias("issues", "bugs", "board")]
        public async Task TrelloBoardCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                var result = await APIInteractions.GetTrelloCards();

                StringBuilder topIssues = new();
                StringBuilder hotfixNotes = new();
                StringBuilder incominghotfix = new();
                StringBuilder alreadyFixedInLIVE = new();

                int count = 0;
                int livecount = 0;
                // Appending the issues
                foreach (var item in result)
                {
                    // Top Issues
                    if (item.idList == "5c740d7d4e18c107890167ea")
                    {
                        topIssues.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Hotfix PatchNotes
                    if (item.idList == "5c740da2ff81b93a4039da81" && count != 10)
                    {
                        count++;
                        hotfixNotes.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Incoming hotfix
                    if (item.idList == "5c804623d75e55500472cf9a")
                    {
                        incominghotfix.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Fixed in LIVE
                    if (item.idList == "5c7e9e5e30dbfd27cb7c4442" && livecount != 7)
                    {
                        livecount++;
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
                if (incominghotfix.ToString() != "")
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
                        x.IsInline = true;
                        x.Name = "Hotfix Patch Notes";
                        x.Value = hotfixNotes.ToString();
                    });
                }

                embed.WithColor(210, 144, 52);
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

                // SLASH
                embed.WithFooter(x => x.Text = $"{slash}/bugs");

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the Trello API is down. Try visiting the [website](https://trello.com/b/d4fJtBlo/smite-community-issues) instead.");
                await ReplyAsync(embed: embed);
                await Reporter.SendError($"**Trello Error: **\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        [Command("livematch", RunMode = RunMode.Async)] // DONE
        [Summary("Match details if provided `PlayerName` is in a match.")]
        [Alias("live", "lm", "l")]
        public async Task LiveMatchCommand([Remainder] string PlayerName = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                var playerHandler = await PlayerHandler(PlayerName, Context);
                if (playerHandler.playerID == 0)
                {
                    return;
                }
                int playerID = playerHandler.playerID;
                var sentMessage = playerHandler.userMessage;
                if (PlayerName == "")
                {
                    PlayerName = playerHandler.playerName;
                }

                var playerstatus = JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(await hirezAPI.GetPlayerStatus(playerID));
                // Checking if the player is online and is in match
                if (playerstatus[0]?.Match == 0)
                {
                    if (sentMessage == null)
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{PlayerName} is not in a match.");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                    else
                    {
                        var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{PlayerName} is not in a match.");
                        await sentMessage.ModifyAsync(x =>
                        {
                            x.Embed = embed;
                        });
                        return;
                    }
                }
                var matchPlayerDetails = new List<MatchPlayerDetails.PlayerMatchDetails>(
                    JsonConvert.DeserializeObject<List<MatchPlayerDetails.PlayerMatchDetails>>(
                        await hirezAPI.GetMatchPlayerDetails(playerstatus[0].Match)));

                if (matchPlayerDetails[0].ret_msg == null)
                {
                    if (sentMessage == null)
                    {
                        var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                        embed.WithFooter($"{slash}/livemd");
                        await sentMessage.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                    }
                }
                else
                {
                    await ReplyAsync(matchPlayerDetails[0].ret_msg.ToString());
                    await Reporter.RespondToCommandOnErrorAsync(null, Context, matchPlayerDetails[0].ret_msg.ToString());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("matchdetails", RunMode = RunMode.Async)] // DONE
        [Summary("Match details for the provided `MatchID` or latest match played of provided `PlayerName`.")]
        [Alias("md", "мд")]
        public async Task MatchDetailsCommand([Name("MatchID or PlayerName")][Remainder] string MatchID = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                PlayerHandlerStruct playerHandler = new();
                int mID = 0, playerID = 0;
                var matchHistory = new List<MatchHistoryModel>();

                // If the provided string is not a match ID
                if (!(MatchID.All(char.IsDigit)) || MatchID == "")
                {
                    playerHandler = await PlayerHandler(MatchID, Context);
                }
                else
                {
                    mID = Convert.ToInt32(MatchID);
                }

                if (playerHandler.playerID == 0 && mID == 0)
                {
                    return;
                }
                playerID = playerHandler.playerID;
                var sentMessage = playerHandler.userMessage;

                // If there's no Match ID, get latest match played from history
                if (mID == 0)
                {
                    matchHistory = await hirezAPI.GetMatchHistory(playerID);
                    if (matchHistory.Count == 0)
                    {
                        var em = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.APIEmptyResponse, 254, 0, 0);
                        await ReplyAsync(embed: em);
                        return;
                    }
                    mID = matchHistory[0].Match;
                }

                if (mID == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"There are no recent matches in record.");
                    await ReplyAsync(embed: embed);
                    return;
                }

                // We have a match ID, we go for it
                string matchDetailsString = await hirezAPI.GetMatchDetails(mID);
                if (matchDetailsString.ToLowerInvariant().Contains("<"))
                {
                    await ReplyAsync("Hi-Rez API sent a weird response... Please try again later.");
                    await Reporter.SendError(matchDetailsString);
                    return;
                }
                else if (matchDetailsString == "[]")
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the API sent an empty response which most of the time means the match" +
                        " is not available anymore.\n" +
                        "You can try Smite.Guru instead", $"MatchID: {mID}");
                    await ReplyAsync(embed: embed);
                    return;
                }
                var matchDetails = JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(matchDetailsString);
                if (matchDetails.Count == 1 && matchDetails[0].ret_msg != null)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync(matchDetails[0].ret_msg.ToString(), $"MatchID: {mID}", 255);
                    await ReplyAsync(embed: embed);
                    return;
                }
                var finalembed = await EmbedHandler.MatchDetailsEmbed(matchDetails);
                finalembed.WithFooter($"{slash}/mdlast or /md");
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = finalembed.Build();
                    });
                }
                else
                {
                    await ReplyAsync(embed: finalembed.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("matchhistory", true, RunMode = RunMode.Async)] // DONE
        [Summary("Latest match history for `PlayerName`.")]
        [Alias("mh", "мх", "history", "matchistory")]
        public async Task MatchHistoryCommand([Remainder] string PlayerName = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                var matchHistory = new List<MatchHistoryModel>();

                var playerHandler = await PlayerHandler(PlayerName, Context);
                if (playerHandler.playerID == 0)
                {
                    return;
                }

                matchHistory = await hirezAPI.GetMatchHistory(playerHandler.playerID);
                var sentMessage = playerHandler.userMessage;

                if (matchHistory.Count == 0)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.APIEmptyResponse, 254, 0, 0);
                    await ReplyAsync(embed: em);
                    return;
                }
                if (matchHistory[0].ret_msg != null && matchHistory[0].ret_msg.ToString().ToLowerInvariant().Contains("no match history"))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{playerHandler.playerName} has no recent matches.");
                    await ReplyAsync(embed: emb);
                    return;
                }
                var finalembed = await EmbedHandler.BuildMatchHistoryEmbedAsync(matchHistory);
                var da = finalembed.ToEmbedBuilder().WithFooter(x => x.Text = $"{slash}/history"); // SLASH

                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = da.Build();
                    });
                }
                else
                {
                    await ReplyAsync(embed: da.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("motd", true, RunMode = RunMode.Async)] // DONE
        [Summary("Information about upcoming MOTDs in the game.")]
        [Alias("motds", "мотд", "мотдс")]
        public async Task MotdCommand()
        {
            try
            {
                string json = await hirezAPI.GetMOTD();
                var motdList = JsonConvert.DeserializeObject<List<Motd>>(json);
                string desc = "";
                string embedValue = "";
                bool addedTodaysMotd = false;

                var embed = new EmbedBuilder();

                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithAuthor(x =>
                {
                    x.Name = "Current & Upcoming Matches Of The Day";
                    x.IconUrl = Constants.botIcon;
                });
                embed.WithFooter(x =>
                {
                    x.IconUrl = Constants.botIcon;
                    x.Text = $"{slash}/motd";
                });

                Motd motdDay = new();

                for (int i = 0; i < 5; i++)
                {
                    string[] finalDesc = Array.Empty<string>();
                    motdDay = motdList.Find(x => x.startDateTime.Date == DateTime.Today.AddDays(i));

                    // Checking if the motd for the day has actually changed or not
                    if (motdDay != null && DateTime.Now <= motdDay.startDateTime && !addedTodaysMotd)
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
                    embed.WithDescription("No data available.");
                }
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("wp", RunMode = RunMode.Async)] // DONE
        [Summary("Shows all masteries for gods played by the provided `PlayerName`.")]
        [Alias("wps", "worshipers", "mastery", "masteries", "worshippers", "worshipper", "worshiper")]
        public async Task WorshipersCommand([Remainder] string PlayerName = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                int playerID = 0;
                var playerHandler = await PlayerHandler(PlayerName, Context);

                if (playerHandler.playerID == 0)
                {
                    return;
                }
                playerID = playerHandler.playerID;
                var sentMessage = playerHandler.userMessage;

                string getplayerjson = await hirezAPI.GetPlayer(playerID.ToString());
                var getplayer = JsonConvert.DeserializeObject<List<Player.PlayerStats>>(getplayerjson);
                string json = await hirezAPI.GetGodRanks(playerID);
                var ranks = JsonConvert.DeserializeObject<List<GodRanks>>(json);
                var finalEmbed = await EmbedHandler.BuildWorshipersEmbedAsync(ranks, getplayer[0]);

                //SLASH
                var emm = finalEmbed.ToEmbedBuilder().WithFooter($"{slash}/wp");

                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = emm.Build();
                    });
                }
                else
                {
                    await Context.Channel.SendMessageAsync(embed: emm.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("winrates", RunMode = RunMode.Async)] // DONE
        [Summary("Shows win rate percentage for gods played by the provided `PlayerName`.")]
        [Alias("wr", "rates", "winrate")]
        public async Task GodWinRatesCommand([Remainder] string PlayerName = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                int playerID = 0;
                var playerHandler = await PlayerHandler(PlayerName, Context);

                if (playerHandler.playerID == 0)
                {
                    return;
                }
                playerID = playerHandler.playerID;
                var sentMessage = playerHandler.userMessage;

                string getplayerjson = await hirezAPI.GetPlayer(playerID.ToString());
                var getplayer = JsonConvert.DeserializeObject<List<Player.PlayerStats>>(getplayerjson);
                string json = await hirezAPI.GetGodRanks(playerID);
                var ranks = JsonConvert.DeserializeObject<List<GodRanks>>(json);
                var finalEmbed = await EmbedHandler.BuildWinRatesEmbedAsync(ranks, getplayer[0]);
                var emm = finalEmbed.ToEmbedBuilder().WithFooter($"{slash}/wp");
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = emm.Build();
                    });
                }
                else
                {
                    await Context.Channel.SendMessageAsync(embed: emm.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("link", true)] // DONE
        [Summary("Link your Discord and SMITE accounts.")]
        public async Task LinkingInfoCommand()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = "Thoth Account Linking";
                x.IconUrl = Constants.botIcon;
            });
            embed.WithDescription($"Thoth Account Linking will link your Discord and SMITE account in our database which will " +
                $"allow executing commands without providing InGameName. \nCommands using this feature are:\n" +
                $"`{Credentials.botConfig.prefix}stats`, `{Credentials.botConfig.prefix}livematch`, " +
                $"`{Credentials.botConfig.prefix}matchdetails`, `{Credentials.botConfig.prefix}matchhistory`, " +
                $"`{Credentials.botConfig.prefix}wp`, `{Credentials.botConfig.prefix}wr`\n" +
                $"❗**Before starting the linking process, make sure your account in SMITE is NOT hidden! " +
                $"Linking requires changing your Personal Status Message in-game to verify that the said account is yours.** " +
                $"You can change it to your previous status message after linking is completed." +
                $"\n\n__To start the linking process use `/link` and follow the instructions.__");
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithFooter(x => x.Text = $"This is not an official Hi-Rez linking!\n{slash}/link");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("patch", true, RunMode = RunMode.Async)] // DONE
        [Summary("Sends the last two patches posted on the SMITEgame.com website")]
        [Alias("lastpatch", "notes", "patchnotes", "updatenotes")]
        public async Task PatchNotesCommand()
        {
            await Context.Channel.TriggerTypingAsync();
            var posts = await APIInteractions.FetchPostsAsync();
            var foundPost = posts.FindAll(x => x.real_categories.ToLowerInvariant().Contains("notes"));
            for (int i = 0; i < 2; i++)
            {
                var actualPost = await APIInteractions.GetPostBySlugAsync(foundPost[i].slug);
                string description = await PatchPageReader.ReadPatch(actualPost);
                Embed embed = await EmbedHandler.BuildPatchNotesEmbedAsync(actualPost, description, foundPost[i].featured_image, foundPost[i].slug);
                await ReplyAsync($"`{slash}/updatenotes`", embed: embed);
            }
        }

        [Command("events", true, RunMode = RunMode.Async)] // DONE
        [Alias("event", "eventnow", "eventsnow", "eventtoday", "eventstoday")]
        [Summary("Shows if there are any events currently available in-game.")]
        public async Task EventsCommand()
        {
            try
            {
                StringBuilder sb = new();
                var result = await APIInteractions.GetLandingPanel();
                var embed = new EmbedBuilder();
                embed.WithAuthor(x =>
                {
                    x.Name = "SMITE Events";
                    x.IconUrl = Constants.SmiteBolt;
                    x.Url = "https://www.smitegame.com/news/";
                });
                embed.WithFooter(x=> 
                {
                    x.Text = $"{slash}/events";
                });
                if (result.events.content.Count == 0)
                {
                    embed.WithTitle("There are no events at the moment.");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }
                var header = result.events.content.FirstOrDefault().eventList.Find(x => x.header != null);
                if (header != null)
                {
                    embed.WithTitle(header.header.@default);
                }
                embed.WithImageUrl(result.events.content.FirstOrDefault()?.imageUrl);
                embed.WithColor(Constants.FeedbackColor);
                foreach (var item in result.events.content[0].eventList)
                {
                    sb.AppendLine($"🔹 " + (item.desc.@default.Contains("Today") ? $"**{item.desc.@default}**" : $"{item.desc.@default}"));
                }
                embed.WithDescription(sb.ToString());
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                await Reporter.RespondToCommandOnErrorAsync(ex, Context);
            }
        }

        [Command("tt", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TestGetPlayer(int id)
        {
            try
            {
                //var gods = MongoConnection.GetAllGods();

                //await HiRez.GetGodSkinsAsync();
                var da = Text.GetQueueName(id);
                await ReplyAsync(da);

                //var settings = MongoConnection.GetBotSettings();
                //settings.Placeholders = new[] 
                //{ 
                //    "We're no strangers to love\nYou know the rules and so do I\nA full commitment's what I'm thinking of",
                //    "You wouldn't get this from any other guy\nI just wanna tell you how I'm feeling",
                //    "Never gonna give you up\nNever gonna let you down\nNever gonna run around and desert you",
                //    "We've known each other for so long\nYour heart's been aching but you're too shy to say it",
                //    "We know the game and we're gonna play it\nAnd if you ask me how I'm feeling",
                //    "Somebody once told me the world is gonna roll me\nI ain't the sharpest tool in the shed",
                //    "She was looking kind of dumb with her finger and her thumb\nIn the shape of an \"L\" on her forehead",
                //    "Well the years start coming and they don't stop coming and they don't stop coming and they don't"
                //};

                //await MongoConnection.SaveBotSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("nz", RunMode = RunMode.Async)] // keep it simple pls
        [RequireOwner]
        public async Task NzVrat(string endpoint, [Remainder]string value)
        {
            string json = "";
            dynamic parsedJson = null;
            try
            {
                json = await hirezAPI.APITestMethod(endpoint, value);
                Console.WriteLine(json);
                parsedJson = JsonConvert.DeserializeObject(json);

                await ReplyAsync($"```json\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}```");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync($"{endpoint}.json", json);
                    await Context.Channel.SendFileAsync($"{endpoint}.json");
                }
                else
                {
                    await ReplyAsync(ex.Message);
                }
            }
        }

        // Shit/fun commands lul
        [Command("rank", true, RunMode = RunMode.Async)] // DONE
        [Summary("Gives you random ranked division.")]
        [Alias("ранк")]
        public async Task RandomRankCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                int n;
                n = rnd.Next(0, 28);
                embed.WithColor(rnd.Next(255), rnd.Next(255), rnd.Next(255));
                embed.WithAuthor(x =>
                {
                    x.IconUrl = Context.Message.Author.GetAvatarUrl();
                    x.Name = $"{Context.Message.Author.Username}'s rank is:";
                });
                if (Context.Message.MentionedUsers.Count > 0)
                {
                    var mentionedUser = Context.Message.MentionedUsers.First();
                    embed.Author.Name = $"{mentionedUser.Username}'s rank is:";
                    embed.Author.IconUrl = mentionedUser.GetAvatarUrl();
                }
                embed.WithTitle($"{Text.GetRankedConquest(n).Item2} {Text.GetRankedConquest(n).Item1}");
                embed.WithFooter(x =>
                {
                    x.Text = Text.GetRandomTip();
                });

                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        public async Task<PlayerHandlerStruct> PlayerHandler(string input, SocketCommandContext context)
        {
            var handler = new PlayerHandlerStruct
            {
                userMessage = null
            };

            PlayerSpecial getPlayerByDiscordID;

            // Checking if we are searching for Player who linked his Discord with SMITE account
            if (input == "")
            {
                getPlayerByDiscordID = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Message.Author.Id);
                if (getPlayerByDiscordID != null)
                {
                    handler.playerID = getPlayerByDiscordID._id;
                }
                else
                {
                    // TO DO: do a thing to check the command usage automatically.
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Please check the command usage via the `!!help` command");
                    await context.Channel.SendMessageAsync(embed: embed);
                    return handler;
                }
            }
            else if (Context.Message.MentionedUsers.Count != 0)
            {
                var mentionedUser = Context.Message.MentionedUsers.Last();
                getPlayerByDiscordID = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(mentionedUser.Id);
                if (getPlayerByDiscordID != null)
                {
                    handler.playerID = getPlayerByDiscordID._id;
                }
                else
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                    await context.Channel.SendMessageAsync(embed: embed);
                    return handler;
                }
            }
            else if (input.StartsWith("<@!"))
            {
                ulong id = (ulong)Int64.Parse(input.Split('!')[1].Split('>')[0]);
                getPlayerByDiscordID = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(id);
                if (getPlayerByDiscordID != null)
                {
                    handler.playerID = getPlayerByDiscordID._id;
                }
                else
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync(Constants.NotLinked);
                    await context.Channel.SendMessageAsync(embed: embed);
                    return handler;
                }
            }
            if (input.Contains('\\') || input.Contains('/'))
            {
                if (input.Contains('\\'))
                {
                    input = input.Replace("\\", String.Empty);
                }
                if (input.Contains('/'))
                {
                    input = input.Replace("/", String.Empty);
                }
            }

            var actualPlayers = new List<SearchPlayers>();
            // If the player is not linked
            if (handler.playerID == 0)
            {
                var searchPlayer = await hirezAPI.SearchPlayer(input);

                // Finding all occurrences of provided username and adding them in a list
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == input.ToLowerInvariant())
                        {
                            actualPlayers.Add(player);
                        }
                    }
                }
                // No players
                if (actualPlayers.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(input);
                    await context.Channel.SendMessageAsync(embed: embed.Build());
                    return handler;
                }
                // Only one player
                else if (!(actualPlayers.Count > 1))
                {
                    if (actualPlayers[0].privacy_flag != "n")
                    {
                        var embed = await EmbedHandler.HiddenProfileEmbed(input);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        return handler;
                    }
                    handler.playerID = actualPlayers[0].player_id;
                    handler.playerName = actualPlayers[0].Name;
                }
                // Multiple players
                else
                {
                    var result = await MultiplePlayersHandler(actualPlayers, Context);
                    if (result.searchPlayers != null && result.searchPlayers.player_id == 0)
                    {
                        var embed = await EmbedHandler.ProfileNotFoundEmbed(input);
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        return handler;
                    }
                    else if (result.searchPlayers == null && result.userMessage == null)
                    {
                        return handler;
                    }
                    handler.playerID = result.searchPlayers.player_id;
                    handler.userMessage = result.userMessage;
                    handler.playerName = result.searchPlayers.Name;
                }
            }
            return handler;
        }
        public async Task<MultiplePlayersStruct> MultiplePlayersHandler(List<SearchPlayers> searchPlayers, SocketCommandContext context, IUserMessage message = null)
        {
            var multiplePlayersStruct = new MultiplePlayersStruct
            {
                searchPlayers = null,
                userMessage = null
            };
            if (searchPlayers.Count > 20)
            {
                var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"There are more than 20 accounts({searchPlayers.Count}) " +
                    $"with the username **{searchPlayers[0].Name}**. Due to Discord limits, I cannot fit all of them in one message." +
                    $" Please [contact]({Constants.SupportServerInvite}) the bot developer " +
                    $"for further assistance.", 107, 70, 147);
                await context.Channel.SendMessageAsync(embed: emb);
                return multiplePlayersStruct;
            }
            var embed = await EmbedHandler.MultiplePlayers(searchPlayers);
            if (message == null)
            {
                message = await context.Channel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
            }
            var response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(120));
            if (response == null || !(response.Value.Content.All(char.IsDigit)))
            {
                embed.WithFooter(x =>
                {
                    x.Text = "CANCELLED";
                });
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return multiplePlayersStruct;
            }
            int responseNum = int.Parse(response.Value.Content);
            if (responseNum == 0 || --responseNum > searchPlayers.Count)
            {
                await ReplyAsync("Invalid number");
                embed.WithFooter(x =>
                {
                    x.Text = "CANCELLED";
                });
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return multiplePlayersStruct;
            }
            if (searchPlayers[responseNum].privacy_flag == "y")
            {
                embed = await EmbedHandler.HiddenProfileEmbed(searchPlayers[responseNum].Name);
                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return multiplePlayersStruct;
            }
            embed = await EmbedHandler.LoadingStats(Text.GetPortalEmoji(searchPlayers[responseNum].portal_id.ToString()) + searchPlayers[responseNum].Name);
            await message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
            });
            multiplePlayersStruct.searchPlayers = searchPlayers[responseNum];
            multiplePlayersStruct.userMessage = message;
            try
            {
                await response.Value.DeleteAsync();
            }
            catch {}
            return multiplePlayersStruct;
        }
        public struct MultiplePlayersStruct
        {
            public SearchPlayers searchPlayers;
            public IUserMessage userMessage;
        }
        public struct PlayerHandlerStruct
        {
            public int playerID;
            public string playerName;
            public IUserMessage userMessage;
        }
    }
}
