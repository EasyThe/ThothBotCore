using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ThothBotCore.Models;

namespace ThothBotCore.Utilities
{
    public class Text
    {
        static Random rnd = new();
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static string ToTitleCase(string text)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
        }
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        public static string Truncate(string value, int maxChars) // Didnt try if works
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
        public static string InvariantDate(DateTime dateTime)
        {
            return dateTime.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);
        }
        public static string InvariantDefaultDate(DateTime dateTime)
        {
            return dateTime.ToString(CultureInfo.InvariantCulture);
        }
        public static string ToRfc3339String(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo);
        }
        public static string UserNotFound(string username)
        {
            return $"<:X_:579151621502795777>*{username}* was not found!";
        }
        public static string UserIsHidden(string username)
        {
            return $"<:Hidden:591666971234402320>*{username}*'s account is hidden!";
        }
        public static string GenerateString(int count)
        {
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
            {
                chars[i] = alphabet[rnd.Next(alphabet.Length)];
            }
            return new string(chars);
        }
        public static void WriteLine(string message, ConsoleColor backColor, ConsoleColor textColor)
        {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = textColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
        public static string AbbreviationRegions(string region)
        {
            if (region.ToLowerInvariant() == "europe")
            {
                return "EU";
            }
            else if (region.ToLowerInvariant() == "oceania")
            {
                return "OCE";
            }
            else
            {
                return string.Join(string.Empty, region
                .Where(char.IsLetter)
                .Where(char.IsUpper));
            }
        }
        public static string NumberEmojies(int number)
        {
            return number switch
            {
                0 => "0️⃣",
                1 => "1️⃣",
                2 => "2️⃣",
                3 => "3️⃣",
                4 => "4️⃣",
                5 => "5️⃣",
                6 => "6️⃣",
                7 => "7️⃣",
                8 => "8️⃣",
                9 => "9️⃣",
                10 => "🔟",
                _ => "🤷‍♂️",
            };
        }
        public static string GetRoleIcon(string role)
        {
            return role switch
            {
                "Solo" => "<:solol:862261457009246218>",
                "Jungle" => "<:jungle:862261456527294465>",
                "Mid" => "<:mid:862261456937812008>",
                "Support" => "<:support:862261457110302720>",
                "Carry" => "<:adc:862261456901111878>",
                _ => ""
            };
        }
        public static string StatusEmoji(string status)
        {
            return status switch
            {
                "operational" or "up" => "<:operational:579125995618172929>",
                "degraded_performance" => "<:incident:579145224522301480>",
                "under_maintenance" => "<:maintenance:579145936396353586>",
                "limited_access" => "🚫",
                "down" => "🔻",
                _ => "<:incident:579145224522301480>",
            };
        }
        public static string SideName(int taskForce)
        {
            if (taskForce == 1)
            {
                return "Order";
            }
            return "Chaos";
        }
        public static string SideEmoji(int taskForce)
        {
            if (taskForce == 1)
            {
                return "🔹";
            }
            return "🔸";
        }
        public static string URLifyGodName(string godName) => godName.Replace(" ", "-");

        // SMITE Portals
        public static string GetPortalName(int portal)
        {
            return portal switch
            {
                1 => "Hi-Rez",
                5 => "Steam",
                9 => "PS4",
                10 => "Xbox",
                22 => "Switch",
                28 => "Epic Games Store",
                _ => "n/a",
            };
        }
        public static string GetPortalEmoji(string portal)
        {
            return portal switch
            {
                "1" => "<:windows:587119127953670159>",// PC
                "5" => "<:steam:581485150043373578>",// Steam
                "9" => "<:PS4:537745670518472714>",// PS4
                "10" => "<:XB:537749895029850112>",// Xbox
                "22" => "<:SW:537752006719176714>",// Switch
                "28" => "<:egs:705963938340274247>",// Epic Games Store
                _ => "<:Hidden:591666971234402320>"
            };
        }
        public static int GetPortalNumber(string portal)
        {
            return portal switch
            {
                "Hirez" => 1,
                "Steam" => 5,
                "PSN" => 9,
                "XboxLive" => 10,
                "Nintendo Switch" => 22,
                "Epic Games" => 28,
                _ => 0,
            };
        }
        public static string GetPortalIconLinksByPortalID(string portal)
        {
            return portal switch
            {
                "1" => "https://i.imgur.com/0TWCr6X.png",// PC
                "5" => "https://cdn.discordapp.com/emojis/581485150043373578.png",// Steam
                "9" => "https://cdn.discordapp.com/emojis/537745670518472714.png",// PS4
                "10" => "https://cdn.discordapp.com/emojis/537749895029850112.png",// Xbox
                "22" => "https://i.imgur.com/f11agtV.png",// Switch
                "28" => "https://i.imgur.com/k9FrhA4.png",// Epic Games Store
                _ => Constants.botIcon,
            };
        }
        public static string GetPortalIconLinksByPortalName(string portal)
        {
            return portal switch
            {
                "Hirez" => "https://i.imgur.com/0TWCr6X.png",// PC
                "Steam" => "https://cdn.discordapp.com/emojis/581485150043373578.png",// Steam
                "PSN" => "https://cdn.discordapp.com/emojis/537745670518472714.png",// PS4
                "XboxLive" => "https://cdn.discordapp.com/emojis/537749895029850112.png",// Xbox
                "Nintendo Switch" => "https://i.imgur.com/f11agtV.png",// Switch
                "Epic Games" => "https://i.imgur.com/k9FrhA4.png",// Epic Games Store
                _ => Constants.botIcon,
            };
        }
        // SMITE Esports
        public static string GetEsportsTeamEmoji(string team)
        {
            return team switch
            {
                "BOLTS" => "<:BOLTS:875188138111279164>",
                "JADE" => "<:JADE:875189087299072050>",
                "KINGS" => "<:KINGS:875188137045942272>",
                "LVTHN" => "<:LVTHN:875189087533932544>",
                "ONI" => "<:ONI:875189088033054741>",
                "SOLAR" => "<:SOLAR:875189087810768896>",
                "TITAN" => "<:TITAN:875189088238600243>",
                "VALKS" => "<:VALKS:875188135527600149>",
                _ => ""
            };
        }

        // SMITE Queue names
        public static string GetQueueName(int queueID, string name = "")
        {
            var queue = Constants.SmiteQueues.FirstOrDefault(x => x.Key == queueID.ToString()).Value;
            if (queue == null)
            {
                if (name.Length != 0)
                {
                    return name;
                }
                _ = Reporter.SendError($"**Missing Queue:**\nID:{queueID} || {name}");
                return "Unknown Queue";
            }
            return queue;
        }
        public static string GetRankEmoji(int rank)
        {
            return rank switch
            {
                0 => "<:blank:570291209906552848>",
                1 => "<:1_:695991295960940554>",
                2 => "<:2_:695991295952420994>",
                3 => "<:3_:695991295998427197>",
                4 => "<:4_:695991296111935550>",
                5 => "<:5_:695991296052953150>",
                6 => "<:6_:695991296145489961>",
                7 => "<:7_:695991296187301948>",
                8 => "<:8_:695991298712404038>",
                9 => "<:9_:695991296401080330>",
                _ => "<:10:695983354453033010>"
            };
        }
        public static string GetPartyEmoji(int number)
        {
            return number switch
            {
                1 => "<:Party_1:848904863173312512>",
                2 => "<:Party_2:848904863479103518>",
                3 => "<:Party_3:848904863240159262>",
                4 => "<:Party_4:848904863063605280>",
                _ => "<:Solo:857722598721060884>"
            };
        }
        /// <param name="pantheon">lower case</param>
        /// <returns></returns>
        public static string GetPantheonEmoji(string pantheon)
        {
            return pantheon switch
            {
                "arthurian" => "<:Arthurian:960310480189161534>",
                "babylonian" => "<:Babylonian:960310480306577448>",
                "chinese" => "<:Chinese:960310480424030249>",
                "celtic" => "<:Celtic:960310480017162251>",
                "egyptian" => "<:Egyptian:960310480377888808>",
                "greek" => "<:Greek:960310480335933521>",
                "great old ones" => "<:Great_Old_Ones:960310480457576548>",
                "hindu" => "<:Hindu:960310480432406579>",
                "japanese" => "<:Japanese:960310480633741382>",
                "maya" => "<:Maya:960310480692465706>",
                "norse" => "<:Norse:960310480415629342>",
                "polynesian" => "<:Polynesian:960310480390467655>",
                "roman" => "<:Roman:960310480751198258>",
                "slavic" => "<:Slavic:960310480684064778>",
                "voodoo" => "<:Voodoo:960310480239480873>",
                "yoruba" => "<:Yoruba:960310480486936698>",
                _ => "<:blank:570291209906552848>"
            };
        }
        public static string GetGodRoleEmoji(string role)
        {
            return role.ToLowerInvariant() switch
            {
                "mage" => "<:Mage:607990144380698625>",
                "warrior" => "<:Warrior:607990144338886658>",
                "assassin" => "<:Assassin:607990143915261983>",
                "guardian" => "<:Guardian:607990144385024000>",
                _ => "<:Hunter:607990144271646740>"
            };
        }
        public static string GetGodTypeEmoji(string type)
        {
            return type == "magical" ? "<:Magical:960310480533090324>" : "<:Physical:960310480759586836>";
        }
        public static Tuple<string, string> GetRankedConquest(int tier)
        {
            return tier switch
            {
                0 => Tuple.Create("Unranked", "<:q_:528617317534269450>"),
                1 => Tuple.Create("Bronze V", "<:cqbr:528617350027673620>"),
                2 => Tuple.Create("Bronze IV", "<:cqbr:528617350027673620>"),
                3 => Tuple.Create("Bronze III", "<:cqbr:528617350027673620>"),
                4 => Tuple.Create("Bronze II", "<:cqbr:528617350027673620>"),
                5 => Tuple.Create("Bronze I", "<:cqbr:528617350027673620>"),
                6 => Tuple.Create("Silver V", "<:cqsi:528617356151488512>"),
                7 => Tuple.Create("Silver IV", "<:cqsi:528617356151488512>"),
                8 => Tuple.Create("Silver III", "<:cqsi:528617356151488512>"),
                9 => Tuple.Create("Silver II", "<:cqsi:528617356151488512>"),
                10 => Tuple.Create("Silver I", "<:cqsi:528617356151488512>"),
                11 => Tuple.Create("Gold V", "<:cqgo:528617356491227136>"),
                12 => Tuple.Create("Gold IV", "<:cqgo:528617356491227136>"),
                13 => Tuple.Create("Gold III", "<:cqgo:528617356491227136>"),
                14 => Tuple.Create("Gold II", "<:cqgo:528617356491227136>"),
                15 => Tuple.Create("Gold I", "<:cqgo:528617356491227136>"),
                16 => Tuple.Create("Platinum V", "<:cqpl:528617357485015041>"),
                17 => Tuple.Create("Platinum IV", "<:cqpl:528617357485015041>"),
                18 => Tuple.Create("Platinum III", "<:cqpl:528617357485015041>"),
                19 => Tuple.Create("Platinum II", "<:cqpl:528617357485015041>"),
                20 => Tuple.Create("Platinum I", "<:cqpl:528617357485015041>"),
                21 => Tuple.Create("Diamond V", "<:cqdi:528617356625313792>"),
                22 => Tuple.Create("Diamond IV", "<:cqdi:528617356625313792>"),
                23 => Tuple.Create("Diamond III", "<:cqdi:528617356625313792>"),
                24 => Tuple.Create("Diamond II", "<:cqdi:528617356625313792>"),
                25 => Tuple.Create("Diamond I", "<:cqdi:528617356625313792>"),
                26 => Tuple.Create("Masters", "<:cqma:528617357669826560>"),
                27 => Tuple.Create("Grandmaster", "<:cqgm:528617358500298753>"),
                _ => Tuple.Create("This", "Report")
            };
        }
        public static Tuple<string, string> GetRankedJoust(int tier)
        {
            return tier switch
            {
                0 => Tuple.Create("Unranked", "<:q_:528617317534269450>"),
                1 => Tuple.Create("Bronze V", "<:jobr:528617414171164697>"),
                2 => Tuple.Create("Bronze IV", "<:jobr:528617414171164697>"),
                3 => Tuple.Create("Bronze III", "<:jobr:528617414171164697>"),
                4 => Tuple.Create("Bronze II", "<:jobr:528617414171164697>"),
                5 => Tuple.Create("Bronze I", "<:jobr:528617414171164697>"),
                6 => Tuple.Create("Silver V", "<:josi:528617415903412244>"),
                7 => Tuple.Create("Silver IV", "<:josi:528617415903412244>"),
                8 => Tuple.Create("Silver III", "<:josi:528617415903412244>"),
                9 => Tuple.Create("Silver II", "<:josi:528617415903412244>"),
                10 => Tuple.Create("Silver I", "<:josi:528617415903412244>"),
                11 => Tuple.Create("Gold V", "<:jogo:528617415500890112>"),
                12 => Tuple.Create("Gold IV", "<:jogo:528617415500890112>"),
                13 => Tuple.Create("Gold III", "<:jogo:528617415500890112>"),
                14 => Tuple.Create("Gold II", "<:jogo:528617415500890112>"),
                15 => Tuple.Create("Gold I", "<:jogo:528617415500890112>"),
                16 => Tuple.Create("Platinum V", "<:jopl:528617415677050909>"),
                17 => Tuple.Create("Platinum IV", "<:jopl:528617415677050909>"),
                18 => Tuple.Create("Platinum III", "<:jopl:528617415677050909>"),
                19 => Tuple.Create("Platinum II", "<:jopl:528617415677050909>"),
                20 => Tuple.Create("Platinum I", "<:jopl:528617415677050909>"),
                21 => Tuple.Create("Diamond V", "<:jodi:528617416452997120>"),
                22 => Tuple.Create("Diamond IV", "<:jodi:528617416452997120>"),
                23 => Tuple.Create("Diamond III", "<:jodi:528617416452997120>"),
                24 => Tuple.Create("Diamond II", "<:jodi:528617416452997120>"),
                25 => Tuple.Create("Diamond I", "<:jodi:528617416452997120>"),
                26 => Tuple.Create("Masters", "<:joma:528617417170223144>"),
                27 => Tuple.Create("Grandmaster", "<:jogm:528617416331362334>"),
                _ => Tuple.Create("Thot", "Begone"),
            };
        }

        public static Tuple<string, string> GetRankedDuel(int tier)
        {
            return tier switch
            {
                0 => Tuple.Create("Unranked", "<:q_:528617317534269450>"),
                1 => Tuple.Create("Bronze V", "<:dubr:528617383011549184>"),
                2 => Tuple.Create("Bronze IV", "<:dubr:528617383011549184>"),
                3 => Tuple.Create("Bronze III", "<:dubr:528617383011549184>"),
                4 => Tuple.Create("Bronze II", "<:dubr:528617383011549184>"),
                5 => Tuple.Create("Bronze I", "<:dubr:528617383011549184>"),
                6 => Tuple.Create("Silver V", "<:dusi:528617384395931649>"),
                7 => Tuple.Create("Silver IV", "<:dusi:528617384395931649>"),
                8 => Tuple.Create("Silver III", "<:dusi:528617384395931649>"),
                9 => Tuple.Create("Silver II", "<:dusi:528617384395931649>"),
                10 => Tuple.Create("Silver I", "<:dusi:528617384395931649>"),
                11 => Tuple.Create("Gold V", "<:dugo:528617384463040533>"),
                12 => Tuple.Create("Gold IV", "<:dugo:528617384463040533>"),
                13 => Tuple.Create("Gold III", "<:dugo:528617384463040533>"),
                14 => Tuple.Create("Gold II", "<:dugo:528617384463040533>"),
                15 => Tuple.Create("Gold I", "<:dugo:528617384463040533>"),
                16 => Tuple.Create("Platinum V", "<:dupl:528617384848785446>"),
                17 => Tuple.Create("Platinum IV", "<:dupl:528617384848785446>"),
                18 => Tuple.Create("Platinum III", "<:dupl:528617384848785446>"),
                19 => Tuple.Create("Platinum II", "<:dupl:528617384848785446>"),
                20 => Tuple.Create("Platinum I", "<:dupl:528617384848785446>"),
                21 => Tuple.Create("Diamond V", "<:dudi:528617385310289922>"),
                22 => Tuple.Create("Diamond IV", "<:dudi:528617385310289922>"),
                23 => Tuple.Create("Diamond III", "<:dudi:528617385310289922>"),
                24 => Tuple.Create("Diamond II", "<:dudi:528617385310289922>"),
                25 => Tuple.Create("Diamond I", "<:dudi:528617385310289922>"),
                26 => Tuple.Create("Masters", "<:duma:528617385452634122>"),
                27 => Tuple.Create("Grandmaster", "<:dugm:528617385410822154>"),
                _ => Tuple.Create("Thot", "Begone"),
            };
        }

        public static List<int> LegitQueueIDs()
        {
            List<int> list = new() { 423, 426, 430, 433, 435, 440, 445, 448, 450, 451, 452, 459, 466, 502, 503, 504, 10189 };
            return list;
        }

        public static string ReFormatMOTDText(string text)
        {
            return text.Replace("<li>", "\n")
                       .Replace("</li>", "")
                       .Replace("Map", "**Map**")
                       .Replace("Starting/Maximum Cooldown Reduction", "**Starting**/**Maximum Cooldown Reduction**")
                       .Replace("Starting Level", "**Starting Level**")
                       .Replace("Starting Gold", "**Starting Gold**")
                       .Replace("Gods:", "**Gods:**")
                       .Replace("Selection", "**Selection**")
                       .Replace("Infinite Mana", "**Infinite Mana**")
                       .Replace("Maximum Cooldown Reduction", "**Maximum Cooldown Reduction**")
                       .Replace("Starting Cooldown Reduction", "**Starting Cooldown Reduction**");
        }
        public static string ReformatSecondaryItemDescription(string text)
        {
            return text != null ? text.Replace("PASSIVE", "**PASSIVE**")
                       .Replace("<n>", "\n")
                       .Replace("GLYPH", "**GLYPH**")
                       .Replace("<font color='#42F46E'>", "")
                       .Replace("<font color='#F44242'>", "")
                       .Replace("AURA", "**AURA**")
                       .Replace("ROLE QUEST", "**ROLE QUEST**") : "";
        }
        public static string CleanDirtyText(string dirty)
        {
            return dirty.Replace("\\", String.Empty)
                        .Replace("/", String.Empty)
                        .Replace("*", String.Empty)
                        .Replace(".", String.Empty)
                        .Replace(",", String.Empty)
                        .Replace("?", String.Empty)
                        .Replace("!", String.Empty)
                        .Replace(";", String.Empty)
                        .Replace(":", String.Empty);
        }

        public static string EmptyStringCheck(string value)
        {
            if (value == null || value?.Length == 0)
            {
                return "n/a";
            }
            return value;
        }

        public static string HiddenProfileCheck(string name)
        {
            if (name == "")
            {
                return "~~Hidden Profile~~";
            }
            else
            {
                return name;
            }
        }

        public static string HiddenProfileCheck(string Name, string HzName, string GamerTag, object Ret_Msg)
        {
            if (Ret_Msg != null && Ret_Msg.ToString().ToLowerInvariant().Contains("privacy flag set")) return "~~Hidden Profile~~";
            if (HzName != null && Name != null && Name.Length != 0 && Name.Contains(HzName)) return HzName;
            if (GamerTag != null && Name != null && Name.Length != 0 && Name.Contains(GamerTag)) return GamerTag;
            if (HzName != null && HzName.Length != 0) return HzName;
            if (GamerTag != null && GamerTag.Length != 0) return GamerTag;
            if (Name != null && Name.Length != 0)
            {
                if (Name.Contains('[')) return Name.Split("]")[1];
                return Name;
            }
            return "~~Hidden Profile~~";
        }
        public static string RelativeTimestamp(DateTime dateTime)
        {
            return DiscordTimestamp(dateTime, 'R');
        }
        public static string ShortTimeTimestamp(DateTime dateTime)
        {
            return DiscordTimestamp(dateTime, 't');
        }
        public static string ShortDateTimeTimestamp(DateTime dateTime)
        {
            return DiscordTimestamp(dateTime, 'f');
        }
        public static string LongDateTimestamp(DateTime dateTime)
        {
            return DiscordTimestamp(dateTime, 'D');
        }
        /// <summary>
        /// Discord Timestamp implementation
        /// </summary>
        /// <param name="dateTime">DateTime must be in UTC</param>
        /// <param name="type">t - 16:20, T - 16:20:30, d - 20/04/2021, D - 20 April 2021, f - 20 April 2021 16:20,
        /// F - long date, R - 2 months ago</param>
        /// <returns></returns>
        private static string DiscordTimestamp(DateTime dateTime, char type)
        {
            return $"<t:{DateTimeToUnix(dateTime)}:{type}>";
        }
        public static int DateTimeToUnix(DateTime dateTime)
        {
            return (Int32)(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }
        public static DateTime UnixToDateTime(int unix)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unix);
            return dateTime;
        }
        public static double CalculateKDA(int kills, int deaths, int assists)
        {
            if (kills == 0 && deaths == 0 && assists == 0)
            {
                return 0;
            }
            if (kills == 0) kills = 1;
            if (deaths == 0) deaths = 1;
            if (assists == 0) assists = 1;
            return (double)(kills + (assists / 2)) / deaths;
        }

        public static string PlaceholderText()
        {
            var plc = Constants.Placeholders;
            return plc[rnd.Next(plc.Length)];
        }

        // Tips
        public static string GetRandomTip()
        {
            int hmm = rnd.Next(0, 100);
            if (hmm <= 20)
            {
                List<TipsModel> tips = Constants.TipsList;
                if (tips.Count != 0)
                {
                    int n = rnd.Next(tips.Count);
                    return $"ℹ Tip: {tips[n].TipText}";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
