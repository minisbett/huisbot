﻿using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Services;
using huisbot.Utilities;
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
        // Get all reworks and check whether the request was successful. If not, return an error result.
        HuisRework[]? reworks = await services.GetRequiredService<HuisApiService>().GetReworksAsync();
        if (reworks is null)
            return AutocompletionResult.FromError(PreconditionResult.FromError("Failed to get the reworks from the Huis API."));

        // If the user does not have Onion-level authorization, strip off all Onion-level reworks.
        if (!await ModuleBase.IsOnionAsync((SocketInteractionContext)context))
            reworks = reworks.Where(x => !x.IsOnionLevel).ToArray();

        // Get all suggested reworks where the name or code contains the input value.
        string userInput = acInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";
        IEnumerable<HuisRework> suggestedReworks = reworks.Where(x => (x.Name?.ToLower().Contains(userInput) ?? false) || (x.Code?.ToLower().Contains(userInput) ?? false));

        // Return the first 25 reworks, since more are not supported due to Discord API limitations.
        return AutocompletionResult.FromSuccess(suggestedReworks.Select(x => new AutocompleteResult(x.Name, x.Code)).Take(25));
    }
}