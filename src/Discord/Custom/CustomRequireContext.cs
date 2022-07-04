using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace ThothBotCore.Discord
{
    [Flags]
    public enum ContextType
    {
        Guild = 0x01,
        DM = 0x02,
        Group = 0x04
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CustomRequireContext : PreconditionAttribute
    {
        public ContextType Contexts { get; }

        public CustomRequireContext(ContextType contexts)
        {
            Contexts = contexts;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            bool isValid = false;

            if ((Contexts & ContextType.Guild) != 0)
                isValid = !context.Interaction.IsDMInteraction;
            if ((Contexts & ContextType.DM) != 0 && (Contexts & ContextType.Group) != 0)
                isValid = context.Interaction.IsDMInteraction;

            if (isValid)
                return PreconditionResult.FromSuccess();
            else
                await context.Interaction.RespondAsync("Sorry, this command can only be used in a server.", ephemeral: true);
                return PreconditionResult.FromError(ErrorMessage ?? $"Invalid context for command; accepted contexts: {Contexts}.");
        }
    }
}