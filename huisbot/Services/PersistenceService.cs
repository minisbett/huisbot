using huisbot.Models.Huis;
using huisbot.Models.Persistence;
using huisbot.Persistence;
using huisbot.Utilities;
using Microsoft.EntityFrameworkCore;

namespace huisbot.Services;

/// <summary>
/// The persistence service is responsible for managing the access to the persistence database.
/// </summary>
public class PersistenceService
{
  private readonly Database _database;

  public PersistenceService(Database database)
  {
    _database = database;
  }

  /// <summary>
  /// Returns the link between the specified discord user and the linked osu! user or null if no link exists.
  /// </summary>
  /// <param name="discordId">The discord ID of the user.</param>
  /// <returns>The link between the specified discord user and the linked osu! user or null if no link exists.</returns>
  public async Task<OsuDiscordLink?> GetOsuDiscordLinkAsync(ulong discordId)
  {
    return await _database.OsuDiscordLinks.FirstOrDefaultAsync(x => x.DiscordId == discordId);
  }

  /// <summary>
  /// Adds or updates a link between the specified discord user and the specified osu! user to the database.
  /// </summary>
  /// <param name="discordId">The discord ID of the user.</param>
  /// <param name="osuId">The osu! user ID of the user.</param>
  public async Task SetOsuDiscordLinkAsync(ulong discordId, int osuId)
  {
    // Check whether the link already exists. If it does, remove it first.
    if (await GetOsuDiscordLinkAsync(discordId) is OsuDiscordLink link)
      _database.OsuDiscordLinks.Remove(link);

    // Add the link to the database.
    _database.OsuDiscordLinks.Add(new OsuDiscordLink(discordId, osuId));
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Returns all beatmap aliases.
  /// </summary>
  /// <returns>The beatmap aliases.</returns>
  public async Task<BeatmapAlias[]> GetBeatmapAliasesAsync()
  {
    return await _database.BeatmapAliases.ToArrayAsync();
  }

  /// <summary>
  /// Returns the beatmap alias by the specified alias text.
  /// </summary>
  /// <param name="alias">The alias text.</param>
  /// <returns>The beatmap alias.</returns>
  public async Task<BeatmapAlias?> GetBeatmapAliasAsync(string alias)
  {
    // Get all beatmap aliases and try to find the specified one.
    return await _database.BeatmapAliases.FirstOrDefaultAsync(x => x.Alias == Utils.GetFormattedAlias(alias));
  }

  /// <summary>
  /// Adds the specified alias to the database.
  /// </summary>
  /// <param name="alias">The score alias.</param>
  /// <param name="beatmapId">The beatmap ID.</param>
  /// <param name="displayName">The display name for the score.</param>
  public async Task AddBeatmapAliasAsync(string alias, long beatmapId, string displayName)
  {
    // Add the beatmap alias to the database.
    _database.BeatmapAliases.Add(new BeatmapAlias(Utils.GetFormattedAlias(alias), beatmapId, displayName));
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Removes the specified beatmap alias from the database.
  /// </summary>
  /// <param name="alias">The alias.</param>
  public async Task RemoveBeatmapAliasAsync(BeatmapAlias alias)
  {
    // Remove the beatmap alias from the database.
    _database.BeatmapAliases.Remove(alias);
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Returns all score aliases.
  /// </summary>
  /// <returns>The score aliases.</returns>
  public async Task<ScoreAlias[]> GetScoreAliasesAsync()
  {
    return await _database.ScoreAliases.ToArrayAsync();
  }

  /// <summary>
  /// Returns the score alias by the specified alias text.
  /// </summary>
  /// <param name="alias">The alias text.</param>
  /// <returns>The score alias.</returns>
  public async Task<ScoreAlias?> GetScoreAliasAsync(string alias)
  {
    // Get all score aliases and try to find the specified one.
    return await _database.ScoreAliases.FirstOrDefaultAsync(x => x.Alias == Utils.GetFormattedAlias(alias));
  }

  /// <summary>
  /// Adds the specified score alias to the database.
  /// </summary>
  /// <param name="alias">The score alias.</param>
  /// <param name="scoreId">The score ID.</param>
  /// <param name="displayName">The display name for the score.</param>
  public async Task AddScoreAliasAsync(string alias, long scoreId, string displayName)
  {
    // Add the score alias to the database.
    _database.ScoreAliases.Add(new ScoreAlias(Utils.GetFormattedAlias(alias), scoreId, displayName));
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Removes the specified score alias from the database.
  /// </summary>
  /// <param name="alias">The score alias.</param>
  public async Task RemoveScoreAliasAsync(ScoreAlias alias)
  {
    // Remove the score alias from the database.
    _database.ScoreAliases.Remove(alias);
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Adds a new cache entry for the specified score simulation request and it's resulting score.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <param name="score">The simulated score.</param>
  public async Task AddCachedScoreSimulationAsync(HuisSimulationRequest request, HuisSimulatedScore score)
  {
    // To make this thread-safe, make sure there is no cache entry at this point in time (again).
    if (await GetCachedScoreSimulationAsync(request) is HuisSimulatedScore)
      return;

    // Add the cached simulation to the database.
    _database.CachedScoreSimulations.Add(new CachedScoreSimulation(request, score));
    await _database.SaveChangesAsync();
  }

  /// <summary>
  /// Returns the cached simulation score for the specified score simulation request.<br/>
  /// If no cache entry exists, null is returned instead.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <returns>The cached simulated score or null, if no cache entry exists.</returns>
  public async Task<HuisSimulatedScore?> GetCachedScoreSimulationAsync(HuisSimulationRequest request)
  {
    return (await _database.CachedScoreSimulations.FirstOrDefaultAsync(
      x => x.RequestIdentifier == CachedScoreSimulation.GetRequestIdentifier(request)))?.Score;
  }
}
