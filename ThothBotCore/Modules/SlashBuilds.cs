using Discord;
using Discord.Interactions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Autocomplete;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Modules
{
    [Group("builds", "Builds created by the users of the bot")]
    public class SlashBuilds : InteractionModuleBase
    {
        [SlashCommand("create", "Create a build for a god")]
        public async Task SlashCreateBuildCommand(
            [Summary("GodName", "Full or partial name of the god you're making a build for")]
            [Autocomplete(typeof(GodNameAutocompleteHandler))] string GodName,
            [Summary("FirstItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string FirstItem,
            [Summary("SecondItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string SecondItem,
            [Summary("ThirdItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string ThirdItem,
            [Summary("FourthItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string FourthItem,
            [Summary("FifthItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string FifthItem,
            [Summary("SixthItem", "Full or partial name of the item")]
            [Autocomplete(typeof(ItemNameAutocompleteHandler))] string SixthItem)
        {
            try
            {
                await DeferAsync();

                var embed = new EmbedBuilder();
                var god = Constants.GodsHashSet.First(x => x.Name.ToLowerInvariant().Contains(GodName.ToLowerInvariant()));
                var items = Constants.ItemsHashSet;
                var finalBuild = new List<GetItems.Item>();

                //1
                var found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(FirstItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }
                //2
                found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(SecondItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }
                //3
                found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(ThirdItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }
                //4
                found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(FourthItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }
                //5
                found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(FifthItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }
                //6
                found = items.FirstOrDefault(x => x.DeviceName.ToLowerInvariant().Contains(SixthItem.ToLowerInvariant()));
                if (found != null)
                {
                    finalBuild.Add(found);
                }

                await FollowupAsync($"{GodName} - {FirstItem}, {SecondItem}, {ThirdItem}, {FourthItem}, {FifthItem}, {SixthItem}\n" +
                    $"{god.Name} - {finalBuild[0].DeviceName}, {finalBuild[1].DeviceName}, {finalBuild[2].DeviceName}, {finalBuild[3].DeviceName}, " +
                    $"{finalBuild[4].DeviceName}, {finalBuild[5].DeviceName}");
            }
            catch (System.Exception ex)
            {
                await FollowupAsync(ex.Message);
            }
        }

        [SlashCommand("top", "Check out the most upvoted builds")]
        public async Task TopBuildsCommand()
        {
            await RespondAsync("yeah!");
        }
    }
}
