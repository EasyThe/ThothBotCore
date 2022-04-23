using Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Tournament
{
    public static class TournamentTimer
    {
        private static Timer PlayersStatusCheckerTimer;
        private static HiRezAPI HiRezAPI = new();
        private static int MatchId1, MatchId2;
        public static Task StartServerStatusTimer()
        {
            PlayersStatusCheckerTimer = new Timer() 
            {
                AutoReset = false
            };
            PlayersStatusCheckerTimer.Elapsed += TournamentTimer_Elapsed;
            PlayersStatusCheckerTimer.Start();

            return Task.CompletedTask;
        }

        private static async void TournamentTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var openMatches = await ChallongeAPI.GetMatches(Global.TourneyName, "open");
                var participants = await ChallongeAPI.GetParticipants(Global.TourneyName);

                if (openMatches.Count == 0)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync($"Tournament {Global.TourneyName} has finished. Stopping the tracker.", Constants.VulpisColor);
                    await Connection.Client.GetGuild(Global.TourneyTimerIDs[2])
                                    .GetTextChannel(Global.TourneyTimerIDs[0])
                                    .ModifyMessageAsync(Global.TourneyTimerIDs[1], x => x.Embed = em);
                    await StopTournamentTimer($"Tournament {Global.TourneyName} has finished. Stopping the timer.");
                    return;
                }
                var embed = new EmbedBuilder();
                embed.WithTitle(Global.TourneyName);
                embed.WithColor(Constants.VulpisColor);
                string player1, player2;
                foreach (var match in openMatches)
                {
                    player1 = await PlayerChecker(participants.Find(x => x.participant.id == match.match.player1_id).participant.name, false);
                    player2 = await PlayerChecker(participants.Find(x => x.participant.id == match.match.player2_id).participant.name, true);
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = $"{player1} 🆚 {player2}";
                        x.Value = MatchId1 == MatchId2 ? MatchId1 : $"{MatchId1} 🆚 {MatchId2}";
                    });
                }
                embed.WithCurrentTimestamp();
                embed.WithFooter(x =>
                {
                    x.Text = "Last updated";
                });

                await Connection.Client.GetGuild(Global.TourneyTimerIDs[2])
                    .GetTextChannel(Global.TourneyTimerIDs[0])
                    .ModifyMessageAsync(Global.TourneyTimerIDs[1], x => x.Embed = embed.Build());
            }
            catch (System.Exception ex)
            {
                var da = await EmbedHandler.BuildDescriptionEmbedAsync($"Shit, it crashed:\n{ex.Message}");
                await Connection.Client.GetGuild(Global.TourneyTimerIDs[2])
                    .GetTextChannel(Global.TourneyTimerIDs[0])
                    .ModifyMessageAsync(Global.TourneyTimerIDs[1], x => x.Embed = da);
                await Connection.Client.GetGuild(Global.TourneyTimerIDs[2])
                    .GetTextChannel(Global.TourneyTimerIDs[0]).SendMessageAsync(MentionUtils.MentionUser(171675309177831424));
            }

            PlayersStatusCheckerTimer.Interval = 100000;
            PlayersStatusCheckerTimer.Enabled = true;
        }

        public static async Task StopTournamentTimer(string message)
        {
            Global.TourneyName = null;
            Global.TourneyTimerIDs = null;
            PlayersStatusCheckerTimer.Enabled = false;
            Text.WriteLine(message);
            await Reporter.SendError(message);
        }
        private static async Task<string> PlayerChecker(string PlayerName, bool isRight)
        {
            List<SearchPlayers> finalsearchPlayers = new();
            string json;

            var searchPlayers = await HiRezAPI.SearchPlayer(PlayerName);
            if (searchPlayers.Count != 0)
            {
                foreach (var pl in searchPlayers)
                {
                    if (pl.Name.ToLowerInvariant() == PlayerName.ToLowerInvariant())
                    {
                        finalsearchPlayers.Add(pl);
                    }
                }
            }

            if (finalsearchPlayers.Count >= 1)
            {
                if (finalsearchPlayers[0].privacy_flag == "y")
                {
                    if (isRight)
                    {
                        return $"{finalsearchPlayers[0].Name} <:Hidden:591666971234402320>";
                    }
                    return $"<:Hidden:591666971234402320> {finalsearchPlayers[0].Name}";
                }
                else
                {
                    json = await HiRezAPI.GetPlayerStatus(finalsearchPlayers[0].player_id);
                    var playerStatus = JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(json);
                    if (playerStatus[0].status == 3 && playerStatus[0].Match != 0)
                    {
                        if (isRight)
                        {
                            MatchId2 = playerStatus[0].Match;
                        }
                        else
                        {
                            MatchId1 = playerStatus[0].Match;
                        }
                    }
                    if (isRight)
                    {
                        return $"{finalsearchPlayers[0].Name}[{playerStatus[0].status_string}]";
                    }
                    return $"[{playerStatus[0].status_string}]{finalsearchPlayers[0].Name}";
                }
            }
            else
            {
                return $"{PlayerName}[Not Found]";
            }
        }
    }
}
