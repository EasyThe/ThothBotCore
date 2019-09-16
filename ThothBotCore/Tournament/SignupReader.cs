using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ThothBotCore.Models;

namespace ThothBotCore.Tournament
{
    class SignupReader
    {
        DiscordSocketClient _client;

        public async Task InitializeSignupReaderAsync(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += SignupReaderReceivedAction;
        }

        private async Task SignupReaderReceivedAction(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot)
            {
                return;
            }
            var context = new SocketCommandContext(_client, msg);

            if (context.Guild.Id == 518408306415632384 && context.Channel.Id == 588144272553803789)
            {
                Console.WriteLine("zapis");
                var playersList = new List<DuelModel.Player>();
                if (!Directory.Exists("Tourneys"))
                {
                    Directory.CreateDirectory("Tourneys");
                }
                if (!File.Exists($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json"))
                {
                    string jsont = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                    await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json", jsont);
                }

                playersList = JsonConvert.DeserializeObject<List<DuelModel.Player>>(await File.ReadAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json"));

                // Checking if the player registered already
                if (playersList.Count != 0)
                {
                    foreach (var item in playersList)
                    {
                        if (item.DiscordID == context.Message.Author.Id)
                        {
                            //await ReplyAsync($"You are already registered as " + $"**{item.Name}**");
                            return;
                        }
                    }
                }

                playersList.Add(new DuelModel.Player()
                {
                    Name = context.Message.Content,
                    DiscordName = context.Message.Author.Username + "#" + context.Message.Author.DiscriminatorValue,
                    DiscordID = context.Message.Author.Id
                });

                string json = JsonConvert.SerializeObject(playersList, Formatting.Indented);
                await File.WriteAllTextAsync($"Tourneys/{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}duel.json", json);
            }
            else
            {
                return;
            }
        }
    }
}
