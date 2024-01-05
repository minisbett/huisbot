using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation request to be sent to Huismetbenen.
/// </summary>
public class HuisCalculationRequest
{
  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("map_id")]
  public int BeatmapId { get; }

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("combo")]
  public int? Combo { get; set; }

  /// <summary>
  /// The 100s/oks of the score.
  /// </summary>
  [JsonProperty("ok")]
  public int? Count100 { get; set; }

  /// <summary>
  /// The 50s/mehs of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int? Count50 { get; set; }

  /// <summary>
  /// The misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int? Misses { get; set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[]? Mods { get; set; }

  /// <summary>
  /// The code of the rework. This property is used for JSON serialization for sending the request.
  /// </summary>
  [JsonProperty("rework")]
  public string ReworkCode => Rework.Code!;

  /// <summary>
  /// The rework.
  /// </summary>
  [JsonIgnore]
  public HuisRework Rework { get; set; }

  /// <summary>
  /// Creates a new <see cref="HuisCalculationRequest"/> for the specified beatmap and rework.
  /// </summary>
  /// <param name="beatmapId">The ID of the beatmap.</param>
  /// <param name="rework">The rework.</param>
  public HuisCalculationRequest(int beatmapId, HuisRework rework)
  {
    BeatmapId = beatmapId;
    Rework = rework;
  }

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
