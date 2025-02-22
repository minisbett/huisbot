using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a user from the osu! API v1.
/// </summary>
public class OsuUser
{
  /// <summary>
  /// The ID of the user.
  /// </summary>
  [JsonProperty("user_id")]
  public int Id { get; private set; }

  /// <summary>
  /// The name of the user.
  /// </summary>
  [JsonProperty("username")]
  public string? Name { get; private set; }

  /// <summary>
  /// The total PP of the user.
  /// </summary>
  [JsonProperty("pp_raw")]
  public double? PP { get; private set; }

  /// <summary>
  /// The global rank of the user in performance ranking.
  /// </summary>
  [JsonProperty("pp_rank")]
  public int GlobalRank { get; private set; }

  /// <summary>
  /// The country rank of the user in performance ranking.
  /// </summary>
  [JsonProperty("pp_country_rank")]
  public int CountryRank { get; private set; }

  /// <summary>
  /// The ISO country code of the user.
  /// </summary>
  [JsonProperty("country")]
  public string Country { get; private set; } = null!;
}
