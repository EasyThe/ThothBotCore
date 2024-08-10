using Discord;
using Discord.Commands;
using Fergun.Interactive;
using System;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Tournament;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class Vulpis : ModuleBase<SocketCommandContext>
    {
        static Random rnd = new();
        public InteractiveService Interactive { get; set; }

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

        [Command("trackmatches")]
        public async Task VulpisChallongeMatchesChecker([Remainder]string tournament)
        {
            try
            {
                if (Context.Guild.Id != 321367254983770112 || !(TournamentUtilities.IsTournamentManagerCheck(Context)))
                {
                    return;
                }
                Global.TourneyName = tournament;
                var emb = await EmbedHandler.BuildDescriptionEmbedAsync("Starting the tracker...", Constants.VulpisColor);
                var message = await ReplyAsync(embed: emb);
                Global.TourneyTimerIDs = new[] { message.Channel.Id, message.Id, Context.Guild.Id };
                await TournamentTimer.StartTournamentTimer();
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
        [Command("stoptracker")]
        public async Task VulpisChallongeMatchesCheckerStop()
        {
            await ReplyAsync($"Hopefully stopped {Global.TourneyName}. Ask <@171675309177831424>");
            await TournamentTimer.StopTournamentTimer(Global.TourneyName);
        }
    }
}
