using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ThothBotCore.Connections;
using ThothBotCore.Models;
using ThothBotCore.Storage;

namespace ThothBotCore.Utilities
{
    public class Utils
    {
        static readonly DominantColor domColor = new DominantColor();

        public static async void AddNewGodEmojiInGuild(string link)
        {
            var thothGods3guild = Discord.Connection.Client.GetGuild(591932765880975370);
            string[] firstsplit = link.Split('/');
            string[] secondsplit = firstsplit[firstsplit.Length - 1].Split('.');
            var image = new Image($"Storage\\Gods\\{firstsplit[firstsplit.Length - 1]}");
            var createdEmote = await thothGods3guild.CreateEmoteAsync(secondsplit[0], image);
            await ErrorTracker.SendError($"**ADDED NEW EMOTE **<:{createdEmote.Name}:{createdEmote.Id}>");
            await Database.InsertEmojiForGod(secondsplit[0], $"<:{createdEmote.Name}:{createdEmote.Id}>");
        }

        public static async void UpdateDb(HiRezAPI hiRezAPI)
        {
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(await hiRezAPI.GetGods());
            await Database.SaveGods(gods);
            string newjson = JsonConvert.SerializeObject(gods, Formatting.Indented);
            domColor.DoAllGodColors();
            await ErrorTracker.SendError($"{gods.Count} Gods were found and saved to the DB.");
            try
            {
                List<GetItems.Item> itemsList = JsonConvert.DeserializeObject<List<GetItems.Item>>(await hiRezAPI.GetItems());
                await Database.InsertItems(itemsList);
                domColor.DoAllItemColors();

                await ErrorTracker.SendError($"{itemsList.Count} Items were found and saved to the DB.");
            }
            catch (Exception ex)
            {
                await ErrorTracker.SendException(ex, null);
            }
        }
    }
}
