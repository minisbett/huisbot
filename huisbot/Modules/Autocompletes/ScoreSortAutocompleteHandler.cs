using Discord.Interactions;
using Discord;
using huisbot.Models.Huis;
using huisbot.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using huisbot.Enums;

namespace huisbot.Modules.Autocompletes;

/// <summary>
/// Autocomplete for the sort parameter on the leaderboard score command.
/// </summary>
public class ScoreSortAutocompleteHandler : AutocompleteHandler
{
  public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Return the sorting options.
    return Task.FromResult(AutocompletionResult.FromSuccess(HuisScoreSort.All.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}