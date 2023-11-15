using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents the body of a score calculation response received from Huismetbenen.
/// </summary>
public class HuisCalculationResult
{
  /// <summary>
  /// The fully qualified name of the map of the score in the format "(map id) - (artist) - (title) ((creator)) [(version)]".
  /// </summary>
  [JsonProperty("map_name")]
  public string? MapName { get; private set; }

  /// <summary>
  /// The accuracy of the score.
  /// </summary>
  [JsonProperty("accuracy")]
  public double Accuracy { get; set; }

  /// <summary>
  /// The maximum combo of the score.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; set; }

  /// <summary>
  /// Bool (0 or 1) whether the score is a perfect combo.
  /// </summary>
  [JsonProperty("perfect")]
  public int Perfect { get; set; }

  /// <summary>
  /// The 300s/greats of the score.
  /// </summary>
  [JsonProperty("great")]
  public int Count300 { get; set; }

  /// <summary>
  /// The 100s/oks of the score.
  /// </summary>
  [JsonProperty("ok")]
  public int Count100 { get; set; }

  /// <summary>
  /// The 50s/mehs of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int Count50 { get; set; }

  /// <summary>
  /// The misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int Misses { get; set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string Mods { get; set; } = "";

  /// <summary>
  /// The PP for the aim skill.
  /// </summary>
  [JsonProperty("aim_pp")]
  public double AimPP { get; set; }

  /// <summary>
  /// The PP for the tapping skill.
  /// </summary>
  [JsonProperty("tap_pp")]
  public double TapPP { get; set; }

  /// <summary>
  /// The PP for the accuracy skill.
  /// </summary>
  [JsonProperty("acc_pp")]
  public double AccPP { get; set; }

  /// <summary>
  /// The PP for the flashlight skill.
  /// </summary>
  [JsonProperty("fl_pp")]
  public double FLPP { get; set; }

  /// <summary>
  /// The total PP of the score.
  /// </summary>
  [JsonProperty("local_pp")]
  public double TotalPP { get; set; }
}
