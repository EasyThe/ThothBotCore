using Discord.Commands;
using System.Diagnostics.Metrics;
using Discord.Interactions;

namespace ThothBotCore
{
    public static class Global
    {
        public static int CommandsRun { get; set; } = 1;
        public static string ErrorMessageByOwner { get; set; } = "";
        internal static Meter Metrics { get; set; }
        internal static CommandService commandService { get; set; }
        internal static InteractionService interactionService { get; set; }
        // 0-ChannelID 1-MessageID 2-GuildID
        public static ulong[] TourneyTimerIDs { get; set; }
        public static string TourneyName { get; set; }
    }
}
