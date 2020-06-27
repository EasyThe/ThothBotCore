using Discord;
using Newtonsoft.Json;
using System.IO;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Utilities
{
    class ConstantsConfig
    {
        private static readonly string constantsFolder = "Config";
        private static readonly string constantsFile = "Constants.json";

        public static Constants constants;
        static ConstantsConfig()
        {
            if (!Directory.Exists(constantsFolder))
            {
                Directory.CreateDirectory(constantsFolder);
            }

            if (!File.Exists(constantsFolder + "/" + constantsFile))
            {
                constants = new Constants();
                string json = JsonConvert.SerializeObject(constants, Formatting.Indented);
                File.WriteAllText(constantsFolder + "/" + constantsFile, json);
            }
            else
            {
                string json = File.ReadAllText(constantsFolder + "/" + constantsFile);
                constants = JsonConvert.DeserializeObject<Constants>(json);

                SaveConstants();
            }
        }

        public static void SaveConstants()
        {
            string json = JsonConvert.SerializeObject(constants, Formatting.Indented);
            File.WriteAllText(constantsFolder + "/" + constantsFile, json);
        }
    }

    public class Constants
    {
        public static readonly string botIcon = "https://i.imgur.com/8qNdxse.png";
        public static readonly ulong OwnerID = 171675309177831424;
        public static readonly ulong SupportServerID = 518408306415632384;
        public static readonly string SupportServerInvite = "https://discord.gg/hU6MTbQ";
        public static string JoinedMessage { get; set; } = ":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can set a custom prefix for your server with {Credentials.botConfig.prefix}prefix `your-prefix-here`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`\n" +
                $":small_orange_diamond:Please make sure I have **Send Messages**, **Read Messages**, **Embed Links** and **Use External Emojis** in the channels you would like me to react to your commands.";
        public static string FailedToSendJoinedMessage { get; set; } = "Couldn't send JoinedMessage to the Guild.";
        public static string DefaultPrefixMessage { get; set; } = $"My default prefix is `{Credentials.botConfig.prefix}`";
        public static string NotLinked { get; set; } = "This Discord user has not linked his/hers Discord and SMITE account. To link your Discord and SMITE accounts, use `!!link` and follow the instructions.";
        public static string APIEmptyResponse { get; set; } = "Sorry, the SmiteAPI sent an empty response.\nTry again later.";
        public static Emoji CheckMarkEmoji { get; set; } = new Emoji("✅");
        public static Color DefaultBlueColor { get; set; } = new Color(85, 172, 238);
        public static Color VulpisColor { get; set; } = new Color(230, 175, 43);
        public static Color ErrorColor { get; set; } = new Color(255, 148, 148);
        public static Color FeedbackColor = new Color(107, 70, 147);

        // Vulpis
        public static string VulpisLogoLink { get; set; } = "https://i.imgur.com/WePnHmR.png";
        public static string VulpisLogoEmote { get; set; } = "<:VulpisEsports:717095280906076240>";
        public static string VulpisDescription { get; set; } = "European Smite organisation focused on organizing regular Conquest, Arena, Assault, Joust & Duel tournaments.";
    }
}
