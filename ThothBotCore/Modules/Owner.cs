using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    public class Owner : InteractiveBase<SocketCommandContext>
    {
        HiRezAPI hirezAPI = new HiRezAPI();
        DominantColor domColor = new DominantColor();

        [Command("updatedb", true, RunMode = RunMode.Async)]
        public async Task UpdateDBFromSmiteAPI()
        {
            try
            {
                var msg = await ReplyAsync("<a:updating:403035325242540032> Working on gods...");
                var newGodList = JsonConvert.DeserializeObject<List<Gods.God>>(await hirezAPI.GetGods());
                var godsInDb = MongoConnection.GetAllGods();

                // Adding the emojis and domcolors from the old db to the new one
                foreach (var god in godsInDb)
                {
                    newGodList.Find(x => x.id == god.id).Emoji = god.Emoji;
                    newGodList.Find(x => x.id == god.id).DomColor = god.DomColor;
                }

                // Missing Emoji?
                if (newGodList.Any(x => x.Emoji == null))
                {
                    foreach (var god in newGodList)
                    {
                        if (god.Emoji == null)
                        {
                            // tva go proveri dali raboti kakto trqbva zashtoto
                            // mi se struva che kogato dobavi emojito, toq method
                            // shte go premahne kato go rugne v dbto.. shte chakame nov bog za da vidim
                            Utils.AddNewGodEmojiInGuild(god);
                        }
                    }
                }
                Thread.Sleep(200);
                // Missing DomColor?
                if (newGodList.Any(x => x.DomColor == 0))
                {
                    foreach (var god in newGodList)
                    {
                        if (god.DomColor == 0)
                        {
                            // Getting the dominant color from the gods icon
                            try
                            {
                                if (god.godIcon_URL != "")
                                {
                                    god.DomColor = DominantColor.GetDomColor(god.godIcon_URL);
                                }
                                else
                                {
                                    await Reporter.SendError($"{god.Name} has missing icon.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                    }
                }
                // Saving the gods to the DB
                foreach (var god in newGodList)
                {
                    await MongoConnection.SaveGodAsync(god);
                }

                // ITEMS ====================================================================================================
                await msg.ModifyAsync(x => x.Content = "<a:updating:403035325242540032> Working on items...");

                var newItemsList = JsonConvert.DeserializeObject<List<GetItems.Item>>(await hirezAPI.GetItems());
                var itemsInDb = MongoConnection.GetAllItems();

                // Adding the emojis and domcolors from the old db to the new one
                foreach (var item in itemsInDb)
                {
                    var foundIndex = newItemsList.FindIndex(x => x.ItemId == item.ItemId);
                    if (foundIndex != -1)
                    {
                        newItemsList[foundIndex]._id = item._id;
                        newItemsList[foundIndex].Emoji = item.Emoji;
                        newItemsList[foundIndex].DomColor = item.DomColor;
                        newItemsList[foundIndex].GodType = item.GodType;
                    }
                }

                // Missing Emoji?
                if (newItemsList.Any(x => x.Emoji == null && x.ActiveFlag == "y"))
                {
                    foreach (var item in newItemsList)
                    {
                        if (item.Emoji == null && item.ActiveFlag == "y")
                        {
                            try
                            {
                                item.Emoji = await Utils.AddMissingItemEmojiAsync(item);
                            }
                            catch (Exception exx)
                            {
                                await ReplyAsync($"{item.DeviceName} {exx.Message}");
                            }
                        }
                    }
                }

                // Missing DomColor?
                if (newItemsList.Any(x=> x.DomColor == 0))
                {
                    foreach (var item in newItemsList)
                    {
                        if (item.DomColor == 0 && item.ActiveFlag == "y")
                        {
                            if (item.itemIcon_URL != "")
                            {
                                try
                                {
                                    item.DomColor = DominantColor.GetDomColor(item.itemIcon_URL);
                                }
                                catch (Exception x)
                                {
                                    await ReplyAsync($"{item.DeviceName} {x.Message}");
                                }
                            }
                        }
                    }
                }

                foreach (var item in newItemsList)
                {
                    await MongoConnection.SaveItemAsync(item);
                }

                await msg.ModifyAsync(x => x.Content = "<:check:314349398811475968> Done!");
            }
            catch (Exception ex)
            {
                await Reporter.SendException(ex, Context, "");
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
                            using WebClient client = new WebClient();
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

        [Command("lg")] // Leave Guild
        public async Task LeaveGuild(ulong id)
        {
            await Connection.Client.GetGuild(id).LeaveAsync();
        }

        [Command("sm")]
        public async Task SendMessageAsOwner(ulong server, ulong channel, [Remainder] string message)
        {
            var chn = Connection.Client.GetGuild(server).GetTextChannel(channel);

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

        [Command("sac")]
        public async Task SetActivityCommand(string url, [Remainder] string game)
        {
            await Connection.Client.SetGameAsync(game, url);
            await ReplyAsync($"Successfully set the activity to '**{game}**'");
        }

        [Command("sg")]
        public async Task SetGame([Remainder] string game)
        {
            await Connection.Client.SetGameAsync(game);
            await ReplyAsync($"Successfully set the game to '**{game}**'");
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
                scount.Append($"{count - 2}, ");
                sdate.Append($"\"{message.Timestamp.ToString("d", CultureInfo.InvariantCulture)}\", ");

                sb.AppendLine($"{count - 2},{message.Timestamp:dd-MM-yyyy}");
            }
            StringBuilder nzbr = new StringBuilder();
            nzbr.AppendLine(scount.ToString());
            nzbr.AppendLine(sdate.ToString());
            await File.AppendAllTextAsync("spimise.txt", nzbr.ToString());
            await File.AppendAllTextAsync("zxc.csv", sb.ToString());
            Text.WriteLine((count - 2).ToString());
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
                $"Users: {guild.MemberCount}";
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
                    await Database.SetGuild(guild.Id, gname);
                    missingCount++;
                    sb.AppendLine($"{guild.Name} [{guild.Id}]");
                }
            }
            await ReplyAsync($"Finished!\n" +
                $"Missing: {missingCount}\n" +
                $"{sb}");
        }

        [Command("sendDM", RunMode = RunMode.Async)]
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
                x.Name = "-------";
                x.Value = $"If you want to answer to this message you can use the `{Credentials.botConfig.prefix}feedback` command or " +
                $"[join the support server]({Constants.SupportServerInvite}) of Thoth and chat with the developer directly!";
            });
            embed.WithDescription(message);
            embed.WithFooter(x =>
            {
                x.Text = "This message was sent from the developer of the bot";
                x.IconUrl = Constants.botIcon;
            });

            var testmessage = await ReplyAsync("Respond with 'yes' if the message looks good.",
                embed: embed.Build());
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response != null)
            {
                if (response.Content.ToLowerInvariant() == "yes")
                {
                    await channel.SendMessageAsync(embed: embed.Build());
                }
                else
                {
                    await ReplyAsync("Message was not sent. :ok_hand:");
                }
            }
            else
            {
                await testmessage.ModifyAsync(x=> x.Content = "Time is up.");
            }
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

        [Command("cleansqlite", RunMode = RunMode.Async)]
        public async Task ClearMissingGuildsFromSqliteDBCommand()
        {
            await ReplyAsync("cheti konzolata");
            try
            {
                var db = await Database.GetAllGuilds();
                SocketGuild nzvrat = null;
                foreach (var guild in db)
                {
                    try
                    {
                        nzvrat = Connection.Client.Guilds.Single(x => x.Id == guild._id);
                    }
                    catch
                    {
                        Text.WriteLine(guild._id.ToString());
                    }
                }
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                if (response != null || response.Content.Contains("yes"))
                {
                    foreach (var guild in db)
                    {
                        try
                        {
                            nzvrat = Connection.Client.Guilds.Single(x => x.Id == guild._id);
                        }
                        catch
                        {
                            Text.WriteLine($"{nzvrat.Id} {nzvrat.Name}");
                            await Database.DeleteServerConfig(guild._id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Text.WriteLine(ex.Message);
            }
        }

        [Command("reload")]
        public async Task ReloadConstantsCommand()
        {
            Constants.ReloadConstants();
            await ReplyAsync("Reloaded!");
        }

        // Badges
        [Command("badges")]
        public async Task GetAllBadgesCommandAsync()
        {
            EmbedBuilder embed = new EmbedBuilder 
            { 
                Color = Constants.FeedbackColor
            };
            StringBuilder keys = new StringBuilder();
            StringBuilder badge = new StringBuilder();
            embed.WithAuthor(x =>
            {
                x.IconUrl = Constants.botIcon;
                x.Name = "All available badges";
            });
            var allBadges = MongoConnection.GetAllBadges();
            foreach (var bdg in allBadges)
            {
                keys.AppendLine(bdg.Key);
                badge.AppendLine($"{bdg.Emote} {bdg.Title}");
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Key";
                x.Value = keys.ToString();
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Badge";
                x.Value = badge.ToString();
            });
            await ReplyAsync(embed: embed.Build());
        }

        // Communities

        [Command("addcommunity", true, RunMode = RunMode.Async)]
        [Alias("addcomm")]
        public async Task InsertCommunityCommandAsync()
        {
            CommunityModel community = new CommunityModel();
            // Name?
            var message = await ReplyAsync("Name?");
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Name = response.Content;
            await response.DeleteAsync();
            // Description
            await message.ModifyAsync(x => x.Content = "Description?");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Description = response.Content;
            await response.DeleteAsync();
            // LogoLink
            await message.ModifyAsync(x => x.Content = "Logo link? (imgur)");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.LogoLink = response.Content;
            await response.DeleteAsync();
            // Mods
            await message.ModifyAsync(x => x.Content = "Mod ID? Add only one!");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Mods = new ulong[1] { UInt64.Parse(response.Content) };
            await response.DeleteAsync();
            // Type
            await message.ModifyAsync(x => x.Content = "Type?");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Type = response.Content;
            await response.DeleteAsync();
            // Discord invite or else
            await message.ModifyAsync(x => x.Content = "Discord invite or Twitter?");
            response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null || response.Content.ToLowerInvariant().Contains("cancel"))
            {
                return;
            }
            community.Link = response.Content;
            await response.DeleteAsync();

            await MongoConnection.SaveOrCreateCommunityAsync(community);
            Constants.ReloadConstants();
            await message.ModifyAsync(x => x.Content = "✅ Done!");
        }

        [Command("communities")]
        [Alias("comms")]
        public async Task GetAllCommunitiesCommandAsync()
        {
            EmbedBuilder embed = new EmbedBuilder
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
            TipsModel tip = new TipsModel
            {
                TipText = tipText
            };
            await MongoConnection.SaveTipAsync(tip);
            Constants.ReloadConstants();
            var alltips = Constants.TipsList;
            await ReplyAsync($"Tips in DB: {alltips.Count}");
        }

        [Command("tips")]
        public async Task PrintAllTips()
        {
            var alltips = Constants.TipsList;
            EmbedBuilder embed = new EmbedBuilder
            {
                Color = Constants.FeedbackColor
            };
            StringBuilder main = new StringBuilder();
            embed.WithAuthor(x =>
            {
                x.IconUrl = Constants.botIcon;
                x.Name = "All tips";
            });
            int count = 1;
            foreach (var tip in alltips)
            {
                main.AppendLine($"**{count}.** {tip.TipText}");
                count++;
            }
            embed.WithDescription(main.ToString());
            await ReplyAsync(embed: embed.Build());
        }

        [Command("ff")]
        public async Task Testingstufbrat()
        {
            var pages = new EmbedBuilder[]
            {
                new EmbedBuilder().WithTitle("Passive"),
                new EmbedBuilder().WithTitle("1"),
                new EmbedBuilder().WithTitle("2"),
                new EmbedBuilder().WithTitle("3"),
                new EmbedBuilder().WithTitle("4")
            };
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
