using Discord;
using Discord.Interactions;
using huisbot.Models.Utility;

namespace huisbot.Modules.Autocompletes;

/// <summary>
/// Autocomplete for the sort parameter on the topplays command.
/// </summary>
public class ProfileScoresSortAutocomplete : AutocompleteHandler
{
  public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Return the sorting options.
    return Task.FromResult(AutocompletionResult.FromSuccess(Sort.ProfileScores.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}