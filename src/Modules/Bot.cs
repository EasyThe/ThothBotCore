using Discord.Commands;
using Fergun.Interactive;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Modules
{
    public class Bot : ModuleBase<SocketCommandContext>
    {
        readonly HiRezAPI hirezAPI = new();
        private const string slash = "⚠Thoth is switching to Slash Commands! Please use ";
        public InteractiveService Interactive { get; set; }

        [Command("thoth", true)]
        public async Task BasicInfoCommand()
        {
            await ReplyAsync($"My default prefix is `{Credentials.botConfig.prefix}`\n{slash}`/` from now on!");
        }

        private static string GetUptime()
        {
            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "";

            if (time.Days != 0)
            {
                str += $"{time.Days}d ";
            }

            if (time.Hours != 0)
            {
                str += $"{time.Hours}h ";
            }

            if (time.Minutes != 0)
            {
                str += $"{time.Minutes}m ";
            }

            if (time.Seconds != 0 && time.Hours !>= 0 && time.Days !<= 0)
            {
                str += $"{time.Seconds}s";
            }

            return str;
        }
    }
}
