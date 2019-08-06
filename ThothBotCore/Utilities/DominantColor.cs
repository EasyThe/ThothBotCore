using System;
using System.Collections.Generic;
using System.Net;
using ColorThiefDotNet;
using System.Globalization;
using ThothBotCore.Storage.Models;
using ThothBotCore.Storage;
using System.Drawing;
using System.IO;

namespace ThothBotCore.Utilities
{
    public class DominantColor
    {
        private Bitmap img = null;

        public int GetDomColor(string link)
        {
            string[] splitLink = link.Split('/');

            if (!Directory.Exists("Storage/Gods"))
            {
                Directory.CreateDirectory("Storage/Gods");
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(link), $@"./Storage/Gods/{splitLink[5]}");
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
            List<Gods.God> godsList = Database.LoadGodsDomColor();
            int c = godsList.Capacity;

            for (int i = 0; i < c; i++)
            {
                if (godsList[i].DomColor == 0)
                {
                    Console.WriteLine($"{i}. {godsList[i].godIcon_URL}");
                    int getdcolor = GetDomColor(godsList[i].godIcon_URL);
                    Database.SaveGodDomColor(godsList[i].id, getdcolor);
                }
            }
        }

        public void DoAllItemColors()
        {
            var items = Database.GetAllItems().Result;

            for (int c = 0; c < items.Count; c++)
            {
                if (items[c].DomColor == 0)
                {
                    if (items[c].itemIcon_URL != "" || items[c].itemIcon_URL != null)
                    {
                        Console.WriteLine($"{c} {items[c].DeviceName}");
                        int getdcolor = GetDomColor(items[c].itemIcon_URL);
                        Database.SaveItemDomColor(items[c].ItemId, getdcolor);
                    }
                }
            }
        }
    }
}
