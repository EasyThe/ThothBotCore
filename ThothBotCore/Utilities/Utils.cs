using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
    public class Utils
    {
        static readonly DominantColor domColor = new DominantColor();

        public static async void AddNewGodEmojiInGuild(Gods.God god)
        {
            var thothGods3guild = Connection.Client.GetGuild(591932765880975370);
            string[] firstsplit = god.godIcon_URL.Split('/');
            string[] secondsplit = firstsplit[^1].Split('.');
            var image = new Image($"Storage\\Gods\\{firstsplit[^1]}");
            var createdEmote = await thothGods3guild.CreateEmoteAsync(secondsplit[0], image);
            image.Dispose();
            await Reporter.SendError($"**ADDED NEW EMOTE **<:{createdEmote.Name}:{createdEmote.Id}>");
            god.Emoji = $"<:{createdEmote.Name}:{createdEmote.Id}>";
            await MongoConnection.SaveGodAsync(god);
        }

        public static async Task UpdateDb(HiRezAPI hiRezAPI, SocketCommandContext context)
        {
            // Gods
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hiRezAPI.GetGods());
            await Database.SaveGods(gods);
            domColor.DoAllGodColors();
            var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{gods.Count} Gods were found and saved to the DB.", g: 205);
            await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());
            
            // Items
            try
            {
                List<GetItems.Item> itemsList = JsonConvert.DeserializeObject<List<GetItems.Item>>(await hiRezAPI.GetItems());
                await Database.InsertItems(itemsList);
                await domColor.DoAllItemColors();

                emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{itemsList.Count} Items were found and saved to the DB.", g: 205);
                await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());
            }
            catch (Exception ex)
            {
                await Reporter.SendException(ex, context);
            }
        }

        public static async Task CommandStatsHandler()
        {
            // todo for command usage stats
        }
    }
}
