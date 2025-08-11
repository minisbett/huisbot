using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace huisbot.Modules.Huis;

/// <summary>
/// Autocomplete for the rework parameters on commands.
/// </summary>
public class ReworkAutocompleteHandler : AutocompleteHandler
{
  public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    IEnumerable<HuisRework>? reworks = await services.GetRequiredService<HuisApiService>().GetReworksAsync();
    if (reworks is null)
      return AutocompletionResult.FromError(PreconditionResult.FromError("Failed to get the reworks from the Huis API."));

    // Strip off all Onion-level reworks if the user does not have Onion-authorization.
    if (!ModuleBase.CheckOnion((SocketInteractionContext)context, services))
      reworks = reworks.Where(x => !x.IsOnionLevel);

    // Filter all reworks based on the input terms with a contains lookup on the name and code.
    string input = acInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";
    IEnumerable<HuisRework> suggestedReworks = reworks.Where(x => (x.Name?.Contains(input, StringComparison.CurrentCultureIgnoreCase) ?? false)
                                                               || (x.Code?.Contains(input, StringComparison.CurrentCultureIgnoreCase) ?? false));

    // Return the first 25 reworks, since more are not supported due to Discord API limitations.
    return AutocompletionResult.FromSuccess(suggestedReworks.Select(x => new AutocompleteResult(x.Name, x.Code)).Take(25));
  }
}