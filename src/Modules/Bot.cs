using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Bot : ModuleBase<SocketCommandContext>
    {
        readonly HiRezAPI hirezAPI = new();
        private const string slash = "⚠Thoth is switching to Slash Commands! Please use ";
        public InteractiveService Interactive { get; set; }

        [Command("help", true, RunMode = RunMode.Async)]
        [Summary("List of all available commands.")]
        [Alias("commands", "command", "cmd", "comamands", "h")]
        public async Task Help([Remainder] string commandName = null)
        {
            string prefix = Credentials.botConfig.prefix;
            if (Context.Guild != null)
            {
                var serverConfig = await GetServerConfig(Context.Guild.Id);
                if (serverConfig.Count > 0)
                {
                    prefix = serverConfig[0].prefix;
                }
            }

            var helpEmbed = HelpCommand.GetHelpEmbed(Global.commandService, commandName, prefix);
            try
            {
                await ReplyAsync($"⚠Thoth is switching to Slash Commands! By the end of August 2022 the normal commands will stop functioning.", embed: helpEmbed);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("50013"))
                {
                    try
                    {
                        await ReplyAsync($"I need **Embed Links** permissions in this channel.");
                    }
                    catch (Exception)
                    {
                        IUser user = Connection.Client.GetUser(Context.Message.Author.Id);
                        await user.SendMessageAsync($"I don't have **Send Messages** or **Embed Links** permissions in #{Context.Channel.Name}.");
                    }
                }
                else
                {
                    await ReplyAsync("I am missing permissions in this channel.");
                }
            }
        }

        [Command("botstats", true, RunMode = RunMode.Async)]
        [Summary("Bot statistics, invite link, support server etc.")]
        [Alias("bi", "botinfo", "about", "info", "invite")]
        public async Task BotInfoCommand()
        {
            var recomm = await Connection.Client.GetRecommendedShardCountAsync();
            Text.WriteLine($"Recommended shard count: {recomm}");
            int totalUsers = 0;
            foreach (var guild in Connection.Client.Guilds)
            {
                totalUsers += guild.MemberCount;
            }
            string patch = "";
            try
            {
                string json = await hirezAPI.GetPatchInfo();
                HiRezAPI.PatchInfo patchInfo = JsonConvert.DeserializeObject<HiRezAPI.PatchInfo>(json);
                patch = patchInfo.version_string;
            }
            catch (Exception ex)
            {
                patch = "n/a";
                await Reporter.SendError($"Error in PatchInfo from **botinfo** command.\n{ex.Message}");
            }
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("About Thoth Bot")
                    .WithIconUrl(Constants.botIcon);
            });
            embed.WithDescription($"Creator: EasyThe#2836 - {Connection.Client.GetUser(171675309177831424).Mention}");
            embed.WithColor(Constants.DefaultBlueColor);

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Statistics";
                x.Value = $":stopwatch: **Uptime**: {GetUptime()}\n" +
                $"⛓ **Shards Connected: **{Connection.shardsConnected.Count}\n" +
                $":chart_with_upwards_trend: **Servers**: {Connection.Client.Guilds.Count}\n" +
                $":busts_in_silhouette: **Users**: {totalUsers}\n" +
                $":1234: **Commands Run**: {Global.CommandsRun}";
            });
            long playersCount = await MongoConnection.PlayersCount();
            long linkedCount = await MongoConnection.LinkedPlayersCount();
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Thoth Database";
                x.Value = $":video_game: **Players**: {playersCount}\n" +
                $":link: **Linked Players**: {linkedCount}\n" +
                $":loudspeaker: **Status Update Subs**: {CountOfStatusUpdatesActivatedInDB()[0]}\n" +
                $"<:Gods:567146088985919498> **SMITE Version**: {patch}";
            });
            var settings = MongoConnection.GetBotSettings();
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Links";
                x.Value = $"[Bot Invite]({settings.s[0]}) | " +
                $"[Support Server]({settings.s[3]})\n" +
                $"[Twitter](https://twitter.com/ThothDiscordBot) | " +
                $"[Website]({settings.s[1]})\n" +
                $"[Privacy Policy]({settings.s[2]}) | " +
                $"[PayPal](https://www.paypal.me/EasyTheBG)\n" +
                $"[Referral Link to SMITE Store](https://link.xsolla.com/M43fjVPi)";
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Discord.NET {DiscordConfig.Version} | {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n{slash}/about";
            });
            await ReplyAsync(embed: embed.Build());
        }

        [Command("prefix")] // Custom Prefix
        [Summary("Set custom prefix for your server.")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Owner")]
        [RequireOwner(Group = "Owner")]
        public async Task SetPrefix([Remainder] string prefix)
        {
            if (prefix.Length > 5)
            {
                await ReplyAsync("This prefix is too long.");
            }
            else
            {
                if (Context.Guild == null)
                {
                    var em = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but you cannot set a custom prefix when using the bot in DMs.", 254);
                    await ReplyAsync(embed: em);
                    return;
                }
                if (prefix.Contains('\''))
                {
                    var emb = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, but apostrophes (single quote) is not allowed to be set as a prefix.", 254);
                    await ReplyAsync(embed: emb);
                    return;
                }
                await Database.SetPrefix(Context.Guild.Id, prefix);
                // Consider adding a check if the prefix was set successfully.
                await Context.Channel.SendMessageAsync($"Prefix for **{Context.Guild.Name}** set to `{prefix}`\n{slash}`/` " +
                    $"from now on. **Custom prefixes and normal commands will stop working by the end of August 2022**");
            }
        }

        [Command("feedback")]
        [Summary("If you have got feedback for the bot, this is the command.")]
        public async Task FeedbackCommand([Name("Enter your feedback here")][Remainder] string FeedbackMessage)
        {
            await Reporter.SendFeedback(FeedbackMessage, Context);
            await ReplyAsync($"♥ Thanks for the feedback! The bot owner got your message.\n{slash}/about");
        }

        [Command("thoth", true)]
        public async Task BasicInfoCommand()
        {
            await ReplyAsync($"My default prefix is `{Credentials.botConfig.prefix}`\n{slash}`/` from now on!");
        }

        private static string GetUptime()
        {
            var time = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var str = "";

            if (time.Days != 0)
            {
                str += $"{time.Days}d ";
            }

            if (time.Hours != 0)
            {
                str += $"{time.Hours}h ";
            }

            if (time.Minutes != 0)
            {
                str += $"{time.Minutes}m ";
            }

            if (time.Seconds != 0 && time.Hours !>= 0 && time.Days !<= 0)
            {
                str += $"{time.Seconds}s";
            }

            return str;
        }
    }
}
