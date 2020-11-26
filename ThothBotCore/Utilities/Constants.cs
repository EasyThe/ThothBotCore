using Discord;
using System.Collections.Generic;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
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
        public static Color VulpisColor { get; set; } = new Color(233, 78, 26);
        public static Color ErrorColor { get; set; } = new Color(255, 148, 148);
        public static Color FeedbackColor = new Color(107, 70, 147);
        public static List<Models.TipsModel> TipsList { get; set; } = MongoConnection.GetAllTips();
        public static List<Models.CommunityModel> CommList { get; set; } = MongoConnection.GetAllCommunities();

        // Vulpis
        public static string VulpisLogoLink { get; set; } = "https://i.imgur.com/WePnHmR.png";

        public static void ReloadConstants()
        {
            TipsList = MongoConnection.GetAllTips();
            CommList = MongoConnection.GetAllCommunities();
        }
    }
}
