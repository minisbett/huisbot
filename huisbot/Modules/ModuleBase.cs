using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Options;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace huisbot.Modules;

/// <summary>
/// A wrapper around the interaction module base for all modules.
/// This wrapper provides utility methods for parsing parameters like reworks or users.
/// </summary>
public partial class ModuleBase(IServiceProvider services) : InteractionModuleBase<SocketInteractionContext>
{
  /// <summary>
  /// The <see cref="HuisApiService"/>.
  /// </summary>
  public HuisApiService HuisApi { get; } = services.CreateScope().ServiceProvider.GetRequiredService<HuisApiService>();

  /// <summary>
  /// The <see cref="OsuApiService"/>.
  /// </summary>
  public OsuApiService OsuApi { get; } = services.GetRequiredService<OsuApiService>();

  /// <summary>
  /// The <see cref="PersistenceService"/>.
  /// </summary>
  public PersistenceService Persistence { get; } = services.CreateScope().ServiceProvider.GetRequiredService<PersistenceService>();

  /// <summary>
  /// The <see cref="DiscordService"/>.
  /// </summary>
  public DiscordService Discord { get; } = services.GetRequiredService<DiscordService>();

  /// <summary>
  /// The <see cref="EmbedService"/>.
  /// </summary>
  public EmbedService Embeds { get; } = services.GetRequiredService<EmbedService>();

  #region Huis

  /// <summary>
  /// Returns all available reworks on Huismetbenen.<br/>
  /// If it failed, the user will automatically be notified, unless the seeError parameter is set. In this case, this method returns null.
  /// </summary>
  /// <returns>All available Huis reworks.</returns>
  public async Task<HuisRework[]?> GetReworksAsync()
  {
    // Get all reworks and check whether the request was successful. If not, notify the user.
    HuisRework[]? reworks = await HuisApi.GetReworksAsync();
    if (reworks is null)
    {
      Embed embed = Embeds.InternalError("Failed to get the reworks from the Huis API.");

      if (Context.Interaction is SocketMessageComponent interaction)
        await interaction.UpdateAsync(msg => msg.Embed = embed);
      else
        await FollowupAsync(embed: embed);

      return null;
    }

    // Order the reworks by relevancy for the user and return them.
    return [
      .. reworks.Where(x => x.IsLive),
      .. reworks.Where(x => x.IsConfirmed),
      .. reworks.Where(x => x.IsPublic && x.IsActive),
      // Exclude confirmed, historic & live reworks because they are non-public and active.
      .. reworks.Where(x => !x.IsPublic && x.IsActive && !x.IsConfirmed && !x.IsHistoric && !x.IsLive),
      .. reworks.Where(x => !x.IsActive),
      .. reworks.Where(x => x.IsHistoric)
    ];
  }

