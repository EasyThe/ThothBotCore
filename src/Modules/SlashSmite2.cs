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
        static Random rnd = new();
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
                    foreach (var latestGod in latestGods)
                    {
                        sb.Append($"{latestGod.Emoji} {latestGod.Name}\n🔹 {latestGod.Title}\n🔹 {latestGod.Roles}\n\n");
                    }
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"Latest God{(latestGods.Count > 1 ? "s": "")}";
                        x.Value = sb.ToString();
                    });
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
                    await RespondAsync($"Something is not right... Please report this in my [support server]({Utilities.Constants.SupportServerInvite}).");
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
    }
}