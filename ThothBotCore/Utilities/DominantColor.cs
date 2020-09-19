﻿using System;
using System.Net;
using ColorThiefDotNet;
using System.Globalization;
using ThothBotCore.Storage;
using System.Drawing;
using System.IO;
using ThothBotCore.Discord;
using System.Threading.Tasks;
using Discord;
using System.Text;

namespace ThothBotCore.Utilities
{
    public class DominantColor
    {
        private Bitmap img;

        public int GetDomColor(string link)
        {
            string[] splitLink = link.Split('/');

            if (!Directory.Exists("Storage/Gods"))
            {
                Directory.CreateDirectory("Storage/Gods");
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(link), $@"./Storage/Gods/{splitLink[5]}");
                }
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
            }
            string image = $@"./Storage/Gods/{splitLink[5]}";
            var colorThief = new ColorThief();
            img = new Bitmap(image);

            string hexString = colorThief.GetColor(img).Color.ToHexString();
            string[] splitHex = hexString.Split('#');
            int intHex = int.Parse(splitHex[1], NumberStyles.HexNumber);

            return intHex;
        }

        public void DoAllGodColors()
        {
            var godsList = Database.LoadGodsDomColor();
            int c = godsList.Capacity;

            for (int i = 0; i < c; i++)
            {
                if (godsList[i].DomColor == 0)
                {
                    Text.WriteLine($"{i}. {godsList[i].godIcon_URL}");
                    int getdcolor = GetDomColor(godsList[i].godIcon_URL);
                    Database.SaveGodDomColor(godsList[i].id, getdcolor);
                }
            }
        }

        public async Task DoAllItemColors()
        {
            var items = Database.GetAllItems().Result;
            var sb = new StringBuilder();

            for (int c = 0; c < items.Count; c++)
            {
                try
                {
                    if (items[c].DomColor == 0)
                    {
                        if (items[c].itemIcon_URL != "" || items[c].itemIcon_URL != null)
                        {
                            Text.WriteLine($"{c} {items[c].DeviceName}");
                            int getdcolor = GetDomColor(items[c].itemIcon_URL);
                            Database.SaveItemDomColor(items[c].ItemId, getdcolor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"[{items[c].ItemId}]**{items[c].DeviceName}**'s icon doesn't exist. | {ex.Message}");
                }
            }
            if (sb.Length != 0)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync(sb.ToString(), 254);
                await Reporter.SendEmbedToBotLogsChannel(embed.ToEmbedBuilder());
            }
        }
    }
}
