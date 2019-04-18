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
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Bot : ModuleBase<SocketCommandContext>
    {
        readonly string botIcon = "https://i.imgur.com/8qNdxse.png"; // https://i.imgur.com/AgNocjS.png

        readonly HiRezAPI hirezAPI = new HiRezAPI();
        readonly DominantColor domColor = new DominantColor();

        [Command("help", true)] // Help command
        [Alias("commands", "command", "cmd", "comamands", "h")]
        public async Task Help()
        {
            string prefix = Credentials.botConfig.prefix;
            string desc = $"Default prefix: `{Credentials.botConfig.prefix}`";
            if (Database.GetPrefix(Context.Guild).Count > 0)
            {
                if (Database.GetPrefix(Context.Guild)[0].prefix != "!!")
                {
                    prefix = Database.GetPrefix(Context.Guild)[0].prefix;
                    desc = desc + $"\nCustom prefix: `{prefix}`";
                }
            }
            desc = desc + $"\nYou can also @tag the bot or set a custom prefix by using\n **{prefix}prefix `your-prefix-here`**\n";
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("Available commands");
                author.WithIconUrl(botIcon);
            });
            // Warning 
            embed.WithTitle(":bangbang:This bot is still in development. I try to keep it online for atleast 12 hours a day. I may stop developing it depending on what other bots offer for SMITE stats in the future. If I do so I will notify all servers the bot is in at that time.");
            embed.WithDescription("[Support server](http://discord.gg/hU6MTbQ)\n" + desc);
            embed.WithColor(8190976);
            embed.WithThumbnailUrl(botIcon);
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}stats `username`";
                field.Value = $"Checks the Hi-Rez API for `username` and sends his stats if found.\n**Alias**: `{prefix}stat` `{prefix}pc` `{prefix}st` `{prefix}stata` `{prefix}ст` `{prefix}статс`";
            });
            //embed.AddField(field =>
            //{
            //    field.IsInline = true;
            //    field.Name = $"{prefix}istats `username`";
            //    field.Value = $"**Alias**: `{prefix}istat` `{prefix}ipc` `{prefix}ist` `{prefix}istata` `{prefix}ист` `{prefix}истатс`";
            //});
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}status";
                field.Value = $"Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.\n" +
                $"**Alias**: `{prefix}s` `{prefix}статус` `{prefix}statis` `{prefix}server` `{prefix}servers` `{prefix}se` `{prefix}се`";
            });
            //!statusupdates #channel
            //Automatically SMITE incidents and scheduled maintenances to #channel
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}statusupdates `#channel`";
                field.Value = $"Sends a message when SMITE incidents and scheduled maintenances appear in the status page to `#channel`\n" +
                $"**Alias**: `{prefix}statusupd` `{prefix}su`";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = $"{prefix}stopstatusupdates";
                field.Value = $"Stops sending messages from the SMITE status page.\n" +
                $"**Alias**: `{prefix}ssu`";
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
            embed.WithFooter(footer =>
            {
                footer.Text = "If something isn't working properly, its probably because the bot is still in development.";
            });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("prefix")] // Custom Prefix
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix([Remainder] string prefix)
        {
            await Database.SetPrefix(Context.Guild.Id, Context.Guild.Name, prefix);
            // Consider adding a check if the prefix was set successfully.
            await Context.Channel.SendMessageAsync($"Prefix for **{Context.Guild.Name}** set to `{prefix}`");
        }

        [Command("statusupdates")]
        [Alias("statusupd", "su")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetStatusUpdatesChannel(SocketChannel message)
        {
            await SetNotifChannel(Context.Guild.Id, Context.Guild.Name, message.Id);
            SocketTextChannel channel = Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(message.Id);
            try
            {
                await channel.SendMessageAsync($":white_check_mark: {channel.Mention} is now set to receive notifications about SMITE Server Status updates.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Missing Permissions"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Send Messages** permission for {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages** and **Use External Emojis** permissions in {channel.Mention}.");
                }
                else if (ex.Message.Contains("Missing Access"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Access** to {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages** and **Use External Emojis** permissions in {channel.Mention}.");
                }
                else
                {
                    await ReplyAsync(":warning: Something went wrong. This error was reported to the bot creator and will soon be checked.");
                    await ErrorTracker.SendError($"**Error in StatusUpdates command**\n" +
                        $"{ex.Message}\n**Message: **{message}\n" +
                        $"**Server: **{Context.Guild.Name}[{Context.Guild.Id}]\n" +
                        $"**Channel: **{Context.Channel.Name}[{Context.Channel.Id}]");
                }
            }
        }

        [Command("stopstatusupdates")]
        [Alias("ssu")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task StopStatusUpdates()
        {
            await StopNotifs(Context.Guild.Id);

            await ReplyAsync($"**{Context.Guild.Name}** will no longer receive SMITE Server Status updates.");
        }

        [Command("ping")]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync(Context.Client.Latency.ToString() + " ms");
        }

        // Owner Commands

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

        [Command("botstats", true)]
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
                field.Name = "Servers";
                field.Value = Connection.Client.Guilds.Count;

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Uptime";
                field.Value = GetUptime();

            });
            embed.AddField(field =>
            {
                field.IsInline = false;
                field.Name = $"{pingResArr[0]} Statistics";
                field.Value = "\u2015\u2015\u2015\u2015\u2015\u2015\u2015\u2015";
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Active Sessions";
                field.Value = dataUsed[0].Active_Sessions;

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Total Requests Today";
                field.Value = dataUsed[0].Total_Requests_Today;

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Total Sessions Today";
                field.Value = dataUsed[0].Total_Sessions_Today;

            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Concurrent Sessions";
                field.Value = dataUsed[0].Concurrent_Sessions;

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

        [Command("updategods", true)]
        [Alias("ug")]
        [RequireOwner]
        public async Task UpdateGodsColors()
        {
            domColor.DoAllGodColors();

            await ReplyAsync("Done!:shrug:");
        }

        [Command("invite")]
        [RequireOwner]
        public async Task InviteLink()
        {
            await Context.Channel.SendMessageAsync("https://discordapp.com/api/oauth2/authorize?client_id=454145330347376651&permissions=262144&scope=bot");
        }

        [Command("testupdates")]
        [RequireOwner]
        public async Task TestUpdates([Remainder]string message)
        {
            var nz = GetServerStatusUpdates(message);
            await ReplyAsync(GetServerStatusUpdates(message)[0]);
            //await StatusNotifier.SendNotifs(message);
        }

        private static string GetUptime()
        {
            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "";

            if (time.Days != 0)
            {
                str += $"**{time.Days}** days, ";
            }

            if (time.Hours != 0)
            {
                str += $"**{time.Hours}** hours, ";
            }

            if (time.Minutes != 0)
            {
                str += $"**{time.Minutes}** minutes, ";
            }

            if (time.Seconds != 0)
            {
                str += $"**{time.Seconds}** seconds";
            }

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
