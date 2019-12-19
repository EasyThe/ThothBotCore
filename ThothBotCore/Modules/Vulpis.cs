using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage;
using ThothBotCore.Tournament;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Vulpis : InteractiveBase<SocketCommandContext>
    {
        static Random rnd = new Random();

        [Command("duel")]
        public async Task Duel([Remainder]string username)
        {
            if (Context.Guild.Id == 518408306415632384)
            {
                var playersList = new List<VulpisPlayerModel.Player>();
                if (!Directory.Exists("Tourneys"))
                {
                    Directory.CreateDirectory("Tourneys");
                }
                if (!File.Exists($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json"))
                {
                    string jsont = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                    await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json", jsont);
                }

                playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json"));

                // Checking if the player registered already
                if (playersList.Count != 0)
                {
                    foreach (var item in playersList)
                    {
                        if (item.DiscordID == Context.Message.Author.Id)
                        {
                            await ReplyAsync($"You are already registered as " + $"**{item.Name}**");
                            return;
                        }
                    }
                }

                playersList.Add(new VulpisPlayerModel.Player()
                {
                    Name = username,
                    DiscordName = Context.Message.Author.Username + "#" + Context.Message.Author.DiscriminatorValue,
                    DiscordID = Context.Message.Author.Id
                });

                string json = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json", json);

                await ReplyAsync($"Done, {Context.Message.Author.Mention}! You are #{playersList.Count} player in the tournament.");
            }
        }

        [Command("unduel")]
        public async Task UnDuel()
        {
            if (Context.Guild.Id == 518408306415632384)
            {

            }
        }

        [Command("genduel")]
        public async Task GenerateDuelBracketsCommand()
        {

        }

        [Command("assaultteam")]
        [Alias("asteam")]
        public async Task AssaultTeamCommand()
        {
            var embed = new EmbedBuilder();
            var gods = Database.LoadAllGodsWithLessInfo();

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
                if (Context.Guild.Id != 321367254983770112)
                {
                    return;
                }
                var meganz = new VulpisPlayerModel.BaseTourney();
                // Interactive BOII

                // Tournament type
                await ReplyAsync(":heart:**Hello, qtie and welcome to TOURNAMENT CREATOR 1.0**\n:small_blue_diamond: Please tell me what the tournament will be (conquest, duel)((you have 60 seconds btw))");
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
                await ReplyAsync("**Okay, okay gimme a date!**\n*Date format:* **dd-MM-yyyy HH:mm**");
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(60));
                var tourneyDate = DateTime.Parse(response.Content);

                meganz.Tournament.SignupsAllowed = true;

                // END INTERACTIVE BOII

                await ReplyAsync($"**Okay, I think we did it Reddit.**\n" +
                    $"**Type: **{meganz.Tournament.Type}\n");

                // SAving
                
                string json = JsonConvert.SerializeObject(meganz, Formatting.Indented);
                await File.WriteAllTextAsync($"Tourneys/{tourneyDate.ToString("dd-MM-yyyy")}{filename}.json", json);
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
            if (Context.Guild.Id != 321367254983770112)
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
            if (Context.Guild.Id != 321367254983770112)
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
                if (Context.Guild.Id != 321367254983770112)
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
            if (Context.Guild.Id != 321367254983770112)
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

        [Command("checkin")]
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
            if (Context.Guild.Id != 321367254983770112)
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
                x.IconUrl = "https://media.discordapp.net/attachments/481545705630990337/621121723462582282/ve-1.png";
                x.Name = filePath;
            });
            embed.WithDescription($"**Type:** {tourney.Tournament.Type}\n**SignupsAllowed: **{tourney.Tournament.SignupsAllowed}\n" +
                $"**CheckinsAllowed:** {tourney.Tournament.CheckinsAllowed}");
            await ReplyAsync("", false, embed.Build());
        }

        [Command("checkplayers")]
        public async Task PlayersInTourneyCommand()
        {
            if (Context.Guild.Id != 321367254983770112)
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
                x.IconUrl = "https://media.discordapp.net/attachments/481545705630990337/621121723462582282/ve-1.png";
                x.Name = "Players";
            });
            var desc = new StringBuilder();
            foreach (var player in tourney.Players)
            {
                //invis \u200b
                desc.Append((player.CheckedIn == true ? ":green_circle:" : ":red_circle:") + player.Name);
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
            await TeamGenerator.SoloQConquest(Context);
        }

        [Command("dothemagic")]
        [Alias("dtm")]
        public async Task DoTheMagic()
        {
            await TeamGenerator.SoloQConquestInfo(Context);
        }

        [Command("givemethemall")]
        public async Task Iwanthemall()
        {
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
            await Signups.AssaultSignup(input, Context);
        }

        [Command("rsem")]
        public async Task ResendTeamsEmbed(SocketTextChannel channel, ulong messageID, SocketTextChannel channelToSendTo)
        {
            if (Context.Guild.Id != 321367254983770112)
            {
                return;
            }
            var chan = Discord.Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(channel.Id).GetMessageAsync(messageID);
            var sendChannel = Discord.Connection.Client.GetGuild(Context.Guild.Id).GetTextChannel(channelToSendTo.Id);

            var embed = new EmbedBuilder();
            foreach (var em in chan.Result.Embeds)
            {
                embed = em.ToEmbedBuilder();
                await sendChannel.SendMessageAsync(chan.Result.Content, false, embed.Build());
            }
        }

        [Command("testss")]
        public async Task asdasda(string name)
        {
            //await ReplyAsync(TournamentUtilities.GetTournamentFileName(name));
        }
    }
}
