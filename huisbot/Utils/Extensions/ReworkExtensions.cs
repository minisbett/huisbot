using huisbot.Models.Huis;

namespace huisbot.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Rework"/> class.
/// </summary>
internal static class ReworkExtensions
{
  public static string GetBranchUrl(this Rework rework)
  {
    if (rework.Url is null || rework.Commit is null)
      return "";

    // If the URL of the rework points to a non-master branch or a commit on the ppy/osu repository, return the URL as-is.
    if (rework.Url.StartsWith("https://github.com/ppy/osu/tree") && !rework.Url.Contains("/tree/master"))
      return rework.Url;

    // Otherwise, get the base repository URL (there might be something appending it) and append the commit.
    return string.Join('/', rework.Url.Split('/').Take(5)) + $"/tree/{rework.Commit}";
  }

  /// <summary>
  /// Returns a human readable string for the rework type.
  /// </summary>
  /// <param name="rework">The rework.</param>
  /// <returns>A human readable string for the rework type.</returns>
  public static string GetReadableReworkType(this Rework rework)
    => rework.ReworkType switch
    {
      "LIVE" => "Live",
      "REWORK_PUBLIC_ACTIVE" => "Active Public Rework",
      "REWORK_PUBLIC_INACTIVE" => "Inactive Public Rework",
      "HISTORIC" => "Historic",
      "MASTER" => "Confirmed for next deploy",
      _ => "Unknown"
    };

  /// <summary>
  /// Returns a human readable name for the ruleset targetted by the rework.
  /// </summary>
  /// <param name="rework">The rework.</param>
  /// <returns>A human readable string for the ruleset.</returns>
  public static string GetReadableRuleset(this Rework rework)
   => rework.RulesetId switch
   {
     0 => "osu!",
     1 => "osu!taiko",
     2 => "osu!catch",
     3 => "osu!mania",
     _ => "Unknown"
   };
}