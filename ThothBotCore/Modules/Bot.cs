using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using ThothBotCore.Utilities;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Modules
{
    public class Bot : InteractiveBase<SocketCommandContext>
    {
        readonly HiRezAPI hirezAPI = new HiRezAPI();

        [Command("help", true, RunMode = RunMode.Async)]
        [Summary("List of all available commands.")]
        [Alias("commands", "command", "cmd", "comamands", "h")]
        public async Task Help([Remainder] string commandName = null)
        {
            string prefix = Credentials.botConfig.prefix;
            if (GetServerConfig(Context.Guild.Id).Result.Count > 0)
            {
                if (GetServerConfig(Context.Guild.Id).Result[0].prefix != "!!")
                {
                    var conf = await GetServerConfig(Context.Guild.Id);
                    prefix = conf[0].prefix;
                }
            }

            var helpEmbed = HelpCommand.GetHelpEmbed(Global.commandService, commandName, prefix);
            try
            {
                await ReplyAsync(embed: helpEmbed);
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
            int totalUsers = 0;
            foreach (var guild in Context.Client.Guilds)
            {
                totalUsers += guild.Users.Count;
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
            //https://api.github.com/repos/EasyThe/ThothBotCore
            //updated_at
            var embed = new EmbedBuilder();
            embed.WithAuthor(author =>
            {
                author
                    .WithName("About Thoth Bot")
                    .WithIconUrl(Constants.botIcon);
            });
            embed.WithDescription($"Creator: EasyThe#2836 ({Connection.Client.GetUser(171675309177831424).Mention})");
            embed.WithColor(Constants.DefaultBlueColor);

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Statistics";
                x.Value = $":stopwatch: Uptime: {GetUptime()}\n" +
                $":chart_with_upwards_trend: Servers: {Connection.Client.Guilds.Count}\n" +
                $":busts_in_silhouette: Users: {totalUsers}\n" +
                $":1234: Commands Run: {Global.CommandsRun}";
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Thoth Database";
                x.Value = $":video_game: Players: {PlayersInDbCount()[0]}\n" +
                $":link: Linked Players: {LinkedPlayersInDBCount()[0]}\n" +
                $":loudspeaker: Status Update Subs: {CountOfStatusUpdatesActivatedInDB()[0]}\n" +
                $"<:Gods:567146088985919498> Smite Patch Version: {patch}";
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Links";
                x.Value = $"[Bot Invite](https://discordapp.com/oauth2/authorize?client_id=454145330347376651&permissions=537259072&scope=bot) | " +
                $"[Support Server]({Constants.SupportServerInvite})\n" +
                $"[Website](http://thothbot.tk) | " +
                $"[Privacy Policy](http://thothbot.tk/privacy-policy.html)\n" +
                $"[PayPal](https://www.paypal.me/EasyThe) | " +
                $"[Referral Link to SMITE Store](https://link.xsolla.com/M43fjVPi)";
            });
            embed.WithFooter(x =>
            {
                x.Text = $"Discord.NET {DiscordConfig.Version} | {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
            });
            await ReplyAsync("", false, embed.Build());
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
                await Database.SetPrefix(Context.Guild.Id, Context.Guild.Name, prefix);
                // Consider adding a check if the prefix was set successfully.
                await Context.Channel.SendMessageAsync($"Prefix for **{Context.Guild.Name}** set to `{prefix}`");
            }
        }

        [Command("feedback")]
        [Summary("If you have got feedback for the bot, this is the command.")]
        public async Task FeedbackCommand([Remainder] string FeedbackMessage)
        {
            await Reporter.SendFeedback(FeedbackMessage, Context.Message.Author);
            await ReplyAsync("♥ Thanks for the feedback! The bot owner got your message. ");
        }

        [Command("ping", true)]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync(Context.Client.Latency.ToString() + " ms");
        }

        [Command("thoth", true)]
        public async Task BasicInfoCommand()
        {
            await ReplyAsync($"My default prefix is `{Credentials.botConfig.prefix}`");
        }
        
        [Command("changelog", true, RunMode = RunMode.Async)]
        [Summary("Latest changes to ThothBot.")]
        public async Task ChangelogCommand()
        {
            var channel = Connection.Client.GetGuild(Constants.SupportServerID).GetTextChannel(567192879026536448);
            var messages = await channel.GetMessagesAsync(1).FlattenAsync().ConfigureAwait(false);

            var embed = new EmbedBuilder();
            embed.Title = "Latest Update of ThothBot";
            embed.WithColor(new Color(169,11,212));
            foreach (var item in messages)
            {
                embed.Description = item.Content;
            }

            await ReplyAsync(embed: embed.Build());
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

            if (time.Seconds != 0)
            {
                str += $"{time.Seconds}s";
            }

            return str;
        }
    }
}
