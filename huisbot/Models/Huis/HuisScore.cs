using Newtonsoft.Json;

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
  /// The PP in the live PP system. If the information is not available (eg. osu! API does not provide PP for this score), this is 0.
  /// </summary>
  public double LivePP => LivePPInternal ?? 0;

  /// <summary>
  /// The PP in the live PP system. This may be null if Huismetbenen returned a score where it couldn't get the live PP.
  /// This can happen when a user manually adds a score to the Huismetbenen database, and the osu! API does not provide the live PP.
  /// </summary>
  [JsonProperty("live_pp")]
  private double? LivePPInternal { get; set; }

  /// <summary>
  /// The PP in the local PP system. If the information is not available (Huismetbenen did not calculate it), this is 0.
  /// </summary>
  public double LocalPP => LocalPPInternal ?? 0;

  /// <summary>
  /// The PP in the local PP system. This may be null if <see cref="LivePPInternal"/>, as Huismetbenen then does not calculate it.
  /// </summary>
  [JsonProperty("local_pp")]
  private double? LocalPPInternal { get; set; }

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string Mods { get; private set; } = null!;

  /// <summary>
  /// The amount of 300s/greats of the score.
  /// </summary>
  [JsonProperty("great")]
  public int Count300 { get; private set; }

  /// <summary>
  /// The amount of 100s/oks of the score.
  /// </summary>
  [JsonProperty("good")] // BUG: This is actually called "ok", but Huismetbenen mistakenly calls them "good".
  public int Count100 { get; private set; }

  /// <summary>
  /// The amount of 50s/mehs of the score.
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
    string mods = Mods == "" ? "" : $" +{Mods}";
    return $"{Username} | {Artist} - {Title} ({Mapper}) [{Version}]{mods} " +
           $"{Accuracy}% {MaxCombo}x {LivePP} -> {LocalPP}pp [{Count300}/{Count100}/{Count50}/{Misses}]";
  }
}