using System;
using System.Globalization;

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

            if (daysDiff == 0)
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
            if (daysDiff == 1)
            {
                return "Yesterday";
            }
            if (daysDiff == 7)
            {
                return string.Format("{0} week ago [{1}]",
                    Math.Ceiling((double)daysDiff / 7), dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }
            if (daysDiff < 7)
            {
                return string.Format("{0} days ago [{1}]",
                    daysDiff, dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }
            if (daysDiff < 31)
            {
                return string.Format("{0} weeks ago [{1}]",
                    Math.Ceiling((double)daysDiff / 7), dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture));
            }

            return dateTime.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
        }

        // SMITE Queue names
        public static string GetQueueName(int queueID)
        {
            switch (queueID)
            {
                case 426:
                    return "Conquest";
                case 429:
                    return "Conquest Challenge";
                case 434:
                    return "MOTD";
                case 435:
                    return "Arena";
                case 436:
                    return "Basic Tutorial";
                case 438:
                    return "Arena Challenge";
                case 440:
                    return "Ranked Duel(1v1)";
                case 441:
                    return "Joust Challenge";
                case 443:
                    return "Arena Practice (Easy)";
                case 445:
                    return "Assault";
                case 446:
                    return "Assault Challenge";
                case 448:
                    return "Joust(3v3)";
                case 450:
                    return "Ranked Joust(3v3)";
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
                    return "Siege(4v4)";
                case 460:
                    return "Siege Challenge";
                case 461:
                    return "Conquest(Co-Op) Medium";
                case 462:
                    return "Arena Tutorial";
                case 464:
                    return "Joust Practice (Easy)";
                case 466:
                    return "Clash";
                case 467:
                    return "Clash Challenge";
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
                    return "Ranked Duel(1v1) Console";
                case 503:
                    return "Ranked Joust(3v3) Console";
                case 504:
                    return "Ranked Conquest(Console)";
                default:
                    return "";
            }
        }
    }
}
