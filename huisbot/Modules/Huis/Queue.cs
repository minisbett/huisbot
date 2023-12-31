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
public class QueueCommandModule : ModuleBase
{
  public QueueCommandModule(OsuApiService osu, HuisApiService huis, PersistenceService persistence) : base(huis, osu, persistence) { }

  [SlashCommand("queue", "Queues you or the specified player in the specified rework.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocomplete))] string reworkId,
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? playerId = null)
  {
    await DeferAsync();

    // Make sure the user is an onion.
    if (!IsOnion)
    {
      await FollowupAsync(embed: Embeds.NotOnion);
      return;
    }

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

    // Get the calculation queue.
    HuisQueue? queue = await GetHuisQueueAsync();
    if (queue is null)
      return;

    // Check whether the player is already queued. If so, notify the user.
    if (queue.Entries!.Any(x => x.UserId == user.Id && x.ReworkId == rework.Id))
    {
      await FollowupAsync(embed: Embeds.Neutral($"The player `{user.Name}` is currently being calculated in the specified rework. Please try again later."));
      return;
    }

    // Queue the player.
    await QueuePlayerAsync(user, rework.Id);

    // Asynchronously check whether the player is no longer in the queue and if so, notify the user.
    _ = Task.Run(async () =>
    {
      // Wait an initial 20 seconds, since it's not only pointless to check immediately,
      // but it also takes some time before the player appears in the queue.
      await Task.Delay(20000);

      // Wait until the player is no longer in the queue.
      while (true)
      {
        // Check if the player is still in the queue.
        queue = await GetHuisQueueAsync();
        if (queue is null)
          return;
        if (!queue.Entries!.Any(x => x.UserId == user.Id && x.ReworkId == rework.Id))
        {
          await FollowupAsync(embed: Embeds.Success($"`{user.Name}` has been successfully re-calculated."));
          break;
        }

        // Wait 3 seconds before checking again.
        await Task.Delay(TimeSpan.FromSeconds(3));
      }
    });
  }
}