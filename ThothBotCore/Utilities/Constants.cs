using Discord;
using System.Collections.Generic;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Utilities
{
    public class Constants
    {
        public static readonly string botIcon = "https://i.imgur.com/8qNdxse.png";
        public static readonly string SmiteBolt = "https://i.imgur.com/QqXLx5U.png";
        public static readonly ulong OwnerID = 171675309177831424;
        public static readonly ulong SupportServerID = 518408306415632384;
        public static readonly string SupportServerInvite = "https://discord.gg/hU6MTbQ";
        public static readonly string JoinedMessage = ":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can set a custom prefix for your server with {Credentials.botConfig.prefix}prefix `your-prefix-here`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`\n" +
                $":small_orange_diamond:Please make sure I have **View Channel**, **Send Messages**, **Read Messages**, **Embed Links** and **Use External Emojis** in the channels you would like me to respond to commands.";
        public static readonly string FailedToSendJoinedMessage = "Couldn't send JoinedMessage to the Guild.";
        public static readonly string DefaultPrefixMessage = $"My default prefix is `{Credentials.botConfig.prefix}`";
        public static readonly string NotLinked = "This Discord user has not linked their Discord and SMITE accounts. To link your Discord and SMITE accounts, use `!!link` and follow the instructions.";
        public static readonly string APIEmptyResponse = "Sorry, the SmiteAPI sent an empty response.\nTry again later.";
        public static readonly Emoji CheckMarkEmoji = new("✅");
        public static readonly Color DefaultBlueColor = new(85, 172, 238);
        public static readonly Color VulpisColor = new(233, 78, 26);
        public static readonly Color ErrorColor = new(255, 148, 148);
        public static readonly Color FeedbackColor = new(107, 70, 147);
        public static readonly Color SPLColor = new(255, 194, 67);
        public static List<Models.TipsModel> TipsList = MongoConnection.GetAllTips();
        public static List<Models.CommunityModel> CommList = MongoConnection.GetAllCommunities();

        // Vulpis
        public static readonly string VulpisLogoLink = "https://i.imgur.com/WePnHmR.png";

        public static void ReloadConstants()
        {
            TipsList = MongoConnection.GetAllTips();
            CommList = MongoConnection.GetAllCommunities();
        }
    }
}
