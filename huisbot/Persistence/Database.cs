﻿using huisbot.Models.Persistence;
using Microsoft.EntityFrameworkCore;

namespace huisbot.Persistence;

/// <summary>
/// The database context for the SQL persistence in the application.
/// </summary>
public class Database : DbContext
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
  /// The cache for scores simulated on Huismetbenen.
  /// </summary>
  public DbSet<CachedScoreSimulation> CachedScoreSimulations { get; set; }

  public Database(DbContextOptions<Database> options) : base(options) { }
}