using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Fergun.Interactive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    [RequireOwner]
    public class Owner : ModuleBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new();
        public InteractiveService Interactive { get; set; }

        [Command("migratetofeeds", true, RunMode = RunMode.Async)]
        public async Task MigrateFromSqliteToMongoFeeds()
        {
            StringBuilder sb = new();
            var oldDb = await Database.GetNotifChannels();
            foreach (var oldGuild in oldDb)
            {
                try
                {
                    var guild = Connection.Client.GetGuild(oldGuild._id);
                    var chnl = guild.GetTextChannel(oldGuild.statusChannel);
                    var webhook = await chnl.CreateWebhookAsync("Thoth Feeds");
                    if (webhook != null)
                    {
                        var guildSettings = new GuildSettingsModel()
                        {
                            _id = oldGuild._id,
                            Feeds = new List<GuildSettingsModel.Feed>()
                            {
                                new GuildSettingsModel.Feed
                                {
                                    ChannelID = oldGuild.statusChannel,
                                    Type = GuildSettingsModel.FeedType.ServerStatus,
                                    WebhookID = webhook.Id,
                                    WebhookToken = webhook.Token
                                }
                            }
                        }; 
                        await MongoConnection.SaveGuildSettingsAsync(guildSettings);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Guild: {oldGuild._id} - {ex.Message}");
                    if (ex.Message.Contains("Permission"))
                    {
                        sb.AppendLine(oldGuild._id.ToString());
                        var guildSettings = new GuildSettingsModel()
                        {
                            _id = oldGuild._id,
                            Feeds = new List<GuildSettingsModel.Feed>()
                            {
                                new GuildSettingsModel.Feed
                                {
                                    ChannelID = oldGuild.statusChannel,
                                    Type = GuildSettingsModel.FeedType.ServerStatus
                                }
                            }
                        };
                        await MongoConnection.SaveGuildSettingsAsync(guildSettings);
                    }
                }
            }
            await ReplyAsync("Migration complete!");
            if (sb.Length != 0)
            {
                await ReplyAsync($"Servers failed to migrate due to no permissions:\n{sb}");
            }
        }

        [Command("testwebhook", RunMode = RunMode.Async)]
        public async Task TestWebhookCOmmand([Remainder]string text)
        {
            try
            {
                var em = await EmbedHandler.BuildDescriptionEmbedAsync(text);
                await Feeds.Feeder.SendServerStatusWebhooks(em, GuildSettingsModel.FeedType.ServerStatus, "Testing Webhook");
                await ReplyAsync(":pray:");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("testcal", RunMode = RunMode.Async)]
        public async Task TestCalendarCall()
        {
            try
            {
                //var result = await APIInteractions.GetEsportsEventID();
                //var split = result.Split("eventId:");
                //var hopefullyID = split[1].Split('"');
                //Console.WriteLine(hopefullyID[1]);
                await ReplyAsync(":pray:");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("reinsertitems", true, RunMode = RunMode.Async)]
        public async Task ReinsertItemsCommandAsync()
        {          
            var emoteGuilds = new List<SocketGuild>
            {
                Connection.Client.GetGuild(592787276795347056),
                Connection.Client.GetGuild(595336005180063797),
                Connection.Client.GetGuild(597444275944292372),
                Connection.Client.GetGuild(772225334652829706),
                Connection.Client.GetGuild(772225406195466241),
                Connection.Client.GetGuild(772225707560140831)
            };

            // Run ONLY IF 100% SURE
            // Deleting all emotes from the emote guilds
            if (true)
            {
                foreach (var guild in emoteGuilds)
                {
                    if (guild.Emotes.Count != 0)
                    {
                        foreach (var emote in guild.Emotes)
                        {
                            Text.WriteLine(emote.Id.ToString());
                            await guild.DeleteEmoteAsync(emote);
                        }
                    }
                }
                Text.WriteLine("Deleted all emotes.");
            }
            var items = MongoConnection.GetAllItems();
            try
            {
                foreach (var item in items)
                {
                    if (item.ActiveFlag == "y")
                    {
                        string[] splitLink = item.itemIcon_URL.Split('/');

                        if (!Directory.Exists("Storage/Items"))
                        {
                            Directory.CreateDirectory("Storage/Items");
                        }

                        // Downloading the image
                        try
                        {
                            using WebClient client = new();
                            client.DownloadFile(new Uri(item.itemIcon_URL), $@"./Storage/Items/{splitLink[5]}");
                        }
                        catch (Exception ex)
                        {
                            Text.WriteLine(ex.Message);
                        }

                        // Adding the image as emoji in emojiguilds
                        foreach (var guild in emoteGuilds)
                        {
                            if (guild.Emotes.Count != 50)
                            {
                                string emojiname = item.DeviceName.Trim().Replace("\'", "").ToLowerInvariant();
                                emojiname = Regex.Replace(emojiname, @"\s+", "");
                                var image = new Image($"Storage\\Items\\{splitLink[5]}");
                                Text.WriteLine(emojiname);
                                var insertedEmote = await guild.CreateEmoteAsync(emojiname, image);
                                image.Dispose();
                                // Saving to DB
                                item.Emoji = $"<{insertedEmote.Name}:{insertedEmote.Id}>";
                                await MongoConnection.SaveItemAsync(item);
                                break;
                            }
                            else
                            {
                                Text.WriteLine($"{guild.Name} is full.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
            }
            await ReplyAsync("Ready");
        }

        [Command("sm", RunMode = RunMode.Async)]
        public async Task SendMessageAsOwner([Name("ServerID")] ulong server, [Name("ChannelID")] ulong channel, [Remainder] string message)
        {
            var chn = Connection.Client.GetGuild(server).GetTextChannel(channel);
            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.Message.Author);
            embed.Author.Url = Constants.SupportServerInvite;
            embed.WithColor(Constants.FeedbackColor);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "-------";
                x.Value = $"If you want to answer to this message you can use the `{Credentials.botConfig.prefix}feedback` command or " +
                $"[join the support server]({Constants.SupportServerInvite}) of Thoth and chat with the developer directly!";
            });
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = "This message was sent by the developer of the bot";
                x.IconUrl = Constants.botIcon;
            });

            var testmessage = await ReplyAsync("Respond with 'yes' if the message looks good.",
                embed: embed.Build());
            var response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response != null)
            {
                if (response.Value.Content.ToLowerInvariant() == "yes")
                {
                    await testmessage.ModifyAsync(x => x.Content = "Sent!");
                    await chn.SendMessageAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync("Message was not sent. :ok_hand:");
                }
            }
            else
            {
                await testmessage.ModifyAsync(x => x.Content = "Time is up.");
            }
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
                    Text.WriteLine(ex.Message);
                }
            }
        }

        [Command("setglobalerrormessage", true)]
        [Alias("sgem")]
        public async Task SetGlobalErrorMessageCommand([Remainder] string message = "")
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
        public async Task SetGameInConfigCommand([Remainder] string text)
        {
            Credentials.botConfig.setGame = text;
            await ReplyAsync("Ready, boss.\n" + Credentials.botConfig.setGame);
        }

        [Command("bs", true)]
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

        [Command("desc")]
        public async Task EmbedDescriptionOwnerCommand([Remainder] string text)
        {
            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(text);
            await ReplyAsync(embed: embed);
        }

        [Command("zxc", RunMode = RunMode.Async)]
        public async Task ReadAllMessagesInChannel()
        {
            var messages = await Context.Channel.GetMessagesAsync(2000).FlattenAsync();
            var allembeds = messages.Where(x => x.Embeds.Count != 0);

            Text.WriteLine($"Found {allembeds.Count()} messages.");
            int count = 0;
            StringBuilder sb = new();
            StringBuilder scount = new();
            StringBuilder sdate = new();
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
                scount.Append($"{count - 2}, ");
                sdate.Append($"\"{message.Timestamp.ToString("d", CultureInfo.InvariantCulture)}\", ");

                sb.AppendLine($"{count - 2},{message.Timestamp:dd-MM-yyyy}");
            }
            StringBuilder nzbr = new();
            nzbr.AppendLine(scount.ToString());
            nzbr.AppendLine(sdate.ToString());
            await File.AppendAllTextAsync("spimise.txt", nzbr.ToString());
            await File.AppendAllTextAsync("zxc.csv", sb.ToString());
            Text.WriteLine((count - 2).ToString());
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
            embed.WithDescription($"ID: {guildinfo[0]._id}\n" +
                $"Prefix: {guildinfo[0].prefix}\n" +
                $"Status Updates Enabled: {guildinfo[0].statusBool}\n" +
                $"Status Updates Channel: {guildinfo[0].statusChannel}");
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "More";
                x.Value = $"Name: {guild.Name}\n" +
                $"Owner: {guild.Owner} [{guild.OwnerId}]\n" +
                $"Preferred Culture: {guild.PreferredCulture}\n" +
                $"Preferred Locale: {guild.PreferredLocale}\n" +
                $"Channels: {guild.TextChannels.Count}\n" +
                $"Users: {guild.MemberCount}\n" +
                $"Shard: {Connection.Client.GetShardIdFor(Context.Guild)}";
            });
            if (guild.IconUrl != null)
            {
                embed.WithThumbnailUrl(guild.IconUrl);
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("searchguild")]
        public async Task SearchGuildCommand([Remainder] string term)
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
                    Text.WriteLine($"{guild.Name} [{guild.Id}]");
                    await Database.SetGuild(guild.Id);
                    missingCount++;
                    sb.AppendLine($"{guild.Name} [{guild.Id}]");
                }
            }
            await ReplyAsync($"Finished!\n" +
                $"Missing: {missingCount}\n" +
                $"{sb}");
        }

        [Command("permcheck", true, RunMode = RunMode.Async)]
        public async Task PermissionsCheckCommand()
        {
            int count = 0;
            var sb = new StringBuilder();

            foreach (var guild in Connection.Client.Guilds)
            {
                var perms = guild.CurrentUser.GuildPermissions;
                if (!perms.ManageMessages)
                {
                    count++;
                    if (count < 10)
                    {
                        sb.AppendLine($"{guild.Name} [{guild.Id}]");
                    }
                }
            }
            Text.WriteLine($"Guilds with missing ManageMessages permission: {count}\n{sb}");
            await ReplyAsync($"Guilds with missing ManageMessages permission: {count}\n{sb}");
        }

        // Communities

        [Command("addcommunity", true, RunMode = RunMode.Async)]
        [Alias("addcomm")]
        public async Task InsertCommunityCommandAsync()
        {
            CommunityModel community = new();
            // Name?
            var message = await ReplyAsync("Name?");
            var response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Name = response.Value.Content;
            await response.Value.DeleteAsync();
            // Description
            await message.ModifyAsync(x => x.Content = "Description?");
            response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Description = response.Value.Content;
            await response.Value.DeleteAsync();
            // LogoLink
            await message.ModifyAsync(x => x.Content = "Logo link? (imgur)");
            response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.LogoLink = response.Value.Content;
            await response.Value.DeleteAsync();
            // Mods
            await message.ModifyAsync(x => x.Content = "Mod ID? Add only one!");
            response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Mods = new ulong[1] { UInt64.Parse(response.Value.Content) };
            await response.Value.DeleteAsync();
            // Type
            await message.ModifyAsync(x => x.Content = "Type?");
            response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Type = response.Value.Content;
            await response.Value.DeleteAsync();
            // Discord invite or else
            await message.ModifyAsync(x => x.Content = "Discord invite or Twitter?");
            response = await Interactive.NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Value.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Link = response.Value.Content;
            await response.Value.DeleteAsync();

            await MongoConnection.SaveOrCreateCommunityAsync(community);
            Constants.ReloadConstants();
            await message.ModifyAsync(x => x.Content = "✅ Done!");
        }

        [Command("communities")]
        [Alias("comms")]
        public async Task GetAllCommunitiesCommandAsync()
        {
            EmbedBuilder embed = new()
            {
                Color = Constants.FeedbackColor
            };
            var communities = Constants.CommList;
            embed.WithAuthor(x =>
            {
                x.IconUrl = Constants.botIcon;
                x.Name = "All communities in the database";
            });
            foreach (var cm in communities)
            {
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = cm.Name;
                    x.Value = $"{cm.Description}\n{cm.LogoLink}\n**Type:** {cm.Type}, **Mods:** {cm.Mods.FirstOrDefault()}";
                });
            }
            await ReplyAsync(embed: embed.Build());
        }

        // Tips
        [Command("addtip")]
        public async Task InsertTipCommandAsync([Remainder] string tipText)
        {
            TipsModel tip = new()
            {
                TipText = tipText
            };
            await MongoConnection.SaveTipAsync(tip);
            Constants.ReloadConstants();
            var alltips = Constants.TipsList;
            await ReplyAsync($"Tips in DB: {alltips.Count}");
        }

        [Command("ff")]
        public async Task Testingstufbrat()
        {
            try
            {
                var result = await APIInteractions.GetLandingPanel();
                var singlePanel = result.singlePanel.content.Where(x => x.maxLevel > 100).ToList();

                Embed[] embeds = new Embed[singlePanel.Count];

                for (int i = 0; i < singlePanel.Count; i++)
                {
                    EmbedBuilder em = new()
                    {
                        Title = singlePanel[i].header.@default,
                        ImageUrl = singlePanel[i].imageUrl.INT
                    };
                    await ReplyAsync(embed: em.Build());
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
