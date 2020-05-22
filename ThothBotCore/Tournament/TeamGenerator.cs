using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Models.Vulpis;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tournament
{
    public class TeamGenerator
    {
        static Random rnd = new Random();

        public static async Task SoloQConquestInfo(SocketCommandContext context)
        {
            var playersList = JsonConvert.DeserializeObject<List<VulpisPlayerModel.Player>>(await File.ReadAllTextAsync(TournamentUtilities.GetTournamentFileName("conquest")));
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
        private static async Task GetConquestTeams(SocketCommandContext context, List<VulpisPlayerModel.Player> playersList, List<VulpisConquestTeamModel> teamslist)
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Constants.DefaultBlueColor);
            embed.WithAuthor(x =>
            {
                x.IconUrl = "https://media.discordapp.net/attachments/481545705630990337/621121723462582282/ve-1.png";
                x.Name = $"Conquest 5v5 Solo Signup";
            });
            embed.WithFooter(x =>
            {
                x.Text = $"{teamslist.Count} Teams";
            });
            for (int tm = 0; tm < teamslist.Count; tm++)
            {
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"Team {tm+1}";
                    x.Value = $"<:Warrior:607990144338886658>{teamslist[tm].Solo}\n" +
                    $"<:Assassin:607990143915261983>{teamslist[tm].Jungle}\n" +
                    $"<:Mage:607990144380698625>{teamslist[tm].Mid}\n" +
                    $"<:Guardian:607990144385024000>{teamslist[tm].Support}\n" +
                    $"<:Hunter:607990144271646740>{teamslist[tm].ADC}";
                });
            }

            await context.Channel.SendMessageAsync("", false, embed.Build());
            if (playersList.Count != 0)
            {
                var nz = new StringBuilder();

                foreach (var player in playersList)
                {
                    nz.Append($"{player.Name} {player.PrimaryRole}/{player.SecondaryRole}\n");
                }
                await context.Channel.SendMessageAsync("**Unassigned players: **\n" +
                    $"{nz}");
            }
        }
        public static async Task SoloQConquest(SocketCommandContext context)
        {
            var tournamentObj = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq")));
            var mainTournObj = JsonConvert.DeserializeObject<VulpisPlayerModel.BaseTourney>(await File.ReadAllTextAsync(TournamentUtilities.GetTournamentFileName("soloqcq")));
            var teamsList = new List<VulpisConquestTeamModel>();

            try
            {
                while (tournamentObj.Players.Count > 4)
                {
                    
                    var fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));

                    for (int i = 0; i < tournamentObj.Players.Count / 5; i++)
                    {
                        string solo = "n/a";
                        string jungle = "n/a";
                        string mid = "n/a";
                        string support = "n/a";
                        string adc = "n/a";

                        //SOLO
                        var soloPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("solo"));
                        if (soloPlayers.Count != 0)
                        {
                            solo = soloPlayers[rnd.Next(soloPlayers.Count)].Name;
                        }
                        else
                        {
                            fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("solo"));
                            if (fillPlayers.Count != 0)
                            {
                                solo = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                            }
                            else
                            {
                                fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("fill"));
                                if (fillPlayers.Count != 0)
                                {
                                    solo = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                                else
                                {
                                    fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));
                                    solo = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                            }
                        }
                        tournamentObj.Players.RemoveAll(x => x.Name == solo);

                        //JUNGLE
                        var junglePlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("j"));
                        if (junglePlayers.Count != 0)
                        {
                            jungle = junglePlayers[rnd.Next(junglePlayers.Count)].Name;
                        }
                        else
                        {
                            fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("j"));
                            if (fillPlayers.Count != 0)
                            {
                                jungle = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                            }
                            else
                            {
                                fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("fill"));
                                if (fillPlayers.Count != 0)
                                {
                                    jungle = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                                else
                                {
                                    fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));
                                    jungle = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                            }
                        }
                        tournamentObj.Players.RemoveAll(x => x.Name == jungle);

                        //MID
                        var midPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("mid"));
                        if (midPlayers.Count != 0)
                        {
                            mid = midPlayers[rnd.Next(midPlayers.Count)].Name;
                        }
                        else
                        {
                            fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("mid"));
                            if (fillPlayers.Count != 0)
                            {
                                mid = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                            }
                            else
                            {
                                fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("fill"));
                                if (fillPlayers.Count != 0)
                                {
                                    mid = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                                else
                                {
                                    fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));
                                    mid = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                            }
                        }
                        tournamentObj.Players.RemoveAll(x => x.Name == mid);

                        //SUPP
                        var suppPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("sup"));
                        if (suppPlayers.Count != 0)
                        {
                            support = suppPlayers[rnd.Next(suppPlayers.Count)].Name;
                        }
                        else
                        {
                            fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("sup"));
                            if (fillPlayers.Count != 0)
                            {
                                support = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                            }
                            else
                            {
                                fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("fill"));
                                if (fillPlayers.Count != 0)
                                {
                                    support = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                                else
                                {
                                    fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));
                                    support = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                            }
                        }
                        tournamentObj.Players.RemoveAll(x => x.Name == support);

                        //ADC
                        var adcPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("adc"));
                        if (adcPlayers.Count != 0)
                        {
                            adc = adcPlayers[rnd.Next(adcPlayers.Count)].Name;
                        }
                        else
                        {
                            fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("adc"));
                            if (fillPlayers.Count != 0)
                            {
                                adc = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                            }
                            else
                            {
                                fillPlayers = tournamentObj.Players.FindAll(x => x.SecondaryRole.ToLowerInvariant().Contains("fill"));
                                if (fillPlayers.Count != 0)
                                {
                                    adc = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                                else
                                {
                                    fillPlayers = tournamentObj.Players.FindAll(x => x.PrimaryRole.ToLowerInvariant().Contains("fill"));
                                    adc = fillPlayers[rnd.Next(fillPlayers.Count)].Name;
                                }
                            }
                        }
                        tournamentObj.Players.RemoveAll(x => x.Name == adc);

                        if (solo != "n/a" && jungle != "n/a" && mid != "n/a" && support != "n/a" && adc != "n/a")
                        {
                            mainTournObj.Players.RemoveAll(x => x.Name == solo);
                            mainTournObj.Players.RemoveAll(x => x.Name == jungle);
                            mainTournObj.Players.RemoveAll(x => x.Name == mid);
                            mainTournObj.Players.RemoveAll(x => x.Name == support);
                            mainTournObj.Players.RemoveAll(x => x.Name == adc);
                            Console.WriteLine(mainTournObj.Players.Count);
                        }

                        //Add players into teamsList list
                        teamsList.Add(new VulpisConquestTeamModel
                        {
                            Solo = solo,
                            Jungle = jungle,
                            Mid = mid,
                            Support = support,
                            ADC = adc
                        });
                    }
                }

                Console.WriteLine("FINISH BRAT" + teamsList.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR BRAT : " + ex.Message);
            }
            await GetConquestTeams(context, mainTournObj.Players, teamsList);
        }
    }
}
