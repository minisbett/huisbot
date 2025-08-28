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
  /// The effective miss count of the calculated score.
  /// </summary>
  [JsonProperty("effective_miss_count")]
  public double EffectiveMissCount { get; private set; }

  /// <summary>
  /// The estimated number of slider breaks for the aim portion of the score.
  /// </summary>
  [JsonProperty("aim_estimated_slider_breaks")]
  public double? AimEstimatedSliderBreaks { get; private set; }

  /// <summary>
  /// The estimated number of slider breaks for the speed portion of the score.
  /// </summary>
  [JsonProperty("speed_estimated_slider_breaks")]
  public double? SpeedEstimatedSliderBreaks { get; private set; }
}
