using huisbot.Models.Persistence;
using huisbot.Persistence;
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
    // Check whether the link already exists. If it does, update it.
    if (await GetOsuDiscordLinkAsync(discordId) is OsuDiscordLink link)
    {
      link.OsuId = osuId;
      await _database.SaveChangesAsync();
      return;
    }

    // Otherwise add the link to the database.
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
    return await _database.BeatmapAliases.FirstOrDefaultAsync(x => x.Alias == alias);
  }

  /// <summary>
  /// Adds the specified alias to the database.
  /// </summary>
  /// <param name="alias">The alias.</param>
  public async Task AddBeatmapAliasAsync(BeatmapAlias alias)
  {
    // Add the beatmap alias to the database.
    _database.BeatmapAliases.Add(alias);
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
    return await _database.ScoreAliases.FirstOrDefaultAsync(x => x.Alias == alias);
  }

  /// <summary>
  /// Adds the specified score alias to the database.
  /// </summary>
  /// <param name="alias">The score alias.</param>
  public async Task AddScoreAliasAsync(ScoreAlias alias)
  {
    // Add the score alias to the database.
    _database.ScoreAliases.Add(alias);
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
}
