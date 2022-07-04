using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
            var schedule = UpdateSchedule(doc, patchPost);
            sb.AppendLine(NewGod(doc, gods)); // New God
            sb.AppendLine(NewSkins(doc, gods)); // New Skins
            sb.AppendLine(GodsChanged(doc, gods)); // God Changes
            if (schedule.Length > 15)
            {
                sb.AppendLine(schedule); // Update Schedule
            }
            
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
                    if (newskins.Any(x => x.ChildNodes.Count != 0 && x.ChildNodes.FindFirst("p").InnerText.Contains(gd.Name)))
                    {
                        if (gd.Name == "Ra")
                        {
                            var ihatera = newskins.Where(x => x.ChildNodes.FindFirst("p").InnerText.Contains("Ra"));
                            var split = ihatera?.FirstOrDefault()?.ChildNodes.FindFirst("p").InnerText.Split(" ");
                            if (split.Last() != "Ra")
                            {
                                continue;
                            }
                        }
                        sb.Append($"{gd.Name}, ");
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
            if (allWrappedHeaders != null)
            {
                var schedule = allWrappedHeaders.Where(x => x.InnerText.Contains("Schedule")).FirstOrDefault();
                if (schedule != null)
                {
                    sb.AppendLine($"🗓 **Update Release Schedule: **");
                    var dates = schedule.SelectNodes("following::h6[position()<5]");
                    if (dates != null && dates.Any(x => CultureInfo.InvariantCulture.DateTimeFormat.MonthGenitiveNames.Contains(x.InnerText)))
                    {
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

                            if (d != dates.Count - 1)
                            {
                                sb.Append('\n');
                            }
                        }
                    }
                    else
                    {
                        // unknown format 
                        string elementName = "";
                        var checkingSchedule = schedule;
                        while (elementName.Length == 0)
                        {
                            if (CultureInfo.InvariantCulture.DateTimeFormat.MonthGenitiveNames.Contains(checkingSchedule.NextSibling.InnerText.Split(' ').FirstOrDefault()))
                            {
                                elementName = checkingSchedule.NextSibling.Name;
                            }
                            else
                            {
                                checkingSchedule = schedule.NextSibling;
                            }
                        }
                        dates = schedule.SelectNodes($"following::{elementName}[position()<5]");
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

                            if (d != dates.Count - 1)
                            {
                                sb.Append('\n');
                            }
                        }
                    }
                    if (sb.ToString().Contains(';'))
                    {
                        return HttpUtility.HtmlDecode(sb.ToString());
                    }
                    return sb.ToString();
                }
                else
                {
                    return "";
                }
            }
            // no wrapped headers, this should never happen, ever!
            return "";
        }
        private static string GodsChanged(HtmlDocument doc, List<Gods.God> gods)
        {
            var sb = new StringBuilder();
            var allChangedGodDivs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'god--name')]").ToList();
            if (allChangedGodDivs.Count != 0)
            {
                sb.Append("↔ **God Changes:** ");
                foreach (var gd in gods)
                {
                    if (allChangedGodDivs.Any(x => x.InnerText.Contains(gd.Name)))
                    {
                        if (gd.Name == "Ra")
                        {
                            var ihatera = allChangedGodDivs.Where(x => x.InnerText.Contains("Ra")).FirstOrDefault();
                            if (ihatera.InnerText != "Ra")
                            {
                                continue;
                            }
                        }
                        // We add that changed god to the string
                        sb.Append($"{gd.Name}, ");
                    }
                }
                sb.Length--;
                sb.Length--;
                return sb.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
