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
  public HuisSimulationScore Score { get; private set; } = null!;

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
    public double? FLPP { get; private set; }

    /// <summary>
    /// The reading PP of the simulated score.
    /// </summary>
    [JsonProperty("reading")]
    public double? ReadingPP { get; private set; }
  }

  /// <summary>
  /// Represents the score in a <see cref="HuisSimulationResponse"/>.
  /// </summary>
  public class HuisSimulationScore
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
    /// The mods of the score.
    /// </summary>
    [JsonIgnore]
    public Mods Mods => Mods.Parse(OsuMods.Select(x => x.Acronym).ToArray());

    /// <summary>
    /// The mods of the score, in the osu-tools format.
    /// </summary>
    [JsonProperty("mods")]
    private HuisSimulationScoreMod[] OsuMods { get; set; } = null!;

    /// <summary>
    /// The hit statistics of the score.
    /// </summary>
    [JsonProperty("statistics")]
    public HuisSimulationScoreStatistics Statistics { get; private set; } = null!;
  }

  /// <summary>
  /// Represents a mod of a <see cref="HuisSimulatedScore"/> in the osu-tools format.
  /// </summary>
  public class HuisSimulationScoreMod
  {
    /// <summary>
    /// The acronym of the mod.
    /// </summary>
    [JsonProperty("acronym")]
    public string Acronym { get; private set; } = null!;
  }

  /// <summary>
  /// Represents the hit statistics of a <see cref="HuisSimulatedScore"/>.
  /// </summary>
  public class HuisSimulationScoreStatistics
  {
    /// <summary>
    /// The amount of 300s/greats in the score.
    /// </summary>
    [JsonProperty("great")]
    public int Count300 { get; private set; }

    /// <summary>
    /// The amount of 100s/oks in the score.
    /// </summary>
    [JsonProperty("ok")]
    public int Count100 { get; private set; }

    /// <summary>
    /// The amount of 50s/mehs in the score.
    /// </summary>
    [JsonProperty("meh")]
    public int Count50 { get; private set; }

    /// <summary>
    /// The amount of misses in the score.
    /// </summary>
    [JsonProperty("miss")]
    public int Misses { get; private set; }
  }
}