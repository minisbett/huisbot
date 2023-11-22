using Newtonsoft.Json;
using huisbot.Services;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a beatmap from the osu! API v1.
/// </summary>
public class OsuBeatmap
{
  /// <summary>
  /// Returns a beatmap object representing no beatmap was found.
  /// </summary>
  public static OsuBeatmap NotFound => new OsuBeatmap() { WasFound = false };

  /// <summary>
  /// Bool whether the beatmap could be found or not. This property is used in <see cref="OsuApiService.GetBeatmapAsync(int)"/>, which returns
  /// an object where this is false in order to report that the request was successful, but no beatmap was found back to the caller.
  /// </summary>
  public bool WasFound { get; init; } = true;

  /// <summary>
  /// The ID of the beatmap set the beatmap belongs to.
  /// </summary>
  [JsonProperty("beatmapset_id")]
  public int SetId { get; private set; }

  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("beatmap_id")]
  public int Id { get; private set; }

  /// <summary>
  /// The maximum combo of the beatmap.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// The hit length of the beatmap in seconds.
  /// </summary>
  [JsonProperty("hit_length")]
  public int Length { get; private set; }

  /// <summary>
  /// The BPM of the beatmap.
  /// </summary>
  [JsonProperty("bpm")]
  public double BPM { get; private set; }

  /// <summary>
  /// The amount of hit circles in the beatmap.
  /// </summary>
  [JsonProperty("count_normal")]
  public int CircleCount { get; private set; }

  /// <summary>
  /// The amount of sliders in the beatmap.
  /// </summary>
  [JsonProperty("count_slider")]
  public int SliderCount { get; private set; }

  /// <summary>
  /// The amount of spinners in the beatmap.
  /// </summary>
  [JsonProperty("count_spinner")]
  public int SpinnerCount { get; private set; }

  /// <summary>
  /// The circle size of the beatmap.
  /// </summary>
  [JsonProperty("diff_size")]
  public double CircleSize { get; private set; }

  /// <summary>
  /// The approach rate of the beatmap.
  /// </summary>
  [JsonProperty("diff_approach")]
  public double ApproachRate { get; private set; }

  /// <summary>
  /// The overall difficulty of the beatmap.
  /// </summary>
  [JsonProperty("diff_overall")]
  public double OverallDifficulty { get; private set; }

  /// <summary>
  /// The hp drain rate of the beatmap.
  /// </summary>
  [JsonProperty("diff_drain")]
  public double DrainRate { get; private set; }

  /// <summary>
  /// The artist of the song of the beatmap.
  /// </summary>
  [JsonProperty("artist")]
  public string? Artist { get; private set; }

  /// <summary>
  /// The title of the song of the beatmap.
  /// </summary>
  [JsonProperty("title")]
  public string? Title { get; private set; }

  /// <summary>
  /// The dififculty name of the beatmap.
  /// </summary>
  [JsonProperty("version")]
  public string? Version { get; private set; }

  /// <summary>
  /// The creator of the beatmap.
  /// </summary>
  [JsonProperty("creator")]
  public string? Mapper { get; private set; }
}
