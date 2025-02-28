namespace huisbot.Helpers;

/// <summary>
/// Represents a sorting option with it's sorting code and order.
/// </summary>
/// <param name="code">The code for this sorting option.</param>
/// <param name="isAscending">Bool whether this sorting option is in ascending order.</param>
/// <param name="displayName">The display name of this sorting order.</param>
public class Sort(string code, bool isAscending, string displayName)
{
  /// <summary>
  /// A unique identifier for this sorting option.
  /// </summary>
  public string Id => $"{Code}_{(IsAscending ? "asc" : "desc")}";

  /// <summary>
  /// The code for this sorting option.
  /// </summary>
  public string Code { get; } = code;

  /// <summary>
  /// Bool whether this sorting option is ascending.
  /// </summary>
  public bool IsAscending { get; } = isAscending;

  /// <summary>
  /// The display name for this sorting option.
  /// </summary>
  public string DisplayName { get; } = displayName;

  /// <summary>
  /// The sort options for the ranking of all players.
  /// </summary>
  public static Sort[] RankingPlayers =>
  [
    new("old_pp", false, "Live PP"),
    new("new_pp_incl_bonus", false, "Local PP"),
    new("pp_change", false, "PP Difference (Descending)"),
    new("pp_change", true, "PP Difference (Ascending)"),
    new("weighted_aim_pp", false, "Weighted Aim PP"),
    new("weighted_tap_pp", false, "Weighted Tap PP"),
    new("weighted_acc_pp", false, "Weighted Acc PP"),
    new("weighted_fl_pp", false, "Weighted FL PP"),
    new("bonus_pp", false, "Bonus PP"),
    new("new_pp_excl_bonus", false, "Local PP Excl. Bonus"),
  ];

  /// <summary>
  /// The sort options for the ranking of all scores.
  /// </summary>
  public static Sort[] RankingScores =>
  [
    new("live_pp", false, "Live PP"),
    new("local_pp", false, "Local PP"),
    new("pp_diff", false, "PP Difference (Descending)"),
    new("pp_diff", true, "PP Difference (Ascending)"),
    new("aim_pp", false, "Weighted Aim PP"),
    new("tap_pp", false, "Weighted Tap PP"),
    new("fl_pp", false, "Weighted FL PP"),
  ];

  /// <summary>
  /// The sort options for the scores on the profile of a player.
  /// </summary>
  public static Sort[] ProfileScores =>
  [
    new("live_pp", false, "Live PP"),
    new("local_pp", false, "Local PP"),
    new("pp_diff", false, "PP Difference (Descending)"),
    new("pp_diff", true, "PP Difference (Ascending)")
  ];
}