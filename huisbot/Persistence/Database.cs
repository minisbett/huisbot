using huisbot.Models.Persistence;
using Microsoft.EntityFrameworkCore;

namespace huisbot.Persistence;

/// <summary>
/// The database context for the SQL persistence in the application.
/// </summary>
public class Database(DbContextOptions<Database> options) : DbContext(options)
{
  /// <summary>
  /// The table containing links between osu! accounts and Discord accounts.
  /// </summary>
  public DbSet<OsuDiscordLink> OsuDiscordLinks { get; set; }

  /// <summary>
  /// The table containing aliases for beatmap IDs, providing easier access.
  /// </summary>
  public DbSet<BeatmapAlias> BeatmapAliases { get; set; }

  /// <summary>
  /// The table containing aliases for score IDs, providing easier access.
  /// </summary>
  public DbSet<ScoreAlias> ScoreAliases { get; set; }

  /// <summary>
  /// The cache for scores calculated on Huismetbenen.
  /// </summary>
  public DbSet<CachedScoreCalculation> CachedScoreCalculations { get; set; }
}