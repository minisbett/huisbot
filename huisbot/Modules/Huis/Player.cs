using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the player command.
/// </summary>
public class PlayerCommandModule : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;

  public PlayerCommandModule(HuisApiService huis)
  {
    _huis = huis;
  }

  [SlashCommand("player", "Displays info about the specified player in the specified rework.")]
  public async Task HandleAsync(
    [Summary("playerId", "The osu! id of the player.")] int playerId,
    [Summary("reworkIdentifier", "An identifier for the rework, either the ID or code.")] string reworkIdentifier)
  {
    // Get all reworks and check whether the request was successful. If not, notify the user about an internal error.
    Rework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      await RespondAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }

    // Try to get the specified rework by the specified identifier. If it doesn't exist, notify the user.
    Rework? rework = reworks.FirstOrDefault(x => x.Id.ToString() == reworkIdentifier || x.Code == reworkIdentifier);
    if (rework is null)
    {
      await RespondAsync(embed: Embeds.Error($"The specified rework (`{reworkIdentifier}`) could not be found."));
      return;
    }

    // Get the player from the specified rework and check whether the request was successful. If not, notify the user about an internal error.
    Player? player = await _huis.GetPlayerAsync(playerId, rework.Id);
    if (player is null)
    {
      await RespondAsync(embed: Embeds.InternalError("Failed to get the player from the Huis API."));
      return;
    }

    // Show the player embed.
    await RespondAsync(embed: Embeds.Player(player, rework));
  }
}
