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
using ThothBotCore.Storage.Models;
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
            if (GetServerConfig(Context.Guild).Result.Count > 0)
            {
                if (GetServerConfig(Context.Guild).Result[0].prefix != "!!")
                {
                    prefix = GetServerConfig(Context.Guild).Result[0].prefix;
                    desc = desc + $"\nCustom prefix for this server: `{prefix}`";
                }
            }
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author.WithName("Available commands");
                author.WithIconUrl(botIcon);
            });
            embed.WithColor(new Color(85, 172, 238));
            embed.WithTitle(":bangbang:Unfortunately on 4th June 2019 all custom prefixes and status channels were reset. Please set them again.");
            embed.WithDescription("[Support server](http://discord.gg/hU6MTbQ)\n" + desc);
            embed.AddField(x =>
            {
                x.Name = ":zap:SMITE";
                x.Value = $":white_small_square:`{prefix}stats PlayerName` - Display stats for PlayerName\nAlias: `{prefix}stat` `{prefix}pc` `{prefix}st` `{prefix}stata` `{prefix}ст` `{prefix}статс` `{prefix}ns`\n" +
                $":white_small_square:`{prefix}status` - Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.\nAlias: `{prefix}s` `{prefix}статус` `{prefix}statis` `{prefix}server` `{prefix}servers` `{prefix}se` `{prefix}се`\n" +
                $":white_small_square:`{prefix}statusupdates #channel` - Sends a message when SMITE incidents and scheduled maintenances appear in the status page to `#channel`\nAlias: `{prefix}statusupd` `{prefix}su`\n" +
                $":white_small_square:`{prefix}stopstatusupdates` - Stops sending messages from the SMITE status page.\nAlias: `{prefix}ssu`\n" +
                $":white_small_square:`{prefix}god GodName` - Gives you information about `GodName`.\nAlias: `{prefix}g` `{prefix}gods`\n" +
                $":white_small_square:`{prefix}rgod` - Gives you a random God.\nAlias: `{prefix}rg` `{prefix}randomgod` `{prefix}random`";
                x.IsInline = false;
            });
            embed.AddField(x =>
            {
                x.Name = ":robot:Bot";
                x.Value = $":white_small_square:`{prefix}help` - List of all available commands.\nAlias: `{prefix}h` `{prefix}commands` `{prefix}command` `{prefix}cmd` `{prefix}comamands`\n" +
                $":white_small_square:`{prefix}prefix your-prefix-here` - Set custom prefix for your server.\n" +
                $":white_small_square:`{prefix}botstats` - Bot statistics, invite link, support server etc.\nAlias: `{prefix}about` `{prefix}botinfo` `{prefix}info` `{prefix}bi`";
                x.IsInline = false;
            });
            embed.WithFooter(footer =>
            {
                footer.Text = "If something isn't working properly, its probably because the bot is still in development.";
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("botinfo", true)]
        [Alias("bi", "botstats", "about", "info")]
        public async Task BotInfoCommand()
        {
            int totalUsers = 0;
            foreach (var guild in Context.Client.Guilds)
            {
                totalUsers = totalUsers + guild.Users.Count;
            }
            string patch = "";
            try
            {
                string json = await hirezAPI.GetPatchInfo();
                HiRezAPI.PatchInfo patchInfo = JsonConvert.DeserializeObject<HiRezAPI.PatchInfo>(json);
                patch = patchInfo.version_string;
            }
            catch (Exception ex)
            {
                patch = "n/a";
                await ErrorTracker.SendError($"Error in PatchInfo from **botinfo** command.\n{ex.Message}");
            }

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("Statistics for Thoth")
                    .WithIconUrl(botIcon);
            });
            embed.WithDescription("Creator: EasyThe#2836");
            embed.WithColor(new Color(85, 172, 238));
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Uptime";
                field.Value = GetUptime();
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Servers";
                field.Value = Connection.Client.Guilds.Count;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Users";
                field.Value = totalUsers;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Smite Patch Version";
                field.Value = patch;
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Players";
                field.Value = PlayersInDbCount()[0];
            });
            embed.AddField(field =>
            {
                field.IsInline = true;
                field.Name = "Commands Run";
                field.Value = Global.CommandsRun;
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Bot Invite";
                x.Value = $"[Invite](https://discordapp.com/api/oauth2/authorize?client_id=454145330347376651&permissions=537185344&scope=bot)";
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Support Server";
                x.Value = $"[Invite](https://discord.gg/hU6MTbQ)";
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Donate";
                x.Value = $"[PayPal](https://www.paypal.com/donate/?token=lIMLgua4KDmtcUhJhFR8y0VDZOlTt3D9qlNBMV-GGKPlsVc9mAONvfO88H4Xh7rOufPn3G)";
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("prefix")] // Custom Prefix
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix([Remainder] string prefix)
        {
            if (prefix.Length > 5)
            {
                await ReplyAsync("This prefix is too long.");
            }
            else
            {
                await Database.SetPrefix(Context.Guild.Id, Context.Guild.Name, prefix);
                // Consider adding a check if the prefix was set successfully.
                await Context.Channel.SendMessageAsync($"Prefix for **{Context.Guild.Name}** set to `{prefix}`");
            }
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
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.Contains("Missing Access"))
                {
                    await Context.Channel.SendMessageAsync($":warning: I am missing **Access** to {channel.Mention}\n" +
                        $"Please make sure I have **Read Messages, Send Messages**, **Use External Emojis** and **Embed Links** permissions in {channel.Mention}.");
                }
                else if (ex.Message.ToLowerInvariant().Contains("multiple matches"))
                {
                    await Context.Channel.SendMessageAsync("Multiple matches found. Please #mention the channel.");
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

        [Command("stopstatusupdates", true)]
        [Alias("ssu")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task StopStatusUpdates()
        {
            await StopNotifs(Context.Guild.Id);

            await ReplyAsync($"**{Context.Guild.Name}** will no longer receive SMITE Server Status updates.");
        }

        [Command("settimezone")]
        [Alias("stz")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetTimeZone([Remainder] string value)
        {
            List<TimeZoneInfo> allTimeZones = new List<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
            string timezone = allTimeZones.Find(x => x.DisplayName.Contains(Text.ToTitleCase(value))).ToSerializedString();

            DateTime now = DateTime.UtcNow;

            // Saving to DB
            await Database.SetTimeZone(Context.Guild, timezone);

            // Deserialize timezone string from db
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FromSerializedString(timezone);

            // utc to timezone
            DateTime timezoned = TimeZoneInfo.ConvertTimeFromUtc(now, timeZoneInfo);

            await ReplyAsync($"UTC Now: {now}\n" +
                $"In your time: {timezoned}\n" +
                $"{Database.GetTimeZone(Context.Guild.Id).Result[0]}");
        }

        [Command("ping", true)]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync(Context.Client.Latency.ToString() + " ms");
        }

        // Owner Commands

        [Command("ibasikuchimaikata")]
        public async Task StopSSUTimer()
        {
            var user = Context.User;
            await StatusTimer.StopServerStatusTimer($"{user.Username}#{user.Discriminator} stopped SSUTimer.");

            await ReplyAsync("Gotovo, ms. :kissing:");
        }

        [Command("sg")]
        [Summary("Sets a'Game' for the bot :video_game: (Accessible only by the bot owner)")]
        [RequireOwner]
        public async Task SetGame([Remainder] string game)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(game);
            await Context.Channel.SendMessageAsync($"Successfully set the game to '**{game}**'");
            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm, d.MM.yyyy]")}: Game was changed to {game}");
        }

        [Command("bs", true)]
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

        [Command("dg", true)] // Working!
        [RequireOwner]
        public async Task DownloadGods()
        {
            string json = await hirezAPI.GetGods();
            List<Gods.God> gods = JsonConvert.DeserializeObject<List<Gods.God>>(json);
            await SaveGods(gods);
            string newjson = JsonConvert.SerializeObject(gods, Formatting.Indented);
            await UpdateGodsColors(); // not sure if this line works
            await ReplyAsync("done");
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

            if (time.Seconds != 0)
            {
                str += $"{time.Seconds}s";
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
