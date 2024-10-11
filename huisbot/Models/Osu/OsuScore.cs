using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a score from the osu! API v2.
/// </summary>
public class OsuScore
{
  /// <summary>
  /// The maximum achieved combo of the score.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[] Mods { get; private set; } = [];

  /// <summary>
  /// The statistics (300s, 100s, 50s, misses) of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = new OsuScoreStatistics();

  /// <summary>
  /// Represents the <see cref="Statistics"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreStatistics
  {
    /// <summary>
    /// The amount of 300s in the score.
    /// </summary>
    [JsonProperty("count_300")]
    public int Count300 { get; private set; }

    /// <summary>
    /// The amount of 100s in the score.
    /// </summary>
    [JsonProperty("count_100")]
    public int Count100 { get; private set; }

    /// <summary>
    /// The amount of 50s in the score.
    /// </summary>
    [JsonProperty("count_50")]
    public int Count50 { get; private set; }

    /// <summary>
    /// The amount of misses in the score.
    /// </summary>
    [JsonProperty("count_miss")]
    public int Misses { get; private set; }
  }
}