using huisbot.Utilities;
using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a score from the osu! API v2.
/// </summary>
public class OsuScore
{
  /// <summary>
  /// The accuracy of the score.
  /// </summary>
  [JsonProperty("accuracy")]
  public double Accuracy { get; private set; }

  /// <summary>
  /// The beatmap of the score.
  /// </summary>
  [JsonProperty("beatmap")]
  public OsuScoreBeatmap Beatmap { get; private set; } = new OsuScoreBeatmap();

  /// <summary>
  /// The beatmap set of the score.
  /// </summary>
  [JsonProperty("beatmapset")]
  public OsuScoreBeatmapSet BeatmapSet { get; private set; } = new OsuScoreBeatmapSet();

  /// <summary>
  /// The maximum achieved combo of the score.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// The ID of the ruleset this score was set in. (0 = osu!std, 1 = osu!taiko, ...)
  /// </summary>
  [JsonProperty("ruleset_id")]
  public int RulesetId { get; private set; }

  /// <summary>
  /// The mods of the score, in the osu!lazer APIMod format.
  /// </summary>
  [JsonProperty("mods")]
  public OsuMod[] Mods { get; private set; } = [];

    /// <summary>
  /// The user of the score.
  /// </summary>
  [JsonProperty("user")]
  public OsuScoreUser User { get; private set; } = new OsuScoreUser();

  /// <summary>
  /// The statistics (300s, 100s, 50s, misses) of the score.
  /// </summary>
  [JsonProperty("statistics")]
  public OsuScoreStatistics Statistics { get; private set; } = new OsuScoreStatistics();

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
  /// Represents the <see cref="Statistics"/> component of the <see cref="OsuScore"/> type.
  /// </summary>
  public class OsuScoreStatistics
  {
    /// <summary>
    /// The amount of 300s in the score.
    /// </summary>
    [JsonProperty("great")]
    public int Count300 { get; private set; }

    /// <summary>
    /// The amount of 100s in the score.
    /// </summary>
    [JsonProperty("ok")]
    public int Count100 { get; private set; }

    /// <summary>
    /// The amount of 50s in the score.
    /// </summary>
    [JsonProperty("meh")]
    public int Count50 { get; private set; }

    /// <summary>
    /// The amount of misses in the score.
    /// </summary>
    [JsonProperty("miss")]
    public int Misses { get; private set; }
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