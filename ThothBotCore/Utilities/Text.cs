using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Models;

namespace ThothBotCore.Utilities
{
    public class Text
    {
        public static string ToTitleCase(string text)
        {
            return text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
        }

        public static string InvariantDate(DateTime dateTime)
        {
            string invariantDate = "";
            return invariantDate = dateTime.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        }

        public static string PrettyDate(DateTime dateTime)
        {
            TimeSpan timeSpan = DateTime.UtcNow.Subtract(dateTime);
            int daysDiff = (int)timeSpan.TotalDays;
            int secsDiff = (int)timeSpan.TotalSeconds;

            if (daysDiff < 0 || daysDiff >= 31)
            {
                return dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
            }

            else if (daysDiff == 0)
            {
                if (secsDiff < 60)
                {
                    return "Just now";
                }
                if (secsDiff < 120)
                {
                    return "1 minute ago";
                }
                if (secsDiff < 3600)
                {
                    return string.Format("{0} minutes ago",
                        Math.Floor((double)secsDiff / 60));
                }
                if (secsDiff < 7200)
                {
                    return "1 hour ago";
                }
                if (secsDiff < 86400)
                {
                    return string.Format("{0} hours ago [{1}]",
                        Math.Floor((double)secsDiff / 3600), dateTime.ToString("HH:mm UTC", CultureInfo.InvariantCulture));
                }
            }
            else if (daysDiff == 1)
            {
                return string.Format("Yesterday [{0}]", dateTime.ToString("HH:mm UTC", CultureInfo.InvariantCulture));
            }
            else if (daysDiff == 7)
            {
                return string.Format("{0} week ago [{1}]",
                    Math.Ceiling((double)daysDiff / 7), dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }
            else if (daysDiff < 7)
            {
                return string.Format("{0} days ago [{1}]",
                    daysDiff, dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }
            else if (daysDiff < 31)
            {
                return string.Format("{0} weeks ago [{1}]",
                    Math.Ceiling((double)daysDiff / 7), dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }

            return dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
        }

        public static async Task<string> CheckSpecialsForPlayer(string id)
        {
            List<PlayerSpecial> playerSpecial = await Database.GetPlayerSpecials(id);
            if (playerSpecial.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                if (playerSpecial[0].pro_bool != 0)
                {
                    sb.Append(":mouse_three_button: Pro Player");
                }
                if (playerSpecial[0].streamer_bool != 0)
                {
                    if (playerSpecial[0].streamer_link != null)
                    {
                        sb.Append($"<:streamer:579125715874742280> Streamer - {playerSpecial[0].streamer_link}");
                    }
                    else
                    {
                        sb.Append("<:streamer:579125715874742280> Streamer");
                    }
                }
                if (playerSpecial[0].special.Contains("dev"))
                {
                    sb.Append(":star: Thoth Dev");
                }
                return sb.ToString();
            }
            return "";
        }

        public static string StatusEmoji(string status)
        {
            switch (status)
            {
                case "operational":
                    return "<:operational:579125995618172929>"; //<:operational:579125995618172929>
                case "degraded_performance":
                    return "<:incident:579145224522301480>";
                case "under_maintenance":
                    return "<:maintenance:579145936396353586>";
                default:
                    return "<:incident:579145224522301480>";
            }
        }

        public static DateTime TimezoneSpecific(DateTime utctime, string timezone)
        {
            TimeZoneInfo destionationTimeZone = TimeZoneInfo.FromSerializedString(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(utctime, destionationTimeZone);
        }

        // SMITE Portals
        public static string GetPortalName(int portal)
        {
            switch (portal)
            {
                case 1:
                    return "Hi-Rez";
                case 5:
                    return "Steam";
                case 9:
                    return "PS4";
                case 10:
                    return "Xbox";
                case 22:
                    return "Switch";
                default:
                    return "n/a";
            }
        }

        public static string GetPortalIcon(string portal)
        {
            switch (portal)
            {
                case "1":
                    return "<:windows:587119127953670159>"; // PC
                case "5":
                    return "<:steam:581485150043373578>"; // Steam
                case "9":
                    return "<:playstationicon:537745670518472714>"; // PS4
                case "10":
                    return "<:xboxicon:537749895029850112>"; // Xbox
                case "22":
                    return "<:switchicon:537752006719176714>"; // Switch
                default:
                    return "<:blank:570291209906552848>";
            }
        }

        public static string GetPortalIconLinks(string portal)
        {
            switch (portal)
            {
                case "1":
                    return "https://i.imgur.com/0TWCr6X.png"; // PC
                case "5":
                    return "https://cdn.discordapp.com/emojis/581485150043373578.png"; // Steam
                case "9":
                    return "https://cdn.discordapp.com/emojis/537745670518472714.png"; // PS4
                case "10":
                    return "https://cdn.discordapp.com/emojis/537749895029850112.png"; // Xbox
                case "22":
                    return "https://cdn.discordapp.com/emojis/537752006719176714.png"; // Switch
                default:
                    return "https://i.imgur.com/8qNdxse.png";
            }
        }

        // SMITE Queue names
        public static string GetQueueName(int queueID)
        {
            switch (queueID)
            {
                case 426:
                    return "Conquest";
                case 429:
                    return "Custom Conquest";
                case 434:
                    return "MOTD";
                case 435:
                    return "Arena";
                case 436:
                    return "Basic Tutorial";
                case 438:
                    return "Custom Arena";
                case 440:
                    return "Ranked Duel";
                case 441:
                    return "Custom Joust";
                case 443:
                    return "Arena Practice (Easy)";
                case 444:
                    return "Jungle Practice";
                case 445:
                    return "Assault";
                case 446:
                    return "Custom Assault";
                case 448:
                    return "Joust";
                case 450:
                    return "Ranked Joust";
                case 451:
                    return "Ranked Conquest";
                case 454:
                    return "Assault(Co-Op) Medium";
                case 456:
                    return "Joust(Co-Op) Medium";
                case 457:
                    return "Arena(Co-Op) Easy";
                case 458:
                    return "Conquest Practice (Easy)";
                case 459:
                    return "Siege";
                case 460:
                    return "Custom Siege";
                case 461:
                    return "Conquest(Co-Op) Medium";
                case 462:
                    return "Arena Tutorial";
                case 464:
                    return "Joust Practice (Easy)";
                case 466:
                    return "Clash";
                case 467:
                    return "Custom Clash";
                case 468:
                    return "Arena(Co-Op) Medium";
                case 469:
                    return "Clash(Co-Op) Medium";
                case 470:
                    return "Clash Practice (Easy)";
                case 471:
                    return "Clash Tutorial";
                case 472:
                    return "Arena Practice (Medium)";
                case 473:
                    return "Joust Practice (Medium)";
                case 474:
                    return "Joust(Co-Op) Easy";
                case 475:
                    return "Conquest Practice (Medium)";
                case 476:
                    return "Conquest(Co-Op) Easy";
                case 477:
                    return "Clash Practice (Medium)";
                case 478:
                    return "Clash(Co-Op) Easy";
                case 479:
                    return "Assault Practice (Easy)";
                case 480:
                    return "Assault Practice (Medium)";
                case 481:
                    return "Assault(Co-Op) Easy";
                case 502:
                    return "Ranked Duel(Console)";
                case 503:
                    return "Ranked Joust(Console)";
                case 504:
                    return "Ranked Conquest(Console)";
                default:
                    return "";
            }
        }

        public static Tuple<string, string> GetRankedConquest(int tier)
        {
            switch (tier)
            {
                case 0:
                    return Tuple.Create("Unranked", "<:q_:528617317534269450>");
                case 1:
                    return Tuple.Create("Bronze V", "<:cqbr:528617350027673620>");
                case 2:
                    return Tuple.Create("Bronze IV", "<:cqbr:528617350027673620>");
                case 3:
                    return Tuple.Create("Bronze III", "<:cqbr:528617350027673620>");
                case 4:
                    return Tuple.Create("Bronze II", "<:cqbr:528617350027673620>");
                case 5:
                    return Tuple.Create("Bronze I", "<:cqbr:528617350027673620>");
                case 6:
                    return Tuple.Create("Silver V", "<:cqsi:528617356151488512>");
                case 7:
                    return Tuple.Create("Silver IV", "<:cqsi:528617356151488512>");
                case 8:
                    return Tuple.Create("Silver III", "<:cqsi:528617356151488512>");
                case 9:
                    return Tuple.Create("Silver II", "<:cqsi:528617356151488512>");
                case 10:
                    return Tuple.Create("Silver I", "<:cqsi:528617356151488512>");
                case 11:
                    return Tuple.Create("Gold V", "<:cqgo:528617356491227136>");
                case 12:
                    return Tuple.Create("Gold IV", "<:cqgo:528617356491227136>");
                case 13:
                    return Tuple.Create("Gold III", "<:cqgo:528617356491227136>");
                case 14:
                    return Tuple.Create("Gold II", "<:cqgo:528617356491227136>");
                case 15:
                    return Tuple.Create("Gold I", "<:cqgo:528617356491227136>");
                case 16:
                    return Tuple.Create("Platinum V", "<:cqpl:528617357485015041>");
                case 17:
                    return Tuple.Create("Platinum IV", "<:cqpl:528617357485015041>");
                case 18:
                    return Tuple.Create("Platinum III", "<:cqpl:528617357485015041>");
                case 19:
                    return Tuple.Create("Platinum II", "<:cqpl:528617357485015041>");
                case 20:
                    return Tuple.Create("Platinum I", "<:cqpl:528617357485015041>");
                case 21:
                    return Tuple.Create("Diamond V", "<:cqdi:528617356625313792>");
                case 22:
                    return Tuple.Create("Diamond IV", "<:cqdi:528617356625313792>");
                case 23:
                    return Tuple.Create("Diamond III", "<:cqdi:528617356625313792>");
                case 24:
                    return Tuple.Create("Diamond II", "<:cqdi:528617356625313792>");
                case 25:
                    return Tuple.Create("Diamond I", "<:cqdi:528617356625313792>");
                case 26:
                    return Tuple.Create("Masters", "<:cqma:528617357669826560>");
                case 27:
                    return Tuple.Create("Grandmaster", "<:cqgm:528617358500298753>");
                default:
                    return Tuple.Create("This", "Report");
            }
        }

        public static Tuple<string, string> GetRankedJoust(int tier)
        {
            switch (tier)
            {
                case 0:
                    return Tuple.Create("Unranked", "<:q_:528617317534269450>");
                case 1:
                    return Tuple.Create("Bronze V", "<:jobr:528617414171164697>");
                case 2:
                    return Tuple.Create("Bronze IV", "<:jobr:528617414171164697>");
                case 3:
                    return Tuple.Create("Bronze III", "<:jobr:528617414171164697>");
                case 4:
                    return Tuple.Create("Bronze II", "<:jobr:528617414171164697>");
                case 5:
                    return Tuple.Create("Bronze I", "<:jobr:528617414171164697>");
                case 6:
                    return Tuple.Create("Silver V", "<:josi:528617415903412244>");
                case 7:
                    return Tuple.Create("Silver IV", "<:josi:528617415903412244>");
                case 8:
                    return Tuple.Create("Silver III", "<:josi:528617415903412244>");
                case 9:
                    return Tuple.Create("Silver II", "<:josi:528617415903412244>");
                case 10:
                    return Tuple.Create("Silver I", "<:josi:528617415903412244>");
                case 11:
                    return Tuple.Create("Gold V", "<:jogo:528617415500890112>");
                case 12:
                    return Tuple.Create("Gold IV", "<:jogo:528617415500890112>");
                case 13:
                    return Tuple.Create("Gold III", "<:jogo:528617415500890112>");
                case 14:
                    return Tuple.Create("Gold II", "<:jogo:528617415500890112>");
                case 15:
                    return Tuple.Create("Gold I", "<:jogo:528617415500890112>");
                case 16:
                    return Tuple.Create("Platinum V", "<:jopl:528617415677050909>");
                case 17:
                    return Tuple.Create("Platinum IV", "<:jopl:528617415677050909>");
                case 18:
                    return Tuple.Create("Platinum III", "<:jopl:528617415677050909>");
                case 19:
                    return Tuple.Create("Platinum II", "<:jopl:528617415677050909>");
                case 20:
                    return Tuple.Create("Platinum I", "<:jopl:528617415677050909>");
                case 21:
                    return Tuple.Create("Diamond V", "<:jodi:528617416452997120>");
                case 22:
                    return Tuple.Create("Diamond IV", "<:jodi:528617416452997120>");
                case 23:
                    return Tuple.Create("Diamond III", "<:jodi:528617416452997120>");
                case 24:
                    return Tuple.Create("Diamond II", "<:jodi:528617416452997120>");
                case 25:
                    return Tuple.Create("Diamond I", "<:jodi:528617416452997120>");
                case 26:
                    return Tuple.Create("Masters", "<:joma:528617417170223144>");
                case 27:
                    return Tuple.Create("Grandmaster", "<:jogm:528617416331362334>");
                default:
                    return Tuple.Create("Thot", "Begone");
            }
        }

        public static Tuple<string, string> GetRankedDuel(int tier)
        {
            switch (tier)
            {
                case 0:
                    return Tuple.Create("Unranked", "<:q_:528617317534269450>");
                case 1:
                    return Tuple.Create("Bronze V", "<:dubr:528617383011549184>");
                case 2:
                    return Tuple.Create("Bronze IV", "<:dubr:528617383011549184>");
                case 3:
                    return Tuple.Create("Bronze III", "<:dubr:528617383011549184>");
                case 4:
                    return Tuple.Create("Bronze II", "<:dubr:528617383011549184>");
                case 5:
                    return Tuple.Create("Bronze I", "<:dubr:528617383011549184>");
                case 6:
                    return Tuple.Create("Silver V", "<:dusi:528617384395931649>");
                case 7:
                    return Tuple.Create("Silver IV", "<:dusi:528617384395931649>");
                case 8:
                    return Tuple.Create("Silver III", "<:dusi:528617384395931649>");
                case 9:
                    return Tuple.Create("Silver II", "<:dusi:528617384395931649>");
                case 10:
                    return Tuple.Create("Silver I", "<:dusi:528617384395931649>");
                case 11:
                    return Tuple.Create("Gold V", "<:dugo:528617384463040533>");
                case 12:
                    return Tuple.Create("Gold IV", "<:dugo:528617384463040533>");
                case 13:
                    return Tuple.Create("Gold III", "<:dugo:528617384463040533>");
                case 14:
                    return Tuple.Create("Gold II", "<:dugo:528617384463040533>");
                case 15:
                    return Tuple.Create("Gold I", "<:dugo:528617384463040533>");
                case 16:
                    return Tuple.Create("Platinum V", "<:dupl:528617384848785446>");
                case 17:
                    return Tuple.Create("Platinum IV", "<:dupl:528617384848785446>");
                case 18:
                    return Tuple.Create("Platinum III", "<:dupl:528617384848785446>");
                case 19:
                    return Tuple.Create("Platinum II", "<:dupl:528617384848785446>");
                case 20:
                    return Tuple.Create("Platinum I", "<:dupl:528617384848785446>");
                case 21:
                    return Tuple.Create("Diamond V", "<:dudi:528617385310289922>");
                case 22:
                    return Tuple.Create("Diamond IV", "<:dudi:528617385310289922>");
                case 23:
                    return Tuple.Create("Diamond III", "<:dudi:528617385310289922>");
                case 24:
                    return Tuple.Create("Diamond II", "<:dudi:528617385310289922>");
                case 25:
                    return Tuple.Create("Diamond I", "<:dudi:528617385310289922>");
                case 26:
                    return Tuple.Create("Masters", "<:duma:528617385452634122>");
                case 27:
                    return Tuple.Create("Grandmaster", "<:dugm:528617385410822154>");
                default:
                    return Tuple.Create("Thot", "Begone");
            }
        }

        public static List<int> LegitQueueIDs()
        {///////////////////////////////423, 426, 430, 433, 434, 435, 440, 445, 448, 451, 452, 459, 466, 450, 502, 503, 504
            List<int> list = new List<int> { 426, 435, 440, 445, 448, 450, 451, 459, 466, 502, 503, 504 };
            return list;
        }
    }
}
