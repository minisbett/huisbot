using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Options;

/// <summary>
/// Represents options for the osu! API.
/// </summary>
public class OsuApiOptions
{
  /// <summary>
  /// the OAuth client ID for the osu! API v2.
  /// </summary>
  [Required]
  [Range(0, 65535)]
  public int ClientId { get; set; }

  /// <summary>
  /// the OAuth client secret for the osu! API v2.
  /// </summary>
  [Required]
  public string ClientSecret { get; set; } = null!;
}
