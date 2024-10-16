using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation request to be sent to Huismetbenen.
/// </summary>
/// <remarks>
/// Creates a new <see cref="HuisSimulationRequest"/> for the specified beatmap and rework.
/// </remarks>
/// <param name="beatmapId">The ID of the beatmap.</param>
/// <param name="rework">The rework.</param>
public class HuisSimulationRequest(int beatmapId, HuisRework rework, string[]? mods = null, int? combo = null, int? count100 = null,
                                   int? count50 = null, int? misses = null)
{
  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[]? Mods { get; } = mods;

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
  public HuisSimulationRequest WithRework(HuisRework rework) => new(BeatmapId, rework, Mods, Combo, Count100, Count50, Misses);

  /// <summary>
  /// Returns the JSON string for this calculation request, removing all json properties with null values.
  /// </summary>
  /// <returns>The JSON string.</returns>
  public string ToJson()
  {
    // Convert this object to a JSON string.
    string json = JsonConvert.SerializeObject(this, Formatting.Indented);

    // Dirty workaround: Remove all lines that end with "null,", getting rid of all unset parameters.
    return Regex.Replace(json, @"^.+null,\s*$", string.Empty, RegexOptions.Multiline);
  }
}
