using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Options;

/// <summary>
/// Represents the Discord IDs for guilds, channels and roles configured in the appsettings.
/// </summary>
internal class DiscordIdOptions
{
  /// <summary>
  /// The ID of the official PP Guild.
  /// </summary>
  [Required]
  public ulong PPGuild { get; set; }

  /// <summary>
  /// The ID of the Onion role in the PP guild.
  /// </summary>
  [Required]
  public ulong OnionRole { get; set; }

  /// <summary>
  /// The ID of the PP role in the PP guild.
  /// </summary>
  [Required]
  public ulong PPRole { get; set; }
}
