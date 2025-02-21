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
  public string ClientId { get; set; } = null!;

  /// <summary>
  /// the OAuth client secret for the osu! API v2.
  /// </summary>
  [Required]
  public string ClientSecret { get; set; } = null!;

  /// <summary>
  /// the API key for the osu! API v1.
  /// </summary>
  [Required]
  public string ApiKey { get; set; } = null!;
}
