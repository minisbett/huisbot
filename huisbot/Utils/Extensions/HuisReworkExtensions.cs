using huisbot.Models.Huis;

namespace huisbot.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="HuisRework"/> class.
/// </summary>
internal static class HuisReworkExtensions
{
  /// <summary>
  /// Returns an URL to the commit of the rework.
  /// </summary>
  /// <param name="rework">The rework.</param>
  /// <returns>An URL to the commit of the rework.</returns>
  public static string GetCommitUrl(this HuisRework rework)
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
  /// Returns a human readable string for the rework status.
  /// Example: 🔒 Private • ✅ Active
  /// </summary>
  /// <param name="rework">The rework.</param>
  /// <returns>A human readable string for the rework status.</returns>
  public static string GetReworkStatusString(this HuisRework rework)
    => rework switch
      {
        { IsLive: true } => "🔴 Live",
        { IsHistoric: true } => "📜 Historic",
        { IsConfirmed: true } => "✅ Confirmed for next deploy",
        { IsPublic: true, IsActive: true } => "🌐 Public • ✅ Active",
        { IsPublic: true, IsActive: false } => "🌐 Public • 💀 Inactive",
        { IsPublic: false, IsActive: true } => "🔒 Private • ✅ Active",
        { IsPublic: false, IsActive: false } => "🔒 Private • 💀 Inactive",
        _ => rework.ReworkType ?? "null"
      };

  /// <summary>
  /// Returns a human readable name for the ruleset targetted by the rework.
  /// </summary>
  /// <param name="rework">The rework.</param>
  /// <returns>A human readable string for the ruleset.</returns>
  public static string GetReadableRulesetName(this HuisRework rework)
   => rework.RulesetId switch
   {
     0 => "osu!",
     1 => "osu!taiko",
     2 => "osu!catch",
     3 => "osu!mania",
     _ => "Unknown"
   };

  /// <summary>
  /// Sorts the reworks by relevancy to the user.
  /// </summary>
  /// <param name="reworks">The reworks.</param>
  /// <returns>The sorted reworks.</returns>
  public static HuisRework[] OrderByRelevancy(this HuisRework[] reworks)
  {
    // Order the reworks.
    return reworks.OrderBy(x => !x.IsLive).ThenBy(x => x.IsConfirmed).ThenBy(x => x.IsHistoric).ThenBy(x => !x.IsActive)
                  .ThenBy(x => !x.IsPublic).ToArray();
  }
}