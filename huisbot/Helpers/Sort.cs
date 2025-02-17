namespace huisbot.Utilities;

/// <summary>
/// Represents a sorting option with it's sorting code and order.
/// </summary>
public class Sort
{
  /// <summary>
  /// A unique identifier for this sorting option.
  /// </summary>
  public string Id => $"{Code}_{(IsAscending ? "asc" : "desc")}";

  /// <summary>
  /// The code for this sorting option.
  /// </summary>
  public string Code { get; }

  /// <summary>
  /// Bool whether this sorting option is ascending.
  /// </summary>
  public bool IsAscending { get; }

  /// <summary>
  /// The display name for this sorting option.
  /// </summary>
  public string DisplayName { get; }

  /// <summary>
  /// Creates a new sorting option with the specified code, order and display name.
  /// </summary>
  /// <param name="code">The code for this sorting option.</param>
  /// <param name="isAscending">Bool whether this sorting option is in ascending order.</param>
  /// <param name="displayName">The display name of this sorting order.</param>
  public Sort(string code, bool isAscending, string displayName)
  {
    Code = code;
    IsAscending = isAscending;
    DisplayName = displayName;
  }

  /// <summary>
  /// The sort options for the ranking of all players.
  /// </summary>
  public static Sort[] RankingPlayers =>
  [
    new("old_pp", false, "Live PP"),
    //new("old_pp", true, "Old PP (Ascending)"),
    new("new_pp_incl_bonus", false, "Local PP"),
    //new("new_pp_incl_bonus", true, "New PP (Ascending)"),
    new("pp_change", false, "PP Difference (Descending)"),
    new("pp_change", true, "PP Difference (Ascending)"),
    new("weighted_aim_pp", false, "Weighted Aim PP"),
    //new("weighted_aim_pp", true, "Weighted Aim PP (Ascending)"),
    new("weighted_tap_pp", false, "Weighted Tap PP"),
    //new("weighted_tap_pp", true, "Weighted Tap PP (Ascending)"),
    new("weighted_acc_pp", false, "Weighted Acc PP"),
    //new("weighted_acc_pp", true, "Weighted Acc PP (Ascending)"),
    new("weighted_fl_pp", false, "Weighted FL PP"),
    //new("weighted_fl_pp", true, "Weighted FL PP (Ascending)"),
    new("bonus_pp", false, "Bonus PP"),
    //new("bonus_pp", true, "Bonus PP (Ascending)"),
    new("new_pp_excl_bonus", false, "Local PP Excl. Bonus"),
    //new("new_pp_excl_bonus", true, "New PP Excl. Bonus (Ascending)")
  ];

  /// <summary>
  /// The sort options for the ranking of all scores.
  /// </summary>
  public static Sort[] RankingScores =>
  [
    new("live_pp", false, "Live PP"),
   // new("live_pp", true, "Live PP (Ascending)"),
    new("local_pp", false, "Local PP"),
    //new("local_pp", true, "Local PP (Ascending)"),
    new("pp_diff", false, "PP Difference (Descending)"),
    new("pp_diff", true, "PP Difference (Ascending)"),
    new("aim_pp", false, "Weighted Aim PP"),
    //new("aim_pp", true, "Weighted Aim PP (Ascending)"),
    new("tap_pp", false, "Weighted Tap PP"),
    //new("tap_pp", true, "Weighted Tap PP (Ascending)"),
    new("fl_pp", false, "Weighted FL PP"),
    //new("fl_pp", true, "Weighted FL PP (Ascending)")
  ];

  /// <summary>
  /// The sort options for the scores on the profile of a player.
  /// </summary>
  public static Sort[] ProfileScores =>
  [
    new("live_pp", false, "Live PP"),
    //new("live_pp", true, "Live PP (Ascending)"),
    new("local_pp", false, "Local PP"),
    //new("local_pp", true, "Live PP (Ascending)"),
    new("pp_diff", false, "PP Difference (Descending)"),
    new("pp_diff", true, "PP Difference (Ascending)")
  ];
}