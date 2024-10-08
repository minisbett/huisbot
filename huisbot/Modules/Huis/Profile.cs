using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using huisbot.Utilities;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the player command, displaying info about a player in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class ProfileCommandModule(OsuApiService osu, HuisApiService huis, PersistenceService persistence) : ModuleBase(huis, osu, persistence)
{
  [SlashCommand("profile", "Displays info about you or the specified player in the specified rework.")]
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

    // The live rework is required to be passed to GetHuisPlayerAsync down below, since it
    // requires information about the pp version to determine whether a player is up-to-date.
    HuisRework? live = await GetReworkAsync(HuisRework.LiveId.ToString());
    if (live is null)
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

    // Loop through the following logic, getting the player in both the local and the live rework.
    List<HuisPlayer> players = [];
    foreach (HuisRework _rework in new HuisRework[] { rework, live })
    {
      // Get the player in the current rework.
      HuisPlayer? player = await GetHuisPlayerAsync(user.Id, _rework, user.Name ?? "");
      if (player is null)
        return;

      // Add the player to the list.
      players.Add(player);
    }

    // Show the player embed.
    await FollowupAsync(embed: Embeds.Player(players[0], players[1], rework));
  }
}