using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Models;

namespace ThothBotCore.Tournament
{
    public class Signups
    {
        public static async Task SoloQConquest(string input, SocketCommandContext context)
        {
            string[] splittedString = input.Split(' ');
            string[] roles = splittedString[1].Split('/');
            var playersList = new List<VulpisPlayerModel.Player>();

            // Checking if anything exists
            if (!Directory.Exists("Tourneys"))
            {
                Directory.CreateDirectory("Tourneys");
            }
            if (!File.Exists($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json"))
            {
                string jsont = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json", jsont);
            }
            else
            {
                playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json"));
            }

            foreach (var x in playersList)
            {
                if (x.Name.ToLowerInvariant() == splittedString[0].ToLowerInvariant())
                {
                    await context.Channel.SendMessageAsync($"{splittedString[0]} is already registered. :rage:");
                    return;
                }
            }

            playersList.Add(new VulpisPlayerModel.Player()
            {
                DiscordID = context.Message.Author.Id,
                DiscordName = context.Message.Author.Username,
                Name = splittedString[0],
                PrimaryRole = roles[0],
                SecondaryRole = roles[1]
            });

            // Saving

            string json = JsonConvert.SerializeObject(playersList, Formatting.Indented);
            await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json", json);

            // Reading again?
            playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json"));
            await context.Channel.SendMessageAsync($"{context.Message.Author.Mention} added: \n" +
                $"**Name: **{playersList[playersList.Count - 1].Name}\n" +
                $"**Primary Role: **{playersList[playersList.Count - 1].PrimaryRole}\n" +
                $"**Secondary Role: **{playersList[playersList.Count - 1].SecondaryRole}\n" +
                $"**Discord Name: **{playersList[playersList.Count - 1].DiscordName}\n" +
                $"**Checkedin: **{playersList[playersList.Count - 1].CheckedIn}");
        }
    }
}
