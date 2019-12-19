using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.Player;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Bot : InteractiveBase<SocketCommandContext>
    {
        readonly string botIcon = "https://i.imgur.com/8qNdxse.png"; // https://i.imgur.com/AgNocjS.png
        private const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

        readonly HiRezAPI hirezAPI = new HiRezAPI();
        readonly DominantColor domColor = new DominantColor();
        static Random rnd = new Random();

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
            embed.WithColor(Constants.DefaultBlueColor);
            //embed.WithTitle($":new:You can now check item information with `{prefix}item itemname`");
            embed.WithDescription("[Support server](http://discord.gg/hU6MTbQ)\n" + desc);
            embed.AddField(x =>
            {
                x.Name = ":zap: SMITE Player Stats";
                x.Value = $":small_blue_diamond:`{prefix}stats PlayerName` - Display stats for `PlayerName`\nAlias: `{prefix}stat` `{prefix}pc` `{prefix}st` `{prefix}stata` `{prefix}ст` `{prefix}статс` `{prefix}ns`\n" +
                $":new:`{prefix}link` - Link your Discord and SMITE accounts.";
            });
            embed.AddField(x =>
            {
                x.Name = ":zap: SMITE Servers & Bugs";
                x.Value = $":small_blue_diamond:`{prefix}status` - Checks the [status page](http://status.hirezstudios.com/) for the status of Smite servers.\nAlias: `{prefix}s` `{prefix}статус` `{prefix}statis` `{prefix}server` `{prefix}servers` `{prefix}se` `{prefix}се`\n" +
                $":small_blue_diamond:`{prefix}statusupdates #channel` - Sends a message when SMITE incidents and scheduled maintenances appear in the status page to `#channel`\nAlias: `{prefix}statusupd` `{prefix}su`\n" +
                $":small_blue_diamond:`{prefix}stopstatusupdates` - Stops sending messages from the SMITE status page.\nAlias: `{prefix}ssu`\n" +
                $":small_blue_diamond:`{prefix}trello` - Checks the [SMITE Community Trello Board](https://trello.com/b/d4fJtBlo/smite-community-issues).\nAlias: `{prefix}issues` `{prefix}bugs` `{prefix}board`";
                x.IsInline = false;
            });
            embed.AddField(x =>
            {
                x.Name = ":zap: SMITE Information";
                x.Value = $":small_blue_diamond:`{prefix}gods` - Overall information about the gods in the game and current free god rotation.\n" +
                $":small_blue_diamond:`{prefix}god GodName` - Gives you information about `GodName`.\nAlias: `{prefix}g`\n" +
                $":small_blue_diamond:`{prefix}item ItemName` - Gives you information about `ItemName`.\nAlias: `{prefix}i`\n" +
                $":new:`{prefix}motd` - Information about upcoming MOTDs in the game.\nAlias: `{prefix}motds` `{prefix}мотд` `{prefix}мотдс`";
            });
            embed.AddField(x =>
            {
                x.Name = ":zap: SMITE Fun & Troll";
                x.Value = $":small_blue_diamond:`{prefix}rgod` - Gives you a random God and randomised build.\nAlias: `{prefix}rg` `{prefix}randomgod` `{prefix}random`\n" +
                $":small_blue_diamond:`{prefix}rteam 5` - Gives you `5` random Gods with randomised builds for them.\nAlias: `{prefix}rt` `{prefix}team` `{prefix}ртеам` `{prefix}теам`\n" +
                $":new:`{prefix}rank` - Gives you random ranked division.";
            });
            embed.AddField(x =>
            {
                x.Name = ":robot: Bot";
                x.Value = $":small_blue_diamond:`{prefix}help` - List of all available commands.\nAlias: `{prefix}h` `{prefix}commands` `{prefix}command` `{prefix}cmd` `{prefix}comamands`\n" +
                $":small_blue_diamond:`{prefix}prefix your-prefix-here` - Set custom prefix for your server.\n" +
                $":small_blue_diamond:`{prefix}botstats` - Bot statistics, invite link, support server etc.\nAlias: `{prefix}about` `{prefix}botinfo` `{prefix}info` `{prefix}bi`\n" +
                $":small_blue_diamond:`{prefix}changelog` - Latest changes to ThothBot.";
                x.IsInline = false;
            });
            embed.WithFooter(footer =>
            {
                footer.Text = "If something isn't working properly, its probably because the bot is in semi-active development.";
            });

            try
            {
                await ReplyAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("50013"))
                {
                    try
                    {
                        await ReplyAsync($"I need **Embed Links** permissions in this channel.");
                    }
                    catch (Exception)
                    {
                        IUser user = Connection.Client.GetUser(Context.Message.Author.Id);
                        await user.SendMessageAsync($"I don't have **Send Messages** or **Embed Links** permissions in #{Context.Channel.Name}.");
                    }
                }
            }
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
            embed.WithColor(Constants.DefaultBlueColor);
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

        [Command("link", true, RunMode = RunMode.Async)]
        public async Task LinkAccountsCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.WithDescription("Please enter your ingame name and **make sure your account is not hidden and you are logged in SMITE**\n*you have 120 seconds to respond*");
                embed.WithAuthor(x =>
                {
                    x.Name = "Thoth Account Linking";
                    x.IconUrl = botIcon;
                });
                embed.WithColor(Constants.DefaultBlueColor);
                embed.WithFooter(x => x.Text = "This is not official Hi-Rez linking!");

                var message = await ReplyAsync("", false, embed.Build());
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));

                if (response == null)
                {
                    embed.Description = ":red_circle: **Time is up, linking cancelled**";
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }

                string playerIdJSON = await hirezAPI.GetPlayerIdByName(response.Content);
                var playerIDList = JsonConvert.DeserializeObject<List<PlayerIDbyName>>(playerIdJSON);
                if (playerIDList.Count == 0)
                {
                    embed.Description = $":red_circle: Couldn't find a player with IGN: {response}";
                    await message.ModifyAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                    return;
                }
                string getplayerJSON = await hirezAPI.GetPlayer(playerIDList[0].player_id.ToString());
                var getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);

                string generatedString = GenerateString();
                embed.Author.IconUrl = Context.Message.Author.GetAvatarUrl();

                embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                embed.WithDescription($"<:level:529719212017451008>**Level**: {getplayerList[0].Level}\n" +
                    $":calendar:**Account Created**: {getplayerList[0].Created_Datetime}\n\n" +
                    $"**If this is your account, change your __Personal Status Message__ to: `{generatedString}` (you can Ctrl+C Ctrl+V) so we can be sure it's your account.**\n*You have 120 seconds to perform this action.*\n" +
                    $"\nWhen you are done, write `done` or anything else to cancel.");
                embed.ImageUrl = "https://media.discordapp.net/attachments/528621646626684928/656237343405244416/Untitled-1.png";

                await message.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                embed.ImageUrl = null;
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(120));
                if (response == null || response.Content.ToLowerInvariant() == "done")
                {
                    getplayerJSON = await hirezAPI.GetPlayer(playerIDList[0].player_id.ToString());
                    getplayerList = JsonConvert.DeserializeObject<List<PlayerStats>>(getplayerJSON);
                    if (getplayerList[0].Personal_Status_Message.ToLowerInvariant() == generatedString)
                    {
                        await Database.SetPlayerSpecials(playerIDList[0].player_id,
                            (getplayerList[0].hz_player_name != null ? getplayerList[0].hz_player_name : getplayerList[0].hz_gamer_tag),
                            Context.Message.Author.Id);
                        embed.WithTitle(getplayerList[0].hz_player_name + " " + getplayerList[0].hz_gamer_tag);
                        embed.WithDescription($":tada: Congratulations, you've successfully linked your Discord and SMITE account into the Thoth Database.");
                        embed.ImageUrl = null;

                        await message.ModifyAsync(x =>
                        {
                            x.Embed = embed.Build();
                        });
                    }
                    else
                    {
                        embed.WithDescription($":red_circle: Linking cancelled. {(response==null?"Time is up! " : "")}Your Personal Status Message is `{getplayerList[0].Personal_Status_Message}`");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
                else
                {
                    embed.WithDescription($":red_circle: Linking cancelled.");
                    embed.ImageUrl = null;
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Something went wrong: {ex.Message}");
                await ErrorTracker.SendError($"**LINKING ERROR**\n{ex.Message}\n{ex.StackTrace}\n{ex.InnerException}\n{ex.Source}\n{ex.Data}");
            }
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

        [Command("testupdates")]
        [RequireOwner]
        public async Task TestUpdates([Remainder]string message)
        {
            var nz = GetServerStatusUpdates(message);
            await ReplyAsync(GetServerStatusUpdates(message)[0]);
            //await StatusNotifier.SendNotifs(message);
        }

        [Command("thoth", true)]
        public async Task BasicInfoCommand()
        {
            await ReplyAsync($"My default prefix is `{Credentials.botConfig.prefix}`");
        }

        [Command("pishka")]
        public async Task Pishka()
        {
            string pishka = "";
            var embed = new EmbedBuilder();
            if (Context.Message.Author.Id == 171675309177831424)
            {
                pishka = $"{Context.Message.Author.Username}'s pishka\n8=====================D";
            }
            else
            {
                pishka = $"{Context.Message.Author.Username}'s pishka\n8=D";
            }
            embed.WithTitle("pishka size machine");
            embed.WithDescription(pishka);
            await ReplyAsync("", false, embed.Build());
        }
        
        [Command("changelog")]
        public async Task ChangelogCommand()
        {
            var channel = Connection.Client.GetGuild(518408306415632384).GetTextChannel(567192879026536448);
            var messages = channel.GetMessagesAsync(1).FlattenAsync();

            var nz = messages.Result;

            var embed = new EmbedBuilder();
            embed.Title = "Latest Update of ThothBot";
            foreach (var item in nz)
            {
                embed.Description = item.Content;
            }

            await ReplyAsync("", false, embed.Build());
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
        private static string GenerateString()
        {
            char[] chars = new char[7];
            for (int i = 0; i < 7; i++)
            {
                chars[i] = alphabet[rnd.Next(alphabet.Length)];
            }
            return new string(chars);
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
