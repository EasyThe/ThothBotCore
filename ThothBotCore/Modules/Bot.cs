using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Bot : ModuleBase<SocketCommandContext>
    {
        readonly string prefix = Credentials.botConfig.prefix;
        readonly string botIcon = "https://i.imgur.com/AgNocjS.png";

        readonly HiRezAPI hirezAPI = new HiRezAPI();
        readonly DominantColor domColor = new DominantColor();

        [Command("help")] // Help command
        [Alias("commands", "command", "cmd", "comamands", "h")]
        public async Task Help()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("Available commands");
                author.WithIconUrl(botIcon);
            });
            embed.WithDescription($"Default prefix: `{prefix}`\nInstead of using prefix, you can also @tag the bot.");
            embed.WithColor(8190976);
            embed.WithThumbnailUrl(botIcon);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}stats `username`";
                field.Value = $"**Alias**: `{prefix}stat` `{prefix}pc` `{prefix}st` `{prefix}stata` `{prefix}ст` `{prefix}статс`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}istats `username`";
                field.Value = $"**Alias**: `{prefix}istat` `{prefix}ipc` `{prefix}ist` `{prefix}istata` `{prefix}ист` `{prefix}истатс`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}status";
                field.Value = $"Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.\n**Alias**: `{prefix}s` `{prefix}статус` `{prefix}statis` `{prefix}server` `{prefix}servers` `{prefix}se` `{prefix}се`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}god `godname`";
                field.Value = $"Gives you information about `godname`.\n**Alias**: `{prefix}g` `{prefix}gods`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}rgod";
                field.Value = $"Gives you a random God.\n**Alias**: `{prefix}rg` `{prefix}randomgod` `{prefix}random`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}help";
                field.Value = $"Information about all available commands for the bot.\n**Alias**: `{prefix}h` `{prefix}commands` `{prefix}command` `{prefix}cmd` `{prefix}comamands`";
            });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("prefix")] // Custom Prefix
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix([Remainder] string prefix)
        {
            Database.SetPrefix(Context.Guild.Id, prefix);

            await Context.Channel.SendMessageAsync($"Prefix for **{Context.Guild.Name}** set to \'{prefix}\'!");
        }

        [Command("SetGame")]
        [Alias("sg")]
        [Summary("Sets a'Game' for the bot :video_game: (Accessible only by the bot owner)")]
        [RequireOwner]
        public async Task SetGame([Remainder] string game)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(game);
            await Context.Channel.SendMessageAsync($"Successfully set the game to '**{game}**'");
            Console.WriteLine($"{DateTime.UtcNow.ToString("[HH:mm, d.MM.yyyy]")}: Game was changed to {game}");
        }

        [Command("botstats")]
        [Alias("bs")]
        [Summary("Information about the bot accessible only by the bot owner.")]
        [RequireOwner]
        public async Task BotStats()
        {
            await hirezAPI.PingAPI();

            string[] pingRePreArr = hirezAPI.pingAPI.Split('"');
            string[] pingResArr = pingRePreArr[1].Split(' ');

            await hirezAPI.DataUsed();

            List<DataUsed> dataUsed = JsonConvert.DeserializeObject<List<DataUsed>>(hirezAPI.dataUsed);

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("Thoth Stats")
                    .WithIconUrl(botIcon);
            });
            embed.WithColor(new Color(0, 255, 0));
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Servers");
                field.Value = (Connection.Client.Guilds.Count);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Uptime");
                field.Value = (GetUptime());

            });
            embed.AddField(field =>
            {
                field.IsInline = false;
                field.Name = $"{pingResArr[0]} Statistics";
                field.Value = $"======================================================";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Active Sessions");
                field.Value = (dataUsed[0].Active_Sessions);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Total Requests Today");
                field.Value = (dataUsed[0].Total_Requests_Today);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Total Sessions Today");
                field.Value = (dataUsed[0].Total_Sessions_Today);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Concurrent Sessions");
                field.Value = (dataUsed[0].Concurrent_Sessions);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Request Limit Daily");
                field.Value = (dataUsed[0].Request_Limit_Daily);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Session Cap");
                field.Value = (dataUsed[0].Session_Cap);

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = ("Session Time Limit");
                field.Value = (dataUsed[0].Session_Time_Limit);

            });
            embed.WithFooter(footer =>
            {
                footer
                    .WithText($"{pingResArr[0]} {pingResArr[1]}. {pingResArr[2]} & Discord.NET (API version: {DiscordConfig.APIVersion} | Version: {DiscordConfig.Version})")
                    .WithIconUrl(botIcon);
            });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("updategods")]
        [Alias("ug")]
        [RequireOwner]
        public async Task UpdateGodsColors()
        {
            domColor.DoAllGodColors();

            await ReplyAsync("Done!:shrug:");
        }

        [Command("ping")]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync(Context.Client.Latency.ToString() + " ms");
        }

        [Command("invite")]
        [RequireOwner]
        public async Task InviteLink()
        {
            await Context.Channel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=454145330347376651&permissions=262144&scope=bot");
        }

        private string GetUptime()
        {
            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "";

            if (time.Days != 0)
                str += $"**{time.Days}** days, ";

            if (time.Hours != 0)
                str += $"**{time.Hours}** hours, ";

            if (time.Minutes != 0)
                str += $"**{time.Minutes}** minutes, ";

            if (time.Seconds != 0)
                str += $"**{time.Seconds}** seconds.";

            return str;
        }

        private class DataUsed
        {
            public int Active_Sessions { get; set; }
            public int Concurrent_Sessions { get; set; }
            public int Request_Limit_Daily { get; set; }
            public int Session_Cap { get; set; }
            public int Session_Time_Limit { get; set; }
            public int Total_Requests_Today { get; set; }
            public int Total_Sessions_Today { get; set; }
            public object ret_msg { get; set; }
        }
    }
}
