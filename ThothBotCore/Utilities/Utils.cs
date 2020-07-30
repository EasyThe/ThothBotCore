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

namespace ThothBotCore.Utilities
{
    public class Utils
    {
        static readonly DominantColor domColor = new DominantColor();

        public static async void AddNewGodEmojiInGuild(string link)
        {
            var thothGods3guild = Connection.Client.GetGuild(591932765880975370);
            string[] firstsplit = link.Split('/');
            string[] secondsplit = firstsplit[^1].Split('.');
            var image = new Image($"Storage\\Gods\\{firstsplit[^1]}");
            var createdEmote = await thothGods3guild.CreateEmoteAsync(secondsplit[0], image);
            await Reporter.SendError($"**ADDED NEW EMOTE **<:{createdEmote.Name}:{createdEmote.Id}>");
            await Database.InsertEmojiForGod(secondsplit[0], $"<:{createdEmote.Name}:{createdEmote.Id}>");
        }

        public static async Task UpdateDb(HiRezAPI hiRezAPI, SocketCommandContext context)
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hiRezAPI.GetGods());
            await Database.SaveGods(gods);
            domColor.DoAllGodColors();
            var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{gods.Count} Gods were found and saved to the DB.", g: 205);
            await Reporter.SendEmbedToBotLogsChannel(emb.ToEmbedBuilder());
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
