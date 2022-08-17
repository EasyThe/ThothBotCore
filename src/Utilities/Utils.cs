using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
    public class Utils
    {
        private static readonly Random rnd = new();
        private static StringBuilder builder;
        public static async Task<string> AddNewGodEmojiInGuild(Gods.God god)
        {
            Image image;
            var thothGods3guild = Connection.Client.GetGuild(591932765880975370);
            SaveImageToFolder(god.godIcon_URL, true);
            string[] firstsplit = god.godIcon_URL.Split('/');
            string[] secondsplit = firstsplit[^1].Split('.');
            try
            {
                image = new Image($"Storage/Gods/{firstsplit[^1]}");
            }
            catch (Exception ex)
            {
                await Reporter.SendErrorAsync($"Missing {firstsplit[^1]} when adding new Emoji for god.\n{ex.Message}");
                return "<:blank:570291209906552848>";
            }
            var createdEmote = await thothGods3guild.CreateEmoteAsync(secondsplit[0], image);
            image.Dispose();
            await Reporter.SendErrorAsync($"**ADDED NEW GOD EMOTE **<:{createdEmote.Name}:{createdEmote.Id}>");
            return $"<:{createdEmote.Name}:{createdEmote.Id}>";
        }
        private static void SaveImageToFolder(string url, bool isGod)
        {
            string path;
            if (isGod)
            {
                if (!Directory.Exists("Storage/Gods"))
                {
                    Directory.CreateDirectory("Storage/Gods");
                }
                path = "Storage/Gods";
            }
            else
            {
                if (!Directory.Exists("Storage/Items"))
                {
                    Directory.CreateDirectory("Storage/Items");
                }
                path = "Storage/Items";
            }
            string[] splitLink = url.Split('/');
            // Downloading the image
            try
            {
                using WebClient client = new();
                client.DownloadFile(new Uri(url), $"{path}/{splitLink[^1]}");
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
            }
        }
        public static async Task<string> AddMissingItemEmojiAsync(GetItems.Item item)
        {
            var emoteGuilds = new List<SocketGuild>
            {
                Connection.Client.GetGuild(592787276795347056),
                Connection.Client.GetGuild(595336005180063797),
                Connection.Client.GetGuild(597444275944292372),
                Connection.Client.GetGuild(772225334652829706),
                Connection.Client.GetGuild(772225406195466241),
                Connection.Client.GetGuild(772225707560140831),
                Connection.Client.GetGuild(803964049217028096),
                Connection.Client.GetGuild(803964253576495136)
            };
            string emojiname = item.DeviceName.Trim().Replace("\'", "").ToLowerInvariant();
            emojiname = Regex.Replace(emojiname, @"\s+", "");

            string[] splitLink = item.itemIcon_URL.Split('/');

            SaveImageToFolder(item.itemIcon_URL, false);

            // Adding the image as emoji in emojiguilds
            foreach (var guild in emoteGuilds)
            {
                foreach (var emote in guild.Emotes)
                {
                    if (emote.Name == emojiname)
                    {
                        return $"<:{emote.Name}:{emote.Id}>";
                    }
                }
                if (guild.Emotes.Count != 50)
                {
                    Thread.Sleep(200);
                    var image = new Image($"Storage/Items/{splitLink[5]}");
                    Text.WriteLine(emojiname);
                    var insertedEmote = await guild.CreateEmoteAsync(emojiname, image);
                    image.Dispose();
                    return $"<:{insertedEmote.Name}:{insertedEmote.Id}>";
                }
                else
                {
                    Text.WriteLine($"{guild.Name} is full.");
                    continue;
                }
            }
            return "";
        }
        public static async Task<string[]> AddMissingAbilityEmojiAsync(Gods.God god)
        {
            string[] emojis = new string[5];
            try
            {
                var emoteGuilds = new List<SocketGuild>
                {
                    Connection.Client.GetGuild(958115981685817414),
                    Connection.Client.GetGuild(958117246985711696),
                    Connection.Client.GetGuild(958117335166775348),
                    Connection.Client.GetGuild(958117463126593567),
                    Connection.Client.GetGuild(958117484324618271),
                    Connection.Client.GetGuild(958117517652557894),
                    Connection.Client.GetGuild(958117543007121468),
                    Connection.Client.GetGuild(958117570576257125),
                    Connection.Client.GetGuild(958117631569838173),
                    Connection.Client.GetGuild(958117658329505802),
                    Connection.Client.GetGuild(958117687731556352),
                    Connection.Client.GetGuild(958117713228730420),
                    Connection.Client.GetGuild(958117755456991262),
                };

                string[] urls = new string[5]
                {
                    god.Ability_1.URL,
                    god.Ability_2.URL,
                    god.Ability_3.URL,
                    god.Ability_4.URL,
                    god.Ability_5.URL
                };

                for (int i = 0; i < urls.Length; i++)
                {
                    string emojiname = urls[i].Split("/")[^1].Split(".")[0].Replace("-", "_");

                    string[] splitLink = urls[i].Split('/');

                    SaveImageToFolder(urls[i], false);

                    // Adding the image as emoji in emojiguilds
                    foreach (var guild in emoteGuilds)
                    {
                        if (guild.Emotes.Count != 50)
                        {
                            Thread.Sleep(500);
                            using var image = new Image($"Storage/Items/{splitLink[5]}");
                            Text.WriteLine(emojiname);
                            var insertedEmote = await guild.CreateEmoteAsync(emojiname, image);
                            emojis[i] = $"<:{insertedEmote.Name}:{insertedEmote.Id}>";
                            break;
                        }
                        else
                        {
                            //Text.WriteLine($"{guild.Name} is full.");
                            continue;
                        }
                    }
                }
                return emojis;
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
                return emojis;
            }
        }
        public static async Task<string> RandomBuilderAsync(Gods.God god)
        {
            StringBuilder sb = new();
            string godType = "physical";
            int itemsCount = 5; // This was 4 when boots existed #RIPBoots

            if (god.Roles.Contains("Mage") || god.Roles.Contains("Guardian"))
            {
                godType = "magical";
            }

            // Random Relics
            var active = await MongoConnection.GetActiveActivesAsync();
            for (int a = 0; a < 2; a++)
            {
                int ar = rnd.Next(active.Count);
                sb.Append(active[ar].Emoji);
                active.RemoveAll(x => x.ChildItemId == active[ar].ChildItemId);
            }

            // Random Starter Item
            var allitems = MongoConnection.GetAllActiveItems();
            var starters = allitems.FindAll(x => x.StartingItem && x.GodType != null && x.GodType.Contains(godType) && x.ItemTier == 2);

            sb.Append(starters[rnd.Next(starters.Count)].Emoji);

            // Boots or Shoes depending on the god type | hehe rip boots
            if (god.Name == "Ratatoskr")
            {
                sb.Append(await GetRandomBoots("Ratatoskr", godType));
                itemsCount = 4;
            }

            var items = await MongoConnection.GetActiveItemsByGodTypeAsync(godType, god.Roles.ToLowerInvariant().Trim());

            // Finishing the build
            for (int i = 0; i < itemsCount; i++)
            {
                int r = rnd.Next(items.Count);
                sb.Append(items[r].Emoji);
                items.RemoveAt(r);
            }

            // Random Build END
            return sb.ToString();
        }
        private static async Task<string> GetRandomBoots(string godName, string godType) //This is here in case HiRez adds boots back lol
        {
            if (godName != "Ratatoskr")
            {
                var boots = await MongoConnection.GetBootsOrShoesAsync(godType);
                int boot = rnd.Next(boots.Count);
                return boots[boot].Emoji;
            }
            var bootsr = await MongoConnection.GetBootsOrShoesAsync("ratatoskr");
            return bootsr[rnd.Next(bootsr.Count)].Emoji;
        }
        public static async Task<string> GetItemsBuiltAsync(MatchHistoryModel match)
        {
            var items = MongoConnection.GetAllItems();
            var build = new StringBuilder();
            build.Append(items.Find(x => x.ItemId == match.ItemId1)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId2)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId3)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId4)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId5)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId6)?.Emoji);
            return await Task.FromResult(build.ToString());
        }
        public static async Task<string> GetItemsBuiltAsync(MatchDetails.MatchDetailsPlayer match)
        {
            var items = MongoConnection.GetAllItems();
            var build = new StringBuilder();
            build.Append(items.Find(x => x.ItemId == match.ItemId1)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId2)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId3)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId4)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId5)?.Emoji);
            build.Append(items.Find(x => x.ItemId == match.ItemId6)?.Emoji);
            return await Task.FromResult(build.ToString());
        }
        /// <summary>
        /// Checks for badges for the requested player id.
        /// </summary>
        /// <param name="id">Player ID</param>
        /// <param name="type">0 - Emote Only, 1 - All, 2 - Only Text</param>
        /// <returns></returns>
        public static async Task<string> CheckSpecialsForPlayer(int id, int type)
        {
            PlayerSpecial playerSpecial = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(id);
            if (playerSpecial != null)
            {
                if (playerSpecial.special != "" || playerSpecial.pro_bool || playerSpecial.streamer_bool || playerSpecial.special != null)
                {
                    var specialsResult = new StringBuilder();
                    if (playerSpecial.pro_bool)
                    {
                        var badge = await MongoConnection.GetBadgeAsync("pro");
                        if (type == 0)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else if (type == 1)
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                        }
                        else
                        {
                            specialsResult.Append(badge.Title);
                        }
                    }
                    if (playerSpecial.streamer_bool)
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (type == 1)
                            {
                                specialsResult.Append('\n');
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync("streamer");
                        if (type == 0)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else if (type == 1)
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                            if (playerSpecial.streamer_link != null)
                            {
                                specialsResult.Append($" {playerSpecial.streamer_link}");
                            }
                        }
                        else
                        {
                            specialsResult.Append(badge.Title);
                        }
                    }
                    // Check specials
                    if (playerSpecial.special != null)
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (type == 1)
                            {
                                specialsResult.Append('\n');
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync(playerSpecial.special);
                        if (badge != null)
                        {
                            if (type == 0)
                            {
                                specialsResult.Append(badge.Emote);
                            }
                            else if (type == 1)
                            {
                                specialsResult.Append($"{badge.Emote} {badge.Title}");
                            }
                            else
                            {
                                specialsResult.Append(badge.Title);
                            }
                        }
                    }
                    return specialsResult.ToString();
                }
            }
            return "";
        }
        public static async Task<string> MaintenancePlatformsAsync(List<Component2> platforms)
        {
            var sb = new StringBuilder();
            for (int i = platforms.Count - 1; i >= 0; i--)
            {
                if (platforms[i].name.ToLowerInvariant().Contains("smite switch"))
                {
                    sb.Append("<:SW:537752006719176714> ");
                }
                if (platforms[i].name.ToLowerInvariant().Contains("smite xbox"))
                {
                    sb.Append("<:XB:537749895029850112> ");
                }
                if (platforms[i].name.ToLowerInvariant().Contains("smite ps4"))
                {
                    sb.Append("<:PS4:537745670518472714> ");
                }
                if (platforms[i].name.ToLowerInvariant().Contains("smite pc"))
                {
                    sb.Append("<:PC:537746891610259467> ");
                }
            }
            return await Task.FromResult(sb.ToString());
        }
        public static async Task<string> ExpectedDowntimeAsync(TimeSpan timeSpan)
        {
            StringBuilder sb = new();
            if (timeSpan.Hours != 0)
            {
                if (timeSpan.Hours == 1)
                {
                    sb.Append(timeSpan.Hours + " hour");
                }
                else
                {
                    sb.Append(timeSpan.Hours + " hours");
                }
            }
            if (timeSpan.Minutes != 0)
            {
                sb.Append(" and ");
                if (timeSpan.Minutes == 1)
                {
                    sb.Append(timeSpan.Minutes + " minute");
                }
                else
                {
                    sb.Append(timeSpan.Minutes + " minutes");
                }
            }
            if (sb.Length == 0)
            {
                sb.Append("n/a");
            }
            return await Task.FromResult(sb.ToString());
        }

        public static bool IsRanked(string queueID) => queueID switch
        {
            "440" or "450" or "451" or "502" or "503" or "504" => true,
            _ => false,
        };

        public static string LiveRankedString(MatchDetails.MatchDetailsPlayer player, string teamEmoji)
        {
            builder?.Clear();
            builder.Append($"{Text.GetRankedConquest(player.Conquest_Tier).Item2}{Text.GetRankedConquest(player.Conquest_Tier).Item1}\n");
            builder.Append($"{teamEmoji}W/L: {player.Conquest_Wins}/{player.Conquest_Losses}\n");
            builder.Append($"{teamEmoji}MMR: {Math.Round(player.Rank_Stat_Conquest, 0)}\n");
            builder.Append($"{teamEmoji}TP:");
            return builder.ToString();
        }
        public static string FindGodEmoji(List<Gods.God> gods, int godId)
        {
            var find = gods.Find(x => x.id == godId);
            if (find != null) return find.Emoji;
            return "<:blank:570291209906552848>";
        }
    }
}
