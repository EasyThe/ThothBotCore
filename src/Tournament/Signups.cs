using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Models.Vulpis;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tournament
{
    public class Signups
    {
        public static async Task SoloQConquestSignup(string input, SocketCommandContext context)
        {
            try
            {
                string[] splittedString;
                string[] roles;
                string playerName = "";

                if (input.Contains("'"))
                {
                    splittedString = input.Split('\'');
                    playerName = splittedString[1];
                    roles = splittedString[2].Split('/');
                }
                else
                {
                    splittedString = input.Split(' ');
                    roles = splittedString[1].Split('/');
                    playerName = splittedString[0];
                }

                var tournament = new VulpisPlayerModel.BaseTourney();
                string filepath = TournamentUtilities.GetTournamentFileName("soloqcq");
                tournament = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(filepath));

                if (tournament.Tournament.SignupsAllowed == false)
                {
                    await context.Channel.SendMessageAsync("Signups are not open.");
                    return;
                }
                foreach (var x in tournament.Players)
                {
                    if (x.Name.ToLowerInvariant() == playerName.ToLowerInvariant())
                    {
                        await context.Channel.SendMessageAsync($"{playerName} is already registered. :rage:");
                        return;
                    }
                }

                tournament.Players.Add(new VulpisPlayerModel.Player()
                {
                    DiscordID = context.Message.Author.Id,
                    DiscordName = context.Message.Author.Username,
                    Name = playerName,
                    PrimaryRole = roles[0].Trim(),
                    SecondaryRole = roles[1].Trim(),
                    CheckedIn = tournament.Tournament.CheckinsAllowed == true ? true : false
                });

                // Saving

                string json = JsonConvert.SerializeObject(tournament, Formatting.Indented);
                await File.WriteAllTextAsync(filepath, json);

                // Reading again to make sure everything went well
                tournament = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(filepath));
                var embed = new EmbedBuilder();
                embed.WithAuthor(x =>
                {
                    x.IconUrl = context.Message.Author.GetAvatarUrl();
                    x.Name = context.Message.Author.Username + " signed up as:";
                });
                embed.WithColor(Constants.VulpisColor);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Name";
                    x.Value = tournament.Players[^1].Name;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Primary Role";
                    x.Value = tournament.Players[^1].PrimaryRole;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Secondary Role";
                    x.Value = tournament.Players[^1].SecondaryRole;
                });
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "CheckedIn";
                    x.Value = tournament.Players[^1].CheckedIn;
                });
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync("If your name contains blank spaces, please use apostrophes in the beginning and the end of your name.\nExample: `??signup \'EasyThe on Xbox\' mid/adc`\n" +
                    "If your name doesn't contain blank spaces use: `??signup EasyThe mid/adc`\n" +
                    $"**Tell this to EasyThe: {ex.Message}**");
            }
        }
        public static async Task AssaultSignup(string input, SocketCommandContext context)
        {
            var playersList = new List<Vulpis5v5TeamModel>();
            if (input.Contains(','))
            {
                string[] splittedString = input.Split(',');

                // Checking if anything exists
                if (!Directory.Exists("Tourneys"))
                {
                    Directory.CreateDirectory("Tourneys");
                }
                if (!File.Exists($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}assault.json"))
                {
                    string jsont = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                    await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}assault.json", jsont);
                }
                else
                {
                    playersList = JsonConvert.DeserializeObject<List<Vulpis5v5TeamModel>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}assault.json"));
                }

                switch (splittedString.Length)
                {
                    case 2:
                        Console.WriteLine(splittedString.Length);
                        // do something
                        break;
                    case 3:
                        Console.WriteLine(splittedString.Length);
                        // neshto
                        break;
                    case 4:
                        Console.WriteLine(splittedString.Length);
                        //da
                        break;
                    default:
                        Console.WriteLine("5+??");
                        break;
                }
            }
            else
            {

            }
        }
        public static async Task DuelSignup(string input, SocketCommandContext context)
        {
            try
            {
                var tournament = new VulpisPlayerModel.BaseTourney();
                string filepath = TournamentUtilities.GetTournamentFileName("duel");
                tournament = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(filepath));

                if (tournament.Tournament.SignupsAllowed == false)
                {
                    await context.Channel.SendMessageAsync("Signups are not open.");
                    return;
                }
                foreach (var x in tournament.Players)
                {
                    if (x.Name.ToLowerInvariant() == input.ToLowerInvariant())
                    {
                        await context.Channel.SendMessageAsync($"{input} is already registered. :rage:");
                        return;
                    }
                }

                tournament.Players.Add(new VulpisPlayerModel.Player()
                {
                    DiscordID = context.Message.Author.Id,
                    DiscordName = context.Message.Author.Username,
                    Name = input,
                    CheckedIn = true
                });

                // Saving

                string json = JsonConvert.SerializeObject(tournament, Formatting.Indented);
                await File.WriteAllTextAsync(filepath, json);

                // Reading again to check if everything went okay i guess
                tournament = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(filepath));
                var embed = new EmbedBuilder();
                embed.WithAuthor(x =>
                {
                    x.IconUrl = context.Message.Author.GetAvatarUrl();
                    x.Name = context.Message.Author.Username + " signed up as:";
                });
                embed.WithColor(Constants.VulpisColor);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Name";
                    x.Value = tournament.Players[tournament.Players.Count - 1].Name;
                });
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("Well... Something isn't right :worried~1:");
            }
        }
    }
}
