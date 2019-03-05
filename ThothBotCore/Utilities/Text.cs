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
                return null;
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

            return dateTime.ToString("d.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
