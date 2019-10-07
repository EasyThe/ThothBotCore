using Discord;
using Discord.Commands;
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
    public class Vulpis : ModuleBase<SocketCommandContext>
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

        [Command("signup")]
        [Alias("su")]
        public async Task SignupCommand([Remainder]string input)
        {
            await Signups.SoloQConquest(input, Context);

            if (Context.Channel.Name.Contains("soloq-conquest"))
            {
                await Signups.SoloQConquest(input, Context);
            }
        }

        [Command("dothemagic")]
        [Alias("dtm")]
        public async Task DoTheMagic()
        {
            await TeamGenerator.SoloQConquest(Context);
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
    }
}
