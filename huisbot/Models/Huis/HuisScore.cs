using huisbot.Models.Osu;
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
  /// The user of the score.
  /// </summary>
  [JsonProperty("user")]
  public HuisScoreUser User { get; private set; } = default!;

  /// <summary>
  /// The beatmap of the score.
  /// </summary>
  [JsonProperty("beatmap")]
  public HuisScoreBeatmap Beatmap { get; private set; } = default!;

  /// <summary>
  /// The difficulty & performance values of the score.
  /// </summary>
  [JsonProperty("values")]
  public HuisScoreValues Values { get; private set; } = default!;

  /// <summary>
  /// The hit statistics of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = default!;

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public OsuMods Mods { get; private set; } = [];

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
  /// The datetime at which the score was set.
  /// </summary>
  [JsonProperty("score_date")]
  public DateTime ScoreDate { get; private set; }

  /// <summary>
  /// The rank of the score. (XH, X, SH, S, A, B, C, D)
  /// </summary>
  [JsonProperty("score_rank")]
  public string? ScoreRank { get; private set; }

  public override string ToString()
  {
    return $"{User.Name} | {Beatmap.Artist} - {Beatmap.Title} ({Beatmap.Creator}) [{Beatmap.Version}]{Mods.PlusString} " +
           $"{Accuracy}% {MaxCombo}x {Values.LivePP} -> {Values.LocalPP}pp [{Statistics.Count300}/{Statistics.Count100}/{Statistics.Count50}/{Statistics.Misses}]";
  }

  /// <summary>
  /// Represents the user of a <see cref="HuisScore"/>.
  /// </summary>
  public class HuisScoreUser
  {
    /// <summary>
    /// The ID of the user of the score.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; private set; }

    /// <summary>
    /// The name of the user of the score.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; private set; } = default!;
  }

  /// <summary>
  /// Represents the beatmap of a <see cref="HuisScore"/>.
  /// </summary>
  public class HuisScoreBeatmap
  {
    /// <summary>
    /// The ID of the beatmap.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; private set; }

    /// <summary>
    /// The title of the beatmap.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// The artist of the beatmap.
    /// </summary>
    [JsonProperty("artist")]
    public string Artist { get; private set; } = null!;

    /// <summary>
    /// The difficulty name of the beatmap.
    /// </summary>
    [JsonProperty("diff_name")]
    public string Version { get; private set; } = null!;

    /// <summary>
    /// The mapper of the beatmap.
    /// </summary>
    [JsonProperty("creator_name")]
    public string Creator { get; private set; } = null!;
  }

  /// <summary>
  /// Represents the difficulty & performance values of a <see cref="HuisScore"/>.
  /// </summary>
  public class HuisScoreValues
  {
    /// <summary>
    /// The PP of the score in the local rework.
    /// </summary>
    [JsonProperty("local_pp")]
    public double LocalPP { get; private set; }

    /// <summary>
    /// The PP of the score in the live PP system.
    /// </summary>
    [JsonProperty("live_pp")]
    public double LivePP { get; private set; }
  }
}