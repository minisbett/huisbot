using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a beatmap from the osu! API v2.
/// </summary>
public class OsuBeatmap
{
  /// <summary>
  /// The ID of the beatmap set the beatmap belongs to.
  /// </summary>
  [JsonProperty("beatmapset_id")]
  public int SetId { get; private set; }

  /// <summary>
  /// The ID of the beatmap.
  /// </summary>
  [JsonProperty("id")]
  public int Id { get; private set; }

  /// <summary>
  /// The maximum combo of the beatmap.
  /// </summary>
  [JsonProperty("max_combo")]
  public int MaxCombo { get; private set; }

  /// <summary>
  /// The hit length of the beatmap in seconds.
  /// </summary>
  [JsonProperty("total_length")]
  public int Length { get; private set; }

  /// <summary>
  /// The amount of hit circles in the beatmap.
  /// </summary>
  [JsonProperty("count_circles")]
  public int CircleCount { get; private set; }

  /// <summary>
  /// The amount of sliders in the beatmap.
  /// </summary>
  [JsonProperty("count_sliders")]
  public int SliderCount { get; private set; }

  /// <summary>
  /// The amount of spinners in the beatmap.
  /// </summary>
  [JsonProperty("count_spinners")]
  public int SpinnerCount { get; private set; }

  /// <summary>
  /// The circle size of the beatmap.
  /// </summary>
  [JsonProperty("cs")]
  public double CircleSize { get; private set; }

  /// <summary>
  /// The approach rate of the beatmap.
  /// </summary>
  [JsonProperty("ar")]
  public double ApproachRate { get; private set; }

  /// <summary>
  /// The overall difficulty of the beatmap.
  /// </summary>
  [JsonProperty("accuracy")]
  public double OverallDifficulty { get; private set; }

  /// <summary>
  /// The hp drain rate of the beatmap.
  /// </summary>
  [JsonProperty("drain")]
  public double DrainRate { get; private set; }

  /// <summary>
  /// The dififculty name of the beatmap.
  /// </summary>
  [JsonProperty("version")]
  public string Version { get; private set; } = null!;

  /// <summary>
  /// The beatmap set this beatmap belongs to.
  /// </summary>
  [JsonProperty("beatmapset")]
  public OsuBeatmapSet Set { get; private set; } = null!;

  /// <summary>
  /// Represents the beatmap set of a <see cref="OsuBeatmap"/> from the osu! API v2.
  /// </summary>
  public class OsuBeatmapSet
  {
    /// <summary>
    /// The artist of the song of the beatmap.
    /// </summary>
    [JsonProperty("artist")]
    public string Artist { get; private set; } = null!;

    /// <summary>
    /// The title of the song of the beatmap.
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// The BPM of the beatmap.
    /// </summary>
    [JsonProperty("bpm")]
    public float BPM { get; private set; }
  }

  /// <summary>
  /// Returns the length of the beatmap, including clock rate changes through specified mods.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The length of the beatmap including the specified mods.</returns>
  public TimeSpan GetLength(OsuMods mods)
  {
    // Return the timespan of the beatmap multiplied by the clock rate of the mods.
    return TimeSpan.FromSeconds(Length) * mods.ClockRate;
  }

  /// <summary>
  /// Returns the BPM of the beatmap, including clock rate changes through mods.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The BPM of the beatmap including the specified mods.</returns>
  public double GetBPM(OsuMods mods)
  {
    // Return the BPM multiplied by the clock rate of the mods.
    return Set.BPM * mods.ClockRate;
  }

  /// <summary>
  /// Returns the mod-adjusted circle size of the beatmap.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The mod-adjusted circle size.</returns>
  public double GetAdjustedCS(OsuMods mods)
  {
    double cs = CircleSize;

    // If HardRock, the CS is multiplied by 1.3.
    if (mods.IsHardRock)
      cs *= 1.3;
    // If Easy, the CS is multiplied by 0.5.
    else if (mods.IsEasy)
      cs *= 0.5;

    return Math.Min(cs, 10);
  }

  /// <summary>
  /// Returns the mod-adjusted approach rate of the beatmap.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The mod-adjusted approach rate.</returns>
  public double GetAdjustedAR(OsuMods mods)
  {
    double ar = ApproachRate;

    // If HardRock, the AR is multiplied by 1.4, up to 10.
    if (mods.IsHardRock)
      ar = Math.Min(ar * 1.4, 10);
    // If Easy, the AR is multiplied by 0.5.
    else if (mods.IsEasy)
      ar *= 0.5;

    // Ensure scaling of the pre-empt through the clock rate of the mods.
    int ms = (int)(ar >= 5 ? ar == 5 ? 1200 : 1200 - 750 * (ar - 5) / 5d : 1200 + 600 * (5 - ar) / 5d);
    ms = (int)(ms / mods.ClockRate);
    return Math.Min(11.11, (ms == 1200) ? 5 : (ms > 1200) ? 5 - 5 * (ms - 1200) / 600d : 5 + 5 * (1200 - ms) / 750d);
  }

  /// <summary>
  /// Returns the mod-adjusted overall difficulty of the beatmap.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The mod-adjusted overall difficulty.</returns>
  public double GetAdjustedOD(OsuMods mods)
  {
    double od = OverallDifficulty;

    // If HardRock, the OD is multiplied by 1.4.
    if (mods.IsHardRock)
      od = Math.Min(od * 1.4, 10);
    // If Easy, the OD is multiplied by 0.5.
    else if (mods.IsEasy)
      od *= 0.5;

    // Ensure scaling of the pre-empt through the clock rate of the mods.
    return Math.Min(11.11, (80 - (80 - 6 * od) / mods.ClockRate) / 6);
  }

  /// <summary>
  /// Returns the mod-adjusted drain rate of the beatmap.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The mod-adjusted drain rate.</returns>
  public double GetAdjustedHP(OsuMods mods)
  {
    double hp = DrainRate;

    // If HardRock, the HP is multiplied by 1.4.
    if (mods.IsHardRock)
      hp *= 1.4;
    // If Easy, the HP is multiplied by 0.5.
    else if (mods.IsEasy)
      hp *= 0.5;

    return Math.Min(hp, 10);
  }
}
