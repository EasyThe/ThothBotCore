using Discord;
using Discord.Interactions;
using System;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Autocomplete;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class SlashSmite2 : InteractionModuleBase
    {
        public HiRezAPIv2 HiRez { get; set; }

        //[SlashCommand("smite2", "The Battleground of the Gods has Evolved")]
        //public async Task SlashSMITE2Command()
        //{
        //    try
        //    {
        //        var embed = new EmbedBuilder();
        //        embed.WithColor(Constants.SMITE2GoldColor);
                
        //        embed.AddField(x =>
        //        {
        //            x.IsInline = true;
        //            x.Name = $"";
        //            x.Value = "";
        //        });

        //        embed.WithFooter(x =>
        //        {
        //            x.Text = "";
        //        });

        //        await RespondAsync(embed: embed.Build());
        //    }
        //    catch (Exception ex)
        //    {
        //        var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
        //        await RespondAsync(embed: embed);
        //    }
        //}

        [SlashCommand("gods2", "Overall information about the gods & skins in SMITE2 and current free god rotation.")]
        public async Task SlashGods2Command()
        {
            try
            {
                StringBuilder sb = new();
                var gods = MongoConnection.GetAllGods(true);

                if (gods.Count != 0)
                {
                    StringBuilder onRotation = new();
                    var embed = new EmbedBuilder();
                    var latestGods = gods.FindAll(x => x.latestGod == "y");

                    var onRot = gods.FindAll(x => x.OnFreeRotation == "true");
                    embed.WithColor(Constants.SMITE2GoldColor);
                    foreach (var god in gods)
                    {
                        sb.AppendLine($"{god.Emoji} {god.Name}");
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"Gods: {gods.Count}";
                        x.Value = sb.ToString();
                    });

                    sb.Clear();
                    if (latestGods != null && latestGods.Count != 0)
                    {
                        foreach (var latestGod in latestGods)
                        {
                            sb.Append($"{latestGod.Emoji} {latestGod.Name}\n🔹 {latestGod.Title}\n🔹 {latestGod.Roles}\n\n");
                        }
                        embed.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = $"Latest God{(latestGods.Count > 1 ? "s" : "")}";
                            x.Value = sb.ToString();
                        });
                    }
                    //if (sb.Length != 0)
                    //{
                    //    embed.AddField(x =>
                    //    {
                    //        x.IsInline = true;
                    //        x.Name = "Most Skins";
                    //        x.Value = sb.ToString();
                    //    });
                    //}
                    embed.WithFooter(x =>
                    {
                        x.Text = $"For God specific information use /god2";
                    });

                    await RespondAsync(embed: embed.Build());
                }
                else
                {
                    await RespondAsync($"Something is not right... Please report this in my [support server]({Constants.SupportServerInvite}).");
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("god2", "Provides information about the requested SMITE2 god.")]
        public async Task SlashGodInfoCommand([Summary("GodName", "Start typing the name of the god and you will get recommendations.")]
            [Autocomplete(typeof(SMITE2GodNameAutocompleteHandler))]string GodName)
        {
            try
            {
                string titleCaseGod = Text.ToTitleCase(GodName);
                var gods = await MongoConnection.GetGodByNameAsync(titleCaseGod, true);

                if (gods == null)
                {
                    await RespondAsync($"{titleCaseGod} was not found.", allowedMentions: AllowedMentions.None);
                }
                else
                {
                    var embed = await EmbedHandler.BuildMainSMITE2GodPageEmbedAsync(gods);

                    var buttons = await ComponentsHandler.Gods2InfoButtonsAsync(gods.id);

                    await RespondAsync(embed: embed,
                    allowedMentions: AllowedMentions.None,
                    components: buttons);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("bugs2", "Checks the SMITE 2 Known Issues Trello Board for current and fixed issues.")]
        public async Task SlashTrelloBoardCommand()
        {
            try
            {
                var embed = new EmbedBuilder();
                var result = await APIInteractions.GetSMITE2TrelloCards();

                StringBuilder topIssues = new();
                StringBuilder hotfixNotes = new();
                StringBuilder incominghotfix = new();
                StringBuilder alreadyFixedInLIVE = new();

                int patchCount = 0;
                // Appending the issues
                foreach (var item in result)
                {
                    // Top Issues
                    if (item.idList == "6626f5a181e79b9c170f012e")
                    {
                        topIssues.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Hotfix PatchNotes
                    if (item.idList == "66eae28467dff5302630ab44" && hotfixNotes.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024 && patchCount < 7)
                    {
                        hotfixNotes.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                        patchCount++;
                    }
                    // Incoming hotfix
                    if (item.idList == "665d158d655c9066c382e89a" && incominghotfix.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024)
                    {
                        incominghotfix.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                    // Fixed in LIVE
                    if (item.idList == "6626f5afc74f6ab41358b700" && alreadyFixedInLIVE.Length + $"🔹[{item.name}]({item.shortUrl})".Length < 1024)
                    {
                        alreadyFixedInLIVE.AppendLine($"🔹[{item.name}]({item.shortUrl})");
                    }
                }

                embed.WithAuthor(x =>
                {
                    x.Name = "SMITE 2 Known Issues Trello Board";
                    x.Url = "https://trello.com/b/mrW6CEFO/";
                    x.IconUrl = "https://cdn3.iconfinder.com/data/icons/popular-services-brands-vol-2/512/trello-512.png";
                });

                // Incoming Hotfix
                if (incominghotfix.Length != 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "🛠️ In Progress";
                        x.Value = incominghotfix.ToString();
                    });
                }

                // Already in LIVE
                if (alreadyFixedInLIVE.Length != 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "✨ Resolved Issues";
                        x.Value = alreadyFixedInLIVE.ToString();
                    });
                }

                // Hotfix Patch Notes
                if (hotfixNotes.ToString() != "")
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "⚙️ Live Patch Notes";
                        x.Value = hotfixNotes.ToString();
                    });
                }

                embed.WithColor(Constants.FeedbackColor);
                embed.WithTitle("🐛 Known Bugs");
                if (!(topIssues.ToString().Length > 2048))
                {
                    embed.WithDescription(topIssues.ToString());
                }
                else
                {
                    // Its longer than 2048
                    embed.WithDescription(Text.Truncate(topIssues.ToString(), 2048));
                }

                await RespondAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync("Sorry, the Trello API is down. Try visiting the [website](https://trello.com/b/d4fJtBlo/smite-community-issues) instead.");
                await RespondAsync(embed: embed);
                await Reporter.SendErrorAsync($"**Trello Error: **\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}