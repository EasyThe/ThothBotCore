﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using ThothBotCore.Utilities.Smite;
using static ThothBotCore.Connections.Models.Player;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    [RequireBotPermission(ChannelPermission.UseExternalEmojis)]
    [RequireBotPermission(ChannelPermission.EmbedLinks)]
    [RequireBotPermission(ChannelPermission.ViewChannel)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [Name("SMITE")]
    public class Smite : InteractiveBase<SocketCommandContext>
    {
        static Random rnd = new Random();

        HiRezAPI hirezAPI = new HiRezAPI();
        HiRezAPIv2 APIv2 = new HiRezAPIv2();
        TrelloAPI trelloAPI = new TrelloAPI();
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        [Command("stats", true, RunMode = RunMode.Async)]
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
                var playerStatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(statusJson);
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
                    var getPlayer = JsonConvert.DeserializeObject<List<PlayerStats>>(getPlayerJson);
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

        [Command("gods", true)]
        [Summary("Overall information about the gods in the game and current free god rotation.")]
        [Alias("годс")]
        public async Task GodsCommand()
        {
            List<Gods.God> gods = MongoConnection.GetAllGods();

            if (gods.Count != 0)
            {
                StringBuilder onRotation = new StringBuilder();
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
                    x.Text = $"For God specific information use !!god GodName";
                });

                await ReplyAsync("", false, embed.Build());

            }
            else
            {
                await ReplyAsync("Something is not right... Please report this in my support server.");
            }
        }

        [Command("god", true)] // Get specific God information
        [Summary("Provides information about `GodName`.")]
        [Alias("g")]
        public async Task GodInfo([Remainder] string GodName)
        {
            string titleCaseGod = Text.ToTitleCase(GodName);
            Gods.God gods = await MongoConnection.LoadGod(titleCaseGod);

            if (gods == null)
            {
                await ReplyAsync($"{titleCaseGod} was not found.");
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
                    x.Text = "More info soon™..";
                });

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("rgod", true)] // Random God
        [Summary("Gives you a random God and randomised build.")]
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
                x.Text = Text.GetRandomTip();
            });
            await ReplyAsync($"{Context.Message.Author.Mention}, your random god is:", false, embed.Build());
        }

        [Command("rteam", true)]
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
                    x.Text = Text.GetRandomTip();
                });

                await ReplyAsync($"Team of {number} for you, {Context.Message.Author.Mention}!", false, embed.Build());
            }
            else
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("That's not a proper number for a team, don't ya' think?", 254);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("status", true, RunMode = RunMode.Async)] // SMITE Server Status 
        [Summary("Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.")]
        [Alias("статус", "statis", "s", "с", "server", "servers", "se", "се", "serverstatus")]
        public async Task ServerStatusCheck()
        {
            await Context.Channel.TriggerTypingAsync();
            var smiteServerStatus = JsonConvert.DeserializeObject<ServerStatus>(await StatusPage.GetStatusSummary());
            string discjson = await StatusPage.GetDiscordStatusSummary();
            var discordStatus = new ServerStatus();
            if (discjson != "")
            {
                discordStatus = JsonConvert.DeserializeObject<ServerStatus>(discjson);
            }

            var statusEmbed = await EmbedHandler.ServerStatusEmbedAsync(smiteServerStatus, discordStatus);
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

        [Command("statusupdates")]
        [Summary("When SMITE incidents and scheduled maintenances appear in the status page they will be sent to #channel")]
        [Alias("statusupd", "su")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task SetStatusUpdatesChannel([Name("#your-channel-here")] SocketChannel channelMention)
        {
            await SetNotifChannel(Context.Guild.Id, Context.Guild.Name, channelMention.Id);
            SocketTextChannel channel = Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(channelMention.Id);
            try
            {
                var perms = Context.Guild.CurrentUser.GetPermissions(channel);
                var sb = new StringBuilder();
                if (!perms.EmbedLinks)
                {
                    sb.Append("Embed Links");
                }
                if (!perms.UseExternalEmojis)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("Use External Emojis");
                }
                if (!perms.SendMessages)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("Send Messages");
                }
                if (!perms.ViewChannel)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("View Channel");
                }

                if (sb.Length != 0)
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"I am missing **{sb}** permissions in {channel.Mention}", 255);
                    await ReplyAsync(embed: emb);
                    return;
                }
                await channel.SendMessageAsync($":white_check_mark: {channel.Mention} is now set to receive notifications about SMITE Server Status updates.");
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("missing permissions"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Send Messages** permission for {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.ToLowerInvariant().Contains("missing access"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Access** to {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.ToLowerInvariant().Contains("multiple matches"))
                {
                    await Context.Channel.SendMessageAsync("Multiple matches found. Please #mention the channel.");
                }
                else
                {
                    await ReplyAsync(":warning: Something went wrong. This error was reported to the bot creator and will be checked ASAP.");
                    await Reporter.SendError($"**Error in StatusUpdates command**\n" +
                        $"{ex.Message}\n**Message: **{channelMention}\n" +
                        $"**Server: **{Context.Guild.Name}[{Context.Guild.Id}]\n" +
                        $"**Channel: **{Context.Channel.Name}[{Context.Channel.Id}]");
                }
            }
        }

        [Command("stopstatusupdates", true)]
        [Summary("Stops sending messages from the SMITE status page.")]
        [Alias("ssu")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task StopStatusUpdates()
        {
            await StopNotifs(Context.Guild.Id);

            await ReplyAsync($":ok_hand: **{Context.Guild.Name}** will no longer receive SMITE Server Status updates.");
        }

        [Command("claninfo")]
        [Alias("clan", "c")]
        [RequireOwner] // vremenno
        public async Task ClanInfoCommand(int id)
        {
            string json = await hirezAPI.GetTeamDetails(id);
            List<ClanInfo> clanList = JsonConvert.DeserializeObject<List<ClanInfo>>(json);
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName(clanList[0].Name);
                author.WithIconUrl(Constants.botIcon);
            });
            embed.WithColor(0, 255, 0);
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Tag");
                field.WithValue(clanList[0].Tag == "" ? "n/a" : clanList[0].Tag);
            });
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Founder");
                field.WithValue(clanList[0].Founder == "" ? "n/a" : clanList[0].Founder);
            });
            embed.AddField(field =>
            {
                field.WithIsInline(true);
                field.WithName("Players");
                field.WithValue(clanList[0].Players.ToString());
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("item")]
        [Summary("Provides information about `ItemName`.")]
        [Alias("i")]
        public async Task ItemInfoCommand([Remainder] string ItemName)
        {
            var item = await MongoConnection.GetSpecificItemAsync(ItemName);
            if (item.Count != 0)
            {
                string secondaryDesc = item[0].ItemDescription.SecondaryDescription;
                var embed = new EmbedBuilder();

                if (secondaryDesc != null || secondaryDesc == "")
                {
                    if (secondaryDesc.Contains("PASSIVE"))
                    {
                        secondaryDesc = secondaryDesc.Replace("PASSIVE", "**PASSIVE**");
                    }
                    if (secondaryDesc.Contains("<font color='#42F46E'>"))
                    {
                        secondaryDesc = secondaryDesc.Replace("<font color='#42F46E'>", "");
                    }
                    if (secondaryDesc.Contains("<font color='#F44242'>"))
                    {
                        secondaryDesc = secondaryDesc.Replace("<font color='#F44242'>", "");
                    }
                    else if (secondaryDesc.Contains("AURA"))
                    {
                        secondaryDesc = secondaryDesc.Replace("AURA", "**AURA**");
                    }
                    else if (secondaryDesc.Contains("ROLE QUEST"))
                    {
                        secondaryDesc = secondaryDesc.Replace("ROLE QUEST", "**ROLE QUEST**");
                    }
                }
                embed.WithAuthor(x =>
                {
                    x.Name = item[0].DeviceName;
                    x.IconUrl = item[0].itemIcon_URL;
                });
                embed.WithColor(new Color((uint)item[0].DomColor));
                embed.WithThumbnailUrl(item[0].itemIcon_URL);
                StringBuilder itemBenefits = new StringBuilder();
                foreach (var benefit in item[0].ItemDescription.Menuitems)
                {
                    itemBenefits.AppendLine($"{benefit.Value} {benefit.Description}");
                }
                embed.WithTitle(itemBenefits.ToString());
                embed.WithDescription($"{(item[0].StartingItem ? "**Starting Item**" : "")}" +
                    $"{(item[0].ItemDescription.Description.Length != 0 ? $"\n{item[0].ItemDescription.Description}" : "")}\n\n{secondaryDesc}");

                // we doin calculations bois
                var itemsForPrice = new List<GetItems.Item>();
                var itemsForPriceTwo = new List<GetItems.Item>();
                int itemPrice = 0;

                if (item[0].ChildItemId != 0)
                {
                    itemsForPrice = await MongoConnection.GetSpecificItemByIDAsync(item[0].ChildItemId);
                    if (itemsForPrice[0].ChildItemId != 0)
                    {
                        itemsForPriceTwo = await MongoConnection.GetSpecificItemByIDAsync(itemsForPrice[0].ChildItemId);
                        itemPrice = itemsForPriceTwo[0].Price;
                    }
                    itemPrice = itemPrice + item[0].Price + itemsForPrice[0].Price;
                }
                else
                {
                    itemPrice = item[0].Price;
                }
                // yeah sure

                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Total Price (Price)";
                    x.Value = $"<:coins:590942235474919464>{itemPrice} ({item[0].Price})";
                });
                if (!item[0].StartingItem || item[0].Type != "Consumable")
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Tier";
                        x.Value = $"{item[0].ItemTier}";
                    });
                }
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await ReplyAsync($"Sorry, I couldn't find `{ItemName}`.");
            }
        }

        [Command("trello", true, RunMode = RunMode.Async)]
        [Summary("Checks the [SMITE Community Issues Trello Board](https://trello.com/b/d4fJtBlo/smite-community-issues).")]
        [Alias("issues", "bugs", "board")]
        public async Task TrelloBoardCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                var result = await trelloAPI.GetTrelloCards();

                StringBuilder topIssues = new StringBuilder();
                StringBuilder hotfixNotes = new StringBuilder();
                StringBuilder incominghotfix = new StringBuilder();
                StringBuilder alreadyFixedInLIVE = new StringBuilder();

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

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the Trello API is down. Try visiting the [website](https://trello.com/b/d4fJtBlo/smite-community-issues) instead.");
                await ReplyAsync(embed: embed);
                await Reporter.SendError($"**Trello Error: **\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        [Command("livematch", RunMode = RunMode.Async)]
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

                var playerstatus = JsonConvert.DeserializeObject<List<PlayerStatus>>(await hirezAPI.GetPlayerStatus(playerID));
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

        [Command("matchdetails", RunMode = RunMode.Async)]
        [Summary("Match details for the provided `MatchID` or latest match played of provided `PlayerName`.")]
        [Alias("md", "мд")]
        public async Task MatchDetailsCommand([Name("MatchID or PlayerName")][Remainder] string MatchID = "")
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();
                PlayerHandlerStruct playerHandler = new PlayerHandlerStruct();
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

        [Command("matchhistory", true, RunMode = RunMode.Async)]
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
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = finalembed;
                    });
                }
                else
                {
                    await ReplyAsync(embed: finalembed);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("motd", true, RunMode = RunMode.Async)]
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

                var embed = new EmbedBuilder();

                embed.WithColor(0, 80, 188);
                embed.WithAuthor(x =>
                {
                    x.Name = "Upcoming Matches Of The Day";
                    x.IconUrl = Constants.botIcon;
                });
                Motd motdDay = new Motd();
                for (int i = 0; i < 5; i++)
                {
                    string[] finalDesc = Array.Empty<string>();
                    motdDay = motdList.Find(x => x.startDateTime.Date == DateTime.Today.AddDays(i));
                    if (motdDay == null)
                    {
                        await ReplyAsync("", false, embed.Build());
                        return;
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
                        x.Name = $":large_blue_diamond: **{motdDay.title}** - {motdDay.startDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}";
                        x.Value = $"{embedValue}";
                    });
                }

                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("wp", RunMode = RunMode.Async)]
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
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = finalEmbed;
                    });
                }
                else
                {
                    await Context.Channel.SendMessageAsync(embed: finalEmbed);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("winrates", RunMode = RunMode.Async)]
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
                if (sentMessage != null)
                {
                    await sentMessage.ModifyAsync(x =>
                    {
                        x.Embed = finalEmbed;
                    });
                }
                else
                {
                    await Context.Channel.SendMessageAsync(embed: finalEmbed);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
            }
        }

        [Command("link", true)]
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
                $"\n\n__To start the linking process write **{Credentials.botConfig.prefix}startlink** and follow the instructions.__");
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithFooter(x => x.Text = "This is not an official Hi-Rez linking!");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("startlink", true, RunMode = RunMode.Async)]
        [Alias("startlinking")]
        public async Task LinkAccountsCommand()
        {
            try
            {
                // Check if the api is available
                string check = await hirezAPI.GetPatchInfo();
                if (check.ToLowerInvariant().Contains("the api is unavailable"))
                {
                    var unavailableEmbed = await EmbedHandler.BuildDescriptionEmbedAsync(
                        "Sorry, we cannot link your accounts, because the Hi-Rez API is unavailable right now. Please try again later.", 163);
                    await ReplyAsync(embed: unavailableEmbed);
                    return;
                }

                int playerID = 0;
                var onMultiplePlayersResult = new MultiplePlayersStruct();

                var embed = new EmbedBuilder();
                embed.WithDescription("Please enter your ingame name and **make sure your account is not hidden and you are logged in SMITE**" +
                    "\n*you have 120 seconds to respond*");
                embed.WithAuthor(x =>
                {
                    x.Name = "Thoth Account Linking";
                    x.IconUrl = Constants.botIcon;
                });
                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithFooter(x => x.Text = "This is not official Hi-Rez linking!");

                var message = await ReplyAsync("", false, embed.Build());
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));

                if (response == null)
                {
                    embed.Description = ":red_circle: **Time is up, linking cancelled**";
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }
                else if (response.Content.StartsWith(Credentials.botConfig.prefix) || 
                    response.Content.StartsWith(GetServerConfig(Context.Guild.Id).Result[0].prefix))
                {
                    embed.WithDescription(Text.UserNotFound(response.Content) + "\n🔴 Linking cancelled.");
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }

                // Finding all occurences of provided username and adding them in a list
                var searchPlayer = await hirezAPI.SearchPlayer(response.Content);
                var realSearchPlayers = new List<SearchPlayers>();
                if (searchPlayer.Count != 0)
                {
                    foreach (var player in searchPlayer)
                    {
                        if (player.Name.ToLowerInvariant() == response.Content.ToLowerInvariant())
                        {
                            realSearchPlayers.Add(player);
                        }
                    }
                }
                // Checking the new list for count of users in it
                if (realSearchPlayers.Count == 0)
                {
                    embed.WithDescription(Text.UserNotFound(response.Content) + "\n🔴 Linking cancelled.");
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }
                else if (!(realSearchPlayers.Count > 1))
                {
                    if (realSearchPlayers[0].privacy_flag == "y")
                    {
                        embed.WithDescription($"{realSearchPlayers[0].Name} is hidden. " +
                            $"Please unhide your profile by unchecking the \"Hide my Profile\" under the Profile tab and try again.");
                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                        return;
                    }
                    playerID = realSearchPlayers[0].player_id;
                }
                else
                {
                    //On Multiple players
                    onMultiplePlayersResult = await MultiplePlayersHandler(realSearchPlayers, Context, message);
                    if (onMultiplePlayersResult.searchPlayers != null && onMultiplePlayersResult.searchPlayers.player_id == 0)
                    {
                        embed.WithDescription("🔴 Linking cancelled.");
                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                        return;
                    }
                    else if (onMultiplePlayersResult.searchPlayers == null && onMultiplePlayersResult.userMessage == null)
                    {
                        return;
                    }
                    playerID = onMultiplePlayersResult.searchPlayers.player_id;
                }

                string getplayerJSON = await hirezAPI.GetPlayer(playerID.ToString());
                var getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);

                string generatedString = GenerateString();
                embed.Author.IconUrl = Context.Message.Author.GetAvatarUrl();
                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                embed.WithDescription($"<:level:529719212017451008>**Level**: {getplayerList[0].Level}\n" +
                    $"📅**Account Created**: {getplayerList[0].Created_Datetime}\n\n" +
                    $"**If this is your account, change your __Personal Status Message__ to: `{generatedString}` so we can be sure " +
                    $"it's your account. You can change it to your previous status message after linking is completed. " +
                    $"**\n*You have 120 seconds to perform this action.*\n" +
                    $"\nWhen you are done, write `done` or anything else to cancel.");
                embed.ImageUrl = "https://media.discordapp.net/attachments/528621646626684928/656237343405244416/Untitled-1.png";

                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                embed.ImageUrl = null;
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));
                if (response == null || response.Content.ToLowerInvariant() == "done")
                {
                    getplayerJSON = await hirezAPI.GetPlayer(playerID.ToString());
                    getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);
                    if (getplayerList[0].Personal_Status_Message.ToLowerInvariant() == generatedString)
                    {
                        PlayerSpecial playerSpecial = new PlayerSpecial { _id = playerID, discordID = Context.Message.Author.Id };
                        await MongoConnection.SavePlayerSpecialsAsync(playerSpecial);
                        embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                        embed.WithDescription($":tada: Congratulations, you've successfully linked your Discord and SMITE account into the Thoth Database.");
                        embed.ImageUrl = null;

                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                    }
                    else
                    {
                        embed.WithDescription($":red_circle: Linking cancelled. {(response == null ? "Time is up! " : "")}Your Personal Status Message is `{getplayerList[0].Personal_Status_Message}`");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
                else
                {
                    embed.WithDescription($":red_circle: Linking cancelled.");
                    embed.ImageUrl = null;
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.RespondToCommandOnErrorAsync(ex, Context);
                await ReplyAsync(embed: embed);
                await Reporter.SendError($"**LINKING ERROR**\n{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}\n{ex.Source}\n{ex.Data}");
            }
        }
        
        [Command("unlink", true, RunMode = RunMode.Async)]
        [Summary("Unlink your SMITE and Discord accounts in Thoth's database")]
        public async Task UnlinkAccountsCommand()
        {
            var db = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Message.Author.Id);
            if (db == null)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"You don't have a linked SMITE account in the database.");
                await ReplyAsync(embed: embed);
                return;
            }
            await MongoConnection.UnlinkPlayerAsync(Context.Message.Author.Id);
            var em = await EmbedHandler.BuildDescriptionEmbedAsync($"{Context.Message.Author.Username} just unlinked an account.", 0, 0, 254);
            await Reporter.SendEmbedToBotLogsChannel(em.ToEmbedBuilder());
            em = await EmbedHandler.BuildDescriptionEmbedAsync("You have successfully unlinked your account!");
            await ReplyAsync(embed: em);
        }

        [Command("patch", true, RunMode = RunMode.Async)]
        [Summary("Sends the last patch posted on the SMITEgame.com website")]
        [Alias("lastpatch", "notes", "patchnotes", "updatenotes")]
        public async Task PatchNotesCommand()
        {
            var posts = await HiRezWebAPI.FetchPostsAsync();
            var foundPost = posts.Find(x => x.real_categories.ToLowerInvariant().Contains("notes"));
            var actualPost = await HiRezWebAPI.GetPostBySlugAsync(foundPost.slug);
            string description = await PatchPageReader.ReadPatch(actualPost);
            Embed embed = await EmbedHandler.BuildPatchNotesEmbedAsync(actualPost, description, foundPost.large_image, foundPost.slug);
            await ReplyAsync(embed: embed);
        }

        // test
        [Command("test")] // Get specific God information
        [RequireOwner]
        public async Task TestAbilities()
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());

            if (gods.Count == 0)
            {
                //titlecasegod was not found imashe tuk nz
            }
            else
            {
                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author.WithName(gods[64].Name);
                    author.WithIconUrl(gods[64].godIcon_URL);
                });
                embed.WithTitle(gods[64].Ability1);
                embed.WithDescription(gods[64].abilityDescription1.itemDescription.description);
                for (int z = 0; z < gods[64].abilityDescription1.itemDescription.menuitems.Count; z++)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[64].abilityDescription1.itemDescription.menuitems[z].description);
                        field.Value = (gods[64].abilityDescription1.itemDescription.menuitems[z].value);
                    });
                }
                for (int a = 0; a < gods[64].abilityDescription1.itemDescription.rankitems.Count; a++)
                {
                    embed.AddField(field =>
                    {
                        field.IsInline = true;
                        field.Name = (gods[64].abilityDescription1.itemDescription.rankitems[a].description);
                        field.Value = (gods[64].abilityDescription1.itemDescription.rankitems[a].value);
                    });
                }
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cooldown");
                    field.Value = (gods[64].abilityDescription1.itemDescription.cooldown);
                });
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = ($"Cost");
                    field.Value = (gods[64].abilityDescription1.itemDescription.cost);
                });

                embed.WithThumbnailUrl(gods[64].godAbility1_URL);
                if (gods[64].DomColor != 0)
                {
                    embed.WithColor(new Color((uint)gods[64].DomColor));
                }

                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("p", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task PaginatedGodsbro(int god, int ability = 1)
        {
            //var gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());
            var json = await File.ReadAllTextAsync("getgods.json");
            var gods = JsonConvert.DeserializeObject<List<Gods.God>>(json);
            var embed = new EmbedBuilder();
            //var json = JsonConvert.SerializeObject(gods, Formatting.Indented);
            //await File.WriteAllTextAsync("getgods.json", json);
            
            try
            {
                if (gods.Count == 0)
                {
                    //titlecasegod was not found imashe tuk nz
                    return;
                }
                else
                {
                    embed.WithAuthor(author =>
                    {
                        author.WithName(gods[god].Name);
                        author.WithIconUrl(gods[god].godIcon_URL);
                    });
                    embed.WithTitle(gods[god].Ability5);
                    embed.WithDescription(gods[god].abilityDescription5.itemDescription.description);
                    for (int z = 0; z < gods[god].abilityDescription5.itemDescription.menuitems.Count; z++)
                    {
                        if (gods[god].abilityDescription5.itemDescription.menuitems[z].value.Length != 0)
                        {
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = gods[god].abilityDescription5.itemDescription.menuitems[z].description;
                                field.Value = gods[god].abilityDescription5.itemDescription.menuitems[z].value;
                            });
                        }
                    }
                    for (int a = 0; a < gods[god].abilityDescription5.itemDescription.rankitems.Count; a++)
                    {
                        if (gods[god].abilityDescription5.itemDescription.rankitems[a].value.Length != 0)
                        {
                            embed.AddField(field =>
                            {
                                field.IsInline = true;
                                field.Name = gods[god].abilityDescription5.itemDescription.rankitems[a].description;
                                field.Value = gods[god].abilityDescription5.itemDescription.rankitems[a].value;
                            });
                        }
                    }
                    if (gods[god].abilityDescription5.itemDescription.cooldown.Length != 0)
                    {
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "Cooldown";
                            field.Value = gods[god].abilityDescription5.itemDescription.cooldown;
                        });
                    }
                    if (gods[god].abilityDescription5.itemDescription.cost.Length != 0)
                    {
                        embed.AddField(field =>
                        {
                            field.IsInline = true;
                            field.Name = "Cost";
                            field.Value = gods[god].abilityDescription5.itemDescription.cost;
                        });
                    }
                    embed.WithThumbnailUrl(gods[god].godAbility5_URL);
                    if (gods[god].DomColor != 0)
                    {
                        embed.WithColor(new Color((uint)gods[god].DomColor));
                    }
                }
                var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                Text.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        [Command("tt", true, RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TestGetPlayer([Remainder]string name)
        {
            try
            {
                var nz = await MongoConnection.LoadGod(name);
                await ReplyAsync(nz.Title);
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
                parsedJson = JsonConvert.DeserializeObject(json);

                await ReplyAsync($"```json\n{JsonConvert.SerializeObject(parsedJson, Formatting.Indented)}```");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    using var client = new HttpClient();
                    var response = await client.PostAsync(
                        "https://hastebin.com/documents",
                         new StringContent(JsonConvert.SerializeObject(parsedJson, Formatting.Indented), Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.StatusCode.ToString() == "ServiceUnavailable")
                    {
                        await File.WriteAllTextAsync($"{endpoint}.json", json);
                        await Context.Channel.SendFileAsync($"{endpoint}.json");
                        return;
                    }
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    await ReplyAsync($"https://hastebin.com/{responseObj.key}");
                }
                else
                {
                    await ReplyAsync(ex.Message);
                }
            }
        }

        // Esports
        [Command("espsch", true)]
        public async Task SPLScheduleCommandAsync()
        {
            string json;
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "https://esports.hirezstudios.com/esportsAPI/smite/schedule/7118"))
                {
                    var response = await httpClient.SendAsync(request);
                    json = await response.Content.ReadAsStringAsync();
                }
            }
            SPLSchedule schedule = JsonConvert.DeserializeObject<SPLSchedule>(json);

            await ReplyAsync("y");
        }

        // Shit/fun commands lul
        [Command("rank", true, RunMode = RunMode.Async)]
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

        // OWNER
        [Command("lookup", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SetPlayerSpecialsCommand([Remainder] string input)
        {
            int playerId = 0;
            ulong discordId = 0;
            IUserMessage mainMessage = null;
            PlayerSpecial playerspecs = new PlayerSpecial();
            if (input.StartsWith("id:"))
            {
                playerId = int.Parse(input.Split(':').Last());
            }
            else if (input.StartsWith("did:"))
            {
                discordId = ulong.Parse(input.Split(':').Last());
            }

            if (discordId != 0)
            {
                playerspecs = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(discordId);
                if (playerspecs != null)
                {
                    playerId = playerspecs._id;
                }
                else
                {
                    await ReplyAsync("This Discord ID is not in the database.");
                    return;
                }
            }

            if (playerId == 0)
            {
                var handler = await PlayerHandler(input, Context);
                mainMessage = handler.userMessage;
                playerId = handler.playerID;
            }

            if (playerspecs._id == 0)
            {
                playerspecs = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(playerId);
            }
            string getplayerJson = await hirezAPI.GetPlayer(playerId.ToString()); ;
            List<PlayerStats> getplayer = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJson);

            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.IconUrl = getplayer[0].Avatar_URL == "" ? Constants.botIcon : getplayer[0].Avatar_URL;
                x.Name = getplayer[0].Name;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Smite Account";
                x.Value = $"🆔 ID: {getplayer[0].ActivePlayerId}\n" +
                $"<:level:529719212017451008> Level: {getplayer[0].Level}\n" +
                $"👀 Last Login: {(getplayer[0].Last_Login_Datetime != "" ? Text.PrettyDate(DateTime.Parse(getplayer[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                $"🎮 Account Created: {(getplayer[0].Created_Datetime != "" ? Text.InvariantDate(DateTime.Parse(getplayer[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                $"🔹 Platform: {getplayer[0].Platform}";
            });
            if (playerspecs != null)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Discord ID";
                    x.Value = playerspecs.discordID;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Streamer?";
                    x.Value = $"{playerspecs.streamer_bool}\n{playerspecs.streamer_link}";
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Pro?";
                    x.Value = playerspecs.pro_bool;
                });
                var badge = await MongoConnection.GetBadgeAsync(playerspecs.special);
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Special Badge";
                    x.Value = $"```\n{playerspecs.special}```{(badge != null ? $"{badge.Emote} {badge.Title}" : "**The badge is not set in the database.**")}";
                });
            }
            embed.WithFooter(x =>
            {
                x.Text = "You have 60 seconds to perform any action. (add/edit)";
            });

            mainMessage = await ReplyAsync(embed: embed.Build());

            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null)
            {
                embed.WithColor(0, 0, 0);
                embed.WithFooter(x =>
                {
                    x.Text = "Time is up!";
                });
                await mainMessage.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                return;
            }

            if (response.Content.Contains("add") || response.Content.Contains("edit"))
            {
                await mainMessage.ModifyAsync(x =>
                {
                    x.Content = "What would you like to add/edit?\n" +
                    "Possible additions: **id, discordid, pro, streamerbool, streamerlink, special**\nRespond with `cancel` to cancel";
                });

                // What to add
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                if (response == null)
                {
                    embed.WithColor(0, 0, 0);
                    embed.WithFooter(x =>
                    {
                        x.Text = "Time is up!";
                    });
                    await mainMessage.ModifyAsync(x =>
                    {
                        x.Content = "";
                        x.Embed = embed.Build();
                    });
                }
                if (playerspecs == null)
                {
                    playerspecs = new PlayerSpecial { _id = getplayer[0].ActivePlayerId };
                }
                // Choosing
                while (response.Content != "cancel")
                {
                    switch (response.Content.ToLowerInvariant())
                    {
                        case "id":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert id";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));

                            playerspecs._id = Int32.Parse(response.Content);
                            break;
                        case "discordid":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert discordid";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            playerspecs.discordID = Convert.ToUInt64(response.Content);
                            break;
                        case "pro":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert pro - true or false";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            playerspecs.pro_bool = Convert.ToBoolean(response.Content);
                            break;
                        case "streamerbool":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert streamerbool - true or false";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            playerspecs.streamer_bool = Convert.ToBoolean(response.Content);
                            break;
                        case "streamerlink":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert streamerlink";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            playerspecs.streamer_link = response.Content;
                            break;
                        case "special":
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Now insert that special badge key";
                            });
                            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                            playerspecs.special = response.Content;
                            break;
                        default:
                            await mainMessage.ModifyAsync(x =>
                            {
                                x.Content = "Invalid response, try again.";
                            });
                            break;
                    }
                    await mainMessage.ModifyAsync(x =>
                    {
                        x.Content = "Something else to add? If not, do `cancel`.\n" +
                        "Possible additions: **id, discordid, pro, streamerbool, streamerlink, special**";
                    });
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                }

                // Saving to DB
                if (playerspecs._id != 0)
                {
                    await MongoConnection.SavePlayerSpecialsAsync(playerspecs);
                    await mainMessage.ModifyAsync(x =>
                    {
                        x.Content = "Saved to the DB!";
                    });
                }
            }
            else
            {
                embed.WithColor(0, 0, 0);
                embed.WithFooter(x =>
                {
                    x.Text = "";
                });
                await mainMessage.ModifyAsync(x =>
                {
                    x.Content = "";
                    x.Embed = embed.Build();
                });
            }
        }

        private static string GenerateString()
        {
            char[] chars = new char[7];
            for (int i = 0; i < 7; i++)
            {
                chars[i] = alphabet[rnd.Next(alphabet.Length)];
            }
            return new string(chars);
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
            if (input.Contains("\\") || input.Contains("/"))
            {
                if (input.Contains("\\"))
                {
                    input = input.Replace("\\", String.Empty);
                }
                if (input.Contains("/"))
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
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || !(response.Content.All(char.IsDigit)))
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
            int responseNum = int.Parse(response.Content);
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
            embed = await EmbedHandler.LoadingStats(Text.GetPortalIcon(searchPlayers[responseNum].portal_id.ToString()) + searchPlayers[responseNum].Name);
            await message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
            });
            multiplePlayersStruct.searchPlayers = searchPlayers[responseNum];
            multiplePlayersStruct.userMessage = message;
            try
            {
                await response.DeleteAsync();
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
