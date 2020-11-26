using Dapper;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Tournament;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Vulpis : InteractiveBase<SocketCommandContext>
    {
        static Random rnd = new Random();

        [Command("vulpis", true)]
        public async Task VulpisInfoCommand()
        {
            var comm = await MongoConnection.GetCommunityAsync("Vulpis Esports");
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = $"Vulpis Esports";
                x.IconUrl = comm.LogoLink;
            });
            embed.WithThumbnailUrl(comm.LogoLink);
            embed.WithColor(Constants.VulpisColor);
            embed.WithDescription(comm.Description);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Invite Link";
                x.Value = comm.Link;
            });
            await ReplyAsync(embed: embed.Build());
        }

        [Command("assaultteam")]
        [Alias("asteam")]
        public async Task AssaultTeamCommand()
        {
            var embed = new EmbedBuilder();
            var gods = Constants.GodsList;

            embed.WithColor(Constants.DefaultBlueColor);

            // Team 1
            StringBuilder team1 = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                int rr = rnd.Next(gods.Count);
                team1.Append(gods[rr].Emoji);
            }
            embed.WithTitle("Team 1");
            embed.WithDescription(team1.ToString());

            await ReplyAsync("", false, embed.Build());
        }

        [Command("createtournament", RunMode = RunMode.Async)]
        [Alias("ctvulpis")]
        public async Task CreateTournamentCommand()
        {
            try
            {
                if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
                {
                    return;
                }
                var meganz = new VulpisPlayerModel.BaseTourney();
                // Interactive BOII

                // Tournament type
                await ReplyAsync(":heart:**Hello, qtie and welcome to TOURNAMENT CREATOR 1.0**\n🔹 Please tell me what the tournament will be (conquest, duel)((you have 60 seconds btw))");
                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                meganz.Tournament.Type = response.Content;

                string filename = "";
                if (response.ToString().ToLowerInvariant() == "conquest")
                {
                    filename = "soloqcq";
                }
                else if (response.ToString().ToLowerInvariant() == "duel")
                {
                    filename = "duel";
                }

                // Date and time
                await ReplyAsync("**Okay, okay gimme a date!**\n*Date format:* **MM-dd-yyyy HH:mm**");
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                var tourneyDate = DateTime.Parse(response.Content);

                meganz.Tournament.SignupsAllowed = true;

                // END INTERACTIVE BOII

                await ReplyAsync($"**Okay, I think we did it Reddit.**\n" +
                    $"**Type: **{meganz.Tournament.Type}\n");

                // SAving
                
                string json = JsonConvert.SerializeObject(meganz, Formatting.Indented);
                if (!Directory.Exists("Tourneys"))
                {
                    Directory.CreateDirectory("Tourneys");
                }
                await File.WriteAllTextAsync($"Tourneys/{tourneyDate:dd-MM-yyyy}{filename}.json", json);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync($"**u fucked up boi**\n{ex.Message}");
            }
        }

        [Command("signup")]
        public async Task SignupCommand([Remainder]string input)
        {
            if (Context.Guild.Id != 321367254983770112)
            {
                return;
            }
            if (Context.Channel.Name.Contains("soloq-conquest"))
            {
                string fileName = TournamentUtilities.GetTournamentFileName("soloqcq");
                if (fileName != "")
                {
                    await Signups.SoloQConquestSignup(input, Context);
                }
                else
                {
                    await ReplyAsync("Sorry. Signups for Solo Queue Conquest are not open.");
                }
            }
            else if (Context.Channel.Name.Contains("duel-1v1"))
            {
                string fileName = TournamentUtilities.GetTournamentFileName("duel");
                if (fileName != "")
                {
                    await Signups.DuelSignup(input, Context);
                }
                else
                {
                    await ReplyAsync("Sorry. Signups for Duel 1v1 are not open.");
                }
            }
        }

        [Command("opensignups")]
        public async Task OpenSignups([Remainder]string input)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            string filePath = TournamentUtilities.GetTournamentFileName(input.Trim());
            string json = await File.ReadAllTextAsync(filePath);
            var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

            tourney.Tournament.SignupsAllowed = true;

            json = JsonConvert.SerializeObject(tourney, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            await ReplyAsync("rede!");
        }

        [Command("closesignups")]
        public async Task CloseSignups([Remainder]string input)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            string filePath = TournamentUtilities.GetTournamentFileName(input.Trim());
            string json = await File.ReadAllTextAsync(filePath);
            var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

            tourney.Tournament.SignupsAllowed = false;
            tourney.Tournament.CheckinsAllowed = false;

            json = JsonConvert.SerializeObject(tourney, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            await ReplyAsync($"Checkins and signups closed.\nChecked in players: {tourney.Players.FindAll(x => x.CheckedIn == true).Count}");
        }

        [Command("opencheckins")]
        public async Task OpenCheckins()
        {
            try
            {
                if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
                {
                    return;
                }
                string filePath = TournamentUtilities.GetTournamentFileName("soloqcq");
                string json = await File.ReadAllTextAsync(filePath);
                var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

                tourney.Tournament.CheckinsAllowed = true;

                json = JsonConvert.SerializeObject(tourney, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                await ReplyAsync("Done");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("closecheckins")]
        public async Task CloseCheckins()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            string filePath = TournamentUtilities.GetTournamentFileName("soloqcq");
            string json = await File.ReadAllTextAsync(filePath);
            var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

            tourney.Tournament.CheckinsAllowed = false;
            tourney.Tournament.SignupsAllowed = false;

            json = JsonConvert.SerializeObject(tourney, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            await ReplyAsync($"Checkins and signups closed.\nChecked in players: {tourney.Players.FindAll(x => x.CheckedIn == true).Count}");
        }

        [Command("checkin", true)]
        public async Task CheckinCommand()
        {
            if (Context.Channel.Name.Contains("soloq-conquest") && Context.Guild.Id == 321367254983770112)
            {
                string fileName = TournamentUtilities.GetTournamentFileName("soloqcq");
                if (fileName != "")
                {
                    string filePath = TournamentUtilities.GetTournamentFileName("soloqcq");
                    string json = await File.ReadAllTextAsync(filePath);
                    var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);
                    if (tourney.Tournament.CheckinsAllowed == true)
                    {
                        var player = tourney.Players.Find(x => x.DiscordID == Context.Message.Author.Id);
                        if (player.DiscordID != 0)
                        {
                            player.CheckedIn = true;

                            json = JsonConvert.SerializeObject(tourney, Formatting.Indented);
                            await File.WriteAllTextAsync(filePath, json);

                            await Context.Message.AddReactionAsync(Constants.CheckMarkEmoji);
                        }
                        else
                        {
                            await ReplyAsync("Couldn't find anyone registered in the tournament with your Discord account.");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Sorry. Checkins for Solo Queue Conquest are not open.");
                    }
                }
                else
                {
                    await ReplyAsync("Sorry. Checkins for Solo Queue Conquest are not open.");
                }
            }
        }

        [Command("checktournament")]
        public async Task CheckTournamentCommand()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            string filePath = TournamentUtilities.GetTournamentFileName("soloqcq");
            string json = await File.ReadAllTextAsync(filePath);
            var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

            var embed = new EmbedBuilder();
            embed.WithColor(Constants.VulpisColor);
            embed.WithAuthor(x =>
            {
                x.IconUrl = Constants.VulpisLogoLink;
                x.Name = filePath;
            });
            embed.WithDescription($"**Type:** {tourney.Tournament.Type}\n**SignupsAllowed: **{tourney.Tournament.SignupsAllowed}\n" +
                $"**CheckinsAllowed:** {tourney.Tournament.CheckinsAllowed}");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("checkplayers")]
        public async Task PlayersInTourneyCommand()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            string filePath = TournamentUtilities.GetTournamentFileName("soloqcq");
            string json = await File.ReadAllTextAsync(filePath);
            var tourney = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(json);

            var embed = new EmbedBuilder();
            embed.WithColor(Constants.VulpisColor);
            embed.WithAuthor(x=> 
            {
                x.IconUrl = Constants.VulpisLogoLink;
                x.Name = "Players";
            });
            var desc = new StringBuilder();
            foreach (var player in tourney.Players)
            {
                //invis \u200b
                desc.Append((player.CheckedIn == true ? ":green_circle:" : ":red_circle:") + player.Name + $" {player.PrimaryRole}/{player.SecondaryRole}");
                desc.Append("\n");
                embed.WithDescription(desc.ToString());
            }
            embed.WithFooter(x =>
            {
                x.Text = $"Count: {tourney.Players.Count}";
            });
            await ReplyAsync("", false, embed.Build());
        }

        [Command("sqmake")]
        public async Task CreateTeamsSQCq()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            await TeamGenerator.SoloQConquest(Context);
        }

        [Command("addteam", true, RunMode = RunMode.Async)]
        public async Task AddTeamEmbed()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            var message = await ReplyAsync("Okay, give me the ID of the message the embed is at...");
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            var embedmessage = await Connection.Client
                .GetGuild(Context.Guild.Id)
                .GetTextChannel(Context.Channel.Id)
                .GetMessageAsync(Convert.ToUInt64(response.Content));
            if (embedmessage.Embeds.Count != 0)
            {
                await message.ModifyAsync(x => 
                {
                    x.Content = "Found this one:\nIs it the right one? Type yes if so.";
                    x.Embed = embedmessage.Embeds.First().ToEmbedBuilder().Build();
                });
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                if (response.Content.ToLowerInvariant().Contains("yes"))
                {
                    var finalembed = embedmessage.Embeds.First().ToEmbedBuilder();
                    string footer = finalembed.Footer.Text;
                    string[] split = footer.Split(' ');
                    int teamnumber = Int16.Parse(split[0]);
                    teamnumber++;
                    var sb = new StringBuilder();
                    var embed = new EmbedBuilder();
                    embed.WithTitle($"Team {teamnumber}");
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Okay then, let's start adding a team...\nTell me the IGN of the **SOLO** laner now";
                        x.Embed = embed.Build();
                    });
                    // solo
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    sb.AppendLine($"<:Warrior:607990144338886658>{response.Content}");
                    embed.WithDescription(sb.ToString());
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Tell me the IGN of the **Jungle** now";
                        x.Embed = embed.Build();
                    });
                    // jungle
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    sb.AppendLine($"<:Assassin:607990143915261983>{response.Content}");
                    embed.WithDescription(sb.ToString());
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Tell me the IGN of the **Mid** laner now";
                        x.Embed = embed.Build();
                    });
                    // MID
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    sb.AppendLine($"<:Mage:607990144380698625>{response.Content}");
                    embed.WithDescription(sb.ToString());
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Tell me the IGN of the **Support** now";
                        x.Embed = embed.Build();
                    });
                    // Support
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    sb.AppendLine($"<:Guardian:607990144385024000>{response.Content}");
                    embed.WithDescription(sb.ToString());
                    // ADC
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Tell me the IGN of the **ADC** now";
                        x.Embed = embed.Build();
                    });
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    sb.AppendLine($"<:Hunter:607990144271646740>{response.Content}");
                    embed.WithDescription(sb.ToString());
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Okay. If everything seems fine, write **okay** to add it to the embed, or anything else to cancel.";
                        x.Embed = embed.Build();
                    });
                    response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                    if (response.Content.ToLowerInvariant().Contains("okay"))
                    {
                        finalembed.Footer.Text = $"{teamnumber} Teams";
                        finalembed.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = $"Team {teamnumber}";
                            x.Value = sb;
                        });
                        await message.ModifyAsync(x=>
                        {
                            x.Content = "";
                            x.Embed = finalembed.Build();
                        });
                    }
                    else
                    {
                        await message.ModifyAsync(x =>
                        {
                            x.Content = "**Cancelled!**";
                        });
                    }
                }
                else
                {
                    await message.ModifyAsync(x =>
                    {
                        x.Content = "Oh well.. Can't do much, sorry!";
                        x.Embed = null;
                    });
                }
            }
            else
            {
                await message.ModifyAsync(x=> x.Content = "Sorry, couldn't find that message. :(");
            }
        }

        [Command("dothemagic")]
        [Alias("dtm")]
        public async Task DoTheMagic()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            await TeamGenerator.SoloQConquestInfo(Context);
        }

        [Command("givemethemall")]
        public async Task Iwanthemall()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            var playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json"));
            var sb = new StringBuilder();

            foreach (var x in playersList)
            {
                sb.Append($"{x.Name}\n");
            }
            await ReplyAsync(sb.ToString());
        }

        [Command("assaulta")]
        [RequireOwner]
        public async Task AssaultSignCommand([Remainder]string input)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }
            await Signups.AssaultSignup(input, Context);
        }

        [Command("rsem")]
        public async Task ResendTeamsEmbed(SocketTextChannel channel, ulong messageID, SocketTextChannel channelToSendTo)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context) || Context.Message.Author.Id != Constants.OwnerID)
            {
                return;
            }
            var chan = await Discord.Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(channel.Id).GetMessageAsync(messageID);
            var sendChannel = Discord.Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(channelToSendTo.Id);

            var embed = new EmbedBuilder();
            foreach (var em in chan.Embeds)
            {
                embed = em.ToEmbedBuilder();
                await sendChannel.SendMessageAsync(chan.Content, false, embed.Build());
            }
        }

        [Command("removeplayer", RunMode = RunMode.Async)]
        public async Task RemovePlayerFromTourneyCommand([Remainder] string username)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }

            var tournamentObj = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq")));
            IUserMessage message = null;
            Embed embed = null;

            var foundPlayer = tournamentObj.Players.Find(x => x.Name.ToLowerInvariant() == username);

            if (foundPlayer != null)
            {
                embed = await EmbedHandler.BuildDescriptionEmbedAsync($"Found: {foundPlayer.Name} with roles " +
                    $"{foundPlayer.PrimaryRole}, {foundPlayer.SecondaryRole}\n" +
                    $"If you want to remove that player respond with **yes** if not, respond with **no**.\n__You have 60 seconds to respond.__");
                message = await ReplyAsync(embed: embed);
            }

            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
            if (response == null)
            {
                await message.ModifyAsync(x => x.Embed = null);
                return;
            }
            if (response.Content.ToLowerInvariant().Contains("yes"))
            {
                for (int i = 0; i < tournamentObj.Players.Count; i++)
                {
                    if (tournamentObj.Players[i].Name == foundPlayer.Name)
                    {
                        tournamentObj.Players.RemoveAt(i);
                    }
                }

                // Saving
                string json = JsonConvert.SerializeObject(tournamentObj, Formatting.Indented);
                await File.WriteAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq"), json);

                await message.ModifyAsync(x =>
                {
                    x.Content = $"{foundPlayer.Name} was removed.";
                });
                await PlayersInTourneyCommand();
            }
            else
            {
                await message.ModifyAsync(x =>
                {
                    x.Content = "k";
                    x.Embed = null;
                });

            }
        }

        [Command("removeall")]
        public async Task RemoveAllPlayers()
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context))
            {
                return;
            }

            var tournamentObj = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq")));

            tournamentObj.Players.Clear();
            await ReplyAsync("Removed all players");

            // Saving
            string json = JsonConvert.SerializeObject(tournamentObj, Formatting.Indented);
            await File.WriteAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq"), json);
        }

        [Command("randomteamassault")]
        [Alias("rta")]
        public async Task RandomAssaultVulpisCommand()
        {
            if (Context.Guild.Id != 321367254983770112)
            {
                await ReplyAsync("This command is available only in Vulpis.");
                return;
            }
            var embed = new EmbedBuilder();
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            bool hasHealer = false;
            bool firstHasHealer = false;
            //       ra, hel, guan yu, aphrodite, change, sylvanus, terra, baron, horus, yemoja
            int[] healers = { 1698, 1718, 1763, 1898, 1921, 2030, 2147, 3518, 3611, 3811 };
            var gods = Constants.GodsList;

            // First team
            for (int i = 0; i < 5; i++)
            {
                var current = gods[rnd.Next(gods.Count)];
                while (hasHealer && healers.AsList().Contains(current.id))
                {
                    current = gods[rnd.Next(gods.Count)];
                }
                if (healers.AsList().Contains(current.id))
                {
                    hasHealer = true;
                    firstHasHealer = true;
                }

                sb1.AppendLine($"{current.Emoji} {current.Name}");
                gods.Remove(current);
            }

            // Second team
            gods = Constants.GodsList;
            hasHealer = false;
            for (int i = 0; i < 5; i++)
            {
                var current = gods[rnd.Next(gods.Count)];
                if (healers.AsList().Contains(current.id) && firstHasHealer)
                {
                    hasHealer = true;
                }
                else if (healers.AsList().Contains(current.id) && !firstHasHealer)
                {
                    while (healers.AsList().Contains(current.id))
                    {
                        current = gods[rnd.Next(gods.Count)];
                    }
                }
                while (firstHasHealer && !hasHealer && i == 4 && !healers.AsList().Contains(current.id))
                {
                    current = gods[rnd.Next(gods.Count)];
                }

                sb2.AppendLine($"{current.Emoji} {current.Name}");
                gods.Remove(current);
            }

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Team 1";
                x.Value = sb1.ToString();
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Team 2";
                x.Value = sb2.ToString();
            });
            embed.WithAuthor(x =>
            {
                x.Name = "Random Assault Teams";
                x.IconUrl = Constants.VulpisLogoLink;
            });
            embed.WithColor(Constants.VulpisColor);
            await ReplyAsync(message: $"Two random assault teams for {Context.Message.Author.Mention}", embed: embed.Build());
        }

        [Command("reroll")]
        public async Task RerollAssaultGodVulpisCommand(int position)
        {
            try
            {
                bool found = false;
                IMessage foundMessage = Context.Message;
                IEmbed foundEmbed;
                var lastMessages = await Context.Channel.GetMessagesAsync(10).FlattenAsync();
                foreach (var message in lastMessages)
                {
                    if (found)
                    {
                        break;
                    }
                    if (message.Author.Id == Connection.Client.CurrentUser.Id)
                    {
                        foreach (var user in message.MentionedUserIds)
                        {
                            if (user == Context.Message.Author.Id)
                            {
                                foundMessage = message;
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    await ReplyAsync("Couldn't find a message containing a mention to " + Context.Message.Author.Mention);
                    return;
                }

                //debugging 
                await ReplyAsync("We gonna work with this one", embed: foundMessage.Embeds.First().ToEmbedBuilder().Build());
                foundEmbed = foundMessage.Embeds.First();
                string[] lines;

                // found, continue
                var gods = Constants.GodsList;
                var embed = new EmbedBuilder();
                embed.WithAuthor(x =>
                {
                    x.Name = "Random Assault Teams REROLL EDITION";
                    x.IconUrl = Constants.VulpisLogoLink;
                });
                embed.WithColor(Constants.VulpisColor);
                var nz = foundMessage.Embeds.First();
                var fields = nz.Fields;
                if (position > 4 && position < 10)
                {
                    lines = fields[1].Value.Split(
                                    new[] { Environment.NewLine },
                                    StringSplitOptions.None);
                    var rerolledGod = lines[position];
                    var newGod = gods[rnd.Next(gods.Count)];
                    while (rerolledGod.Contains(newGod.Name))
                    {
                        newGod = gods[rnd.Next(gods.Count)];
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Team 1";
                        x.Value = fields[0].Value;
                    });
                    var sbb = new StringBuilder();
                    for (int i = 5; i < 10; i++)
                    {
                        if (i != position)
                        {
                            sbb.AppendLine(lines[i]);
                        }
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Team 2";
                        x.Value = "";
                    });
                }
                else if (position < 5)
                {
                    lines = fields[1].Value.Split(
                                    new[] { Environment.NewLine },
                                    StringSplitOptions.None);
                    var rerolledGod = lines[position];
                    var newGod = gods[rnd.Next(gods.Count)];
                    while (rerolledGod.Contains(newGod.Name))
                    {
                        newGod = gods[rnd.Next(gods.Count)];
                    }
                    var sbb = new StringBuilder();
                    for (int i = 0; i < 5; i++)
                    {
                        if (i != position)
                        {
                            sbb.AppendLine(lines[i]);
                        }
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Team 1";
                        x.Value = "";
                    });
                }
                else
                {
                    await ReplyAsync("The number you have entered an invalid number. Please try again and enter number between 0-9.");
                    return;
                }

                await ReplyAsync(Context.Message.Author.Mention, embed: embed.Build());
            }
            catch (Exception ex)
            {
                await Reporter.RespondToCommandOnErrorAsync(ex, Context);
            }
        }

        [Command("vw")]
        public async Task VulpisWarnCommand(SocketTextChannel channel, [Remainder] string text)
        {
            if (Context.Guild.Id != 321367254983770112 || !TournamentUtilities.IsTournamentManagerCheck(Context) || Context.Message.Author.Id != Constants.OwnerID)
            {
                return;
            }

            var embed = await EmbedHandler.BuildDescriptionEmbedAsync(text, 233, 78, 26);
            await channel.SendMessageAsync(embed: embed.ToEmbedBuilder().Build());
        }
    }
}
