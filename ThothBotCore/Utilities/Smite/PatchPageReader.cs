using Discord;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThothBotCore.Utilities.Smite
{
    public static class PatchPageReader
    {
        public static async Task<Embed> GetPatchEmbed(string url)
        {
            var embed = new EmbedBuilder();

            // finding the chrome executable path (bin)
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var driver = new ChromeDriver(path);

            driver.Navigate().GoToUrl(url);

            // refreshing is important for the website js to handle the updated localStorage
            driver.Navigate().Refresh();
            // sleeping/throttling waiting for everything to settle
            Thread.Sleep(6000);

            var sb = new StringBuilder();

            // New God
            string newgod = driver.FindElementByClassName("new-god").Text;
            string[] newgodSplit;
            if (newgod.Length != 0)
            {
                newgodSplit = newgod.Split("\n");
                sb.AppendLine($"🆕 {newgodSplit[0]}: **{newgodSplit[1].Replace("\r\n", "")}** | {newgodSplit[2].Replace("\r\n", "")}");
            }
            // New skins
            var newskins = driver.FindElementById("new-god-skins");
            if (newskins.Text != null)
            {
                var allnewSkins = driver.FindElementsByClassName("god-skin--card");
                string skinName = "";
                sb.Append("\n🎭 **New Skins:** ");
                string allskins;
                foreach (var skin in allnewSkins)
                {
                    skinName = skin.FindElement(by: By.ClassName("name")).Text;
                    allskins = string.Join(',', allnewSkins);
                    sb.AppendJoin(',', skinName);
                }
            }

            embed.WithDescription(sb.ToString());
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle(driver.FindElementByClassName("title").Text);
            embed.WithUrl(driver.Url);
            var featuredImage = driver.FindElementByClassName("featured-image");
            System.Console.WriteLine(featuredImage.Text);

            // Closing
            driver.Close();

            return await Task.FromResult(embed.Build());
        }
    }
}
