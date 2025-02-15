using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the performance attributes in a <see cref="HuisCalculationResponse"/>.
/// </summary>
public class HuisPerformanceAttributes
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
