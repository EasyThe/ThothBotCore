using System;
using System.Net;
using ColorThiefDotNet;
using System.Globalization;
using System.Drawing;
using System.IO;

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
    }
}
