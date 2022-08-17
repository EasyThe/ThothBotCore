using Discord;
using Discord.Interactions;
using System;
using System.Globalization;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    public class ModalSubmissions : InteractionModuleBase
    {
        public HiRezAPIv2 HiRez { get; set; }

        [ModalInteraction("thothfeedback")]
        public async Task FeedbackReceivedInteraction(OneParagraphModal feedback)
        {
            await Reporter.SendFeedback(feedback.Message, Context);
            var embed = new EmbedBuilder()
            {
                Title = $"Your feedback has been received. Thank you! 🥰",
                Author = new() { Name = Context.Interaction.User.Username, IconUrl = Context.Interaction.User.GetAvatarUrl() },
                Description = $">>> {feedback.Message}"
            };
            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [ModalInteraction("startlinkingmodal")]
        public async Task StartLinkingModalInteraction(OneShortModal modal)
        {
            try
            {
                var player = await SlashSmite.GetPlayerIDsByUsername(Context, HiRez, modal.Message);
                if (player.Count == 0)
                {
                    var embed = await EmbedHandler.ProfileNotFoundEmbed(modal.Message);
                    await RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }

                if ((string)player[0].ret_msg == "apidown")
                {
                    var embed = await Reporter.RespondToCommandOnErrorAsync(null, null, "apidown");
                    await RespondAsync(embed: embed, ephemeral: true);
                    return;
                }
                if ((string)player[0].ret_msg == "privacy")
                {
                    var embed = new EmbedBuilder();
                    embed.WithDescription($"{modal.Message} is hidden. " +
                                $"Please unhide your profile by unchecking the \"Hide my Profile\" checkbox under the Profile tab in SMITE and try again.");
                    embed.WithImageUrl("https://cdn.discordapp.com/attachments/528621646626684928/951230342579232778/unknown.png");

                    await RespondAsync(embed: embed.Build(), ephemeral: true);
                    return;
                }

                if (player.Count == 1)
                {
                    var getplayer = await HiRez.GetPlayerAsync(player[0].player_id.ToString());

                    if (getplayer != null && getplayer.Count == 1 && getplayer[0].ret_msg is string && getplayer[0].ret_msg.ToString().ToLowerInvariant().Contains("privacy"))
                    {
                        await RespondAsync(embed: (await EmbedHandler.HiddenProfileEmbed("*")).Build(), ephemeral: true);
                        return;
                    }
                    
                    var getplayerstatus = await HiRez.GetPlayerStatusAsync(player[0].player_id.ToString());
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

                    var button = new ComponentBuilder().WithButton("Done", $"completelinking-{player[0].player_id}-{randomString}", ButtonStyle.Success, Emoji.Parse("👌"));
                    await RespondAsync(embed: embed.Build(), components: button.Build(), ephemeral: true);
                }
                else
                {
                    var comps = await ComponentsHandler.MultiplePlayersSelectMenuAsync(player, "link");
                    await RespondAsync("Multiple players found, please select a player via the select menu:", components: comps, ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                var embed = await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
                await RespondAsync(embed: embed, ephemeral: true);
            }
        }

        // Owner 
        [ModalInteraction("lookupsearchmodal")]
        [CustomRequireOwner]
        public async Task LookupSearchReceivedInteraction(LookupSearchModal lookupSearchModal)
        {
            PlayerSpecial playerSpecial = new(); 
            if (lookupSearchModal.DiscordID.Length != 0)
            {
                playerSpecial = await MongoConnection.GetPlayerSpecialsByDiscordIdAsync(Convert.ToUInt64(lookupSearchModal.DiscordID));
            }
            else
            {
                playerSpecial = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(Convert.ToInt32(lookupSearchModal.ActivePlayerID));
            }

            if (playerSpecial == null)
            {
                await RespondAsync("The player was not found in the database.", ephemeral: true);
                // consider adding a button to link a player manually
                return;
            }

            var getplayer = await HiRez.GetPlayerAsync(playerSpecial._id.ToString());
            var getplayerstatus = await HiRez.GetPlayerStatusAsync(playerSpecial._id.ToString());

            if (getplayer[0].ret_msg is string)
            {
                await RespondAsync(getplayer[0].ret_msg.ToString());
                return;
            }

            var embed = await EmbedHandler.BuildPlayerLookupEmbedAsync(playerSpecial, getplayer, getplayerstatus);

            var component = new ComponentBuilder()
                    .WithButton("Edit",
                                $"lookupplayeredit-{playerSpecial._id}",
                                style: ButtonStyle.Secondary,
                                emote: Emoji.Parse("📝"));

            await RespondAsync(embed: embed, components: component.Build(), ephemeral: true);
        }

        [ModalInteraction("lookupeditplayer-*")]
        [CustomRequireOwner]
        public async Task LookupEditPlayerReceivedInteraction(string playerid, LookupEditModal lookupEditModal)
        {
            try
            {
                var mainPlayerSpecs = await MongoConnection.GetPlayerSpecialsByPlayerIdAsync(Convert.ToInt32(playerid));
                var playerSpecials = new PlayerSpecial()
                {
                    _id = Convert.ToInt32(playerid),
                    discordID = Convert.ToUInt64(lookupEditModal.DiscordID),
                    streamer_bool = Convert.ToBoolean(lookupEditModal.StreamerBool.Length == 0 ? "false" : lookupEditModal.StreamerBool),
                    pro_bool = Convert.ToBoolean(lookupEditModal.ProBool.Length == 0 ? "false" : lookupEditModal.StreamerBool),
                    streamer_link = lookupEditModal.StreamerLink.Length == 0 ? null : lookupEditModal.StreamerLink,
                    special = lookupEditModal.Special,
                    linkedDateTimeUTC = mainPlayerSpecs.linkedDateTimeUTC
                };
                await MongoConnection.SavePlayerSpecialsAsync(playerSpecials);

                var embed = await EmbedHandler.BuildPlayerLookupEmbedAsync(playerSpecials);

                await RespondAsync("Saved successfully!", embed: embed, ephemeral: true);
            }
            catch (Exception ex) 
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ModalInteraction("senddmbyownermodal")]
        [CustomRequireOwner]
        public async Task SendDMbyOwnerReceivedInteraction(SendDMByOwnerModal interaction)
        {
            try
            {
                IUser targetUser = await Connection.Client.Rest.GetUserAsync(Convert.ToUInt64(interaction.DiscordID));
                var channel = await targetUser.CreateDMChannelAsync();
                if (channel == null)
                {
                    await RespondAsync("Channel for this user cannot be found or created. :(", ephemeral: true);
                    return;
                }
                var embed = new EmbedBuilder();
                embed.WithAuthor(Context.Interaction.User);
                embed.Author.Url = Constants.SupportServerInvite;
                embed.WithColor(Constants.FeedbackColor);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "-------";
                    x.Value = $"If you want to answer to this message you can use the \"Respond\" button or " +
                    $"[join the support server]({Constants.SupportServerInvite}) of Thoth and chat with the developer directly!";
                });
                embed.WithDescription(interaction.Message);
                embed.WithFooter(x =>
                {
                    x.Text = "This message was sent by the developer of the bot";
                    x.IconUrl = Constants.botIcon;
                });
                var buttons = new ComponentBuilder().WithButton("Respond", "senddmuserrespond", ButtonStyle.Secondary, Emoji.Parse("📨"))
                                .WithButton("Support Server", style: ButtonStyle.Link, url: Constants.SupportServerInvite);

                var chnl = await targetUser.CreateDMChannelAsync();
                await chnl.SendMessageAsync(embed: embed.Build(), components: buttons.Build());
                await RespondAsync("Message sent!", embed: embed.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync(ex.Message, ephemeral: true);
            }
        }

        [ModalInteraction("userresponsetodm")]
        public async Task UserRespondToDMInteraction(OneParagraphModal interaction)
        {
            await Reporter.SendFeedback(interaction.Message, Context);
            await RespondAsync("Your response was received successfully!", ephemeral: true);
        }

        [ModalInteraction("leavemodal")]
        [CustomRequireOwner]
        public async Task LeaveGuildInteraction(OneShortModal interaction)
        {
            await Connection.Client.GetGuild(Convert.ToUInt64(interaction.Message)).LeaveAsync();
            await RespondAsync("Done!", ephemeral: true);
        }

        [ModalInteraction("add-changelog-modal")]
        [CustomRequireOwner]
        public async Task AddChangelogInteraction(OneParagraphModal interaction)
        {
            var settings = MongoConnection.GetBotSettings();
            settings.Changelog = interaction.Message;
            await MongoConnection.SaveBotSettingsAsync(settings);

            await Reporter.SendChangelog(settings.Changelog);
            Constants.ReloadConstants();
            await RespondAsync($"Changelog saved successfully!\n> {settings.Changelog}", ephemeral: true);
        }

        [ModalInteraction("edit-globalerror-modal")]
        [CustomRequireOwner]
        public async Task EditGlobalErrorInteraction(OneParagraphModal interaction)
        {
            if (interaction.Message == "remove")
            {
                Global.ErrorMessageByOwner = "";
            }
            else
            {
                Global.ErrorMessageByOwner = interaction.Message;
            }
            
            await RespondAsync($"Global Error Message set to\n> {Global.ErrorMessageByOwner}", ephemeral: true);
        }

        [ModalInteraction("set-activity-modal")]
        [CustomRequireOwner]
        public async Task SetActivityInteraction(TwoShortModal interaction)
        {
            try
            {
                if (interaction.SecondInput != null && interaction.SecondInput.Contains("http")) // streaming
                {
                    await Connection.Client.SetGameAsync(interaction.FirstInput, interaction.SecondInput, ActivityType.Streaming);
                }
                else if (interaction.FirstInput.Contains("default"))
                {
                    await Connection.Client.SetGameAsync($"{Credentials.botConfig.setGame} | {Connection.Client.Guilds.Count} servers", type: ActivityType.Playing);
                }
                else
                {
                    await Connection.Client.SetGameAsync(interaction.FirstInput, type: ActivityType.Playing);
                }
                
                await RespondAsync("👌👌👌", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Reporter.SlashRespondToCommandOnErrorAsync(ex, Context);
            }
        }

        [ModalInteraction("submit-msgtoguild")]
        [CustomRequireOwner]
        public async Task SubmitMessageToGuildFromModal(SendMessageByOwnerModal interaction)
        {
            try
            {
                var guild = Connection.Client.GetGuild(Convert.ToUInt64(interaction.GuildID));
                var channel = guild.GetTextChannel(Convert.ToUInt64(interaction.ChannelID));

                var embed = new EmbedBuilder();
                embed.WithAuthor(Context.Interaction.User);
                embed.Author.Url = Constants.SupportServerInvite;
                embed.WithColor(Constants.FeedbackColor);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "-------";
                    x.Value = $"If you want to answer to this message you can use the \"Respond\" button or " +
                    $"[join the support server]({Constants.SupportServerInvite}) of Thoth and chat with the developer directly!";
                });
                embed.WithDescription(interaction.Message);
                embed.WithFooter(x =>
                {
                    x.Text = "This message was sent by the developer of the bot";
                    x.IconUrl = Constants.botIcon;
                });
                var buttons = new ComponentBuilder().WithButton("Respond", "senddmuserrespond", ButtonStyle.Secondary, Emoji.Parse("📨"))
                                .WithButton("Support Server", style: ButtonStyle.Link, url: Constants.SupportServerInvite);

                await channel.SendMessageAsync(embed: embed.Build(), components: buttons.Build());
                await RespondAsync("Message sent successfully!", embed: embed.Build(), ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"Error: {ex.Message}", ephemeral: true);
            }
        }

        public class OneParagraphModal : IModal
        {
            public string Title { get; set; } = "Paragraph";
            [ModalTextInput("msg", TextInputStyle.Paragraph)]
            [RequiredInput(true)]
            public string Message { get; set; }
        }
        public class OneShortModal : IModal
        {
            public string Title { get; set; }
            [ModalTextInput("msg", TextInputStyle.Short)]
            [RequiredInput(true)]
            public string Message { get; set; }
        }
        public class LookupSearchModal : IModal
        {
            public string Title { get; set; }

            [InputLabel("Discord ID")]
            [ModalTextInput("discordid", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string DiscordID { get; set; }

            [InputLabel("Active Player ID")]
            [ModalTextInput("hirezid", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string ActivePlayerID { get; set; }
        }
        public class TwoShortModal : IModal
        {
            public string Title { get; set; }

            [ModalTextInput("first-input", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string FirstInput { get; set; }

            [ModalTextInput("second-input", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string SecondInput { get; set; }
        }
        public class LookupEditModal : IModal
        {
            public string Title { get; set; }

            [InputLabel("Discord ID")]
            [ModalTextInput("discordid", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string DiscordID { get; set; }

            [InputLabel("Streamer bool")]
            [ModalTextInput("streamerbool", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string StreamerBool { get; set; }

            [InputLabel("Streamer link")]
            [ModalTextInput("streamerlink", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string StreamerLink { get; set; }

            [InputLabel("Pro bool")]
            [ModalTextInput("probool", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string ProBool { get; set; }

            [InputLabel("Special")]
            [ModalTextInput("special", TextInputStyle.Short)]
            [RequiredInput(false)]
            public string Special { get; set; }
        }
        public class SendDMByOwnerModal : IModal
        {
            public string Title { get; set; } = "Send direct message to user";
            [InputLabel("Discord ID")]
            [ModalTextInput("discordid", TextInputStyle.Short)]
            [RequiredInput(true)]
            public string DiscordID { get; set; }
            [InputLabel("Message")]
            [ModalTextInput("message", TextInputStyle.Paragraph)]
            [RequiredInput(true)]
            public string Message { get; set; }
        }
        public class SendMessageByOwnerModal : IModal
        {
            public string Title { get; set; } = "Send message to guild channel";
            [InputLabel("Guild ID")]
            [ModalTextInput("guildid", TextInputStyle.Short)]
            [RequiredInput(true)]
            public string GuildID { get; set; }
            [InputLabel("Channel ID")]
            [ModalTextInput("channelid", TextInputStyle.Short)]
            [RequiredInput(true)]
            public string ChannelID { get; set; }
            [InputLabel("Message")]
            [ModalTextInput("message", TextInputStyle.Paragraph)]
            [RequiredInput(true)]
            public string Message { get; set; }
        }
    }
}
