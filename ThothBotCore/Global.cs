using Discord.Addons.Interactive;
using Discord.Commands;

namespace ThothBotCore
{
    public static class Global
    {
        public static int CommandsRun { get; set; } = 1;
        public static string ErrorMessageByOwner { get; set; } = null;
        internal static CommandService commandService { get; set; }
        internal static InteractiveService InteractiveService { get; set; }
    }
}
