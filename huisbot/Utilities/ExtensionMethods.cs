using huisbot.Models.Huis;

namespace huisbot.Utilities;

/// <summary>
/// Provides extension methods for variosu classes.
/// </summary>
public static class ExtensionMethods
{
  /// <summary>
  /// Converts the timespan into a string such as "3 weeks, 6 days, 2 hours, 4 minutes, 1 second".
  /// </summary>
  /// <param name="timeSpan">The timespan to convert.</param>
  /// <returns>The string representation of the timespan.</returns>
  public static string ToUptimeString(this TimeSpan timeSpan)
  {
    // Prepare the units and the values of each unit.
    string[] units = { "week", "day", "hour", "minute", "second" };
    int[] values = { timeSpan.Days / 7, timeSpan.Days % 7, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds };

    // Go through all values, check whether its > 0 and add it to the items with its unit.
    List<string> items = new List<string>();
    for (int i = 0; i < values.Length; i++)
      if (values[i] > 0)
        items.Add($"{values[i]} {units[i]}{(values[i] == 1 ? "" : "s")}");

    // Join the items with a comma and return the string.
    return string.Join(", ", items);
  }

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
