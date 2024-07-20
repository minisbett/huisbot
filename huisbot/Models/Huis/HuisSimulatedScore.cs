using huisbot.Utilities;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation response received from Huismetbenen.
/// </summary>
public class HuisSimulatedScore
{
  /// <summary>
  /// The accuracy of the score.
  /// </summary>
  [JsonProperty("accuracy")]
  public double Accuracy { get; private set; }

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// Bool (0 or 1) whether the score is a perfect combo.
  /// </summary>
  [JsonProperty("perfect")]
  public int Perfect { get; private set; }

  /// <summary>
  /// The 300s/greats of the score.
  /// </summary>
  [JsonProperty("great")]
  public int Count300 { get; private set; }

  /// <summary>
  /// The 100s/oks of the score.
  /// </summary>
  [JsonProperty("ok")]
  public int Count100 { get; private set; }

  /// <summary>
  /// The 50s/mehs of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int Count50 { get; private set; }

  /// <summary>
  /// The misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int Misses { get; private set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public Mods Mods { get; private set; } = null!;

  /// <summary>
  /// The PP for the aim skill.
  /// </summary>
  [JsonProperty("aim_pp")]
  public double AimPP { get; private set; }

  /// <summary>
  /// The PP for the tapping skill.
  /// </summary>
  [JsonProperty("tap_pp")]
  public double TapPP { get; private set; }

  /// <summary>
  /// The PP for the accuracy skill.
  /// </summary>
  [JsonProperty("acc_pp")]
  public double AccPP { get; private set; }

  /// <summary>
  /// The PP for the flashlight skill.
  /// </summary>
  [JsonProperty("fl_pp")]
  public double FLPP { get; private set; }

  /// <summary>
  /// The PP for the congition skill. This skill only exists in some reworks, and therefore may be null.
  /// </summary>
  [JsonProperty("cognition_pp")]
  public double? CognitionPP { get; private set; }

  /// <summary>
  /// The total PP of the score.
  /// </summary>
  [JsonProperty("local_pp")]
  public double TotalPP { get; private set; }

  /// <summary>
  /// The new difficulty rating.
  /// </summary>
  [JsonProperty("newSR")]
  public double NewDifficultyRating { get; private set; }
}