  /// <summary>
  /// Returns a rework available on Huismetbenen based on the specified identifier (ID, code or fully qualified name).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The identifier for the rework (ID, code or fully qualified name). </param>
  /// <returns>The Huis rework.</returns>
  public async Task<HuisRework?> GetReworkAsync(string reworkId)
  {
    // Find a rework with a matching identifier.
    HuisRework[]? reworks = await GetReworksAsync();
    HuisRework? rework = reworks?.FirstOrDefault(x => x.Id.ToString() == reworkId || x.Code == reworkId || x.Name == reworkId);
    
    if (reworks is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the reworks from the Huis API."));
    else if (rework is null)
      await FollowupAsync(embed: Embeds.Error($"The rework `{reworkId}` could not be found."));
    else if (rework.IsOnionLevel && !IsOnion) // Disallow non-Onion users to access Onion-level reworks
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
  /// <param name="sortId">The identifier for the player sorting options (ID or display name).</param>
  /// <param name="sortId">The list of sorting options available.</param>
  /// <returns>The sorting option.</returns>
  public async Task<Sort?> GetSortAsync(string sortId, Sort[] allSorts)
  {
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
    HuisPlayer[]? players = await HuisApi.GetPlayerRankingsAsync(reworkId, sort, onlyUpToDate, hideUnranked);
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
    HuisScore[]? scores = await HuisApi.GetScoreRankingsAsync(reworkId, sort);
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
    HuisStatistic? statistic = await HuisApi.GetStatisticAsync(statisticId, reworkId);
    if (statistic is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the statistic `{statisticId}` from the Huis API."));

    return statistic;
  }

  /// <summary>
  /// Returns the Huis player based on the specified osu! user in the specified rework.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="user">The osu! user.</param>
  /// <param name="rework">The rework.</param>
  /// <returns>The Huis player.</returns>
  public async Task<HuisPlayer?> GetHuisPlayerAsync(OsuUser user, HuisRework rework)
  {
    HuisPlayer? player = await HuisApi.GetPlayerAsync(user.Id, rework);
    if (player is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the {(rework.IsLive ? "live" : "local")} player from the Huis API."));
    else if (player.IsOutdated)
      await FollowupAsync(embed: Embeds.Error($"""
                                               `{user.Username}` is outdated in the *{(rework.IsLive ? "live" : "specified")}* rework.
                                               Please use the `/queue` command to queue the user.
                                               """));

    return (player?.IsOutdated ?? true) ? null : player;
  }

  /// <summary>
  /// Returns the current calculation queue of the specified rework on Huismetbenen.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <returns>The calculation queue.</returns>
  public async Task<int[]?> GetHuisQueueAsync(int reworkId)
  {
    int[]? queue = await HuisApi.GetQueueAsync(reworkId);
    if (queue is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the player calculation queue from the Huis API."));

    return queue;
  }

  /// <inheritdoc cref="HuisApiService.QueuePlayerAsync(int, int, ulong)" />
  /// <param name="user">The osu! user.</param>
  public async Task<bool> QueuePlayerAsync(OsuUser user, int reworkId, ulong discordId)
  {
    bool? queued = await HuisApi.QueuePlayerAsync(user.Id, reworkId, discordId);
    if (queued is null) 
      await FollowupAsync(embed: Embeds.InternalError($"Failed to queue user `{user.Username}`."));
    else if (!queued.Value)
      await FollowupAsync(embed: Embeds.Error("You are currently being ratelimited. Please wait a while beforing queuing someone again."));
    else
      await FollowupAsync(embed: Embeds.Neutral($"`{user.Username}` has been queued. You will be notified once it completed."));

    return queued ?? false;
  }

  /// <summary>
  /// Returns the top plays of the specified user in the specified rework from the Huis API.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="user">The osu! user.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="scoreType">The type of scores (topranks, flashlight or pinned).</param>
  /// <returns>The top plays of the specified user in the specified rework.</returns>
  public async Task<HuisScore[]?> GetTopPlaysAsync(OsuUser user, int reworkId, string scoreType)
  {
    HuisScore[]? scores = await HuisApi.GetTopPlaysAsync(user.Id, reworkId, scoreType);
    if (scores is null)
      await FollowupAsync(embed: Embeds.InternalError($"Failed to get the top plays of `{user.Username}`."));

    return scores;
  }

  /// <summary>
  /// Returns a calculated score, calculated through the specified request. Caching via the persistence database is applied here.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="request">The score calculation request.</param>
  /// <returns>The calculated score.</returns>
  public async Task<HuisCalculationResponse?> CalculateScoreAsync(HuisCalculationRequest request)
  {
    HuisCalculationResponse? response = await HuisApi.CalculateAsync(request);
    if (response is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError($"Failed to calculate the score."));

    return response;
  }

  #endregion

  #region osu!

  /// <summary>
  /// Returns the osu! user with the specified identifier (ID or username).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="userId">An identifier for the osu! user (ID or username).</param>
  /// <returns>The osu! user.</returns>
  public async Task<OsuUser?> GetOsuUserAsync(string userId)
  {
    NotFoundOr<OsuUser>? user = await OsuApi.GetUserAsync(userId);
    if (user is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to resolve the user from the osu! API."));
    else if (!user.Found)
      await FollowupAsync(embed: Embeds.Error($"No user with identifier `{userId}` could not be found."));

    return (user?.Found ?? false) ? user : null!;
  }

  /// <summary>
  /// Returns the beatmap by the specified identifier (ID, URL or alias).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="beatmapId">An identifier for the beatmap (ID, URL or alias).</param>
  /// <returns>The beatmap.</returns>
  public async Task<OsuBeatmap?> GetBeatmapAsync(string beatmapId)
  {
    // Check if the provided identifier is an ID already.
    if (!int.TryParse(beatmapId, out int _))
    {
      // If not an ID, match a beatmap URL and extract the ID from it.
      Match match = BeatmapUrlRegex().Match(beatmapId);
      if (match.Success)
        beatmapId = match.Groups[1].Value;
      else
      {
        // At last, try to find a corresponding alias.
        BeatmapAlias? alias = await Persistence.GetBeatmapAliasAsync(beatmapId);
        if (alias is null)
        {
          await FollowupAsync(embed: Embeds.Error($"No beatmap with alias `{beatmapId}` could not be found."));
          return null;
        }

        beatmapId = alias.BeatmapId.ToString();
      }
    }

    NotFoundOr<OsuBeatmap>? beatmap = await OsuApi.GetBeatmapAsync(int.Parse(beatmapId));
    if (beatmap is null)
      await FollowupAsync(embed: Embeds.InternalError("Failed to get the beatmap from the osu! API."));
    else if (!beatmap.Found)
      await FollowupAsync(embed: Embeds.Error($"No beatmap with ID `{beatmapId}` could be found."));

    // Return the beatmap.
    return (beatmap?.Found ?? false) ? beatmap : null!;
  }

  [GeneratedRegex("https?:\\/\\/osu\\.ppy\\.sh\\/(?:beatmapsets\\/\\d+#osu\\/|s\\/\\d+#osu\\/|beatmaps\\/|b\\/)(\\d+)")]
  private static partial Regex BeatmapUrlRegex();

  /// <summary>
  /// Returns the score by the specified identifier (ID, URL or alias).<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="scoreId">An identifier for the score (ID, URL or alias).</param>
  /// <returns>The score.</returns>
  public async Task<OsuScore?> GetScoreAsync(string scoreId)
  {
    // Check if the provided identifier is an ID already.
    if (!int.TryParse(scoreId, out int _))
    {
      // If not an ID, match a beatmap URL and extract the ID from it.
      Match match = ScoreUrlRegex().Match(scoreId);
      if (match.Success)
        scoreId = match.Groups[1].Value;
      else
      {
        // At last, try to find a corresponding alias.
        ScoreAlias? alias = await Persistence.GetScoreAliasAsync(scoreId);
        if (alias is null)
        {
          await FollowupAsync(embed: Embeds.Error($"No score with alias `{scoreId}` could not be found."));
          return null;
        }

        scoreId = alias.ScoreId.ToString();
      }
    }

    NotFoundOr<OsuScore>? score = await OsuApi.GetScoreAsync(long.Parse(scoreId));
    if (score is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the score from the osu! API."));
    else if (!score.Found)
      await FollowupAsync(embed: Embeds.Error($"No score with ID `{scoreId}` could be found."));

    return (score?.Found ?? false) ? score : null!;
  }

  [GeneratedRegex("https?:\\/\\/osu\\.ppy\\.sh\\/scores\\/(\\d+)")]
  private static partial Regex ScoreUrlRegex();

  /// <summary>
  /// Returns the X-th best score by the specified user.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <param name="userId">The ID of the osu! user.</param>
  /// <param name="index">The one-based index of the score.</param>
  /// <param name="type">The type of score.</param>
  /// <returns>The X-th best score of the user.</returns>
  public async Task<OsuScore?> GetUserScoreAsync(int userId, int index, ScoreType type)
  {
    NotFoundOr<OsuScore>? score = await OsuApi.GetUserScoreAsync(userId, index, type);
    if (score is null)
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.InternalError("Failed to get the score from the osu! API."));

    return (score?.Found ?? false) ? score : null!;
  }

  #endregion

  #region Persistence

  /// <summary>
  /// Returns the link between the Discord user in the current context and an osu! account.<br/>
  /// If it failed, the user will automatically be notified. In this case, this method returns null.
  /// </summary>
  /// <returns>The link between the Discord user and an osu! account.</returns>
  public async Task<OsuDiscordLink?> GetOsuDiscordLinkAsync()
  {
    OsuDiscordLink? link = await Persistence.GetOsuDiscordLinkAsync(Context.User.Id);
    if (link is null)
      await FollowupAsync(embed: Embeds.Error($"You have not linked your osu! account. Please use the `/link` command to link your account."));

    return link;
  }

  #endregion

  /// <summary>
  /// Bool whether the user has the Onion role on the PP Discord, making them eligible to access onion-level reworks.
  /// </summary>
  public static bool CheckOnion(SocketInteractionContext context, IServiceProvider services)
  {
#if DEVELOPMENT
    // In development mode, always grant the permissions.
    return true;
#endif

    DiscordService discord = services.GetRequiredService<DiscordService>();

    // Authorize the user is they are the owner of the application.
    if (context.User.Id == discord.BotOwnerId)
      return true;

    // Check whether the user is in the PP guild and has the Onion role.
    DiscordIdOptions options = services.GetRequiredService<IOptions<DiscordIdOptions>>().Value;
    SocketGuildUser user = context.Client.GetGuild(options.PPGuild).GetUser(context.User.Id);
    return user?.Roles.Any(x => x.Id == options.OnionRole) ?? false;
  }

  /// <summary>
  /// Bool whether the user has the Onion role on the PP Discord, making them eligible to access onion-level reworks.
  /// </summary>
  public bool IsOnion => CheckOnion(Context, services);

  /// <summary>
  /// Bool whether the user has the PP role on the PP Discord, making them eligible for certain more critical commands.
  /// </summary>
  public bool IsPPTeam
  {
    get
    {
#if DEVELOPMENT
      // In development mode, always grant the permissions.
      return true;
#endif

      // Authorize the user is they are the owner of the application.
      if (Context.User.Id == Discord.BotOwnerId)
        return true;

      // Check whether the user is in the PP guild and has the PP role.
      DiscordIdOptions options = services.GetRequiredService<IOptions<DiscordIdOptions>>().Value;
      SocketGuildUser user = Context.Client.GetGuild(options.PPGuild).GetUser(Context.User.Id);
      return user?.Roles.Any(x => x.Id == options.PPRole) ?? false;
    }
  }
}