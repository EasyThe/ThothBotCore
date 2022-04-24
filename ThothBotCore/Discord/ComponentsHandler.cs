using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Discord
{
    public class ComponentsHandler
    {
        public static async Task<MessageComponent> MultiplePlayersSelectMenuAsync(List<SearchPlayers> players, string type)
        {
            var options = new List<SelectMenuOptionBuilder>();
            if (players.Count > 25)
            {
                // PC (HiRez, Steam, EpicGames)
                var pcPid = players.FindIndex(x => x.portal_id == 1 || x.portal_id == 5 || x.portal_id == 28);
                if (pcPid != -1)
                {
                    string desc = players[pcPid].privacy_flag != "n" ? "HIDDEN" : "";
                    desc += await Utils.CheckSpecialsForPlayer(players[pcPid].player_id, 2);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse(Text.GetPortalEmoji(players[pcPid].portal_id.ToString())),
                        Description = desc.Length != 0 ? desc : "\u200b",
                        Label = players[pcPid].Name,
                        Value = players[pcPid].player_id.ToString(),
                    });
                    players.RemoveAt(pcPid);
                }
                // PS4
                var psPid = players.FindIndex(x => x.portal_id == 9);
                if (psPid != -1)
                {
                    string desc = players[psPid].privacy_flag != "n" ? "HIDDEN" : "";
                    desc += await Utils.CheckSpecialsForPlayer(players[psPid].player_id, 2);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse(Text.GetPortalEmoji(players[psPid].portal_id.ToString())),
                        Description = desc.Length != 0 ? desc : "\u200b",
                        Label = players[psPid].Name,
                        Value = players[psPid].player_id.ToString(),
                    });
                    players.RemoveAt(psPid);
                }
                // Xbox
                var xbPid = players.FindIndex(x => x.portal_id == 9);
                if (xbPid != -1)
                {
                    string desc = players[xbPid].privacy_flag != "n" ? "HIDDEN" : "";
                    desc += await Utils.CheckSpecialsForPlayer(players[xbPid].player_id, 2);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse(Text.GetPortalEmoji(players[xbPid].portal_id.ToString())),
                        Description = desc.Length != 0 ? desc : "\u200b",
                        Label = players[xbPid].Name,
                        Value = players[xbPid].player_id.ToString(),
                    });
                    players.RemoveAt(xbPid);
                }
            }
            foreach (var player in players)
            {
                if (options.Count == 25)
                {
                    break;
                }
                string desc = player.privacy_flag != "n" ? "HIDDEN" : "";
                desc += await Utils.CheckSpecialsForPlayer(player.player_id, 2);
                options.Add(new SelectMenuOptionBuilder()
                {
                    Emote = Emote.Parse(Text.GetPortalEmoji(player.portal_id.ToString())),
                    Description = desc.Length != 0 ? desc : "\u200b",
                    Label = player.Name,
                    Value = player.player_id.ToString()
                });
            }
            var builder = new ComponentBuilder()
                    .WithSelectMenu($"playerselect-{type}", options, "Pick a player");
            return builder.Build();
        }
        public static async Task<MessageComponent> AboutThothButtonsAsync(bool isOwner, int position)
        {
            var builder = new ComponentBuilder()
                    .WithButton("Statistics",
                                "statistics",
                                style: ButtonStyle.Secondary,
                                emote: Emoji.Parse("📊"),
                                disabled: position == 0)
                    .WithButton("Send Feedback",
                                "feedback",
                                ButtonStyle.Secondary,
                                Emote.Parse("<:isforme:847274350549401621>"),
                                disabled: position == 1)
                    .WithButton("Changelog",
                                "changelog",
                                ButtonStyle.Secondary,
                                Emoji.Parse("⚙️"),
                                disabled: position == 2);
            if (isOwner && position != 69)
            {
                builder.WithButton(customId: "ownermenu",
                                   style: ButtonStyle.Secondary,
                                   emote: Emote.Parse("<:V_boss:724338439696416831>"));
            }
            if (position == 69)
            {
                builder.WithButton("Hi-Rez API", "hirezapi", ButtonStyle.Secondary, new Emoji("💀"))
                       .WithButton("Shards", "shards", ButtonStyle.Secondary, new Emoji("⛓️"))
                       .WithButton("Player Lookup", "lookup", ButtonStyle.Secondary, new Emoji("🔧"))
                       .WithButton("Send DM", "senddmbyowner", ButtonStyle.Secondary, new Emoji("📨"))
                       .WithButton("Leave", "leave", ButtonStyle.Secondary, new Emoji("🚪"))
                       .WithButton("Reload Settings", "reloadconst", ButtonStyle.Secondary, new Emoji("🔄"))
                       .WithButton("Update Database", "updatedb", ButtonStyle.Secondary, new Emoji("📥"))
                       .WithButton("Badges", "badges", ButtonStyle.Secondary, new Emoji("🏷️"))
                       .WithButton("Tips", "tips", ButtonStyle.Secondary, new Emoji("ℹ️"))
                       .WithButton("Add Changelog", "add-changelog", ButtonStyle.Secondary, new Emoji("⚙"))
                       .WithButton((Global.ErrorMessageByOwner.Length == 0 ? "Add Global Error Message" : "Edit Global Error Message"), "edit-globalerror", ButtonStyle.Secondary, new Emoji("🌍"))
                       .WithButton("Set Activity", "set-activity", ButtonStyle.Secondary, new Emoji("💭"));
            }

            return await Task.FromResult(builder.Build());
        }
        public static async Task<MessageComponent> FeedsSelectMenuAsync(GuildSettingsModel guildSettings, IInteractionContext context)
        {
            var builder = new ComponentBuilder();
            var options = new List<SelectMenuOptionBuilder>();
            var channels = await context.Guild.GetTextChannelsAsync();
            IWebhook webhook = null;

            // if a channel is set, add that channel to the list
            if (guildSettings != null && guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus)?.WebhookID != 0)
            {
                var settings = guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus);
                webhook = await context.Guild.GetWebhookAsync(settings.WebhookID);
                if (webhook != null)
                {
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emoji.Parse("❌"),
                        Label = "No Channel",
                        Description = "Disable SMITE Status Feeds",
                        Value = "0"
                    });
                    var channel = await context.Guild.GetTextChannelAsync(webhook.ChannelId);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        IsDefault = true,
                        Label = channel.Name,
                        Value = webhook.ChannelId.ToString(),
                        Description = "This channel is set to receive SMITE Status Feeds",
                        Emote = Emote.Parse("<:channel_green:957042306521894984>"),
                    });
                }
                else
                {
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse("<:channel_green:957042306521894984>"),
                        Label = context.Channel.Name,
                        Description = "This channel",
                        Value = context.Channel.Id.ToString()
                    });
                }
            }
            if (!options.Exists(x => x.Value == context.Channel.Id.ToString()))
            {
                options.Add(new SelectMenuOptionBuilder()
                {
                    Emote = Emote.Parse("<:channel:957042306723246100>"),
                    Label = context.Channel.Name,
                    Description = "This channel",
                    Value = context.Channel.Id.ToString()
                });
            }
            foreach (var channel in channels)
            {
                if (options.Count == 25)
                {
                    break;
                }
                // <:channel_green:957042306521894984>
                if (!options.Exists(x => x.Value == channel.Id.ToString()))
                {
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse("<:channel:957042306723246100>"),
                        Label = channel.Name,
                        Value = channel.Id.ToString()
                    });
                }
            }
            builder.WithSelectMenu($"feeds-serverstatus", options, "Set channel for SMITE Status Feeds");
            return await Task.FromResult(builder.Build());
        }
        public static async Task<MessageComponent> RelatedItemsSelectMenuAsync(List<GetItems.Item> itemList, GetItems.Item currentItem)
        {
            var builder = new ComponentBuilder();
            var options = new List<SelectMenuOptionBuilder>();
            Dictionary<int, GetItems.Item> relatedItems = new();
            var all = itemList.FindAll(x => x.ChildItemId == currentItem.ItemId);
            if (all.Count != 0)
            {
                foreach (var item in all)
                {
                    relatedItems.TryAdd(item.ItemId, item);
                }
            }
            if (currentItem.ChildItemId != 0)
            {
                var childItem = itemList.Find(x => x.ItemId == currentItem.ChildItemId);
                relatedItems.TryAdd(childItem.ItemId, childItem);
            }
            if (currentItem.ItemTier == 2)
            {
                all = itemList.FindAll(x => x.RootItemId == currentItem.ItemId);
                if (all.Count != 0)
                {
                    foreach (var item in all)
                    {
                        relatedItems.TryAdd(item.ItemId, item);
                    }
                }
            }
            if (currentItem.Type == "Consumable")
            {
                all = itemList.FindAll(x => x.Type == "Consumable");
                foreach (var item in all)
                {
                    relatedItems.TryAdd(item.ItemId, item);
                }
            }
            if (currentItem.ItemTier == 1)
            {
                all = itemList.FindAll(x => x.RootItemId == currentItem.ItemId);
                if (all.Count != 0)
                {
                    foreach (var item in all)
                    {
                        var newall = itemList.FindAll(x => x.RootItemId == item.ItemId);
                        if (newall.Count != 0)
                        {
                            foreach (var tier3 in newall)
                            {
                                relatedItems.TryAdd(tier3.ItemId, tier3);
                            }
                        }
                    }
                }
            }
            if (currentItem.RootItemId != 0)
            {
                var rootItem = itemList.Find(x => x.ItemId == currentItem.RootItemId);
                relatedItems.TryAdd(rootItem.ItemId, rootItem);
                if (currentItem.ItemTier == 3)
                {
                    all = itemList.FindAll(x => x.RootItemId == currentItem.RootItemId);
                    if (all.Count != 0)
                    {
                        foreach (var item in all)
                        {
                            relatedItems.TryAdd(item.ItemId, item);
                        }
                    }
                }
            }
            if (relatedItems.Count != 0)
            {
                var sorted = relatedItems.OrderByDescending(x => x.Value.ItemTier).ToDictionary(z => z.Key, y => y.Value);
                foreach (var relitem in sorted)
                {
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Emote = Emote.Parse(relitem.Value?.Emoji),
                        Label = relitem.Value.DeviceName,
                        Value = relitem.Value.ItemId.ToString(),
                        Description = $"{(relitem.Value.Type != "Active" ? relitem.Value.Type : "Relic")} Tier: {relitem.Value.ItemTier}, Price: {relitem.Value.Price}"
                    });
                }
            }

            builder.WithSelectMenu("related-items-select", options, "Related Items");

            return await Task.FromResult(builder.Build());
        }
        public static async Task<MessageComponent> GodsInfoButtonsAsync(int godId)
        {
            var builder = new ComponentBuilder()
                        .WithButton("Abilities",
                                    $"abilities-{godId}",
                                    ButtonStyle.Primary,
                                    new Emoji("🪄"))
                        .WithButton("Skins",
                                    $"skins-{godId}",
                                    ButtonStyle.Primary,
                                    new Emoji("🎨"));

            return await Task.FromResult(builder.Build());
        }
        public static async Task<MessageComponent> GodsAbilitiesButtonsAsync(Gods.God god, int position = 0)
        {
            Gods.Ability ability = null;
            var builder = new ComponentBuilder();
            builder.WithButton($"P. {god.Ability_5.Summary}",
                                $"abi-{god.id}-5",
                                ButtonStyle.Secondary,
                                god.Ability_5.Emoji != null ? Emote.Parse(god.Ability_5.Emoji) : null,
                                disabled: position == 5);
            builder.WithButton($"1. {god.Ability_1.Summary}",
                                $"abi-{god.id}-1",
                                ButtonStyle.Secondary,
                                god.Ability_1.Emoji != null ? Emote.Parse(god.Ability_1.Emoji) : null,
                                disabled: position == 1);
            builder.WithButton($"2. {god.Ability_2.Summary}",
                                $"abi-{god.id}-2",
                                ButtonStyle.Secondary,
                                god.Ability_2.Emoji != null ? Emote.Parse(god.Ability_2.Emoji) : null,
                                disabled: position == 2);
            builder.WithButton($"3. {god.Ability_3.Summary}",
                                $"abi-{god.id}-3",
                                ButtonStyle.Secondary,
                                god.Ability_3.Emoji != null ? Emote.Parse(god.Ability_3.Emoji) : null,
                                disabled: position == 3);
            builder.WithButton($"4. {god.Ability_4.Summary}",
                                $"abi-{god.id}-4",
                                ButtonStyle.Secondary,
                                god.Ability_4.Emoji != null ? Emote.Parse(god.Ability_4.Emoji) : null,
                                disabled: position == 4);
            switch (position)
            {
                case 1: ability = god.Ability_1;
                    break;
                case 2: ability = god.Ability_2;
                    break;
                case 3: ability = god.Ability_3;
                    break;
                case 4: ability = god.Ability_4;
                    break;
                case 5: ability = god.Ability_5;
                    break;
                default:
                    break;
            }
            if (ability != null && ability.Video != null)
            {
                builder.WithButton($"{ability.Summary} [Video]",
                                   $"abiyt-{ability.Video}",
                                   ButtonStyle.Secondary,
                                   Emote.Parse("<:YT:962689918763692043>"),
                                   row: 2);
            }
            builder.WithButton("Back",
                               $"godinfo-main-{god.id}",
                               ButtonStyle.Secondary,
                               Emote.Parse("<:back:959968077544583298>"),
                               row: 3);
            return await Task.FromResult(builder.Build());
        }
        public static async Task<MessageComponent> GodsSkinsSelectMenuAsync(Gods.God god)
        {
            var builder = new ComponentBuilder();
            var options = new List<SelectMenuOptionBuilder>();

            var ordered = god.Skins.OrderByDescending(x => x.obtainability);
            foreach (var item in ordered)
            {
                options.Add(new SelectMenuOptionBuilder
                {
                    Label = $"{item.skin_name} [{item.obtainability}]",
                    Description = $"{(item.godSkin_URL != null && item.godSkin_URL.Length != 0 ? "" : "Missing Card Art - ")}" +
                    $"{(item.price_favor != 0 ? $"{item.price_favor} favor" : "")} {(item.price_gems != 0 ? $"{item.price_gems} gems" : "")}",
                    Value = $"{item.god_id}-{item.skin_id1}"
                });
            }

            builder.WithSelectMenu($"skin", options, "Select a skin to see the card art");
            builder.WithButton("Back",
                   $"godinfo-main-{god.id}",
                   ButtonStyle.Secondary,
                   Emote.Parse("<:back:959968077544583298>"),
                   row: 3);
            return await Task.FromResult(builder.Build());
        }
    }
}
