using Discord;
using Discord.Interactions;
using huisbot.Enums;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace huisbot.Modules;

/// <summary>
/// A wrapper around the interaction module base for all modules.
/// This wrapper provides utility methods for parsing parameters like reworks or players.
/// </summary>
public class HuisModuleBase : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;
  private readonly OsuApiService _osu;
  private readonly PersistenceService _persistence;

  public HuisModuleBase(HuisApiService huis, OsuApiService osu = null!, PersistenceService persistence = null!)
  {
    _huis = huis;
    _osu = osu;
    _persistence = persistence;
  }

  /// <summary>
  /// Returns all available reworks on Huismetbenen. If it failed, the user will automatically
  /// be notified, unless the seeError parameter is set. In this case, this method returns null.
  /// </summary>
  /// <param name="showError">Bool whether an error should be displayed to the user.</param>
  /// <returns>The available reworks.</returns>
  public async Task<HuisRework[]?> GetReworksAsync(bool showError = true)
  {
    // Get all reworks and check whether the request was successful. If not, notify the user.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    reworks = reworks?.OrderBy(x => !x.IsLive).ThenBy(x => x.IsConfirmed).ThenBy(x => x.IsHistoric).ThenBy(x => !x.IsActive).ThenBy(x => !x.IsPublic).ToArray();
    if (reworks is null && showError)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));

    return reworks;
  }

  /// <summary>
  /// Returns a rework available on Huismetbenen based on the specified identifier.
  /// This can be the ID, code or fully qualified name. If it failed, the user will
  /// automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The identifier for the rework. (ID, code or fully qualified name)</param>
  /// <returns></returns>
  public async Task<HuisRework?> GetReworkAsync(string reworkId)
  {
    // Get all reworks, find the one with a matching identifier and check whether the process was successful. If not, notify the user.
    HuisRework[]? reworks = await GetReworksAsync();
    HuisRework? rework = reworks?.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    if (reworks is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
    else if (rework is null)
      await FollowupAsync(embed: Embeds.Error($"The rework `{reworkId}` could not be found."));

    return rework;
  }

  /// <summary>
  /// Returns the player sorting options based on the specified identifier. This can be the ID or display name.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="sortId">The identifier for the player sorting options. (ID or display name)</param>
  /// <returns>The player sorting options.</returns>
  public async Task<HuisPlayerSort?> GetPlayerSortAsync(string sortId)
  {
    // Try to find the specified sort by the specified identifier. If it doesn't exist, notify the user.
    HuisPlayerSort? sort = HuisPlayerSort.All.FirstOrDefault(x => x.Id == sortId || x.DisplayName == sortId);
    if (sort is null)
      await FollowupAsync(embed: Embeds.Error($"Invalid sort type `{sortId}`."));

    return sort;
  }

  /// <summary>
  /// Returns the score sorting options based on the specified identifier. This can be the ID or display name.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="sortId">The identifier for the player sorting options. (ID or display name)</param>
  /// <returns>The score sorting options.</returns>
  public async Task<HuisScoreSort?> GetScoreSortAsync(string sortId)
  {
    // Try to find the specified sort by the specified identifier. If it doesn't exist, notify the user.
    HuisScoreSort? sort = HuisScoreSort.All.FirstOrDefault(x => x.Id == sortId || x.DisplayName == sortId);
    if (sort is null)
      await FollowupAsync(embed: Embeds.Error($"Invalid sort type `{sortId}`."));

    return sort;
  }

  /// <summary>
  /// Returns the player rankings of the specified rework with the specified sorting options and filters.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <param name="sort">The sorting options.</param>
  /// <param name="onlyUpToDate">Bool whether only calculated, up-to-date players should be included.</param>
  /// <param name="hideUnranked">Bool whether unranked players (inactivity) should be hidden.</param>
  /// <returns>The global player rankings in the specified rework.</returns>
  public async Task<HuisPlayer[]?> GetPlayerRankingsAsync(int reworkId, HuisPlayerSort sort, bool onlyUpToDate, bool hideUnranked)
  {
    // Get the player rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisPlayer[]? players = await _huis.GetPlayerRankingsAsync(reworkId, sort, onlyUpToDate, hideUnranked);
    if (players is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the player rankings."));

    return players;
  }

  /// <summary>
  /// Returns the score rankings of the specified rework with the specified sorting options and filters.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <param name="sort">The sorting options.</param>
  /// <returns>The global score rankings in the specified rework.</returns>
  public async Task<HuisScore[]?> GetScoreRankingsAsync(int reworkId, HuisScoreSort sort)
  {
    // Get the score rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisScore[]? scores = await _huis.GetScoreRankingsAsync(reworkId, sort);
    if (scores is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the score rankings."));

    return scores;
  }

  /// <summary>
  /// Returns the statistic with the specified ID in the specified rework.
  /// </summary>
  /// <param name="statisticId">The statistic ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The statistic comparing the specified rework with the live pp system.</returns>
  public async Task<HuisStatistic?> GetStatisticAsync(string statisticId, int reworkId)
  {
    // Get the statistic from the Huis API and check whether the request was successful. If not, notify the user.
    HuisStatistic? statistic = await _huis.GetStatisticAsync(statisticId, reworkId);
    if (statistic is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the statistic `{statisticId}` from the Huis API."));

    return statistic;
  }

  /// <summary>
  /// Returns the link between the Discord user in the current context and an osu! account.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <returns>The link between the Discord user and an osu! account.</returns>
  public async Task<OsuDiscordLink?> GetOsuDiscordLinkAsync()
  {
    // Get the link and check whether the request was successful. If not, notify the user.
    OsuDiscordLink? link = await _persistence.GetOsuDiscordLinkAsync(Context.User.Id);
    if (link is null)
      await FollowupAsync(embed: Embeds.Error($"You have not linked your osu! account. Please use the `/link` command to link your account."));

    return link;
  }

  /// <summary>
  /// Returns the osu! user with the specified identifier. (ID or username)
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="userId">An identifier for the osu! user. (ID or username)</param>
  /// <returns>The osu! user.</returns>
  public async Task<OsuUser?> GetOsuUserAsync(string userId)
  {
    // Get the user from the osu! API. If it failed or the user was not found, notify the user.
    OsuUser? user = await _osu.GetUserAsync(userId);
    if (user is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to resolve the player from the osu! API."));
    else if (!user.WasFound)
      await FollowupAsync(embed: Embeds.Error($"No player with identifier `{userId}` could not be found."));

    return user;
  }

  /// <summary>
  /// Returns the Huis player with the specified ID in the specified rework.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="playerId">The player ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The Huis player.</returns>
  public async Task<HuisPlayer?> GetHuisPlayerAsync(int playerId, int reworkId)
  {
    string type = reworkId == HuisRework.LiveId ? "live" : "local";

    // Get the player from the Huis API. If it failed, notify the user.
    HuisPlayer? player = await _huis.GetPlayerAsync(playerId, reworkId);
    if (player is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the {type} player from the Huis API."));

    return player;
  }

  /// <summary>
  /// Returns the current calculation queue on Huismetbenen, including all reworks.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <returns>The calculation queue.</returns>
  public async Task<HuisQueue?> GetHuisQueueAsync()
  {
    // Get the queue and check whether the request was successful. If not, notify the user.
    HuisQueue? queue = await _huis.GetQueueAsync();
    if (queue is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the player calculation queue from the Huis API."));

    return queue;
  }

  /// <summary>
  /// Adds the specified player to the calculation queue in the specified rework.
  /// If it failed, the user will automatically be notified. In this case, this method returns false.
  /// </summary>
  /// <param name="player">The player.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>Bool whether the queueing was successful or not.</returns>
  public async Task<bool> QueuePlayerAsync(OsuUser player, int reworkId)
  {
    string type = reworkId == HuisRework.LiveId ? "live" : "local";

    // Queue the player and notify the user whether it was successful.
    bool queued = await _huis.QueuePlayerAsync(player.Id, reworkId);
    if (queued)
      await FollowupAsync(embed: Embeds.Success($"The player `{player.Name}` has been added to the {type} calculation queue."));
    else
      await FollowupAsync(embed: Embeds.InternalError($"Failed to queue the player `{player.Name}` in the {type} rework."));

    return queued;
  }

  /// <summary>
  /// Returns the beatmap by the specified identifier. (Beatmap ID or alias)
  /// </summary>
  /// <param name="beatmapId">An identifier for the beatmap. (Beatmap ID or alias)</param>
  /// <returns>The beatmap.</returns>
  public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmapId)
  {
    // If the identifier is not a number, try to find an alias.
    if (!beatmapId.All(char.IsDigit))
    {
      // Get the beatmap alias. If none could be found, notify the user. Otherwise replace the identifier.
      BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(beatmapId);
      if (alias is null)
        await FollowupAsync(embed: Embeds.Error($"Alias `{beatmapId}` could not be found."));
      else
        beatmapId = alias.Id.ToString();
    }

    // Get the beatmap from the osu! API. If it failed or the beatmap was not found, notify the user.
    OsuBeatmap? beatmap = await _osu.GetBeatmapAsync(int.Parse(beatmapId));
    if (beatmap is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the beatmap from the osu! API."));
    else if (!beatmap.WasFound)
      await FollowupAsync(embed: Embeds.Error($"No beatmap with ID `{beatmapId}` could not be found."));

    return beatmap;
  }

  /// <summary>
  /// Returns the top plays of the specified player in the specified rework from the Huis API.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="player">The player.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The top plays of the specified player in the specified rework.</returns>
  public async Task<HuisScore[]?> GetTopPlaysAsync(OsuUser player, int reworkId)
  {
    // Get the score rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisScore[]? scores = await _huis.GetTopPlaysAsync(player.Id, reworkId);
    if (scores is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the top plays of `{player.Name}`."));

    return scores;
  }

  /// <summary>
  /// Returns the difficulty rating of the specified beatmap in the specified ruleset with the specified mods.
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="rulesetId">The ruleset ID.</param>
  /// <param name="beatmapId">The beatmap ID.</param>
  /// <param name="mods">The mods.</param>
  /// <returns>The difficulty rating.</returns>
  public async Task<double?> GetDifficultyRatingAsync(int rulesetId, int beatmapId, string mods)
  {
    // Get the difficulty rating and check whether the request was successful. If not, notify the user.
    double? rating = await _osu.GetDifficultyRatingAsync(rulesetId, beatmapId, mods);
    if (rating is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the difficulty rating for the beatmap."));

    return rating;
  }
}