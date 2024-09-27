using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using huisbot.Utilities;

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
  /// Returns all available reworks on Huismetbenen.<br/>
  /// If it failed, the user will automatically be notified, unless the seeError parameter is set. In this case, this method returns null.
  /// </summary>
  /// <param name="showError">Bool whether an error should be displayed to the user.</param>
  /// <returns>The available reworks.</returns>
  public async Task<HuisRework[]?> GetReworksAsync(bool showError = true)
  {
    // Get all reworks and check whether the request was successful. If not, notify the user.
    HuisRework[]? reworks = await _huis.GetReworksAsync();
    if (reworks is null)
    {
      if (showError)
        await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));

      return null;
    }

    // Order the reworks by relevancy for the user and return them.
    return reworks.OrderBy(x => !x.IsLive).ThenBy(x => x.IsConfirmed).ThenBy(x => x.IsHistoric)
                   .ThenBy(x => !x.IsActive).ThenBy(x => !x.IsPublic).ToArray();
  }

  /// <summary>
  /// Returns a rework available on Huismetbenen based on the specified identifier (ID, code or fully qualified name).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The identifier for the rework. (ID, code or fully qualified name)</param>
  /// <returns>The Huis rework.</returns>
  public async Task<HuisRework?> GetReworkAsync(string reworkId)
  {
    // Get all reworks, find the one with a matching identifier and check whether the process was successful. If not, notify the user.
    HuisRework[]? reworks = await GetReworksAsync();
    HuisRework? rework = reworks?.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    if (reworks is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
    if (rework is null)
      await FollowupAsync(embed: Embeds.Error($"The rework `{reworkId}` could not be found."));
    // Disallow non-Onion users to access Onion-level reworks.
    else if (rework.IsOnionLevel && !await IsOnionAsync(Context))
    {
      await FollowupAsync(embed: Embeds.NotOnion);
      return null;
    }

    return rework;
  }

  /// <summary>
  /// Returns the sorting option based on the specified identifier (ID or display name) and list of sorts.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="sortId">The identifier for the player sorting options. (ID or display name)</param>
  /// <param name="sortId">The list of sorting options available.</param>
  /// <returns>The sorting option.</returns>
  public async Task<Sort?> GetSortAsync(string sortId, Sort[] allSorts)
  {
    // Try to find the specified sort by the specified identifier. If it doesn't exist, notify the user.
    Sort? sort = allSorts.FirstOrDefault(x => x.Id == sortId || x.DisplayName == sortId);
    if (sort is null)
      await FollowupAsync(embed: Embeds.Error($"Invalid sort option `{sortId}`."));

    return sort;
  }

  /// <summary>
  /// Returns the player rankings of the specified rework with the specified sorting option and filters.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <param name="sort">The sorting option.</param>
  /// <param name="onlyUpToDate">Bool whether only calculated, up-to-date players should be included.</param>
  /// <param name="hideUnranked">Bool whether unranked players (inactivity) should be hidden.</param>
  /// <returns>The global player rankings in the specified rework.</returns>
  public async Task<HuisPlayer[]?> GetPlayerRankingsAsync(int reworkId, Sort sort, bool onlyUpToDate, bool hideUnranked)
  {
    // Get the player rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisPlayer[]? players = await _huis.GetPlayerRankingsAsync(reworkId, sort, onlyUpToDate, hideUnranked);
    if (players is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the player rankings."));

    return players;
  }

  /// <summary>
  /// Returns the score rankings of the specified rework with the specified sorting option and filters.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <param name="sort">The sorting option.</param>
  /// <returns>The global score rankings in the specified rework.</returns>
  public async Task<HuisScore[]?> GetScoreRankingsAsync(int reworkId, Sort sort)
  {
    // Get the score rankings in the rework and check whether the request was successful. If not, notify the user.
    HuisScore[]? scores = await _huis.GetScoreRankingsAsync(reworkId, sort);
    if (scores is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the score rankings."));

    return scores;
  }

  /// <summary>
  /// Returns the statistic with the specified ID in the specified rework.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
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
  /// Returns the link between the Discord user in the current context and an osu! account.<br/>
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
  /// Returns the osu! user with the specified identifier (ID or username).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="userId">An identifier for the osu! user. (ID or username)</param>
  /// <returns>The osu! user.</returns>
  public async Task<OsuUser?> GetOsuUserAsync(string userId)
  {
    // Get the user from the osu! API. If it failed or the user was not found, notify the user.
    NotFoundOr<OsuUser>? user = await _osu.GetUserAsync(userId);
    if (user is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to resolve the player from the osu! API."));
    else if (!user.Found)
      await FollowupAsync(embed: Embeds.Error($"No player with identifier `{userId}` could not be found."));

    return (user?.Found ?? false) ? user : null!;
  }

  /// <summary>
  /// Returns the Huis player with the specified ID in the specified rework.<br/>
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
  /// Returns the current calculation queue on Huismetbenen, including all reworks.<br/>
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
  /// Adds the specified player to the calculation queue in the specified rework.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns false.<br/><br/>
  /// A requester identifier needs to be provided and will be passed to Huismetbenen in order to provide ratelimits
  /// based on the person invoking the queuing, rather than the Onion-key associated with this application.
  /// This is only of relevance if the application is using an Onion-key and is therefore an authenticated 3rd-party app.
  /// </summary>
  /// <param name="player">The player.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="discordId">The Discord ID of the requester.</param>
  /// <returns>Bool whether the queueing was successful or not.</returns>
  public async Task<bool> QueuePlayerAsync(OsuUser player, int reworkId, ulong discordId)
  {
    // Queue the player and notify the user whether it was successful.
    bool? queued = await _huis.QueuePlayerAsync(player.Id, reworkId, discordId);
    if (queued is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to queue the player `{player.Name}`."));
    else if (!queued.Value)
      await FollowupAsync(embed: Embeds.Error("You are currently being ratelimited. Please wait a while beforing queuing someone again " +
                                              "in order to not overload the server."));
    else
      await FollowupAsync(embed: Embeds.Neutral($"`{player.Name}` has been queued. You will be notified once it completed."));

    return queued.HasValue ? queued.Value : false;
  }

  /// <summary>
  /// Returns the beatmap by the specified identifier (ID or alias).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="beatmapId">An identifier for the beatmap. (Beatmap ID or alias)</param>
  /// <returns>The beatmap.</returns>
  public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmapId)
  {
    // If the identifier is not a number, try to find a beatmap alias.
    if (!beatmapId.All(char.IsDigit))
    {
      // Get the beatmap alias. If none could be found, notify the user. Otherwise replace the identifier.
      BeatmapAlias? alias = await _persistence.GetBeatmapAliasAsync(beatmapId);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"Beatmap alias `{beatmapId}` could not be found."));
        return null;
      }
      else
        beatmapId = alias.BeatmapId.ToString();
    }

    // Get the beatmap from the osu! API. If it failed or the beatmap was not found, notify the user.
    NotFoundOr<OsuBeatmap>? beatmap = await _osu.GetBeatmapAsync(int.Parse(beatmapId));
    if (beatmap is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the beatmap from the osu! API."));
    else if (!beatmap.Found)
      await FollowupAsync(embed: Embeds.Error($"No beatmap with ID `{beatmapId}` could be found."));

    // Return the beatmap.
    return (beatmap?.Found ?? false) ? beatmap : null!;
  }

  /// <summary>
  /// Returns the top plays of the specified player in the specified rework from the Huis API.<br/>
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
  /// Returns the score by the specified identifier (ID or alias).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="scoreId">An identifier for the score. (Score ID or alias)</param>
  /// <returns>The score.</returns>
  public async Task<OsuScore?> GetScoreAsync(string scoreId)
  {
    // If the identifier is not a number, try to find a score alias.
    if (!scoreId.All(char.IsDigit))
    {
      // Get the score alias. If none could be found, notify the user. Otherwise replace the identifier.
      ScoreAlias? alias = await _persistence.GetScoreAliasAsync(scoreId);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"Score alias `{scoreId}` could not be found."));
        return null;
      }
      else
        scoreId = alias.ScoreId.ToString();
    }
    // Get the score from the osu! API. If it failed or the score was not found, notify the user.
    NotFoundOr<OsuScore>? score = await _osu.GetScoreAsync(long.Parse(scoreId));
    if (score is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the score from the osu! API."));
    else if (!score.Found)
      await FollowupAsync(embed: Embeds.Error($"No score with ID `{scoreId}` could be found."));

    // Return the score.
    return (score?.Found ?? false) ? score : null!;
  }

  /// <summary>
  /// Returns a simulated score, calculated through the specified request. Caching via the persistence database is applied here.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <returns>The simulated score.</returns>
  public async Task<HuisSimulationResponse?> SimulateScoreAsync(HuisSimulationRequest request)
  {
    // Simulate the score and check whether it was successful. If not, notify the user.
    HuisSimulationResponse? response = await _huis.SimulateAsync(request);
    if (response is null)
    {
      await ModifyOriginalResponseAsync(x =>
        x.Embed = Embeds.InternalError($"Failed to calculate the {(request.Rework.IsLive ? "live" : "local")} score."));
      return null;
    }

    // Return the response.
    return response;
  }

  /// <summary>
  /// Returns whether the user has the Onion role on the PP Discord, making them eligible to use Huis commands.
  /// </summary>
  /// <param name="context">The Discord socket interaction context.</param>
  public static async Task<bool> IsOnionAsync(SocketInteractionContext context)
  {
#if DEVELOPMENT || CUTTING_EDGE
    // In development mode, always grant the permissions.
    return true;
#endif

    // Check whether the user is the owner of the application.
    RestApplication app = await context.Client.GetApplicationInfoAsync();
    if (context.User.Id == app.Owner.Id || context.User.Id != app.Team.OwnerUserId)
      return true;

    // Get the PP Discord guild.
    SocketGuild guild = context.Client.GetGuild(546120878908506119);

    // Check whether the user is in that guild and has the Onion role.
    SocketGuildUser user = guild.GetUser(context.User.Id);
    return user != null && user.Roles.Any(x => x.Id == 577267917662715904);
  }

  /// <summary>
  /// Returns whether the user has the PP role on the PP Discord, making them eligible for certain more critical commands.
  /// </summary>
  /// <param name="context">The Discord socket interaction context.</param>
  public static async Task<bool> IsPPTeamAsync(SocketInteractionContext context)
  {
#if DEVELOPMENT
    // In development mode, always grant the permissions.
    return true;
#endif

    // Check whether the user is the owner of the application.
    RestApplication app = await context.Client.GetApplicationInfoAsync();
    if (context.User.Id == app.Owner.Id || context.User.Id != app.Team.OwnerUserId)
      return true;

    // Get the PP Discord guild.
    SocketGuild guild = context.Client.GetGuild(546120878908506119);

    // Check whether the user is in that guild and has the PP role.
    SocketGuildUser user = guild.GetUser(context.User.Id);
    return user != null && user.Roles.Any(x => x.Id == 975402380411666482);
  }
}