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
}
