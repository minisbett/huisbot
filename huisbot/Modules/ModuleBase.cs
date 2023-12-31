using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Enums;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Utility;
using huisbot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace huisbot.Modules;

/// <summary>
/// A wrapper around the interaction module base for all modules.
/// This wrapper provides utility methods for parsing parameters like reworks or players.
/// </summary>
public class ModuleBase : InteractionModuleBase<SocketInteractionContext>
{
  private readonly HuisApiService _huis;
  private readonly OsuApiService _osu;
  private readonly PersistenceService _persistence;

  public ModuleBase(HuisApiService huis = null!, OsuApiService osu = null!, PersistenceService persistence = null!)
  {
    _huis = huis;
    _osu = osu;
    _persistence = persistence;
  }

  /// <summary>
  /// Bool whether the user has the Onion role on the PP Discord, making them eligible to use Huis commands.
  /// </summary>
  public bool IsOnion
  {
    get
    {
#if DEBUG
      return true;
#endif

      // Get the PP Discord guild.
      SocketGuild guild = Context.Client.GetGuild(546120878908506119);

      // Check whether the user is in that guild and has the Onion role.
      SocketGuildUser user = guild.GetUser(Context.User.Id);
      return user != null && user.Roles.Any(x => x.Id == 577267917662715904);
    }
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
  /// <param name="rework">The rework.</param>
  /// <param name="name">The name of the user. This is dirty but necessary to display the error.</param>
  /// <returns>The Huis player.</returns>
  public async Task<HuisPlayer?> GetHuisPlayerAsync(int playerId, HuisRework rework, string name)
  {
    // Get the player from the Huis API. If it failed, notify the user.
    HuisPlayer? player = await _huis.GetPlayerAsync(playerId, rework);
    if (player is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the {(rework.IsLive ? "live" : "local")} player from the Huis API."));
    // If the player was successfully received but is outdated, notify the user.
    else if (player.IsOutdated)
      await FollowupAsync(embed: Embeds.Error($"`{name}` is outdated in the *{(rework.IsLive ? "live" : "specified")}* rework.\n" +
                                              $"Please use the `/queue` command to queue the player."));

    return (player?.IsOutdated ?? true) ? null : player;
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
      await FollowupAsync(embed: Embeds.Neutral($"`{player.Name}` has been queued. You will be notified once it completed."));
    else
      await FollowupAsync(embed: Embeds.InternalError($"Failed to queue the player `{player.Name}`."));

    return queued;
  }

  /// <summary>
  /// Returns the beatmap by the specified identifier. (Beatmap ID or alias)
  /// </summary>
  /// <param name="beatmapId">An identifier for the beatmap. (Beatmap ID or alias)</param>
  /// <returns>The beatmap.</returns>
  public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmapId)
  {
    // If the identifier is not a number, try to find a beatmap alias.
    if (!beatmapId.All(char.IsDigit))
    {
      // Get the beatmap alias. If none could be found, notify the user. Otherwise replace the identifier.
      IDAlias? alias = await _persistence.GetBeatmapAliasAsync(beatmapId);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"Beatmap alias `{beatmapId}` could not be found."));
        return null;
      }
      else
        beatmapId = alias.Id.ToString();
    }

    // Get the beatmap from the osu! API. If it failed or the beatmap was not found, notify the user.
    OsuBeatmap? beatmap = await _osu.GetBeatmapAsync(int.Parse(beatmapId));
    if (beatmap is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the beatmap from the osu! API."));
    else if (!beatmap.WasFound)
      await FollowupAsync(embed: Embeds.Error($"No beatmap with ID `{beatmapId}` could be found."));

    return (beatmap?.WasFound ?? false) ? beatmap : null;
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

  /// <summary>
  /// Returns the score by the specified identifier. (Score ID or alias)
  /// </summary>
  /// <param name="scoreId">An identifier for the score. (Score ID or alias)</param>
  /// <returns>The score.</returns>
  public async Task<OsuScore?> GetScoreAsync(int rulesetId, string scoreId)
  {
    // If the identifier is not a number, try to find a score alias.
    if (!scoreId.All(char.IsDigit))
    {
      // Get the score alias. If none could be found, notify the user. Otherwise replace the identifier.
      IDAlias? alias = await _persistence.GetScoreAliasAsync(scoreId);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"Score alias `{scoreId}` could not be found."));
        return null;
      }
      else
        scoreId = alias.Id.ToString();
    }
    // Get the score from the osu! API. If it failed or the score was not found, notify the user.
    OsuScore? score = await _osu.GetScoreAsync(rulesetId, long.Parse(scoreId));
    if (score is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the score from the osu! API."));
    else if (!score.WasFound)
      await FollowupAsync(embed: Embeds.Error($"No score with ID `{scoreId}` could be found."));

    return (score?.WasFound ?? false) ? score : null;
  }
}