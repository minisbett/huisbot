using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a user from the osu! API v2.
/// </summary>
public class OsuUser
{
  /// <summary>
  /// The ID of the osu! user.
  /// </summary>
  [JsonProperty("id")]
  public int Id { get; private set; }

  /// <summary>
  /// The name of the osu! user.
  /// </summary>
  [JsonProperty("username")]
  public string? Username { get; private set; }

  /// <summary>
  /// The country of the osu! user.
  /// </summary>
  [JsonProperty("country")]
  public OsuUserCountry Country { get; private set; } = null!;

  /// <summary>
  /// The statistics (global rank, country rank) of the osu! user.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuUserStatistics Statistics { get; private set; } = null!;

  /// <summary>
  /// Represents the <see cref="Country"/> property of a user on the osu! API v2.
  /// </summary>
  public class OsuUserCountry
  {
    /// <summary>
    /// The BCP 47 language tag of this country.
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; private set; } = default!;
  }

  public class OsuUserStatistics
  {
    /// <summary>
    /// The country rank of the osu! user.
    /// </summary>
    [JsonProperty("country_rank")]
    public int CountryRank { get; private set; }

    /// <summary>
    /// The global rank of the osu! user.
    /// </summary>
    [JsonProperty("global_rank")]
    public int GlobalRank { get; private set; }

    /// <summary>
    /// The total performance points the user has.
    /// </summary>
    [JsonProperty("pp")]
    public float PP { get; private set; }
  }
}