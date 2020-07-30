using Discord.Addons.Interactive;
using Discord.Commands;
using System.Collections.Generic;

namespace ThothBotCore
{
    public static class Global
    {
        public static int CommandsRun { get; set; } = 1;
        public static Dictionary<string, string> CommandsStats { get; set; }
        public static string ErrorMessageByOwner { get; set; }
        internal static CommandService commandService { get; set; }
        internal static InteractiveService InteractiveService { get; set; }
    }
}
