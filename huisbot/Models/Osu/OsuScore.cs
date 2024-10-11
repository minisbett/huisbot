using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a score from the osu! API v2.
/// </summary>
public class OsuScore
{

  /// <summary>
  /// Accuracy of this score
  /// </summary>
  [JsonProperty("accuracy")]
  public float Accuracy { get; private set; }

  /// <summary>
  /// Id of the beatmap this score was set on
  /// </summary>
  [JsonProperty("beatmap_id")]
  public int BeatmapId { get; private set; }

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("best_id")]
  public int? BestId { get; private set; }

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("build_id")]
  public int? BuildId { get; private set; }

  /// <summary>
  /// TODO: What is this? (Documentation says, "Only for solo_score type")
  /// </summary>
  [JsonProperty("classic_total_score")]
  public long ClassicTotalScore { get; private set; }

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("ended_at")]
  public DateTimeOffset EndedAt { get; private set; }

  /// <summary>
  /// Whether this score has an associated replay or not
  /// </summary>
  [JsonProperty("has_replay")]
  public bool HasReplay { get; private set; }

  /// <summary>
  /// Id of this score
  /// </summary>
  [JsonProperty("id")]
  public int Id { get; private set; }

  /// <summary>
  /// Whether this score has a perfect combo or not
  /// </summary>
  [JsonProperty("is_perfect_combo")]
  public bool IsPerfectCombo { get; private set; }

  /// <summary>
  /// Whether this score is a perfect score in stable or not
  /// </summary>
  [JsonProperty("legacy_perfect")]
  public bool LegacyPerfect { get; private set; }

  /// <summary>
  /// Legacy score id, if available
  /// </summary>
  [JsonProperty("legacy_score_id")]
  public int? LegacyScoreId { get; private set; }

  /// <summary>
  /// Total score calculated using the legacy score calculation system
  /// </summary>
  [JsonProperty("legacy_total_score")]
  public int LegacyTotalScore { get; private set; }

  /// <summary>
  /// The maximum achieved combo of the score.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("maximum_statistics")]
  public OsuScoreStatistics MaximumStatistics { get; private set; } = new OsuScoreStatistics();

  /// <summary>
  /// The mods of the score.
  /// </summary>
  [JsonProperty("mods")]
  public string[] Mods { get; private set; } = [];

  /// <summary>
  /// Whether this score is a pass or not
  /// </summary>
  [JsonProperty("passed")]
  public bool Passed { get; private set; }

  /// <summary>
  /// TODO: What is this? (Documentation says, "Only for multiplayer score")
  /// </summary>
  [JsonProperty("playlist_item_id")]
  public int? PlaylistItemId { get; private set; }

  /// <summary>
  /// The amount of pp this score gives
  /// </summary>
  [JsonProperty("pp")]
  public float? PP { get; private set; }

  /// <summary>
  /// Whether or not the score may eventually be deleted. Only for solo_score type
  /// </summary>
  [JsonProperty("preserve")]
  public bool PreserveScore { get; private set; }

  /// <summary>
  /// TODO: What is this? (Documentation says, "Only for solo_score type")
  /// </summary>
  [JsonProperty("processed")]
  public bool IsProcessed { get; private set; }

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("rank")]
  public string Rank { get; private set; } = "";

  /// <summary>
  /// Whether or not the score can have pp. Only for solo_score type
  /// </summary>
  [JsonProperty("ranked")]
  public bool IsRanked { get; private set; }

  /// <summary>
  /// Id of the room this score was set in (Only for multiplayer score)
  /// </summary>
  [JsonProperty("room_id")]
  public int? RoomId { get; private set; }

  /// <summary>
  /// Id of the Ruleset this score was achieved in
  /// </summary>
  [JsonProperty("ruleset_id")]
  public int RulesetId;

  /// <summary>
  /// TODO: What is this?
  /// </summary>
  [JsonProperty("started_at")]
  public DateTimeOffset? StartedAt;

  /// <summary>
  /// The statistics (300s, 100s, 50s, misses) of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = new OsuScoreStatistics();

  /// <summary>
  /// The total score of this <see cref="OsuScore"/> Object
  /// </summary>
  [JsonProperty("total_score")]
  public int TotalScore;

  /// <summary>
  /// The type of score this is, could be solo_score or multiplayer score (don't know value)
  /// </summary>
  [JsonProperty("type")]
  public string Type { get; private set; } = "";

  /// <summary>
  /// The id of the user who submitted this score.
  /// </summary>
  [JsonProperty("user_id")]
  public int UserId { get; private set; }

  /// <summary>
  /// Represents the <see cref="Statistics"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreStatistics
  {
    /// <summary>
    /// The amount of 300s in the score.
    /// </summary>
    [JsonProperty("count_300")]
    public int Count300 { get; private set; }

    /// <summary>
    /// The amount of 100s in the score.
    /// </summary>
    [JsonProperty("count_100")]
    public int Count100 { get; private set; }

    /// <summary>
    /// The amount of 50s in the score.
    /// </summary>
    [JsonProperty("count_50")]
    public int Count50 { get; private set; }

    /// <summary>
    /// The amount of misses in the score.
    /// </summary>
    [JsonProperty("count_miss")]
    public int Misses { get; private set; }
  }
}