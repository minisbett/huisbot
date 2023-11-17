using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a score of a user returned from the Huis API.
/// </summary>
public class HuisScore
{
  /// <summary>
  /// The ID of the score.
  /// </summary>
  [JsonProperty("score_id")]
  public long ScoreId { get; private set; }

  /// <summary>
  /// The ID of the user.
  /// </summary>
  [JsonProperty("user_id")]
  public int UserId { get; private set; }

  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("beatmap_id")]
  public int BeatmapId { get; private set; }

  /// <summary>
  /// The PP in the live PP system.
  /// </summary>
  [JsonProperty("live_pp")]
  public double LivePP { get; private set; }

  /// <summary>
  /// The PP in the rework.
  /// </summary>
  [JsonProperty("local_pp")]
  public double LocalPP { get; private set; }

  /// <summary>
  /// The Aim PP in the rework.
  /// </summary>
  [JsonProperty("aim_pp")]
  public double AimPP { get; private set; }

  /// <summary>
  /// The Tap PP in the rework.
  /// </summary>
  [JsonProperty("tap_pp")]
  public double TapPP { get; private set; }

  /// <summary>
  /// The Acc PP in the rework.
  /// </summary>
  [JsonProperty("acc_pp")]
  public double AccPP { get; private set; }

  /// <summary>
  /// The FL PP in the rework.
  /// </summary>
  [JsonProperty("fl_pp")]
  public double FLPP { get; private set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string? Mods { get; private set; }

  /// <summary>
  /// The amount of 300s/greats of the score.
  /// </summary>
  [JsonProperty("great")]
  public int Count300 { get; private set; }

  /// <summary>
  /// The amount of 100s/goods of the score.
  /// </summary>
  [JsonProperty("good")] // BUG: This is actually called "ok", but Huismetbenen mistakenly calls them "goods".
  public int Count100 { get; private set; }

  /// <summary>
  /// The amount of 300s/greats of the score.
  /// </summary>
  [JsonProperty("meh")]
  public int Count50 { get; private set; }

  /// <summary>
  /// The amount of misses of the score.
  /// </summary>
  [JsonProperty("miss")]
  public int Misses { get; private set; }

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
  /// The date the score was set.
  /// </summary>
  [JsonProperty("score_date")]
  public DateTime ScoreDate { get; private set; }

  /// <summary>
  /// The rank of the score. (XH, X, SH, S, A, B, C, D)
  /// </summary>
  [JsonProperty("score_rank")]
  public string? ScoreRank { get; private set; }

  /// <summary>
  /// The username of the user.
  /// </summary>
  [JsonProperty("username")]
  public string? Username { get; private set; }

  /// <summary>
  /// The title of the beatmap.
  /// </summary>
  [JsonProperty("title")]
  public string? Title { get; private set; }

  /// <summary>
  /// The artist of the beatmap.
  /// </summary>
  [JsonProperty("artist")]
  public string? Artist { get; private set; }

  /// <summary>
  /// The difficulty name of the beatmap.
  /// </summary>
  [JsonProperty("diff_name")]
  public string? Version { get; private set; }

  /// <summary>
  /// The mapper of the beatmap.
  /// </summary>
  [JsonProperty("creator_name")]
  public string? Mapper { get; private set; }

  public override string ToString()
  {
    return $"**{Username}** | {Artist} - {Title} ({Mapper}) [{Version}]{(string.IsNullOrEmpty(Mods) ? "" : "+" + Mods!.Replace("CL", "").Replace(", ", ""))} " +
           $"{Accuracy}% {MaxCombo}x {LivePP} -> {LocalPP}pp [{Count300}/{Count100}/{Count50}/{Misses}]";
  }
}