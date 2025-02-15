using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents the hit statistics of an osu! API v2 score.
/// </summary>
public class OsuScoreStatistics(int? count100, int? count50, int? misses, int? largeTickMisses, int? sliderTailHits)
{
  /// <summary>
  /// The amount of 300s in the score.
  /// </summary>
  [JsonProperty("great")]
  public int Count300 { get; private set; }

  /// <summary>
  /// The amount of 100s in the score.
  /// </summary>
  [JsonProperty("ok")]
  public int? Count100 { get; private set; } = count100;

  /// <summary>
  /// The amount of 50s in the score.
  /// </summary>
  [JsonProperty("meh")]
  public int? Count50 { get; private set; } = count50;

  /// <summary>
  /// The amount of misses in the score.
  /// </summary>
  [JsonProperty("miss")]
  public int? Misses { get; private set; } = misses;

  /// <summary>
  /// The amount of large tick misses in the score.
  /// </summary>
  [JsonProperty("large_tick_miss")]
  public int? LargeTickMisses { get; private set; } = largeTickMisses;

  /// <summary>
  /// The amount of slider tail hits in the score.
  /// </summary>
  [JsonProperty("slider_tail_hit")]
  public int? SliderTailHits { get; private set; } = sliderTailHits;
}
