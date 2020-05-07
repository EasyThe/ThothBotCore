using Discord;
using System.Text;
using System.Threading.Tasks;

namespace ThothBotCore.Utilities.Smite
{
    public static class PatchPageReader
    {
        public static async Task<Embed> GetPatchEmbed(string url)
        {
            var embed = new EmbedBuilder();

            var sb = new StringBuilder();

            // New God
            string newgod = "nope";
            string[] newgodSplit;
            if (newgod.Length != 0)
            {
                newgodSplit = newgod.Split("\n");
                sb.AppendLine($"🆕 {newgodSplit[0]}: **{newgodSplit[1].Replace("\r\n", "")}** | {newgodSplit[2].Replace("\r\n", "")}");
            }
            // New skins
            var newskins = "new-god-skins";
            if (newskins != null)
            {
                var allnewSkins = "god-skin--card";
                string skinName = "";
                sb.Append("\n🎭 **New Skins:** ");
                string allskins;
                foreach (var skin in allnewSkins)
                {
                    //skinName = skin.FindElement(by: By.ClassName("name")).Text;
                    allskins = string.Join(',', allnewSkins);
                    sb.AppendJoin(',', skinName);
                }
            }

            embed.WithDescription(sb.ToString());
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithTitle("title");
            //imashe url tuk
            var featuredImage = "featured-image";
            System.Console.WriteLine(featuredImage);

            return await Task.FromResult(embed.Build());
        }
    }
}
