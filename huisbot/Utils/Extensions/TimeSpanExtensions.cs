namespace huisbot.Utils.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="TimeSpan"/> class.
/// </summary>
internal static class TimeSpanExtensions
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
}
