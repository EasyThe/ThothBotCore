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
        private static Random rnd = new Random();
        public static async void AddNewGodEmojiInGuild(Gods.God god)
        {
            var thothGods3guild = Connection.Client.GetGuild(591932765880975370);
            string[] firstsplit = god.godIcon_URL.Split('/');
            string[] secondsplit = firstsplit[^1].Split('.');
            var image = new Image($"Storage/Gods/{firstsplit[^1]}");
            var createdEmote = await thothGods3guild.CreateEmoteAsync(secondsplit[0], image);
            image.Dispose();
            await Reporter.SendError($"**ADDED NEW EMOTE **<:{createdEmote.Name}:{createdEmote.Id}>");
            god.Emoji = $"<:{createdEmote.Name}:{createdEmote.Id}>";
            await MongoConnection.SaveGodAsync(god);
        }
        public static async Task AddMissingItemEmojiAsync(GetItems.Item item)
        {
            var emoteGuilds = new List<SocketGuild>
            {
                Connection.Client.GetGuild(592787276795347056),
                Connection.Client.GetGuild(595336005180063797),
                Connection.Client.GetGuild(597444275944292372),
                Connection.Client.GetGuild(772225334652829706),
                Connection.Client.GetGuild(772225406195466241),
                Connection.Client.GetGuild(772225707560140831)
            };
            string emojiname = item.DeviceName.Trim().Replace("\'", "").ToLowerInvariant();
            emojiname = Regex.Replace(emojiname, @"\s+", "");

            string[] splitLink = item.itemIcon_URL.Split('/');

            if (!Directory.Exists("Storage/Items"))
            {
                Directory.CreateDirectory("Storage/Items");
            }

            // Downloading the image
            try
            {
                using WebClient client = new WebClient();
                client.DownloadFile(new Uri(item.itemIcon_URL), $@"./Storage/Items/{splitLink[5]}");
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
            }

            // Adding the image as emoji in emojiguilds
            foreach (var guild in emoteGuilds)
            {
                if (guild.Emotes.Count != 50)
                {
                    Thread.Sleep(200);
                    var image = new Image($"Storage/Items/{splitLink[5]}");
                    Text.WriteLine(emojiname);
                    var insertedEmote = await guild.CreateEmoteAsync(emojiname, image);
                    image.Dispose();
                    // Saving to DB
                    item.Emoji = $"<:{insertedEmote.Name}:{insertedEmote.Id}>";
                    await MongoConnection.SaveItemAsync(item);
                    break;
                }
                else
                {
                    emoteGuilds.Remove(guild);
                    Text.WriteLine($"{guild.Name} is full.");
                }
            }
        }
        public static async Task<string> RandomBuilderAsync(Gods.God god)
        {
            StringBuilder sb = new StringBuilder();
            string godType;
            // Random Build START

            if (god.Roles.Contains("Mage") || god.Roles.Contains("Guardian"))
            {
                godType = "magical";
            }
            else
            {
                godType = "physical";
            }

            // Random Relics
            var active = await MongoConnection.GetActiveActivesAsync();
            for (int a = 0; a < 2; a++)
            {
                int ar = rnd.Next(active.Count);
                sb.Append(active[ar].Emoji);
                active.RemoveAt(ar);
            }

            // Boots or Shoes depending on the god type
            if (!god.Name.Contains("Ratatoskr"))
            {
                var boots = await MongoConnection.GetBootsOrShoesAsync(godType);
                int boot = rnd.Next(boots.Count);
                sb.Append(boots[boot].Emoji);
            }
            else
            {
                var boots = await MongoConnection.GetBootsOrShoesAsync("ratatoskr");
                sb.Append(boots[0].Emoji);
            }

            var items = await MongoConnection.GetActiveItemsByGodTypeAsync(godType, god.Roles.ToLowerInvariant().Trim());

            // Finishing the build with 5 items
            for (int i = 0; i < 5; i++)
            {
                int r = rnd.Next(items.Count);
                sb.Append(items[r].Emoji);
                items.RemoveAt(r);
            }

            // Random Build END
            return sb.ToString();
        }
        public static async Task<string> CheckSpecialsForPlayer(int id, bool emoteOnly)
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
                        if (emoteOnly)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                        }
                    }
                    if (playerSpecial.streamer_bool)
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (!emoteOnly)
                            {
                                specialsResult.Append("\n");
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync("streamer");
                        if (emoteOnly)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                            if (playerSpecial.streamer_link != null)
                            {
                                specialsResult.Append($" {playerSpecial.streamer_link}");
                            }
                        }
                    }
                    // Check specials
                    if (playerSpecial.special != null)
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (!emoteOnly)
                            {
                                specialsResult.Append("\n");
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync(playerSpecial.special);
                        if (badge != null)
                        {
                            if (emoteOnly)
                            {
                                specialsResult.Append(badge.Emote);
                            }
                            else
                            {
                                specialsResult.Append($"{badge.Emote} {badge.Title}");
                            }
                        }
                    }
                    return specialsResult.ToString();
                }
            }
            return "";
        }
    }
}
