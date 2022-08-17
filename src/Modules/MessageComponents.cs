using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using static ThothBotCore.Models.Gods;
using System.Diagnostics;
using System.Reflection;

namespace ThothBotCore.Modules
{
    public class MessageComponents : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
    {
        public HiRezAPIv2 HiRez { get; set; }
        static Random rnd = new();

        [ComponentInteraction("playerselect-stats")]
        public async Task PlayerSelectStats(string[] selectedPlayer)
        {
            try
            {
                string player = selectedPlayer.FirstOrDefault();

                // Doing the stuff
                var playerStatus = await HiRez.GetPlayerStatusAsync(player);
                List<MatchPlayerDetails.PlayerMatchDetails> match = new();
                if (playerStatus[0].Match != 0)
                {
                    match = await HiRez.GetMatchPlayerDetailsAsync(playerStatus[0].Match.ToString());
                }

                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats(getPlayer[0].Name.Contains(']') ? getPlayer[0].Name.Split(']')[1] : getPlayer[0].Name);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });
                string mostplayed = await SlashSmite.CalculateTopGameModesForStatsAsync(HiRez, player);
                var godRanks = await HiRez.GetGodRanksAsync(player);

                // Generating the embed and sending to channel
                var embed = await EmbedHandler.PlayerStatsEmbed(
                    getPlayer,
                    godRanks,
                    await HiRez.GetPlayerAchievementsAsync(player),
                    playerStatus,
                    match);

