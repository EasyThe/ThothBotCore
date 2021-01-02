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
        public static async Task<string> ReadPatch(WebAPIPostModel patchPost)
        {
            List<Gods.God> gods = MongoConnection.GetAllGods().OrderBy(x => x.Name).ToList();
            var doc = new HtmlDocument();
            doc.LoadHtml(patchPost.content);
            var sb = new StringBuilder();
            sb.AppendLine(NewGod(doc, gods)); // New God
            sb.AppendLine(NewSkins(doc, gods)); // New Skins
            sb.AppendLine(UpdateSchedule(doc, patchPost));
            return sb.ToString();
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
            sb.Append(":performing_arts: **New Skins for** ");
            var newskins = doc.DocumentNode.SelectNodes("//div[contains(@class, 'god-skin--card')]");
            if (newskins != null)
            {
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
                sb.AppendLine($":calendar_spiral: **Update Release Schedule: **");
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
                    sb.Append($":white_small_square:**{dates[d].InnerText}** - __");
                    var numberLIEle = patchNumberULEle.FirstOrDefault()?.ChildNodes.Where(x => x.Name == "li");

                    // patch
                    string patchTitle = numberLIEle.FirstOrDefault()?.FirstChild.InnerText;
                    sb.Append(patchTitle.Replace("\n", ""));
                    sb.Append("__: ");

                    // description
                    var descLIEle = numberLIEle.FirstOrDefault()?.ChildNodes.Where(x => x.Name == "ul");
                    string desc = descLIEle.FirstOrDefault()?.InnerText;
                    if (desc != null)
                    {
                        if (desc.StartsWith("\n"))
                        {
                            desc = Text.ReplaceFirst(desc, "\n", "");
                        }
                        if (desc.EndsWith("\n"))
                        {
                            desc = desc.Remove(desc.LastIndexOf("\n"));
                        }
                    }
                    else
                    {
                        desc = numberLIEle.FirstOrDefault()?.InnerText;
                    }
                    if (desc.Contains("&#8211;"))
                    {
                        desc = desc.Replace("&#8211;", "-");
                    }
                    if (sb.ToString().Contains("&#8211;"))
                    {
                        sb.Replace("&#8211;", "-");
                    }
                    if (desc.Contains(patchTitle))
                    {
                        sb.Append('\n');
                        sb.Replace("__", "");
                        continue;
                    }
                    sb.Append(desc.Replace("\n", ", "));
                    sb.Append('\n');
                }
                var tentativeText = schedule.SelectNodes("following::p[position()<2]");
                if (tentativeText?.FirstOrDefault()?.InnerText.Length! < 0)
                {
                    sb.AppendLine($"*{tentativeText.FirstOrDefault().InnerText}*");
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
