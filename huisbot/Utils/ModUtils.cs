namespace huisbot.Utils;

/// <summary>
/// Provides utility methods for osu! mods.
/// </summary>
internal static class ModUtils
{
  /// <summary>
  /// Returns the clock rate of the sepcified mods.
  /// DT/NC = 1.5, HT = 0.6666666, otherwise 1.
  /// </summary>
  /// <param name="modsStr">The mods.</param>
  /// <returns>The clock rate for the mods.</returns>
  public static double GetClockRate(string modsStr)
  {
    string[] mods = Split(modsStr);

    // Return the clock rate for the correspondign mod.
    return !mods.Contains("HT") ? mods.Contains("DT") || mods.Contains("NC") ? 3 / 2d : 1 : 2 / 3d;
  }

  /// <summary>
  /// Returns an array of mods, splitting the specified mods string.
  /// </summary>
  /// <param name="modsStr">The mods.</param>
  /// <returns>The array of mods.</returns>
  public static string[] Split(string modsStr)
  {
    return modsStr.Chunk(2).Select(x => new string(x)).ToArray();
  }
}