using huisbot.Models.Osu;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a score calculation response received from Huismetbenen.
/// </summary>
public class HuisCalculationResponse
{
  /// <summary>
  /// The difficulty attributes of the calculated score.
  /// </summary>
  [JsonProperty("difficulty_attributes")]
  public HuisCalculationDifficultyAttributes DifficultyAttributes { get; private set; } = null!;

  /// <summary>
  /// The performance attributes of the calculated score.
  /// </summary>
  [JsonProperty("performance_attributes")]
  public HuisCalculationPerformanceAttributes PerformanceAttributes { get; private set; } = null!;

  /// <summary>
  /// The calculated score.
  /// </summary>
  [JsonProperty("score")]
  public HuisCalculationScore Score { get; private set; } = null!;

  /// <summary>
  /// Represents the score in a <see cref="HuisCalculationResponse"/>.
  /// </summary>
  public class HuisCalculationScore
  {
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
    public OsuScore.OsuScoreStatistics Statistics { get; private set; } = null!;
  }

  /// <summary>
  /// Represents the difficulty attributes in a <see cref="HuisCalculationResponse"/>.
  /// </summary>
  public class HuisCalculationDifficultyAttributes
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

  /// <summary>
  /// Represents the performance attributes in a <see cref="HuisCalculationResponse"/>.
  /// </summary>
  public class HuisCalculationPerformanceAttributes
  {
    /// <summary>
    /// The total PP of the calculated score.
    /// </summary>
    [JsonProperty("pp")]
    public double PP { get; private set; }

    /// <summary>
    /// The aim PP of the calculated score.
    /// </summary>
    [JsonProperty("aim")]
    public double AimPP { get; private set; }

    /// <summary>
    /// The tap/speed PP of the calculated score.
    /// </summary>
    [JsonProperty("speed")]
    public double TapPP { get; private set; }

    /// <summary>
    /// The accuracy PP of the calculated score.
    /// </summary>
    [JsonProperty("accuracy")]
    public double AccPP { get; private set; }

    /// <summary>
    /// The flashlight PP of the calculated score.
    /// </summary>
    [JsonProperty("flashlight")]
    public double FLPP { get; private set; }

    /// <summary>
    /// The reading PP of the calculated score. Only available in reading-related reworks.
    /// </summary>
    [JsonProperty("reading")]
    public double? ReadingPP { get; private set; }

    /// <summary>
    /// The deviation (estimated unstable rate) of the calculated score. Only available in statistical accuracy-related reworks.
    /// </summary>
    [JsonProperty("deviation")]
    public double? Deviation { get; private set; }

    /// <summary>
    /// The speed deviation (estimated unstable rate) of the calculated score. Only available in statistical accuracy-related reworks.
    /// </summary>
    [JsonProperty("speed_deviation")]
    public double? SpeedDeviation { get; private set; }
  }
}