using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace ThothBotCore.Discord
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomRequireUserPermission : PreconditionAttribute
    {
        public GuildPermission? GuildPermission { get; }
        public ChannelPermission? ChannelPermission { get; }
        public string NotAGuildErrorMessage { get; set; }

        public CustomRequireUserPermission(GuildPermission guildPermission)
        {
            GuildPermission = guildPermission;
        }
        public CustomRequireUserPermission(ChannelPermission channelPermission)
        {
            ChannelPermission = channelPermission;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var guildUser = context.User as IGuildUser;

            if (GuildPermission.HasValue)
            {
                if (guildUser == null)
                {
                    return Task.FromResult(PreconditionResult.FromError(NotAGuildErrorMessage ?? "Command must be used in a guild channel."));
                }
                if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
                {
                    context.Interaction.RespondAsync($"Sorry, you need **{GuildPermission.Value}** permission in this server to do this. ", ephemeral: true);
                    return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"User requires guild permission {GuildPermission.Value}."));
                }
            }

            if (ChannelPermission.HasValue)
            {
                ChannelPermissions perms;
                if (context.Channel is IGuildChannel guildChannel)
                {
                    perms = guildUser.GetPermissions(guildChannel);
                }
                else
                {
                    perms = ChannelPermissions.All(context.Channel);
                }

                if (!perms.Has(ChannelPermission.Value))
                {
                    context.Interaction.RespondAsync($"Sorry, you need **{ChannelPermission.Value}** permission in this channel to do this. ", ephemeral: true);
                    return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"User requires channel permission {ChannelPermission.Value}."));
                }
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
