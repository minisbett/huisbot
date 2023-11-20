using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Modules.Autocompletes;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the player command, displaying info about a player in a rework.
/// </summary>
public class PlayerCommandModule : HuisModuleBase
{
  public PlayerCommandModule(OsuApiService osu, HuisApiService huis, PersistenceService persistence) : base(huis, osu, persistence) { }

  [SlashCommand("profile", "Displays info about the specified player in the specified rework.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? playerId = null)
  {
    await DeferAsync();

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // If no player identifier was specified, try to get one from a link.
    if (playerId is null)
    {
      // Get the link and check whether the request was successful.
      OsuDiscordLink? link = await GetOsuDiscordLinkAsync();
      if (link is null)
        return;

      // Set the player identifier to the linked osu! user ID. After that, a player will be retrieved from the osu! API.
      playerId = link.OsuId.ToString();
    }

    // Get the osu! user.
    OsuUser? user = await GetOsuUserAsync(playerId);
    if (user is null)
      return;

    // Loop through the following logic once with local = true and local = false, getting the player in both the local and the live rework.
    // Then check whether the player is currently calculated/up-to-date. If not, the player will be queued and the user notified.
    // Otherwise, the players will be stored in the list below, which's two items are then being passed to the embed builder.
    List<HuisPlayer> players = new List<HuisPlayer>();
    foreach (int _reworkId in new int[] { rework.Id, HuisRework.LiveId })
    {
      // Get the player in the current rework.
      HuisPlayer? player = await GetHuisPlayerAsync(user.Id, _reworkId);
      if (player is null)
        return;

      // If the player was successfully received but is uncalculated, queue the player if necessary and notify the user.
      else if (!player.IsCalculated)
      {
        // Get the calculation queue.
        HuisQueue? queue = await GetHuisQueueAsync();
        if (queue is null)
          return;

        // Check whether the player is already queued. If so, notify the user.
        if (queue.Entries!.Any(x => x.UserId == user.Id && x.ReworkId == _reworkId))
        {
          await FollowupAsync(embed: Embeds.Neutral($"The player `{user.Name}` is currently being calculated in the __{(_reworkId == 1 ? "live" : "local")}__ " +
                                                    $"rework. Please try again later."));
          return;
        }

        // Queue the player.
        await QueuePlayerAsync(user, _reworkId);
        return;
      }

      // If the plyer is calculated, add it to the players list.
      players.Add(player);
    }

    // Show the player embed.
    await FollowupAsync(embed: Embeds.Player(players[0], players[1], rework));
  }
}