using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities.Smite
{
    public static class PatchPageReader
    {
        public static Task<string> ReadPatch(WebAPIPostModel patchPost)
        {
            List<Gods.God> gods = MongoConnection.GetAllGods().OrderBy(x => x.Name).ToList();
            var doc = new HtmlDocument();
            doc.LoadHtml(patchPost.content);
            var sb = new StringBuilder();
            sb.AppendLine(NewGod(doc, gods)); // New God
            sb.AppendLine(NewSkins(doc, gods)); // New Skins
            sb.AppendLine(GodsChanged(doc)); // God Changes
            sb.AppendLine(UpdateSchedule(doc, patchPost)); // Update Schedule
            
            return Task.FromResult(sb.ToString());
        }
        private static string NewGod(HtmlDocument doc, List<Gods.God> gods)
        {
            var newGodElement = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'new-god')]");
            if (newGodElement != null)
            {
                var h51 = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'new-god')]//h5");
                var godTitle = doc.DocumentNode.SelectSingleNode("/div[1]/div[1]/h5[1]");
                var godNameElement = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'new-god')]//h3");
                if (!gods.Any(x=> x.Name == godNameElement.InnerText))
                {
                    gods.Add(new Gods.God { Name = godNameElement.InnerText });
                }
                return $"<:Gods:567146088985919498> **{h51.InnerText}**: {godNameElement.InnerText}, {godTitle.InnerText}";
            }
            else
            {
                return "";
            }
        }
        private static string NewSkins(HtmlDocument doc, List<Gods.God> gods)
        {
            var sb = new StringBuilder();
            var newskins = doc.DocumentNode.SelectNodes("//div[contains(@class, 'god-skin--card')]");
            if (newskins != null)
            {
                sb.Append("🎭 **New Skins for** ");
                foreach (var gd in gods)
                {
                    if (newskins.Any(x => x.ChildNodes.FindFirst("p").InnerText.Contains(gd.Name)))
                    {
                        if (gd.Name == "Ra")
                        {
                            var ihatera = newskins.Where(x => x.ChildNodes.FindFirst("p").InnerText.Contains("Ra"));
                            var split = ihatera?.FirstOrDefault()?.ChildNodes.FindFirst("p").InnerText.Split(" ");
                            if (split.Last() != "Ra")
                            {
                                gd.Name += "++";
                                continue;
                            }
                        }
                        sb.Append($"{gd.Name}, ");
                        gd.Name += "++";
                    }
                }
                sb.Remove(sb.ToString().LastIndexOf(','), 1);
            }
            return sb.ToString();
        }
        private static string UpdateSchedule(HtmlDocument doc, WebAPIPostModel patchPost)
        {
            var sb = new StringBuilder();
            var allWrappedHeaders = doc.DocumentNode.SelectNodes("//div[contains(@class, 'wrapped-header')]");
            var schedule = allWrappedHeaders.Where(x => x.InnerText.Contains("Schedule")).FirstOrDefault();
            if (schedule != null)
            {
                sb.AppendLine($"🗓 **Update Release Schedule: **");
                var dates = schedule.SelectNodes("following::h6[position()<5]");
                for (int d = 0; d < dates.Count; d++)
                {
                    if (!dates[d].InnerText.Contains(patchPost.timestamp.ToString("MMMM", CultureInfo.InvariantCulture)) &&
                        !dates[d].InnerText.Contains(patchPost.timestamp.AddMonths(1).ToString("MMMM", CultureInfo.InvariantCulture)) &&
                        !dates[d].InnerText.Contains(patchPost.timestamp.AddMonths(2).ToString("MMMM", CultureInfo.InvariantCulture)))
                    {
                        continue;
                    }
                    // date
                    var patchNumberULEle = dates[d].SelectNodes("following::ul[position()<2]");
                    sb.Append($":white_small_square:{dates[d].InnerText} - ");
                    var numberLIEle = patchNumberULEle.FirstOrDefault()?.ChildNodes.Where(x => x.Name == "li");
                    var numberLIElems = patchNumberULEle.FirstOrDefault()?.ChildNodes.Where(x => x.Name == "li").ToList();

                    // patch info
                    for (int i = 0; i < numberLIElems.Count; i++)
                    {
                        // The element links to somewhere, get the link and format it in Discord markdown
                        if (numberLIElems[i].FirstChild.Name == "a")
                        {
                            sb.Append($"[{numberLIElems[i].FirstChild.InnerText}]" +
                                $"({(numberLIElems[i].FirstChild.Attributes.FirstOrDefault()?.Name == "href" ? numberLIElems[i].FirstChild.Attributes.First().Value : "#")})");
                        }
                        else
                        {
                            sb.Append(numberLIElems[i].FirstChild.InnerText.Replace("\n", ""));
                        }

                        if (i != numberLIElems.Count - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                    
                    if (sb.ToString().Contains("&#8211;"))
                    {
                        sb.Replace("&#8211;", "-");
                    }

                    if (d != dates.Count - 1)
                    {
                        sb.Append('\n');
                    }
                }
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
        private static string GodsChanged(HtmlDocument doc)
        {
            var sb = new StringBuilder();
            var allChangedGodDivs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'god-changes--god')]").ToList();
            if (allChangedGodDivs.Count != 0)
            {
                sb.Append("↔ **God Changes:** ");
                for (int i = 0; i < allChangedGodDivs.Count; i++)
                {
                    if (allChangedGodDivs[i].InnerText.Length > 1)
                    {
                        sb.Append(allChangedGodDivs[i].InnerText.Replace("\n", ""));
                        if (i != allChangedGodDivs.Count - 1)
                        {
                            sb.Append(", ");
                        }
                    }
                }
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
