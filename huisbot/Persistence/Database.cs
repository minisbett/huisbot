using huisbot.Models.Utility;
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

  public Database(DbContextOptions<Database> options) : base(options) { }
}