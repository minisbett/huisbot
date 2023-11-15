using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the player command, displaying info about a player in a rework.
/// </summary>
public class PlayerCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly OsuApiService _osu;
  private readonly HuisApiService _huis;

  public PlayerCommandModule(OsuApiService osu, HuisApiService huis)
  {
    _osu = osu;
    _huis = huis;
  }

  [SlashCommand("player", "Displays info about the specified player in the specified rework.")]
  public async Task HandleAsync(
    [Summary("player", "The osu! id or name of the player.")] string playerId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(PlayerAutocompleteHandler))] string reworkId)
  {
    // Get all reworks and check whether the request was successful. If not, notify the user about an internal error.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      await RespondAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }

    // Try to get the specified rework by the specified identifier. If it doesn't exist, notify the user.
    HuisRework? rework = reworks.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId);
    if (rework is null)
    {
      await RespondAsync(embed: Embeds.Error($"The specified rework (`{reworkId}`) could not be found."));
      return;
    }

    // If the specified player identifier is not a number, try to get the ID by the specified name.
    if (!int.TryParse(playerId, out int userId))
    {
      // Get the ID from the osu! api. If it failed or the user could not be found, notify the user.
      int? id = await _osu.GetIdByUsername(playerId);
      if (id is null)
      {
        await RespondAsync(embed: Embeds.InternalError("Failed to resolve the user ID the osu! API."));
        return;
      }
      else if (id == -1)
      {
        await RespondAsync(embed: Embeds.Error($"The specified user (`{playerId}`) could not be found."));
        return;
      }

      userId = id.Value;
    }

    // Get the player from the specified rework and check whether the request was successful. If not, notify the user about an internal error.
    HuisPlayer? player = await _huis.GetPlayerAsync(userId, rework.Id);
    if (player is null)
    {
      await RespondAsync(embed: Embeds.InternalError("Failed to get the player from the Huis API."));
      return;
    }

    // Show the player embed.
    await RespondAsync(embed: Embeds.Player(player, rework));
  }
}

/// <summary>
/// Autocomplete for the player command.
/// </summary>
public class PlayerAutocompleteHandler : AutocompleteHandler
{
  public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
    IParameterInfo pInfo, IServiceProvider services)
  {
    // Get all reworks and check whether the request was successful. If not, return an error result.
    HuisRework[]? reworks = await services.GetRequiredService<HuisApiService>().GetReworksAsync();
    if (reworks is null)
      return AutocompletionResult.FromError(PreconditionResult.FromError("Failed to get the reworks from the Huis API."));

    // Get all suggested reworks where the name or code contains the input value.
    string userInput = acInteraction.Data.Current.Value?.ToString()?.ToLower() ?? "";
    IEnumerable<HuisRework> suggestedReworks = reworks.Where(x => (x.Name?.ToLower().Contains(userInput) ?? false) || (x.Code?.ToLower().Contains(userInput) ?? false));

    // Return the first 25 reworks, since more are not supported due to Discord API limitations.
    return AutocompletionResult.FromSuccess(suggestedReworks.Select(x => new AutocompleteResult(x.Name, x.Code)).Take(25));
  }
}