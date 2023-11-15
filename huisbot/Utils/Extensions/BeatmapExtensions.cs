using huisbot.Models.Osu;

namespace huisbot.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="OsuBeatmap"/> class.
/// </summary>
internal static class BeatmapExtensions
{
  /// <summary>
  /// Returns the length of the beatmap, including clock rate changes through specified mods.
  /// </summary>
  /// <param name="beatmap">The beatmap.</param>
  /// <param name="modsStr">The mods.</param>
  /// <returns>The length of the beatmap including the specified mods.</returns>
  public static TimeSpan GetLength(this OsuBeatmap beatmap, string modsStr)
  {
    string[] mods = modsStr.Chunk(2).Select(x => new string(x)).ToArray();
    TimeSpan span = TimeSpan.FromSeconds(beatmap.Length);

    // If DoubleTime/Nightcore, the length is divided by 1.5.
    if (mods.Contains("DT") || mods.Contains("NC"))
      return span / 1.5;
    // If HalfTime, the length is multiplied by 1.5.
    else if (mods.Contains("HT"))
      return span * 1.5;

    return span;
  }

  /// <summary>
  /// Returns the BPM of the beatmap, including clock rate changes through mods.
  /// </summary>
  /// <param name="beatmap">The beatmap.</param>
  /// <param name="modsStr">The mods.</param>
  /// <returns>The BPM of the beatmap including the specified mods.</returns>
  public static double GetBPM(this OsuBeatmap beatmap, string modsStr)
  {
    string[] mods = modsStr.Chunk(2).Select(x => new string(x)).ToArray();

    // If DoubleTime/Nightcore, the BPM is multiplied by 1.5.
    if (mods.Contains("DT") || mods.Contains("NC"))
      return Math.Round(beatmap.BPM * 1.5, 2);
    // If HalfTime, the BPM is didided by 1.5.
    else if (mods.Contains("HT"))
      return Math.Round(beatmap.BPM / 1.5, 2);

    return beatmap.BPM;
  }
}
