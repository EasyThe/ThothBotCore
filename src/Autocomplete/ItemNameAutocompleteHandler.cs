using Discord;
using Discord.Interactions;
using System;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Utilities;

namespace ThothBotCore.Autocomplete
{
    public class ItemNameAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            try
            {
                var value = autocompleteInteraction.Data.Current.Value as string; // what the user managed to type into the textbox so far

                if (string.IsNullOrEmpty(value))
                    return AutocompletionResult.FromSuccess();

                var suggestions = Constants.ItemsHashSet.Where(x => x.DeviceName.ToLowerInvariant().Contains(value.ToLowerInvariant()));

                var autocompleteResults = suggestions.Select(s => new AutocompleteResult
                {
                    Name = s.DeviceName, // here's what will appear in the suggestions list
                    Value = s.DeviceName // here's what will actually go into the slashcommand argument on tapping the suggestion
                });

                return AutocompletionResult.FromSuccess(autocompleteResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return AutocompletionResult.FromSuccess();
            }
        }
    }
}
