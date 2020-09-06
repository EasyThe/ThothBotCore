using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
    public class Text
    {
        public static string ToTitleCase(string text)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
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
        public static string UserNotFound(string username)
        {
            return $"<:X_:579151621502795777>*{username}* is not found!";
        }
        public static string UserIsHidden(string username)
        {
            return $"<:Hidden:591666971234402320>*{username}*'s account is hidden!";
        }
        public static void WriteLine(string message, ConsoleColor backColor, ConsoleColor textColor)
        {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = textColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static string AbbreviationRegions(string region)
        {
            if (region.ToLowerInvariant() == "europe")
            {
                return "EU";
            }
            else
            {
                return string.Join(string.Empty, region
                .Where(char.IsLetter)
                .Where(char.IsUpper));
            }
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
                    return string.Format("{0} hours ago",
                        Math.Floor((double)secsDiff / 3600));
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
        public static async Task<string> CheckSpecialsForPlayer(int id, bool emoteOnly)
        {
            PlayerSpecial playerSpecial = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(id);
            if (playerSpecial != null)
            {
                if (playerSpecial.special != "" || playerSpecial.pro_bool || playerSpecial.streamer_bool || playerSpecial.special != null)
                {
                    var specialsResult = new StringBuilder();
                    if (playerSpecial.pro_bool)
                    {
                        var badge = await MongoConnection.GetBadgeAsync("pro");
                        if (emoteOnly)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                        }
                    }
                    if (playerSpecial.streamer_bool)
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (!emoteOnly)
                            {
                                specialsResult.Append("\n");
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync("streamer");
                        if (emoteOnly)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                            if (playerSpecial.streamer_link != null)
                            {
                                specialsResult.Append($" {playerSpecial.streamer_link}");
                            }
                        }
                    }
                    // Check specials
                    if (playerSpecial.special != "")
                    {
                        if (specialsResult.Length != 0)
                        {
                            if (!emoteOnly)
                            {
                                specialsResult.Append("\n");
                            }
                        }
                        var badge = await MongoConnection.GetBadgeAsync(playerSpecial.special);
                        if (emoteOnly)
                        {
                            specialsResult.Append(badge.Emote);
                        }
                        else
                        {
                            specialsResult.Append($"{badge.Emote} {badge.Title}");
                        }
                    }
                    return specialsResult.ToString();
                }
            }
            return "";
        }
        public static string StatusEmoji(string status)
        {
            return status switch
            {
                "operational" => "<:operational:579125995618172929>",
                "degraded_performance" => "<:incident:579145224522301480>",
                "under_maintenance" => "<:maintenance:579145936396353586>",
                _ => "<:incident:579145224522301480>",
            };
        }
        public static string SideName(int taskForce)
        {
            if (taskForce == 1)
            {
                return "Order";
            }
            else
            {
                return "Chaos";
            }
        }
        public static string SideEmoji(int taskForce)
        {
            if (taskForce == 1)
            {
                return "🔹";
            }
            else
            {
                return "🔸";
            }
        }

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

        public static string GetPortalIcon(string portal)
        {
            return portal switch
            {
                "1" => "<:windows:587119127953670159>",// PC
                "5" => "<:steam:581485150043373578>",// Steam
                "9" => "<:PS4:537745670518472714>",// PS4
                "10" => "<:XB:537749895029850112>",// Xbox
                "22" => "<:SW:537752006719176714>",// Switch
                "28" => "<:egs:705963938340274247>",// Epic Games Store
                _ => "<:blank:570291209906552848>",
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

        // SMITE Queue names
        public static string GetQueueName(int queueID)
        {
            return queueID switch
            {
                423 => "Conquest 5v5 __Old Queue__",
                426 => "Conquest",
                429 => "Custom Conquest",
                430 => "Conquest Solo Ranked __Old Queue__",
                433 => "Domination __Old Queue__",
                434 => "MOTD",
                435 => "Arena",
                436 => "Basic Tutorial",
                438 => "Custom Arena",
                440 => "Ranked Duel",
                441 => "Custom Joust",
                443 => "Arena Practice (Easy)",
                444 => "Jungle Practice",
                445 => "Assault",
                446 => "Custom Assault",
                448 => "Joust",
                450 => "Ranked Joust",
                451 => "Ranked Conquest",
                452 => "Ranked Arena __Old Queue__",
                454 => "Assault(vs AI) Medium",
                456 => "Joust (vs AI) Medium",
                457 => "Arena (vs AI) Easy",
                458 => "Conquest Practice (Easy)",
                459 => "Siege",
                460 => "Custom Siege",
                461 => "Conquest (vs AI) Medium",
                462 => "Arena Tutorial",
                464 => "Joust Practice (Easy)",
                466 => "Clash",
                467 => "Custom Clash",
                468 => "Arena (vs AI) Medium",
                469 => "Clash (vs AI) Medium",
                470 => "Clash Practice (Easy)",
                471 => "Clash Tutorial",
                472 => "Arena Practice (Medium)",
                473 => "Joust Practice (Medium)",
                474 => "Joust (Co-Op) Easy",
                475 => "Conquest Practice (Medium)",
                476 => "Conquest (Co-Op) Easy",
                477 => "Clash Practice (Medium)",
                478 => "Clash (Co-Op) Easy",
                479 => "Assault Practice (Easy)",
                480 => "Assault Practice (Medium)",
                481 => "Assault (Co-Op) Easy",
                502 => "Ranked Duel (Console)",
                503 => "Ranked Joust (Console)",
                504 => "Ranked Conquest (Console)",
                10155 => "Adventures: Heimdallr's Crossing",
                10158 => "Arena (vs AI) (Very Hard)",
                10159 => "Assault (vs AI) (Hard)",
                10160 => "Clash (vs AI) (Hard)",
                10161 => "Conquest (vs AI) (Hard)",
                10162 => "Joust (vs AI) (Very Hard)",
                10163 => "Arena (vs AI) (Easy)",
                10164 => "Arena (vs AI) (Hard)",
                10165 => "Joust (vs AI) (Easy)",
                10166 => "Joust (vs AI) (Hard)",
                10167 => "Arena Practice (Hard)",
                10168 => "Assault Practice (Hard)",
                10169 => "Clash Practice (Hard)",
                10170 => "Conquest Practice (Hard)",
                10171 => "Joust Practice (Hard)",
                _ => "Unknown Queue",
            };
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
                _ => "<:10:695983354453033010>",
            };
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
                _ => Tuple.Create("This", "Report"),
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
            List<int> list = new List<int> { 423, 426, 430, 433, 435, 440, 445, 448, 450, 451, 452, 459, 466, 502, 503, 504 };
            return list;
        }

        public static string ReFormatMOTDText(string text)
        {
            text = text.Replace("<li>", "\n");
            text = text.Replace("</li>", "");
            text = text.Replace("Map", "**Map**");
            text = text.Replace("Starting/Maximum Cooldown Reduction", "**Starting**/**Maximum Cooldown Reduction**");
            text = text.Replace("Starting Level", "**Starting Level**");
            text = text.Replace("Starting Gold", "**Starting Gold**");
            text = text.Replace("Gods", "**Gods**");
            text = text.Replace("Selection", "**Selection**");
            text = text.Replace("Infinite Mana", "**Infinite Mana**");
            text = text.Replace("Maximum Cooldown Reduction", "**Maximum Cooldown Reduction**");
            text = text.Replace("Starting Cooldown Reduction", "**Starting Cooldown Reduction**");

            return text;
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

        //Paladins
        public static string GetQueueNamePaladins(int queueID)
        {
            return queueID switch
            {
                424 => "Siege",
                469 => "Team Deathmatch",
                452 => "Onslaught",
                486 => "Competitive KBM",
                470 => "Team Deathmatch Practice",
                425 => "Practice Siege",
                453 => "Onslaught Practice",
                428 => "Competitive Gamepad",
                445 => "Test Maps",
                _ => "Unknown Mode",
            };
        }
    }
}
