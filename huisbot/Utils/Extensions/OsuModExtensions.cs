using huisbot.Enums;

namespace huisbot.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="OsuMod"/> class.
/// </summary>
internal static class OsuModExtensions
{
  /// <summary>
  /// Returns the string representation of the mods.
  /// </summary>
  /// <param name="mods">The mods.</param>
  /// <returns>The string representation of the mods.</returns>
  public static string ToModString(this OsuMod[] mods)
  {
    return string.Join("", mods.Select(x => x.Acronym));
  }
}
