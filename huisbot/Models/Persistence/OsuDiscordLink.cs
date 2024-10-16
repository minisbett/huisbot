using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Persistence;

/// <summary>
/// Represents a link between an osu! user and a discord user.
/// </summary>
public class OsuDiscordLink(ulong discordId, int osuId)
{
  /// <summary>
  /// The discord user ID.
  /// </summary>
  [Key]
  public ulong DiscordId { get; private set; } = discordId;

  /// <summary>
  /// The osu! user ID.
  /// </summary>
  public int OsuId { get; private set; } = osuId;
}
