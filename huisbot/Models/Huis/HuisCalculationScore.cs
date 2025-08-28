using huisbot.Models.Osu;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the score in a <see cref="HuisCalculationResponse"/>.
/// </summary>
public class HuisCalculationScore
{
  /// <summary>
  /// The total legacy (score V1) score of the score.
  /// </summary>
  [JsonProperty("legacy_total_score")]
  public int LegacyTotalScore { get; private set; }

  /// <summary>
  /// The accuracy of the score.
  /// </summary>
  [JsonProperty("accuracy")]
  public double Accuracy { get; private set; }

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// The mods of the score, in the osu!lazer APIMod format.
  /// </summary>
  [JsonProperty("mods")]
  public OsuMods Mods { get; set; } = [];

  /// <summary>
  /// The hit statistics of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = null!;
}
