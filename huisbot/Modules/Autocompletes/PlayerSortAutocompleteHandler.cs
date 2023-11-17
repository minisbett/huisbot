using Discord.Interactions;
using Discord;
using huisbot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Modules.Autocompletes;

/// <summary>
/// Autocomplete for the sort parameter on the leaderboard player command.
/// </summary>
public class PlayerSortAutocompleteHandler : AutocompleteHandler
{
  public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Return the sorting options.
    return Task.FromResult(AutocompletionResult.FromSuccess(HuisPlayerSort.All.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}