using huisbot.Models.Osu;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation request to be sent to Huismetbenen.
/// </summary>
/// <remarks>
/// Creates a new <see cref="HuisSimulationRequest"/> for the specified beatmap and rework.
/// </remarks>
public class HuisSimulationRequest(int beatmapId, HuisRework rework, OsuMods mods, int? combo = null, int? count100 = null,
                                   int? count50 = null, int? misses = null)
{
  /// <summary>
  /// The mods of the score in the osu!lazer APIMod format. This field is kept for cloning via <see cref="WithRework(HuisRework)"/>.
  /// </summary>
  private readonly OsuMods _mods = mods;

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[] Mods { get; } = mods.Select(x => x.Acronym).ToArray();

  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("map_id")]
  public int BeatmapId { get; } = beatmapId;

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("combo")]
  public int? Combo { get; } = combo;

  /// <summary>
  /// The 100s/oks of the score.
  /// </summary>
  [JsonProperty("ok")]
  public int? Count100 { get; } = count100;

  /// <summary>
  /// The 50s/mehs of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int? Count50 { get; } = count50;

  /// <summary>
  /// The misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int? Misses { get; } = misses;

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
  /// Returns a copy of this <see cref="HuisSimulationRequest"/> object with the specified rework.
  /// </summary>
  /// <param name="rework">The rework the create the copy with.</param>
  /// <returns>The copy of the request with the specified rework.</returns>
  public HuisSimulationRequest WithRework(HuisRework rework) => new(BeatmapId, rework, _mods, Combo, Count100, Count50, Misses);

  /// <summary>
  /// Returns the JSON string for this calculation request.
  /// </summary>
  /// <returns>The JSON string.</returns>
  public string ToJson() => JsonConvert.SerializeObject(this);
}
