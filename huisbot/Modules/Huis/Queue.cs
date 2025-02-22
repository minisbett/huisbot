using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the player command, displaying info about a player in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class QueueCommandModule(IServiceProvider services) : ModuleBase(services)
{
  [SlashCommand("queue", "Queues you or the specified player in the specified rework.")]
  public async Task HandleAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("user", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? userId = null)
  {
    await DeferAsync();

    // If no user identifier was specified, try to get one from a link.
    if (userId is null)
      if (await GetOsuDiscordLinkAsync() is OsuDiscordLink link)
        userId = link.OsuId.ToString();
      else
        return;

    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetOsuUserAsync(userId) is not OsuUser user) return;
    if (await GetHuisQueueAsync(rework.Id) is not int[] queue) return;

    // Ensure the user is not queued yet.
    if (queue.Contains(user.Id))
    {
      await FollowupAsync(embed: Embeds.Neutral($"The player `{user.Name}` is already queued in the specified rework."));
      return;
    }

    // Queue the player and asynchronously check whether the player is no longer in the queue, meaning re-calculation finished.
    if (!await QueuePlayerAsync(user, rework.Id, Context.User.Id)) return;
    _ = Task.Run(async () =>
    {
      // Wait an initial 10 seconds, since it's not only pointless to check immediately,
      // but it also takes some time before the player appears on the queue API endpoint.
      await Task.Delay(10000);

      while (true)
      {
        if (await GetHuisQueueAsync(rework.Id) is not int[] queue) return;

        if (!queue.Contains(user.Id))
        {
          await FollowupAsync(embed: Embeds.Success($"`{user.Name}` has been successfully re-calculated."));
          break;
        }

        await Task.Delay(3000);
      }
    });
  }
}