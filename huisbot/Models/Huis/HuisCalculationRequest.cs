using huisbot.Models.Osu;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation request to be sent to Huismetbenen.
/// </summary>
/// <remarks>
/// Creates a new <see cref="HuisCalculationRequest"/> for the specified beatmap and rework.
/// </remarks>
public class HuisCalculationRequest(OsuBeatmap beatmap, HuisRework rework, OsuMods mods, int? combo = null, OsuScoreStatistics? statistics = null)
{
  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[] Mods { get; } = mods.Select(x => x.Acronym).ToArray();

  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("map_id")]
  public int BeatmapId { get; } = beatmap.Id;

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("combo")]
  public int? Combo { get; } = combo;

  /// <summary>
  /// The 100s/oks of the score.
  /// </summary>
  [JsonProperty("ok")]
  public int? Count100 { get; } = statistics?.Count100;

  /// <summary>
  /// The 50s/mehs of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int? Count50 { get; } = statistics?.Count50;

  /// <summary>
  /// The misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int? Misses { get; } = statistics?.Misses;

  /// <summary>
  /// The large tick misses of the score.
  /// </summary>
  [JsonProperty("large_tick_misses")]
  public int? LargeTickMisses { get; } = statistics?.LargeTickMisses;

  /// <summary>
  /// The slider tail misses of the score.
  /// </summary>
  [JsonProperty("slider_tail_misses")]
  public int? SliderTailMisses { get; } = statistics?.SliderTailHits is null ? null : beatmap.SliderCount - statistics.SliderTailHits;

  /// <summary>
  /// The clock rate of the score.
  /// </summary>
  [JsonProperty("clock_rate")]
  public double ClockRate { get; } = mods.ClockRate;

  /// <summary>
  /// The code of the rework. This property is used for JSON serialization for sending the request.
  /// </summary>
  [JsonProperty("rework")]
  public string ReworkCode => Rework.Code!;

  /// <summary>
  /// The rework.
  /// </summary>
  [JsonIgnore]
  public HuisRework Rework { get; } = rework;

  /// <summary>
  /// Returns the JSON string for this calculation request.
  /// </summary>
  /// <returns>The JSON string.</returns>
  public string ToJson() => JsonConvert.SerializeObject(this);
}
