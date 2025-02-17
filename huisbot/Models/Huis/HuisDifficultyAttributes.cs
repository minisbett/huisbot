using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the difficulty attributes in a <see cref="HuisCalculationResponse"/>.
/// </summary>
public class HuisDifficultyAttributes
{
  /// <summary>
  /// The total difficulty rating of the calculated score.
  /// </summary>
  [JsonProperty("star_rating")]
  public double DifficultyRating { get; private set; }

  /// <summary>
  /// The aim difficulty of the calculated score.
  /// </summary>
  [JsonProperty("aim_difficulty")]
  public double AimDifficulty { get; private set; }

  /// <summary>
  /// The speed difficulty of the calculated score.
  /// </summary>
  [JsonProperty("speed_difficulty")]
  public double SpeedDifficulty { get; private set; }

  /// <summary>
  /// The flashlight difficulty of the calculated score.
  /// </summary>
  [JsonProperty("flashlight_difficulty")]
  public double? FlashlightDifficulty { get; private set; }

  /// <summary>
  /// The speed notes in the calculated score.
  /// </summary>
  [JsonProperty("speed_note_count")]
  public double SpeedNoteCount { get; private set; }

  /// <summary>
  /// The slider factor of the calculated score.
  /// </summary>
  [JsonProperty("slider_factor")]
  public double SliderFactor { get; private set; }

  /// <summary>
  /// The amount of difficult aim strains in the calculated score.
  /// </summary>
  [JsonProperty("aim_difficult_strain_count")]
  public double AimDifficultStrainCount { get; private set; }

  /// <summary>
  /// The amount of difficult speed strains in the calculated score.
  /// </summary>
  [JsonProperty("speed_difficult_strain_count")]
  public double SpeedDifficultStrainCount { get; private set; }
}
