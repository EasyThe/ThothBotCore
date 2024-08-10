using Discord;
using System.Collections.Generic;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
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
        public static readonly string DefaultPrefixMessage = $"My default prefix is `{Credentials.botConfig.prefix}` but I will be switching to using ONLY `/` slash commands by the end of August 2022.";
        public static readonly string NotLinked = "This Discord user has not linked their Discord and SMITE accounts. To link your Discord and SMITE accounts, use `/link` and follow the instructions.";
        public static readonly string APIEmptyResponse = "Sorry, the SmiteAPI sent an empty response.\nTry again later.";
        public static readonly Emoji CheckMarkEmoji = new("✅");
        public static readonly Color DefaultBlueColor = new(85, 172, 238);
        public static readonly Color VulpisColor = new(233, 78, 26);
        public static readonly Color ErrorColor = new(255, 0, 0);
        public static readonly Color FeedbackColor = new(107, 70, 147);
        public static readonly Color SPLColor = new(255, 194, 67);
        public static readonly Color SuccessColor = new(67, 181, 129);
        public static readonly Color FeedsColor = new(51, 212, 163);
        public static readonly Color SMITE2GoldColor = new(242, 197, 114);
        public static readonly Color SMITE2BlueColor = new(38, 46, 66);
        public static List<TipsModel> TipsList = MongoConnection.GetAllTips();
        public static List<CommunityModel> CommList = MongoConnection.GetAllCommunities();
        public static BotSettingsModel BotSettings = MongoConnection.GetBotSettings();
        public static Dictionary<string, string> SmiteQueues = BotSettings.SmiteQueues;
        public static string[] Placeholders = BotSettings.Placeholders;
        public static HashSet<Gods.God> GodsHashSet = new(MongoConnection.GetAllGods());
        public static HashSet<GetItems.Item> ItemsHashSet = new(MongoConnection.GetAllActiveItems());
        public static HashSet<Gods.God> SMITE2GodsHashSet = new(MongoConnection.GetAllGods(true));

        // Vulpis
        public static readonly string VulpisLogoLink = "https://i.imgur.com/WePnHmR.png";

        public static void ReloadConstants()
        {
            TipsList = MongoConnection.GetAllTips();
            CommList = MongoConnection.GetAllCommunities();
            BotSettings = MongoConnection.GetBotSettings();
            GodsHashSet = new HashSet<Gods.God>(MongoConnection.GetAllGods());
            ItemsHashSet = new HashSet<GetItems.Item>(MongoConnection.GetAllActiveItems());
            SMITE2GodsHashSet = new(MongoConnection.GetAllGods(true));
        }
    }
}
