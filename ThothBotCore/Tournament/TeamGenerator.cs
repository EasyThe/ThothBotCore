using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tournament
{
    public class TeamGenerator
    {
        public static async Task SoloQConquest(SocketCommandContext context)
        {
            var playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}soloqcq.json"));
            Console.WriteLine(playersList.Count);
            var midPlayers = playersList.FindAll(x=> x.PrimaryRole.ToLowerInvariant().Contains("mid") || x.SecondaryRole.ToLowerInvariant().Contains("mid"));
            var adcPlayers = playersList.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("adc") || x.SecondaryRole.ToLowerInvariant().Contains("adc"));
            var suppPlayers = playersList.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("supp") || x.SecondaryRole.ToLowerInvariant().Contains("supp"));
            var junglePlayers = playersList.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("j") || x.SecondaryRole.ToLowerInvariant().Contains("j"));
            var soloPlayers = playersList.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("solo") || x.SecondaryRole.ToLowerInvariant().Contains("solo"));
            var fillPlayers = playersList.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill") || x.SecondaryRole.ToLowerInvariant().Contains("fill"));

            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Mid Players";
                x.Value = midPlayers.Count;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Adc Players";
                x.Value = adcPlayers.Count;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Supp Players";
                x.Value = suppPlayers.Count;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Jungle Players";
                x.Value = junglePlayers.Count;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Solo Players";
                x.Value = soloPlayers.Count;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Fill Players";
                x.Value = fillPlayers.Count;
            });
            embed.WithFooter(x =>
            {
                x.Text = "Count of all registered players: " + playersList.Count;
            });

            await context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}
