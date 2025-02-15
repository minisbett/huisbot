using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a score from the osu! API v2.
/// </summary>
public class OsuScore
{
  /// <summary>
  /// The ID of the score.
  /// </summary>
  [JsonProperty("id")]
  public long Id { get; private set; }

  /// <summary>
  /// The accuracy of the score.
  /// </summary>
  [JsonProperty("accuracy")]
  public double Accuracy { get; private set; }

  /// <summary>
  /// The beatmap of the score.
  /// </summary>
  [JsonProperty("beatmap")]
  public OsuScoreBeatmap Beatmap { get; private set; } = null!;

  /// <summary>
  /// The beatmap set of the score.
  /// </summary>
  [JsonProperty("beatmapset")]
  public OsuScoreBeatmapSet BeatmapSet { get; private set; } = null!;

  /// <summary>
  /// The maximum achieved combo of the score.
  /// </summary>
  public int MaxCombo => MaxComboAPI ?? MaxComboHuis!.Value;

  /// <summary>
  /// The max combo if this object origins from the osu! API.
  /// </summary>
  [JsonProperty("max_combo")]
  private int? MaxComboAPI { get; set; }

  /// <summary>
  /// The max combo if this object origins from the osu-tools response of Huis.
  /// </summary>
  [JsonProperty("combo")]
  private int? MaxComboHuis { get; set; }

  /// <summary>
  /// The ID of the ruleset this score was set in. (0 = osu!std, 1 = osu!taiko, ...)
  /// </summary>
  [JsonProperty("ruleset_id")]
  public int RulesetId { get; private set; }

  /// <summary>
  /// The mods of the score, in the osu!lazer APIMod format.
  /// </summary>
  [JsonProperty("mods")]
  public OsuMods Mods { get; private set; } = [];

  /// <summary>
  /// The user of the score.
  /// </summary>
  [JsonProperty("user")]
  public OsuScoreUser User { get; private set; } = null!;

  /// <summary>
  /// The statistics (300s, 100s, 50s, misses) of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = null!;

  /// <summary>
  /// The time the score was submitted at.
  /// </summary>
  [JsonProperty("ended_at")]
  public DateTime SubmittedAt { get; private set; }

  // TODO: Make those inner classes redundant by generally fetching beatmaps/users via API v2 in the client, replacing the API v1 models.

  /// <summary>
  /// Represents the <see cref="Beatmap"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreBeatmap
  {
    /// <summary>
    /// The ID of the beatmap of the score.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; private set; }

    /// <summary>
    /// The difficulty name of the beatmap of the score.
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; private set; } = "";
  }

  /// <summary>
  /// Represents the <see cref="BeatmapSet"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreBeatmapSet
  {
    /// <summary>
    /// The title of the beatmap set of the score.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; private set; } = "";
  }

  /// <summary>
  /// Represents the <see cref="User"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreUser
  {
    /// <summary>
    /// The name of the user of the score.
    /// </summary>
    [JsonProperty("username")]
    public string Name { get; private set; } = "";

    /// <summary>
    /// The osu! user ID of the user of the score.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; private set; }
  }
}