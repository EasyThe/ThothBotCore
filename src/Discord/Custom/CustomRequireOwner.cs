using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace ThothBotCore.Discord
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomRequireOwner : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            switch (context.Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    if (context.User.Id != application.Owner.Id)
                    {
                        await context.Interaction.RespondAsync("You're not allowed to do that, sorry!", ephemeral: true);
                        return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by the owner of the bot.");
                    }
                    return PreconditionResult.FromSuccess();
                default:
                    return PreconditionResult.FromError($"{nameof(CustomRequireOwner)} is not supported by this {nameof(TokenType)}.");
            }
        }
    }
}
