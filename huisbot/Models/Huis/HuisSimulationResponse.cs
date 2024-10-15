using huisbot.Models.Osu;
using huisbot.Utilities;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a score calculation response received from Huismetbenen.
/// </summary>
public class HuisSimulationResponse
{
  /// <summary>
  /// The difficulty attributes of the simulated score.
  /// </summary>
  [JsonProperty("difficulty_attributes")]
  public HuisSimulationDifficultyAttributes DifficultyAttributes { get; private set; } = null!;

  /// <summary>
  /// The performance attributes of the simulated score.
  /// </summary>
  [JsonProperty("performance_attributes")]
  public HuisSimulationPerformanceAttributes PerformanceAttributes { get; private set; } = null!;

  /// <summary>
  /// The simulated score.
  /// </summary>
  [JsonProperty("score")]
  public OsuScore Score { get; private set; } = null!;

  /// <summary>
  /// Represents the difficulty attributes in a <see cref="HuisSimulationResponse"/>.
  /// </summary>
  public class HuisSimulationDifficultyAttributes
  {
    /// <summary>
    /// The total difficulty rating of the simulated score.
    /// </summary>
    [JsonProperty("star_rating")]
    public double DifficultyRating { get; private set; }

    /// <summary>
    /// The aim difficulty of the simulated score.
    /// </summary>
    [JsonProperty("aim_difficulty")]
    public double AimDifficulty { get; private set; }

    /// <summary>
    /// The speed difficulty of the simulated score.
    /// </summary>
    [JsonProperty("speed_difficulty")]
    public double SpeedDifficulty { get; private set; }

    /// <summary>
    /// The flashlight difficulty of the simulated score.
    /// </summary>
    [JsonProperty("flashlight_difficulty")]
    public double? FlashlightDifficulty { get; private set; }

    /// <summary>
    /// The speed notes in the simulated score.
    /// </summary>
    [JsonProperty("speed_note_count")]
    public double SpeedNoteCount { get; private set; }

    /// <summary>
    /// The slider factor of the simulated score.
    /// </summary>
    [JsonProperty("slider_factor")]
    public double SliderFactor { get; private set; }

    /// <summary>
    /// The amount of difficult aim strains in the simulated score.
    /// </summary>
    [JsonProperty("aim_difficult_strain_count")]
    public double AimDifficultStrainCount { get; private set; }

    /// <summary>
    /// The amount of difficult speed strains in the simulated score.
    /// </summary>
    [JsonProperty("speed_difficult_strain_count")]
    public double SpeedDifficultStrainCount { get; private set; }
  }

  /// <summary>
  /// Represents the performance attributes in a <see cref="HuisSimulationResponse"/>.
  /// </summary>
  public class HuisSimulationPerformanceAttributes
  {
    /// <summary>
    /// The total PP of the simulated score.
    /// </summary>
    [JsonProperty("pp")]
    public double PP { get; private set; }

    /// <summary>
    /// The aim PP of the simulated score.
    /// </summary>
    [JsonProperty("aim")]
    public double AimPP { get; private set; }

    /// <summary>
    /// The tap/speed PP of the simulated score.
    /// </summary>
    [JsonProperty("speed")]
    public double TapPP { get; private set; }

    /// <summary>
    /// The accuracy PP of the simulated score.
    /// </summary>
    [JsonProperty("accuracy")]
    public double AccPP { get; private set; }

    /// <summary>
    /// The flashlight PP of the simulated score.
    /// </summary>
    [JsonProperty("flashlight")]
    public double? FLPP { get; private set; }

    /// <summary>
    /// The reading PP of the simulated score.
    /// </summary>
    [JsonProperty("reading")]
    public double? ReadingPP { get; private set; }
  }
}