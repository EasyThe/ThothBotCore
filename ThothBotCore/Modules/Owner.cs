using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using ThothBotCore.Utilities.Smite;

namespace ThothBotCore.Modules
{
    [RequireOwner]
    public class Owner : InteractiveBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new HiRezAPI();

        [Command("setplayersspec")]
        [Alias("sps")]
        public async Task SetPlayersSpecial(string username, [Remainder]string parameters)
        {
            List<PlayerIDbyName> playerID = JsonConvert.DeserializeObject<List<PlayerIDbyName>>(await hirezAPI.GetPlayerIdByName(username));
            string[] splitParams = parameters.Split(" ");
            for (int i = 0; i < splitParams.Length; i++)
            {
                if (splitParams[i].Contains("discord")) //discord
                {
                    if (splitParams[i + 1].Contains("link"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, Context.User.Id);
                    }
                    else
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, 0);
                    }
                }
                if (splitParams[i].Contains("pro"))
                {
                    if (splitParams[i + 1].Contains("yes"))
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, 1);
                    }
                    else
                    {
                        await Database.SetPlayerSpecials(playerID[0].player_id, username, null, 0);
                    }
                }
            }
        }

        [Command("setstreamer")]
        public async Task SetStreamerInDb([Remainder]string parameters)
        {
            string[] splitParams = parameters.Split(" ");
            List<PlayerIDbyName> playerID = JsonConvert.DeserializeObject<List<PlayerIDbyName>>(await hirezAPI.GetPlayerIdByName(splitParams[1]));
            if (splitParams[0].Contains("add") || splitParams[0].Contains("update"))
            {
                await Database.SetPlayerSpecials(playerID[0].player_id, splitParams[1], null, 1, splitParams[2]);
            }
            else
            {
                await Database.SetPlayerSpecials(playerID[0].player_id, splitParams[1], null, 0);
            }

            var playerspecs = await Database.GetPlayerSpecialsByPlayerID(playerID[0].player_id.ToString());
            var embed = new EmbedBuilder();
            bool b = Convert.ToBoolean(playerspecs[0].streamer_bool);
            embed.WithColor(Constants.DefaultBlueColor);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = playerspecs[0].Name;
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Streamer";
                x.Value = Text.ToTitleCase(b.ToString());
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Streamer Link";
                x.Value = playerspecs[0].streamer_link == "" || playerspecs[0].streamer_link == null ? "n/a" : playerspecs[0].streamer_link;
            });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("insertallguilds")]
        public async Task DoGuilds()
        {
            foreach (var guild in Discord.Connection.Client.Guilds)
            {
                await Database.SetGuild(guild.Id, guild.Name);
            }
            await ReplyAsync("Done!");
        }

        [Command("deleteserverfromdb")]
        public async Task DeleteGuildFromDB(ulong id)
        {
            await Database.DeleteServerConfig(id);

            await ReplyAsync("Should be done :shrug:");
        }

        [Command("updatedb", true, RunMode = RunMode.Async)]
        public async Task UpdateDBFromSmiteAPI()
        {
            // oppaa
            await Utils.UpdateDb(hirezAPI, Context);
        }

        [Command("ae")]
        public async Task AddEmojiToGodCommand(string emoji, [Remainder]string godname)
        {
            await Database.InsertEmojiForGod(godname, emoji);
        }

        [Command("aei")]
        [RequireOwner]
        public async Task AddEmojiToItemCommand(string emoji, [Remainder]string itemName)
        {
            if (itemName.Contains("'"))
            {
                itemName = itemName.Replace("'", "''");
            }
            await Database.InsertEmojiForItem(itemName, emoji);
        }

        [Command("lg")] // Leave Guild
        public async Task LeaveGuild(ulong id)
        {
            await Discord.Connection.Client.GetGuild(id).LeaveAsync();
        }

        [Command("sm")]
        public async Task SendMessageAsOwner(ulong server, ulong channel, [Remainder]string message)
        {
            var chn = Discord.Connection.Client.GetGuild(server).GetTextChannel(channel);

            await chn.SendMessageAsync(message);
            await ReplyAsync("I guess it worked, idk.");
        }

        [Command("getjson")]
        public async Task GetPlayerOwnerCommand(string username)
        {
            string result = "";
            try
            {
                result = await hirezAPI.GetPlayer(username);
                await ReplyAsync(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("2000"))
                {
                    await File.WriteAllTextAsync("testmethod.json", result);
                    await ReplyAsync("Saved as testmethod.json");
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        [Command("setglobalerrormessage", true)]
        [Alias("sgem")]
        public async Task SetGlobalErrorMessageCommand([Remainder]string message = "")
        {
            if (message == "")
            {
                Global.ErrorMessageByOwner = null;
                await ReplyAsync("Message removed successfully." + Global.ErrorMessageByOwner);
                return;
            }
            Global.ErrorMessageByOwner = message;
            await ReplyAsync("Done, boss!");
        }

        [Command("setgameinconfig", true)]
        [Alias("sgic")]
        public async Task SetGameInConfigCommand([Remainder]string text)
        {
            Credentials.botConfig.setGame = text;
            await ReplyAsync("Ready, boss.\n" + Credentials.botConfig.setGame);
        }

        [Command("bs", true)]
        [Summary("Information about the bot accessible only by the bot owner.")]
        public async Task BotStats()
        {
            await hirezAPI.PingAPI();

            string[] pingRePreArr = hirezAPI.pingAPI.Split('"');
            string[] pingResArr = pingRePreArr[1].Split(' ');

            await hirezAPI.DataUsed();

            var dataUsed = JsonConvert.DeserializeObject<List<DataUsed>>(hirezAPI.dataUsed);

            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("Thoth Stats")
                    .WithIconUrl(Constants.botIcon);
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
                    .WithIconUrl(Constants.botIcon);
            });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("sac")]
        [Summary("Sets a'Activity' for the bot (Accessible only by the bot owner)")]
        public async Task SetActivityCommand(string url, [Remainder] string game)
        {
            await Connection.Client.SetGameAsync(game, url);
            await Context.Channel.SendMessageAsync($"Successfully set the activity to '**{game}**'");
            await Reporter.SendError($"{DateTime.Now:[HH:mm, d.MM.yyyy]}: Activity was changed to {game}");
        }

        [Command("sg")]
        [Summary("Sets a'Game' for the bot :video_game: (Accessible only by the bot owner)")]
        public async Task SetGame([Remainder] string game)
        {
            await Connection.Client.SetGameAsync(game);
            await Context.Channel.SendMessageAsync($"Successfully set the game to '**{game}**'");
            await Reporter.SendError($"{DateTime.Now:[HH:mm, d.MM.yyyy]}: Game was changed to {game}");
        }

        [Command("desc")]
        public async Task EmbedDescriptionOwnerCommand([Remainder] string text)
        {
            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(text);
            await ReplyAsync(embed: embed);
        }

        [Command("notes", RunMode = RunMode.Async)]
        public async Task PatchNotesTestCommand(string url)
        {
            try
            {
                var embed = await PatchPageReader.GetPatchEmbed(url);
                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("zxc", RunMode = RunMode.Async)]
        public async Task ReadAllMessagesInChannel()
        {
            var messages = await Context.Channel.GetMessagesAsync(2000).FlattenAsync();
            var allembeds = messages.Where(x => x.Embeds.Count != 0);

            Console.WriteLine($"Found {allembeds.Count()} messages.");
            int count = 0;
            StringBuilder sb = new StringBuilder();
            StringBuilder scount = new StringBuilder();
            StringBuilder sdate = new StringBuilder();
            sb.AppendLine("Count,Date");
            foreach (var message in allembeds.Reverse())
            {
                if (message.Embeds.FirstOrDefault().Color.Value.RawValue == 16777215)
                {
                    // Join
                    count++;
                }
                else
                {
                    // Leave
                    count--;
                }
                scount.Append($"{count-2}, ");
                sdate.Append($"\"{message.Timestamp.ToString("d", CultureInfo.InvariantCulture)}\", ");

                sb.AppendLine($"{count-2},{message.Timestamp.ToString("dd-MM-yyyy")}");
            }
            StringBuilder nzbr = new StringBuilder();
            nzbr.AppendLine(scount.ToString());
            nzbr.AppendLine(sdate.ToString());
            await File.AppendAllTextAsync("spimise.txt", nzbr.ToString());
            await File.AppendAllTextAsync("zxc.csv", sb.ToString());
            Console.WriteLine((count - 2).ToString());
        }

        [Command("users", true, RunMode = RunMode.Async)]
        public async Task GetUsersCommand(ulong id = 0)
        {
            if (id == 0)
            {
                id = Context.Guild.Id;
            }
            var guild = Connection.Client.GetGuild(id);
            var sb = new StringBuilder();
            foreach (var user in guild.Users)
            {
                sb.AppendLine($"{user.Username}#{user.DiscriminatorValue} [{user.Id}]");
            }
            await ReplyAsync(sb.ToString());
        }

        [Command("guild", true, RunMode = RunMode.Async)]
        public async Task CheckGuildOwnerCommand(ulong id = 0)
        {
            if (id == 0)
            {
                id = Context.Guild.Id;
            }
            var embed = new EmbedBuilder();
            var guild = Connection.Client.GetGuild(id);
            var guildinfo = await Database.GetServerConfig(id);
            embed.WithTitle("Database");
            embed.WithDescription($"Name: {guildinfo[0].serverName}\n" +
                $"ID: {guildinfo[0].serverID}\n" +
                $"Prefix: {guildinfo[0].prefix}\n" +
                $"Status Updates Enabled: {guildinfo[0].statusBool}\n" +
                $"Status Updates Channel: {guildinfo[0].statusChannel}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "More";
                x.Value = $"Name: {guild.Name} [{guild.Id}]\n" +
                $"Owner: {guild.Owner}[{guild.OwnerId}]\n" +
                $"Preferred Culture: {guild.PreferredCulture}\n" +
                $"Preferred Locale: {guild.PreferredLocale}\n" +
                $"Channels: {guild.TextChannels.Count}\n" +
                $"Users: {guild.Users.Count}";
            });
            if (guild.IconUrl != null)
            {
                embed.WithThumbnailUrl(guild.IconUrl);
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("searchguild")]
        public async Task SearchGuildCommand([Remainder]string term)
        {
            var guilds = Connection.Client.Guilds;
            var sb = new StringBuilder();
            int count = 0;
            foreach (var g in guilds)
            {
                if (g.Name.ToLowerInvariant().Contains(term.ToLowerInvariant()))
                {
                    sb.AppendLine($"{g.Name} [{g.Id}]");
                    count++;
                }
            }
            await ReplyAsync($"{count} results\n{sb}");
        }

        [Command("checkguilds", true, RunMode = RunMode.Async)]
        public async Task CheckAllGuilds()
        {
            await ReplyAsync("Starting now...");
            var guilds = Connection.Client.Guilds;
            var sb = new StringBuilder();
            int missingCount = 0;

            foreach (var guild in guilds)
            {
                var config = await Database.GetServerConfig(guild.Id);
                string gname;

                if (config.Count == 0)
                {
                    gname = guild.Name;
                    if (gname.Contains("'"))
                    {
                        gname.Replace("'", "''");
                    }
                    Console.WriteLine($"{guild.Name} [{guild.Id}]");
                    await Database.SetGuild(guild.Id, gname);
                    missingCount++;
                    sb.AppendLine($"{guild.Name} [{guild.Id}]");
                }
            }
            await ReplyAsync($"Finished!\n" +
                $"Missing: {missingCount}\n" +
                $"{sb}");
        }

        [Command("shtc")]
        public async Task StopHourlyTimerCommand()
        {
            await GuildsTimer.StopHourlyTimer();
            await ReplyAsync("done i guess");
        }

        [Command("temb")]
        public async Task TestingEmbeds()
        {
            var date = new DateTime(2020, 5, 31, 18, 00, 00);
            var embed = new EmbedBuilder();
            embed.WithTitle("Free Game!");
            embed.WithDescription("whatever, whatever");
            embed.WithFooter(x =>
            {
                x.Text = "Offer ends in: ";
            });
            embed.WithTimestamp(date);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("testtt")]//test command idk
        public async Task StatCordTestCommand([Remainder]string name)
        {
            // add this to EmbedHandler.Loading() if it works
            var embed = await EmbedHandler.BuildDescriptionEmbedAsync("<:Hidden:591666971234402320>Seitr is hidden!", 254, 255, 255);
            await ReplyAsync(embed: embed);
        }

        [Command("sendDM")]
        public async Task SendDMasOwner(ulong userID, [Remainder] string message)
        {
            IUser targetUser = Connection.Client.GetUser(userID);
            var channel = await targetUser.GetOrCreateDMChannelAsync();
            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.Message.Author);
            embed.Author.Url = Constants.SupportServerInvite;
            embed.WithColor(Constants.FeedbackColor);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "-";
                x.Value = $"If you want to answer to this message you can use the `{Credentials.botConfig.prefix}feedback` command or " +
                $"[join the support server]({Constants.SupportServerInvite}) of Thoth and chat with the developer directly!";
            });
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = "This message was sent from the developer of the bot";
                x.IconUrl = Constants.botIcon;
            });
        }

        [Command("cee", true, RunMode = RunMode.Async)]
        public async Task CreateAnEmbedAsOwnerCommand()
        {
            var embed = new EmbedBuilder();
            var mainMessage = await ReplyAsync("Author?", embed: embed.Build());
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            
            // With author
            if (response.Content.ToLowerInvariant().Contains("y"))
            {
                await response.DeleteAsync();
                await mainMessage.ModifyAsync(x =>
                {
                    x.Content = "Provide Author name: ";
                });
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                embed.WithAuthor(x =>
                {
                    x.IconUrl = Constants.VulpisLogoLink;
                    x.Name = response.Content;
                });
                embed.WithColor(Constants.VulpisColor);
                await mainMessage.ModifyAsync(x =>
                {
                    x.Embed = embed.Build();
                });
                await response.DeleteAsync();
            }

            // Description
            await mainMessage.ModifyAsync(x =>
            {
                x.Content = "**Description?**";
            });
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response.Content.ToLowerInvariant().Contains("y"))
            {
                await response.DeleteAsync();
                await mainMessage.ModifyAsync(x =>
                {
                    x.Content = "**Write Description now:**";
                });
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            }
            await response.DeleteAsync();

            await mainMessage.ModifyAsync(x =>
            {
                x.Content = "**Content**";
            });
            // Content
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            await mainMessage.ModifyAsync(x =>
            {
                x.Content = response.Content;
            });
            await response.DeleteAsync();
        }

        [Command("addfield", true, RunMode = RunMode.Async)]
        public async Task AddFieldToExistingEmbedCommand(ulong messageID)
        {
            
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