                // Add Most played matches
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = $"<:matches:579604410569850891>Most Played Modes";
                    field.Value = mostplayed;
                });

                // Buttons
                var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 0, playerStatus[0].Match != 0);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed.Build();
                    x.Content = "";
                    x.Components = comps;
                });

                // Saving player to DB
                try
                {
                    await MongoConnection.SavePlayerAsync(getPlayer[0]).ConfigureAwait(false);
                    await MongoConnection.SavePlayerGodRanksAsync(godRanks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Text.WriteLine(ex.Message, ConsoleColor.Red, ConsoleColor.White);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("btn-st-*")]
        public async Task PlayerStatsButton(string player)
        {
            try
            {
                await DeferAsync();
                // Doing the stuff
                var playerStatus = await HiRez.GetPlayerStatusAsync(player);
                List<MatchPlayerDetails.PlayerMatchDetails> match = new();
                if (playerStatus[0].Match != 0)
                {
                    match = await HiRez.GetMatchPlayerDetailsAsync(playerStatus[0].Match.ToString());
                }

                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats($"{(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag)}'s stats");
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                string mostplayed = await SlashSmite.CalculateTopGameModesForStatsAsync(HiRez, player);
                var godRanks = await HiRez.GetGodRanksAsync(player);

                // Generating the embed and sending to channel
                var embed = await EmbedHandler.PlayerStatsEmbed(
                    getPlayer,
                    godRanks,
                    await HiRez.GetPlayerAchievementsAsync(player),
                    playerStatus,
                    match);

                // Add Most played matches
                embed.AddField(field =>
                {
                    field.IsInline = true;
                    field.Name = $"<:matches:579604410569850891>Most Played Modes";
                    field.Value = mostplayed;
                });

                // Buttons
                var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 0, playerStatus[0].Match != 0);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed.Build();
                    x.Content = "";
                    x.Components = comps;
                });

                // Saving player to DB
                try
                {
                    await MongoConnection.SavePlayerAsync(getPlayer[0]).ConfigureAwait(false);
                    await MongoConnection.SavePlayerGodRanksAsync(godRanks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Text.WriteLine(ex.Message, ConsoleColor.Red, ConsoleColor.White);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("playerselect-wp")]
        public async Task PlayerSelectWorshipers(string[] selectedPlayer)
        {
            try
            {
                string player = selectedPlayer.FirstOrDefault();
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }
                var getGodRanks = await HiRez.GetGodRanksAsync(player);
                // Generating the embed and sending to channel
                var embed = await EmbedHandler.BuildWorshipersEmbedAsync(getGodRanks, getPlayer[0]);

                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("btn-wp-*")]
        public async Task WorshipersButton(string player)
        {
            try
            {
                await DeferAsync();
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats($"{(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag)}'s god worshipers");
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var getGodRanks = await HiRez.GetGodRanksAsync(player);
                // Generating the embed and sending to channel
                var embed = await EmbedHandler.BuildWorshipersEmbedAsync(getGodRanks, getPlayer[0]);

                // Buttons
                var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 3);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = comps;
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("playerselect-wr")]
        public async Task PlayerSelectWinrates(string[] selectedPlayer)
        {
            try
            {
                string player = selectedPlayer.FirstOrDefault();
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }
                var getGodRanks = await HiRez.GetGodRanksAsync(player);
                // Generating the embed and sending to channel
                var embed = await EmbedHandler.BuildWinRatesEmbedAsync(getGodRanks, getPlayer[0]);

                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("btn-wr-*")]
        public async Task WinRatesButton(string player)
        {
            try
            {
                await DeferAsync();
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats($"{(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag)}'s god win rates");
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var getGodRanks = await HiRez.GetGodRanksAsync(player);
                // Generating the embed and sending to channel
                var embed = await EmbedHandler.BuildWinRatesEmbedAsync(getGodRanks, getPlayer[0]);

                // Buttons
                var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 4);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = comps;
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("playerselect-mh")]
        public async Task PlayerSelectMatchHistory(string[] selectedPlayer)
        {
            try
            {
                string player = selectedPlayer.FirstOrDefault();
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var matchHistory = await HiRez.GetMatchHistoryAsync(player.ToString());

                if (matchHistory.Count == 0)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync(Utilities.Constants.APIEmptyResponse, 254, 0, 0);
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = em;
                        x.Content = "";
                    });
                    return;
                }
                if (matchHistory[0].ret_msg != null && matchHistory[0].ret_msg.ToString().ToLowerInvariant().Contains("no match history"))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{(getPlayer[0].Name.Contains(']') ? getPlayer[0].Name.Split(']')[1] : getPlayer[0].Name)} has no recent matches.");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = emb;
                        x.Content = "";
                    });
                    return;
                }
                var embed = await EmbedHandler.BuildMatchHistoryEmbedAsync(matchHistory);

                // Match details select menu
                List<SelectMenuOptionBuilder> options = new();
                for (int i = 0; i < matchHistory.Count; i++)
                {
                    if (i == 24)
                    {
                        break;
                    }
                    string godemoji = Utils.FindGodEmoji(Utilities.Constants.GodsHashSet.ToList(), matchHistory[i].GodId);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Label = $"[{matchHistory[i].Win_Status}] {Text.GetQueueName(matchHistory[i].Match_Queue_Id, matchHistory[i].Queue)} - ID: {matchHistory[i].Match}",
                        Description = $"KDA: {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists}",
                        Emote = Emote.Parse(godemoji),
                        Value = matchHistory[i].Match.ToString()
                    });
                }
                var comp = new ComponentBuilder().WithSelectMenu("mdselect", placeholder: "Show match details", options: options);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = comp.Build();
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("btn-mh-*")]
        public async Task MatchHistoryButton(string player)
        {
            try
            {
                var getPlayer = await HiRez.GetPlayerAsync(player);
                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                        x.Content = "";
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats($"{(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag)}'s match history");
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var buttons = await ComponentsHandler.RichStatsButtonsAsync(player, 2);
                var matchHistory = await HiRez.GetMatchHistoryAsync(player.ToString());

                if (matchHistory.Count == 0)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync(Utilities.Constants.APIEmptyResponse, 254, 0, 0);
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = em;
                        x.Content = "";
                    });
                    return;
                }
                if (matchHistory[0].ret_msg != null && matchHistory[0].ret_msg.ToString().ToLowerInvariant().Contains("no match history"))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"{(getPlayer[0].Name.Contains(']') ? getPlayer[0].Name.Split(']')[1] : getPlayer[0].Name)} has no recent matches.");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = emb;
                        x.Content = "";
                        x.Components = buttons;
                    });
                    return;
                }
                var embed = await EmbedHandler.BuildMatchHistoryEmbedAsync(matchHistory);

                // Match details select menu
                List<SelectMenuOptionBuilder> options = new();
                for (int i = 0; i < matchHistory.Count; i++)
                {
                    if (i == 24)
                    {
                        break;
                    }
                    string godemoji = Utils.FindGodEmoji(Utilities.Constants.GodsHashSet.ToList(), matchHistory[i].GodId);
                    options.Add(new SelectMenuOptionBuilder()
                    {
                        Label = $"[{matchHistory[i].Win_Status}] {Text.GetQueueName(matchHistory[i].Match_Queue_Id, matchHistory[i].Queue)} - ID: {matchHistory[i].Match}",
                        Description = $"KDA: {matchHistory[i].Kills}/{matchHistory[i].Deaths}/{matchHistory[i].Assists}",
                        Emote = Emote.Parse(godemoji),
                        Value = matchHistory[i].Match.ToString()
                    });
                }
                
                var comp = ComponentBuilder.FromComponents(buttons.Components).WithSelectMenu("mdselect", placeholder: "Show match details", options: options);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = comp.Build();
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("playerselect-livemd")]
        public async Task PlayerSelectLiveMatchDetails(string[] selectedPlayer)
        {
            try
            {
                var player = selectedPlayer.FirstOrDefault();
                var getPlayer = await HiRez.GetPlayerAsync(player);

                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embedh.Build();
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats(getPlayer[0].Name.Contains(']') ? getPlayer[0].Name.Split(']')[1] : getPlayer[0].Name);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var playerstatus = await HiRez.GetPlayerStatusAsync(player);

                // Checking if the player is online and is in match
                if (playerstatus[0].status != 3 && playerstatus[0].Match == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{getPlayer[0].hz_player_name ?? getPlayer[0].hz_gamer_tag} is not in a match. [{playerstatus[0].status_string}]");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                    });
                    return;
                }

                var matchPlayerDetails = await HiRez.GetMatchPlayerDetailsAsync(playerstatus[0].Match.ToString());

                if (matchPlayerDetails == null || matchPlayerDetails.Count == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but this match seems to be unavailable to show.");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                    });
                    return;
                }
                if (matchPlayerDetails[0].ret_msg == null)
                {
                    var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed.Build();
                    });
                }
                else
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = null;
                        x.Content = matchPlayerDetails[0].ret_msg.ToString();
                    });
                    await Reporter.SlashRespondToCommandOnErrorAsync(null, Context, matchPlayerDetails[0].ret_msg.ToString());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("btn-lm-*")]
        public async Task LiveMatchDetailsButton(string player)
        {
            try
            {
                await DeferAsync();
                var getPlayer = await HiRez.GetPlayerAsync(player);

                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embedh.Build();
                    });
                    return;
                }

                var loading = await EmbedHandler.LoadingStats($"{(getPlayer[0].hz_player_name != null ? getPlayer[0].hz_player_name : getPlayer[0].hz_gamer_tag)}'s live match");
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var playerstatus = await HiRez.GetPlayerStatusAsync(player);

                // Checking if the player is online and is in match
                if (playerstatus[0].status != 3 && playerstatus[0].Match == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"{getPlayer[0].hz_player_name ?? getPlayer[0].hz_gamer_tag} is not in a match. [{playerstatus[0].status_string}]");
                    var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 1, true);

                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = comps;
                    });
                    return;
                }

                var matchPlayerDetails = await HiRez.GetMatchPlayerDetailsAsync(playerstatus[0].Match.ToString());

                if (matchPlayerDetails == null || matchPlayerDetails.Count == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but this match seems to be unavailable to show.");
                    var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 1, true);

                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = comps;
                    });
                    return;
                }
                if (matchPlayerDetails[0].ret_msg == null)
                {
                    var embed = await EmbedHandler.LiveMatchEmbed(matchPlayerDetails);
                    var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 1, true);

                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed.Build();
                        x.Components = comps;
                    });
                }
                else
                {
                    var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 1, true);
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = null;
                        x.Content = matchPlayerDetails[0].ret_msg.ToString();
                        x.Components = comps;
                    });
                    await Reporter.SlashRespondToCommandOnErrorAsync(null, Context, matchPlayerDetails[0].ret_msg.ToString());
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                var comps = await ComponentsHandler.RichStatsButtonsAsync(player, 1, true);

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = comps;
                });
            }
        }

        [ComponentInteraction("playerselect-mdlast")]
        public async Task PlayerSelectLastMatchDetails(string[] selectedPlayer)
        {
            try
            {
                string playerId = selectedPlayer.FirstOrDefault();
                var getPlayer = await HiRez.GetPlayerAsync(playerId);

                if (getPlayer[0].ret_msg != null && getPlayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy flag"))
                {
                    var embedh = await EmbedHandler.HiddenProfileEmbed("*");
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embedh.Build());
                    return;
                }

                var loading = await EmbedHandler.LoadingStats(getPlayer[0].Name.Contains(']') ? getPlayer[0].Name.Split(']')[1] : getPlayer[0].Name);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Content = "";
                    x.Embed = loading.Build();
                });

                var matchHistory = await HiRez.GetMatchHistoryAsync(playerId);
                if (matchHistory.Count == 0)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync(Utilities.Constants.APIEmptyResponse, 254, 0, 0);
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = em);
                    return;
                }
                int mID = matchHistory[0].Match;

                if (mID == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"There are no recent matches in record.");
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                    return;
                }
                // We have a match ID, we go for it
                var matchDetails = await HiRez.GetMatchDetailsAsync(mID.ToString());

                if (matchDetails.Count == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but this match seems to be unavailable.");
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                }

                if (matchDetails.Count == 1 && matchDetails[0].ret_msg != null)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync(matchDetails[0].ret_msg.ToString(), $"MatchID: {mID}", 255);
                    await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = embed);
                    return;
                }
                var finalembed = await EmbedHandler.BuildMatchDetailsEmbedAsync(matchDetails);
                await Context.Interaction.ModifyOriginalResponseAsync(x => x.Embed = finalembed);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                });
            }
        }

        [ComponentInteraction("playerselect-link")]
        public async Task PlayerSelectLink(string[] selectedPlayer)
        {
            try
            {
                var player = selectedPlayer.FirstOrDefault();
                var getplayer = await HiRez.GetPlayerAsync(player);

                if (getplayer != null && getplayer.Count == 1 && getplayer[0].ret_msg is string && getplayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy"))
                {
                    await Context.Interaction.UpdateAsync(async x =>
                    {
                        x.Embed = (await EmbedHandler.HiddenProfileEmbed("*")).Build();
                    });
                    return;
                }
                
                var getplayerstatus = await HiRez.GetPlayerStatusAsync(player);
                string randomString = Text.GenerateString(10);
                string statusString = $":eyes: **{getplayerstatus[0].status_string}**";

                if (getplayerstatus[0].status == 0)
                {
                    statusString = $":eyes: **Last Login:** " +
                        $"{(getplayer[0].Last_Login_Datetime != "" ? Text.RelativeTimestamp(DateTime.Parse(getplayer[0].Last_Login_Datetime, CultureInfo.InvariantCulture)) : "n/a")}";
                }

                var embed = new EmbedBuilder();
                embed.WithAuthor(Context.Interaction.User);
                embed.WithColor(Constants.FeedbackColor);
                embed.WithTitle(getplayer[0].hz_player_name + " " + getplayer[0].hz_gamer_tag);
                embed.WithDescription($"<:level:529719212017451008>**Level**: {getplayer[0].Level}\n" +
                    $"📅 **Account Created**: " +
                    $"{(getplayer[0].Created_Datetime != "" ? Text.LongDateTimestamp(DateTime.Parse(getplayer[0].Created_Datetime, CultureInfo.InvariantCulture)) : "n/a")}\n" +
                    $"💭 **Personal Status Message:** {getplayer[0].Personal_Status_Message}\n" +
                    $"⌛ **Playtime:** {getplayer[0].HoursPlayed} hours\n" +
                    $"{statusString}\n\n" +
                    $"**If this is your account, change your __Personal Status Message__ to: ```\n{randomString}``` so we can be sure " +
                    $"it's your account. You can change it to your previous status message after linking is completed." +
                    $"\nWhen you've changed your Personal Status Message, press the \"Done\" button under this message.**");
                embed.ImageUrl = "https://media.discordapp.net/attachments/528621646626684928/656237343405244416/Untitled-1.png";

                var button = new ComponentBuilder().WithButton("Done", $"completelinking-{player}-{randomString}", ButtonStyle.Success, Emoji.Parse("👌"));
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed.Build();
                    x.Components = button.Build();
                    x.Content = "";
                });
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = null;
                });
            }
        }

        [ComponentInteraction("mdselect")]
        public async Task MatchDetailsSelect(string[] matchIdArray)
        {
            var matchId = matchIdArray.FirstOrDefault();
            var loading = await EmbedHandler.LoadingStats($"match with ID: `{matchId}`");
            await Context.Interaction.UpdateAsync(x =>
            {
                x.Content = "";
                x.Embed = loading.Build();
            });
            var matchDetails = await HiRez.GetMatchDetailsAsync(matchId);
            if (matchDetails.Count == 0)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the API sent an empty response which most of the time means the match" +
                    " is not available anymore.\n" +
                    "You can try Smite.Guru instead", $"MatchID: {matchId}");
                await FollowupAsync(embed: embed, ephemeral: true);
                return;
            }

            if (matchDetails.Count == 1 && matchDetails[0].ret_msg != null)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync(matchDetails[0].ret_msg.ToString(), $"MatchID: {matchId}", 255);
                await FollowupAsync(embed: embed, ephemeral: true);
                return;
            }

            var finalembed = await EmbedHandler.BuildMatchDetailsEmbedAsync(matchDetails);
            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = finalembed;
            });
        }

        [ComponentInteraction("startlinking")]
        public async Task StartLinking()
        {
            var isAlive = await SlashSmite.IsSmiteApiAlive(HiRez);
            if (!isAlive)
            {
                var unavailableEmbed = await EmbedHandler.BuildDescriptionEmbedAsync(
                        "Sorry, we cannot link your accounts, because the Hi-Rez API is unavailable right now. Please try again later.", 163);
                await RespondAsync(embed: unavailableEmbed, ephemeral: true);
                return;
            }
            // check if the user is already linked
            var dbCheck = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Interaction.User.Id);
            if (dbCheck != null && dbCheck.discordID != 0)
            {
                var getplayer = await HiRez.GetPlayerAsync(dbCheck._id.ToString());
                var getplayerstatus = await HiRez.GetPlayerStatusAsync(dbCheck._id.ToString());

                var em = await EmbedHandler.BuildAlreadyLinkedEmbedAsync(getplayer, getplayerstatus);
                var button = new ComponentBuilder().WithButton($"Unlink", "unlink", ButtonStyle.Danger, Emoji.Parse("✖️"));
                await RespondAsync(embed: em, components: button.Build(), ephemeral: true);
                return;
            }
            var mb = new ModalBuilder("Thoth Linking", "startlinkingmodal")
                .AddTextInput("SMITE in-game name",
                              "msg",
                              TextInputStyle.Short,
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("completelinking-*-*")]
        public async Task CompleteLinking(string playerId, string randomstring)
        {
            try
            {
                var getplayer = await HiRez.GetPlayerAsync(playerId);
                if (getplayer == null || getplayer.Count == 0)
                {
                    var embed = await EmbedHandler.BuildDescriptionEmbedAsync("The Hi-Rez API is unavailable. Please try again.", Constants.ErrorColor);
                    await RespondAsync(embed: embed, ephemeral: true);
                    return;
                }

                if (getplayer[0].ret_msg != null && getplayer[0].ret_msg.ToString().Contains("privacy"))
                {
                    var embed = new EmbedBuilder();
                    embed.WithDescription($"This player is hidden. " +
                                $"Please unhide your profile by unchecking the \"Hide my Profile\" checkbox under the Profile tab in SMITE and try again.");
                    embed.WithImageUrl("https://cdn.discordapp.com/attachments/528621646626684928/951230342579232778/unknown.png");
                    await RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }
                if (getplayer[0].Personal_Status_Message.ToLowerInvariant().Contains(randomstring))
                {
                    var existingSpecials = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Interaction.User.Id);
                    PlayerSpecial playerSpecial = new() { _id = getplayer[0].ActivePlayerId, discordID = Context.Interaction.User.Id };
                    await MongoConnection.SavePlayerSpecialsAsync(playerSpecial);
                    var embed = new EmbedBuilder();
                    embed.WithAuthor(Context.Interaction.User);
                    embed.WithTitle(getplayer[0].hz_player_name + " " + getplayer[0].hz_gamer_tag);
                    embed.WithDescription($":tada: Congratulations, you've successfully linked your Discord and SMITE accounts in Thoth's Database. :heart:");
                    embed.WithColor(Constants.SuccessColor);
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Components = null;
                        x.Embed = embed.Build();
                    });
                }
                else
                {
                    string pmsg = "empty";
                    if (getplayer[0].Personal_Status_Message != null && getplayer[0].Personal_Status_Message.Length > 0)
                    {
                        pmsg = getplayer[0].Personal_Status_Message;
                    }
                    var embed = new EmbedBuilder();
                    embed.WithDescription($"We were not able to confirm that this account is yours because the personal status message of " +
                        $"**{getplayer[0].hz_player_name + " " + getplayer[0].hz_gamer_tag}** is `{pmsg}`.");
                    await RespondAsync(embed: embed.Build(), ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed, ephemeral: true);
            }
        }

        [ComponentInteraction("unlink")]
        public async Task UnlinkAccounts()
        {
            var db = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Context.Interaction.User.Id);
            if (db == null)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync($"You haven't linked any accounts in the database.");
                await RespondAsync(embed: embed, ephemeral: true);
                return;
            }
            await MongoConnection.UnlinkPlayerAsync(Context.Interaction.User.Id);
            var em = await EmbedHandler.BuildDescriptionEmbedAsync($"{Context.Interaction.User} just unlinked an account.", 0, 0, 254);
            await Reporter.SendEmbedToBotLogsChannel(em.ToEmbedBuilder());
            em = await EmbedHandler.BuildDescriptionEmbedAsync("You have successfully unlinked your account!");
            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = em;
                x.Components = null;
            });
        }

        [ComponentInteraction("abilities-*")]
        public async Task GodAbilityInfoInteraction(string selectedGodId)
        {
            var godId = Convert.ToInt32(selectedGodId);
            var god = await MongoConnection.GetGodByIDAsync(godId);
            var buttons = await ComponentsHandler.GodsAbilitiesButtonsAsync(god);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Components = buttons;
            });
        }

        [ComponentInteraction("godinfo-main-*")]
        public async Task GodInfoMainPage(string selectedGodId)
        {
            var godId = Convert.ToInt32(selectedGodId);
            var god = await MongoConnection.GetGodByIDAsync(godId);
            var buttons = await ComponentsHandler.GodsInfoButtonsAsync(god.id);
            var embed = await EmbedHandler.BuildMainGodPageEmbedAsync(god);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = buttons;
            });
        }

        [ComponentInteraction("abi-*-*")]
        public async Task AbilityInfoButtonInteraction(string godId, string abilityNum)
        {
            var god = Constants.GodsHashSet.FirstOrDefault(x => x.id == Convert.ToInt32(godId));
            int abilityInt = Convert.ToInt32(abilityNum);
            var embed = new EmbedBuilder();
            Ability ability = null;
            switch (abilityNum)
            {
                case "1": ability = god.Ability_1; break;
                case "2": ability = god.Ability_2; break;
                case "3": ability = god.Ability_3; break;
                case "4": ability = god.Ability_4; break;
                case "5": ability = god.Ability_5; break;
                default: break;
            }
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godIcon_URL);
                author.WithUrl($"https://www.smitegame.com/gods/{Text.URLifyGodName(god.Name)}");
            });
            embed.WithTitle($"{(abilityInt != 5 ? abilityInt : "Passive")}. {ability.Summary}");
            embed.WithDescription(ability.Description.itemDescription.description + "\n\n");
            for (int z = 0; z < ability.Description.itemDescription.menuitems.Count; z++)
            {
                if (ability.Description.itemDescription.menuitems[z].value.Length != 0)
                {
                    embed.Description += $"🔹**{ability.Description.itemDescription.menuitems[z].description}** " +
                        $"{ability.Description.itemDescription.menuitems[z].value}\n";
                }
            }
            for (int a = 0; a < ability.Description.itemDescription.rankitems.Count; a++)
            {
                if (ability.Description.itemDescription.rankitems[a].value.Length != 0)
                {
                    embed.Description += $"🔹**{ability.Description.itemDescription.rankitems[a].description}** " +
                        $"{ability.Description.itemDescription.rankitems[a].value}\n";
                }
            }
            if (ability.Description.itemDescription.cooldown.Length != 0)
            {
                embed.Description += $"🔹**Cooldown:** {ability.Description.itemDescription.cooldown}\n";
            }
            if (ability.Description.itemDescription.cost.Length != 0)
            {
                embed.Description += $"🔹**Cost:** {ability.Description.itemDescription.cost}\n";
            }
            embed.WithThumbnailUrl(ability.URL);
            if (ability.DomColor != 0)
            {
                embed.WithColor(new Color((uint)ability.DomColor));
            }

            var buttons = await ComponentsHandler.GodsAbilitiesButtonsAsync(god, abilityInt);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = buttons;
            });
        }

        [ComponentInteraction("abiyt-*")]
        public async Task AbilityVideoButtonInteraction(string videoId)
        {
            try
            {
                await RespondAsync($"https://youtu.be/{videoId}", ephemeral: true);
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context, $"AbiYT: ID:{videoId}");
                await RespondAsync(embed: embed, ephemeral: true);
            }
        }

        [ComponentInteraction("skins-*")]
        public async Task GodSkinsButtonInteraction(string godId)
        {
            var id = Convert.ToInt32(godId);
            if (id == 0)
            {
                await RespondAsync("Invalid god id.");
                return;
            }
            var god = await MongoConnection.GetGodByIDAsync(id);
            var skins = god.Skins;
            if (skins == null || skins.Count == 0)
            {
                skins = god.Skins;
            }
            else
            {
                god.Skins = skins;
            }
            EmbedBuilder embed = new();
            embed.WithColor(new Color((uint)god.DomColor));
            embed.WithAuthor(author =>
            {
                author.WithName(god.Name);
                author.WithIconUrl(god.godIcon_URL);
                author.WithUrl($"https://www.smitegame.com/gods/{Text.URLifyGodName(god.Name)}");
            });
            embed.WithThumbnailUrl(god.godIcon_URL);
            embed.WithTitle($"Skins for {god.Name}");
            var nz = from s in god.Skins
                     group s by s.obtainability into g
                     select new { obtainability = g.Key, Skins = g.ToList() };
            StringBuilder sb = new();
            StringBuilder sbb = new();
            
            foreach (var item in nz)
            {
                sbb.Clear();
                sb.Append($"{item.Skins.Count} **{item.obtainability}**, ");
                foreach (var s in item.Skins)
                {
                    sbb.AppendLine($"{s.skin_name} {(s.price_favor != 0 ? $"<:Favor:962805459528601650> **{s.price_favor}** " : "")}" +
                        $"{(s.price_gems != 0 ? $"<:Gems:962805934776799253> **{s.price_gems}** " : "")}");
                }
                embed.AddField(x =>
                {
                    x.IsInline = embed.Fields.Count != 2;
                    x.Name = $"{item.obtainability} skins";
                    x.Value = sbb.ToString();
                });
            }
            
            embed.WithDescription($"There are {sb.Remove(sb.Length -2, 1)} skins for {god.Name}");
            embed.WithFooter($"👀 You can use the select menu under this message to check the card art of the skins.");

            // select menu and back button
            var comps = await ComponentsHandler.GodsSkinsSelectMenuAsync(god);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = comps;
            });
        }

        [ComponentInteraction("skin")]
        public async Task SkinCardArtSelectMenuInteraction(string[] selected)
        {
            var godId = Convert.ToInt32(selected[0].Split('-')[0]);
            var skinId = Convert.ToInt32(selected[0].Split('-')[1]);
            var god = await MongoConnection.GetGodByIDAsync(godId);
            var skins = god.Skins;

            GodSkinModel skin = god.Skins.Find(x => x.skin_id1 == skinId);
            if (skin == null)
            {
                await RespondAsync("Skin not found.", ephemeral: true);
                return;
            }
            var embed = Context.Interaction.Message.Embeds.FirstOrDefault().ToEmbedBuilder();
            embed.WithDescription($"{skin.obtainability}\n" +
                $"{(skin.price_favor != 0 ? $"<:Favor:962805459528601650> **{skin.price_favor}** " : "")}" +
                $"{(skin.price_gems != 0 ? $"<:Gems:962805934776799253> **{skin.price_gems}** " : "")}");
            embed.Fields.Clear();
            embed.WithTitle($"{skin.skin_name} {skin.god_name}");
            embed.WithImageUrl(skin.godSkin_URL);
            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
            });
        }

        [ComponentInteraction("btn-lore-*")]
        public async Task GodLoreButtonInteraction(string godId)
        {
            var id = Convert.ToInt32(godId);
            if (id == 0)
            {
                await RespondAsync("Invalid god id.", ephemeral: true);
                return;
            }
            var god = await MongoConnection.GetGodByIDAsync(id);
            await RespondAsync(embed: await EmbedHandler.BuildGodLorePageEmbedAsync(god),
                               components: await ComponentsHandler.GodsLoreButtonAsync(god.id));
        }

        [ComponentInteraction("related-items-select")]
        public async Task RelatedItemsSelectMenuInteraction(string[] selectedId)
        {
            int itemId = Convert.ToInt32(selectedId.FirstOrDefault());
            if (itemId != 0)
            {
                var item = await MongoConnection.GetSpecificItemByIDAsync(itemId);
                if (item != null && item.Count != 0)
                {
                    var embed = await EmbedHandler.BuildItemInfoEmbedAsync(item.FirstOrDefault());
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = embed;
                    });
                }
            }
            else
            {
                await Context.Interaction.RespondAsync("Something unexpected has happened. Please contact the developer of the bot.", ephemeral: true);
            }
        }

        [ComponentInteraction("feeds-serverstatus")]
        [CustomRequireUserPermission(GuildPermission.ManageGuild)]
        public async Task FeedsServerStatusSet(string[] id)
        {
            try
            {
                var selectedId = Convert.ToUInt64(id.FirstOrDefault());
                var guildSettings = await MongoConnection.GetGuildSettingsAsync(Context.Guild.Id);
                // get the feed settings
                var feeds = guildSettings?.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus);

                // if we disable feeds
                if (selectedId == 0)
                {
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).ChannelID = 0;
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).WebhookID = 0;
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).WebhookToken = null;

                    // save to db
                    await MongoConnection.SaveGuildSettingsAsync(guildSettings);

                    var em = await EmbedHandler.BuildDefaultFeedsPage("", Context.Guild.TextChannels.Count > 25);
                    var comp = await ComponentsHandler.FeedsSelectMenuAsync(guildSettings, Context);
                    await Context.Interaction.UpdateAsync(x =>
                    {
                        x.Embed = em;
                        x.Components = comp;
                    });
                    // update the message
                    return;
                }

                RestWebhook webhook = null;
                // get the selected channel
                var selectedChannel = Context.Guild.GetTextChannel(selectedId);
                // if the webhook is not in this channel
                var webhooksInChannel = await selectedChannel.GetWebhooksAsync();

                if (webhooksInChannel.Count != 0)
                {
                    foreach (var wh in webhooksInChannel)
                    {
                        if (wh.Creator.IsBot && wh.Creator.Id == Connection.Client.CurrentUser.Id)
                        {
                            webhook = wh;
                            break;
                        }
                    }
                }

                if (webhook == null)
                {
                    using var logo = new FileStream("Config/logo.png", FileMode.Open);
                    webhook = await selectedChannel.CreateWebhookAsync("Thoth Feeds", logo);
                }

                // if guild has never been in the db
                if (guildSettings == null)
                {
                    guildSettings = new GuildSettingsModel()
                    {
                        _id = Context.Guild.Id,
                        Feeds = new List<GuildSettingsModel.Feed>()
                        {
                            new GuildSettingsModel.Feed()
                            {
                            Type = GuildSettingsModel.FeedType.ServerStatus,
                            ChannelID = selectedChannel.Id,
                            WebhookID = webhook.Id,
                            WebhookToken = webhook.Token
                            }
                        }
                    };
                }
                else
                {
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).ChannelID = selectedChannel.Id;
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).WebhookID = webhook.Id;
                    guildSettings.Feeds.Find(x => x.Type == GuildSettingsModel.FeedType.ServerStatus).WebhookToken = webhook.Token;
                }

                await MongoConnection.SaveGuildSettingsAsync(guildSettings);

                var embed = await EmbedHandler.BuildDefaultFeedsPage(selectedChannel.Mention.ToString(), Context.Guild.TextChannels.Count > 25);
                var comps = await ComponentsHandler.FeedsSelectMenuAsync(guildSettings, Context);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Components = comps;
                });
            }
            catch (Exception ex)
            {
                string channelMention = "this channel";
                if (id.FirstOrDefault() != "0")
                {
                    var selectedChannel = Context.Guild.GetTextChannel(Convert.ToUInt64(id.FirstOrDefault()));
                    channelMention = selectedChannel.Mention.ToString();
                }
                if (ex.Message.Contains("Missing Permissions"))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"The bot is missing permissions for **\"Manage Webhooks\"** in {channelMention}. " +
                        "Please grant them and try again.", Constants.ErrorColor);
                    await RespondAsync(embed: emb, ephemeral: true);
                    return;
                }
                else if (ex.Message.Contains("The server responded with error 30007: Maximum number of webhooks reached (10)"))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync($"According to Discord, {channelMention} has reached maximum number of webhooks (10)." +
                        $"\nPlease delete any unused webhooks from the channel and try again or choose a different channel.\n" +
                        $"ℹ **Channels Followed** are also considered **Webhooks**!", Constants.ErrorColor);
                    await RespondAsync(embed: emb, ephemeral: true);
                    return;
                }
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed;
                    x.Content = "";
                    x.Components = null;
                });
            }
        }

        // About & Admin interactions ;)
        [ComponentInteraction("statistics")]
        public async Task BotStatisticsInteraction()
        {
            int totalUsers = 0;
            foreach (var guild in Connection.Client.Guilds)
            {
                totalUsers += guild.MemberCount;
            }
            string patch = "n/a";
            var patchInfo = await HiRez.GetPatchInfoAsync();
            patch = patchInfo.Version_string;
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("About Thoth Bot")
                    .WithIconUrl(Utilities.Constants.botIcon);
            });
            embed.WithDescription($"<:Developer:747217301006319737> Owner & Developer: EasyThe#2836 - <@171675309177831424>\n" +
                    $"⚖ Data provided by Hi-Rez. © {DateTime.Now.Year} [Hi-Rez Studios](https://www.hirezstudios.com/), Inc. All rights reserved.");
            embed.WithColor(Utilities.Constants.DefaultBlueColor);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Statistics";
                x.Value = $":stopwatch: **Uptime**: {SlashSmite.GetUptime()}\n" +
                $"⛓ **Shards Connected: **{Connection.shardsConnected.Count}\n" +
                $":chart_with_upwards_trend: **Servers**: {Connection.Client.Guilds.Count}\n" +
                $":busts_in_silhouette: **Users**: {totalUsers}\n" +
                $":1234: **Commands Run**: {Global.CommandsRun}\n" +
                $"⏳ **Discord Latency**: {Connection.Client.Latency}ms";
            });
            long playersCount = await MongoConnection.PlayersCount();
            long linkedCount = await MongoConnection.LinkedPlayersCount();
            var feedsStatusCount = MongoConnection.GetFeedGuildsAsync(GuildSettingsModel.FeedType.ServerStatus);
            var statusCount = feedsStatusCount.Where(x => x.Feeds.Exists(z => z.Type == GuildSettingsModel.FeedType.ServerStatus && z.WebhookID != 0)).ToList().Count;
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Thoth's Database";
                x.Value = $":video_game: **Players**: {playersCount}\n" +
                $":link: **Linked Players**: {linkedCount}\n" +
                $":loudspeaker: **SMITE Status Subs**: {statusCount}\n" +
                $"<:Gods:567146088985919498> **SMITE Version**: {patch}";
            });
            var settings = MongoConnection.GetBotSettings();
            string links = "";
            int counter = 0;
            foreach (var link in settings.AboutLinks)
            {
                if (counter == 2)
                {
                    counter = 0;
                    links += "\n";
                }
                links += $"[{link.Key}]({link.Value})";
                if (counter == 0)
                {
                    links += " | ";
                }
                counter++;
            }
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Links";
                x.Value = links;
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Discord.NET {DiscordConfig.Version} | {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            });

            // BUTTONS

            var buttons = await ComponentsHandler.AboutThothButtonsAsync(Context.User.Id == Utilities.Constants.OwnerID, 0);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = buttons;
            });
        }

        [ComponentInteraction("feedback")]
        public async Task FeedbackInteraction()
        {
            var mb = new ModalBuilder("Thoth Feedback Form", "thothfeedback")
                .AddTextInput("Feedback Message",
                              "msg",
                              TextInputStyle.Paragraph,
                              placeholder: Text.PlaceholderText(),
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("changelog")]
        public async Task ChangelogInteraction()
        {
            var embed = new EmbedBuilder
            {
                Title = "Latest Update of ThothBot",
                Color = Constants.FeedbackColor
            };
            embed.WithDescription(Constants.BotSettings.Changelog);

            // BUTTONS

            var buttons = await ComponentsHandler.AboutThothButtonsAsync(Context.User.Id == Utilities.Constants.OwnerID, 2);

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = buttons;
            });
        }

        [ComponentInteraction("shards")]
        public async Task ShardsInteraction()
        {
            StringBuilder sb = new();
            EmbedBuilder embed = new();
            embed.WithColor(Constants.DefaultBlueColor);
            foreach (var shard in Connection.Client.Shards)
            {
                var shardclient = Connection.Client.GetShard(shard.ShardId);
                int members = 0;
                foreach (var guild in shardclient.Guilds)
                {
                    members += guild.MemberCount;
                }
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Shard " + shard.ShardId;
                    x.Value = $"**{shard.ConnectionState}**\n**Guilds:** {shard.Guilds.Count}\n**Users:** {members}";
                });
            }

            await Context.Interaction.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
            });
        }

        [ComponentInteraction("ownermenu")]
        [CustomRequireOwner]
        public async Task OwnerMenuInteraction()
        {
            try
            {
                var buttons = await ComponentsHandler.AboutThothButtonsAsync(Context.User.Id == Utilities.Constants.OwnerID, 69);
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Components = buttons;
                });
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ComponentInteraction("reloadconst")]
        [CustomRequireOwner]
        public async Task ReloadConstantsInteraction()
        {
            Constants.ReloadConstants();
            await RespondAsync("Reloaded!", ephemeral: true);
        }

        [ComponentInteraction("badges")]
        [CustomRequireOwner]
        public async Task BadgesInteraction()
        {
            try
            {
                EmbedBuilder embed = new()
                {
                    Color = Constants.FeedbackColor
                };
                StringBuilder keys = new();
                StringBuilder badge = new();
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
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed.Build();
                });
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ComponentInteraction("hirezapi")]
        [CustomRequireOwner]
        public async Task HiRezAPIInteraction()
        {
            try
            {
                string ping = await HiRez.PingAsync();

                string[] pingRePreArr = ping.Split('"');
                string[] pingResArr = pingRePreArr[1].Split(' ');

                var dataUsed = await HiRez.GetDataUsedAsync();

                var embed = new EmbedBuilder();
                embed.WithAuthor(author =>
                {
                    author
                        .WithName("Hi-Rez API Data Used")
                        .WithIconUrl(Constants.botIcon);
                });
                embed.WithColor(new Color(0, 255, 0));
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

                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed.Build();
                });
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ComponentInteraction("tips")]
        [CustomRequireOwner]
        public async Task TipsInteraction()
        {
            try
            {
                var alltips = Constants.TipsList;
                EmbedBuilder embed = new()
                {
                    Color = Constants.FeedbackColor,
                    Author = new()
                    {
                        IconUrl = Constants.botIcon,
                        Name = "All tips"
                    }
                };
                StringBuilder main = new();

                int count = 1;
                foreach (var tip in alltips)
                {
                    main.AppendLine($"**{count}.** {tip.TipText}");
                    count++;
                }
                embed.WithDescription(main.ToString());
                await Context.Interaction.UpdateAsync(x =>
                {
                    x.Embed = embed.Build();
                });
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ComponentInteraction("lookup")]
        [CustomRequireOwner]
        public async Task LookupButtonInteraction()
        {
            var mb = new ModalBuilder("Player Lookup", "lookupsearchmodal")
                .AddTextInput("Discord ID",
                              "discordid",
                              TextInputStyle.Short,
                              required: false)
                .AddTextInput("Active Player ID",
                              "hirezid",
                              TextInputStyle.Short,
                              required: false);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("lookupplayeredit-*")]
        [CustomRequireOwner]
        public async Task LookupEditButtonInteraction(string playerId)
        {
            var player = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(Convert.ToInt32(playerId));
            var mb = new ModalBuilder($"Edit Player", $"lookupeditplayer-{playerId}")
                .AddTextInput("Discord ID",
                              "discordid",
                              value: player.discordID.ToString(),
                              required: false)
                .AddTextInput("Streamer bool",
                              "streamerbool",
                              value: player.streamer_bool.ToString(),
                              required: false)
                .AddTextInput("Streamer link",
                              "streamerlink",
                              value: player.streamer_link,
                              required: false)
                .AddTextInput("Pro bool",
                              "probool",
                              value: player.pro_bool.ToString(),
                              required: false)
                .AddTextInput("Special",
                              "special",
                              value: player.special,
                              required: false);
            await RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("senddmbyowner")]
        [CustomRequireOwner]
        public async Task SendDMByOwnerButtonInteraction() => await Context.Interaction.RespondWithModalAsync<ModalSubmissions.SendDMByOwnerModal>("senddmbyownermodal");

        [ComponentInteraction("senddmuserrespond")]
        public async Task SendDMUserRespondInteraction()
        {
            var mb = new ModalBuilder("Respond to direct message", "userresponsetodm")
                .AddTextInput("Message",
                              "msg",
                              TextInputStyle.Paragraph,
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("leave")]
        [CustomRequireOwner]
        public async Task LeaveGuildInteraction()
        {
            var mb = new ModalBuilder("Leave guild", "leavemodal")
                .AddTextInput("Guild ID",
                              "msg",
                              TextInputStyle.Short,
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("updatedb", runMode: RunMode.Async)]
        [CustomRequireOwner]
        public async Task UpdateDbInteraction()
        {
            try
            {
                await DeferAsync();
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "<a:updating:403035325242540032> Working on gods...";
                });
                var newGodList = await HiRez.GetGodsAsync();
                if (newGodList.Count < 5)
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content = $"Gods list returned from the API is {newGodList.Count}";
                    });
                    return;
                }
                var godsInDb = MongoConnection.GetAllGods();

                // Adding the emojis and domcolors from the old db to the new one
                foreach (var god in godsInDb)
                {
                    var found = newGodList.Find(x => x.id == god.id);
                    if (found != null)
                    {
                        found.Emoji = god.Emoji;
                        found.DomColor = god.DomColor;
                        found.Ability_1.DomColor = god.Ability_1.DomColor;
                        found.Ability_2.DomColor = god.Ability_2.DomColor;
                        found.Ability_3.DomColor = god.Ability_3.DomColor;
                        found.Ability_4.DomColor = god.Ability_4.DomColor;
                        found.Ability_5.DomColor = god.Ability_5.DomColor;
                        found.Ability_1.Emoji = god.Ability_1?.Emoji;
                        found.Ability_2.Emoji = god.Ability_2?.Emoji;
                        found.Ability_3.Emoji = god.Ability_3?.Emoji;
                        found.Ability_4.Emoji = god.Ability_4?.Emoji;
                        found.Ability_5.Emoji = god.Ability_5?.Emoji;
                        found.Ability_1.Video = god.Ability_1?.Video;
                        found.Ability_2.Video = god.Ability_2?.Video;
                        found.Ability_3.Video = god.Ability_3?.Video;
                        found.Ability_4.Video = god.Ability_4?.Video;
                        found.Ability_5.Video = god.Ability_5?.Video;
                        found.godHeader_URL = god.godHeader_URL;
                        found.Skins = god.Skins;
                    }
                    else
                    {
                        await ReplyAsync($"{god.Name} is missing from the API.");
                    }
                }

                // Missing Emoji?
                if (newGodList.Any(x => x.Emoji == null))
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content += $"\nFound Missing god emojis";
                    });
                    foreach (var god in newGodList)
                    {
                        if (god.Emoji == null)
                        {
                            god.Emoji = await Utils.AddNewGodEmojiInGuild(god);
                        }
                    }
                }
                Thread.Sleep(200);
                
                // Missing DomColor?
                if (newGodList.Any(x => x.DomColor == 0))
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content += $"\nFound Missing god domcolor";
                    });
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
                                    await Reporter.SendErrorAsync($"{god.Name} has missing icon.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                    }
                }

                // NOT TESTED ==========================================================================================

                // Ability videos & God Header images from Smite CMS API
                if (newGodList.Any(x => x.Ability_1.Video == null) ||
                    newGodList.Any(x => x.Ability_2.Video == null) ||
                    newGodList.Any(x => x.Ability_3.Video == null) ||
                    newGodList.Any(x => x.Ability_4.Video == null) ||
                    newGodList.Any(x => x.Ability_5.Video == null) ||
                    newGodList.Any(x => x.godHeader_URL == null))
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content += $"\nFound Missing ability videos or godheader images";
                    });
                    var allMissing1 = newGodList.FindAll(x => x.Ability_1.Video == null).ToList();
                    var allMissing2 = newGodList.FindAll(x => x.Ability_2.Video == null).ToList();
                    var allMissing3 = newGodList.FindAll(x => x.Ability_3.Video == null).ToList();
                    var allMissing4 = newGodList.FindAll(x => x.Ability_4.Video == null).ToList();
                    var allMissing5 = newGodList.FindAll(x => x.Ability_5.Video == null).ToList();
                    var allMissinghead = newGodList.FindAll(x => x.godHeader_URL == null).ToList();
                    Text.WriteLine($"ab1 {allMissing1.Count} ab2 {allMissing2.Count} ab3 {allMissing3.Count} " +
                        $"ab4 {allMissing4.Count} ab5 {allMissing5.Count} head {allMissinghead.Count}");
                    Text.WriteLine("- Starting ability videos & god header images");
                    for (int i = 1; i < 13; i++)
                    {
                        var result = await APIInteractions.GetGodAbilityVideoIDsByPageAsync(i);
                        foreach (var item in result)
                        {
                            var god = newGodList.Find(x => x.id.ToString() == item.acf.god_id);
                            if (god == null)
                            {
                                System.Console.WriteLine($"God with id {item.acf.god_id} is missing, skipping to next.");
                                continue;
                            }
                            if (god.Ability_1.Video == null ||
                                god.Ability_2.Video == null ||
                                god.Ability_3.Video == null ||
                                god.Ability_4.Video == null ||
                                god.Ability_5.Video == null ||
                                god.godHeader_URL == null)
                            {
                                god.Ability_1.Video = item.acf.abilitiy_video_1;
                                god.Ability_2.Video = item.acf.abilitiy_video_2;
                                god.Ability_3.Video = item.acf.abilitiy_video_3;
                                god.Ability_4.Video = item.acf.abilitiy_video_4;
                                god.Ability_5.Video = item.acf.ability_video_passive;
                                god.godHeader_URL = item.acf.god_header_image;
                                System.Console.WriteLine($"Added ability videos & godheader urls for {god.Name}");
                            }
                        }
                    }
                    Text.WriteLine("Completed.");
                }

                // Ability DomColor

                Text.WriteLine("Ability DomColor");
                if (newGodList.Any(x => x.Ability_1.DomColor == 0) ||
                    newGodList.Any(x => x.Ability_2.DomColor == 0) ||
                    newGodList.Any(x => x.Ability_3.DomColor == 0) ||
                    newGodList.Any(x => x.Ability_4.DomColor == 0) ||
                    newGodList.Any(x => x.Ability_5.DomColor == 0))
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content += $"\nFound Missing ability domcolor";
                    });
                    foreach (var god in newGodList)
                    {
                        // 1
                        if (god.Ability_1.DomColor == 0)
                        {
                            try
                            {
                                if (god.Ability_1.URL != "")
                                {
                                    var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(god.Ability_1.URL);
                                    var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                                    god.Ability_1.DomColor = (int)clr.RawValue;
                                }
                                else
                                {
                                    await Reporter.SendErrorAsync($"{god.Name} ability 1 error.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        // 2
                        if (god.Ability_2.DomColor == 0)
                        {
                            try
                            {
                                if (god.Ability_2.URL != "")
                                {
                                    var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(god.Ability_2.URL);
                                    var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                                    god.Ability_2.DomColor = (int)clr.RawValue;
                                }
                                else
                                {
                                    await Reporter.SendErrorAsync($"{god.Name} ability 2 error.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        // 3
                        if (god.Ability_3.DomColor == 0)
                        {
                            try
                            {
                                if (god.Ability_3.URL != "")
                                {
                                    var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(god.Ability_3.URL);
                                    var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                                    god.Ability_3.DomColor = (int)clr.RawValue;
                                }
                                else
                                {
                                    await Reporter.SendErrorAsync($"{god.Name} ability 3 error.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        // 4
                        if (god.Ability_4.DomColor == 0)
                        {
                            try
                            {
                                if (god.Ability_4.URL != "")
                                {
                                    var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(god.Ability_4.URL);
                                    var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                                    god.Ability_4.DomColor = (int)clr.RawValue;
                                }
                                else
                                {
                                    await Reporter.SendErrorAsync($"{god.Name} ability 4 error.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        // 5
                        if (god.Ability_5.DomColor == 0)
                        {
                            try
                            {
                                if (god.Ability_5.URL != "")
                                {
                                    var colors = await APIInteractions.GetDominantColorFromCloudVisionAsync(god.Ability_5.URL);
                                    var clr = new Color(colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.red,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.green,
                                        colors.responses.FirstOrDefault().imagePropertiesAnnotation.dominantColors.colors.FirstOrDefault().color.blue);

                                    god.Ability_5.DomColor = (int)clr.RawValue;
                                }
                                else
                                {
                                    await Reporter.SendErrorAsync($"{god.Name} ability 5 error.");
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        Text.WriteLine($"Ability DomColor for {god.Name} completed.");
                    }
                }

                // Ability Emojis
                if (newGodList.Any(x => x.Ability_1.Emoji == null || x.Ability_1.Emoji.Length == 0))
                {
                    await Context.Interaction.ModifyOriginalResponseAsync(x =>
                    {
                        x.Content += $"\nFound ability emojis";
                    });
                    Text.WriteLine("Ability Emojis");
                    foreach (var god in newGodList)
                    {
                        if (god.Ability_1.Emoji == null || god.Ability_1.Emoji.Length == 0)
                        {
                            try
                            {
                                Console.WriteLine(god.Name);
                                var emojis = await Utils.AddMissingAbilityEmojiAsync(god);
                                if (emojis.Length == 5)
                                {
                                    god.Ability_1.Emoji = emojis[0];
                                    god.Ability_2.Emoji = emojis[1];
                                    god.Ability_3.Emoji = emojis[2];
                                    god.Ability_4.Emoji = emojis[3];
                                    god.Ability_5.Emoji = emojis[4];
                                }
                            }
                            catch (Exception exxx)
                            {
                                await ReplyAsync($"{god.Name} {exxx.Message}");
                            }
                        }
                        Text.WriteLine($"Ability Emojis for {god.Name} added.");
                    }
                }

                // Skins
                Text.WriteLine("Skins");
                var settings = MongoConnection.GetBotSettings();
                var skins = await HiRez.GetGodSkinsAsync();
                if (skins.Count != 0)
                {
                    newGodList.ForEach(god => god.Skins = skins.FindAll(x => x.god_id == god.id).ToList()); // thats it i guess? wtf

                    // Top Skins
                    Text.WriteLine("Top Skins");
                    
                    if (skins != null || skins.Count == 1)
                    {
                        var topGods = from s in skins
                                      group s by s.god_id into g
                                      select new { godId = g.Key, Skins = g.ToList() };
                        var orderedTopGods = topGods.OrderByDescending(x => x.Skins.Count).ToList();
                        List<BotSettingsModel.Top> topSkins = new();
                        foreach (var god in orderedTopGods)
                        {
                            var foundGod = newGodList.Find(x => x.id == god.godId);
                            if (foundGod != null && foundGod.id != 0)
                            {
                                topSkins.Add(new BotSettingsModel.Top
                                {
                                    Name = foundGod.Name,
                                    Emoji = foundGod.Emoji,
                                    Count = god.Skins.Count
                                });
                            }
                        }
                        settings.Skins = topSkins;
                    }
                }

                Text.WriteLine("Top Pantheons");
                // Top Pantheons
                var topPantheons = from g in newGodList
                                   group g by g.Pantheon into p
                                   select new { Pantheon = p.Key, Gods = p.ToList() };
                var orderedPanth = topPantheons.OrderByDescending(x => x.Gods.Count).ToList();
                List<BotSettingsModel.Top> topPanth = new();
                foreach (var pant in orderedPanth)
                {
                    topPanth.Add(new BotSettingsModel.Top
                    {
                        Name = pant.Pantheon,
                        Emoji = Text.GetPantheonEmoji(pant.Pantheon.ToLowerInvariant()),
                        Count = pant.Gods.Count
                    });
                }
                settings.Pantheons = topPanth;
                await MongoConnection.SaveBotSettingsAsync(settings);

                // NOT TESTED ==========================================================================================

                StringBuilder sb = new();
                
                sb.AppendLine($"Count: {newGodList.Count}");
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = $"<a:updating:403035325242540032> {sb}";
                });
                // Saving the gods to the DB
                foreach (var god in newGodList)
                {
                    await MongoConnection.SaveGodAsync(god);
                }
                Thread.Sleep(200);
                // ITEMS ====================================================================================================
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "<a:updating:403035325242540032> Working on items...";
                });

                var newItemsList = await HiRez.GetItemsAsync();
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
                if (newItemsList.Any(x => x.DomColor == 0))
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

                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = "<:check:314349398811475968> Done!";
                });
            }
            catch (Exception ex)
            {
                await Reporter.SlashSendException(ex, Context, "");
            }
        }

        [ComponentInteraction("add-changelog")]
        [CustomRequireOwner]
        public async Task AddChangelog()
        {
            var mb = new ModalBuilder("Add Changelog", "add-changelog-modal")
                .AddTextInput("Changelog",
                              "msg",
                              TextInputStyle.Paragraph,
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }
        
        [ComponentInteraction("edit-globalerror")]
        [CustomRequireOwner]
        public async Task EditGlobalErrorInteraction()
        {
            var mb = new ModalBuilder($"Edit Global Error Message", "edit-globalerror-modal")
                .AddTextInput("Message",
                              "msg",
                              TextInputStyle.Paragraph,
                              placeholder: "'remove' to remove the message",
                              value: Global.ErrorMessageByOwner,
                              required: true);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }
        
        [ComponentInteraction("set-activity")]
        [CustomRequireOwner]
        public async Task SetActivityInteraction()
        {
            var mb = new ModalBuilder("Set Activity", "set-activity-modal")
                .AddTextInput("Activity text",
                              "first-input",
                              TextInputStyle.Short,
                              placeholder: "Do 'default' for default kek",
                              value: Connection.Client.Activity?.Details,
                              required: false)
                .AddTextInput("Stream link",
                              "second-input",
                              TextInputStyle.Short,
                              placeholder: "https://www.twitch.tv/smitegame",
                              required: false);

            await Context.Interaction.RespondWithModalAsync(mb.Build());
        }

        [ComponentInteraction("open-msgtoguild")]
        [CustomRequireOwner]
        public async Task OpenModalMessageToGuild() => await Context.Interaction.RespondWithModalAsync<ModalSubmissions.SendMessageByOwnerModal>("submit-msgtoguild");

        [ComponentInteraction("register-globally")]
        [CustomRequireOwner]
        public async Task RegisterGlobally()
        {
            try
            {
                await Global.interactionService.RegisterCommandsGloballyAsync(true);
                await RespondAsync("Done", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ComponentInteraction("btn-testing-random-stuff")]
        [CustomRequireOwner]
        public async Task TestingButton()
        {
            await DeferAsync();
            try
            {
               
                var md = JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(await File.ReadAllTextAsync("D:\\2021\\Thoth\\ssd\\getmatchdetails (1).json"));

                await FollowupAsync(embed: await EmbedHandler.BuildMatchDetailsEmbedAsync(md),
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync($"{ex.Message}\n" +
                    $"```csharp\n" +
                    $"{ex.StackTrace}" +
                    $"```", ephemeral: true);
            }
        }

        [ComponentInteraction("btn-nz")]
        [CustomRequireOwner]
        public async Task NZButton()
        {
            await DeferAsync();
            try
            {
                List<SelectMenuOptionBuilder> options = new();

                MethodInfo[] methodInfos = HiRez.GetType()
                    .GetMethods();

                foreach (var item in methodInfos)
                {
                    Console.WriteLine(item.Name);
                }
                
                var cb = new ComponentBuilder().WithSelectMenu("select-nz", options, "Select endpoint");
                var emb = new EmbedBuilder();
                emb.WithDescription("g");
                await FollowupAsync(".", embed: emb.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                await FollowupAsync(ex.Message, ephemeral: true);
            }
        }
    }
}
