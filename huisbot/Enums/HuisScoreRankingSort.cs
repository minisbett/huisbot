namespace huisbot.Enums;

/// <summary>
/// A "fake enum" representing the sorting and order options for the global score rankings returned by the Huis API.
/// </summary>
public class HuisScoreRankingSort
{
  /// <summary>
  /// Returns all the available sort options.
  /// </summary>
  public static HuisScoreRankingSort[] All => new HuisScoreRankingSort[]
  {
    LivePPDesc, LivePPAsc, LocalPPDesc, LocalPPAsc, PPDifferenceDesc, PPDifferenceAsc, AimPPDesc, AimPPAsc, TapPPDesc, TapPPAsc, FLPPDesc, FLPPAsc
  };

  /// <summary>
  /// Sorts by the Live PP of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort LivePPDesc { get; } = new("live_pp", false, "Live PP (Descending)");

  /// <summary>
  /// Sorts by the Live PP of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort LivePPAsc { get; } = new("live_pp", true, "Live PP (Ascending)");

  /// <summary>
  /// Sorts by the Local PP of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort LocalPPDesc { get; } = new("local_pp", false, "Local PP (Descending)");

  /// <summary>
  /// Sorts by the Local PP of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort LocalPPAsc { get; } = new("local_pp", true, "Local PP (Ascending)");

  /// <summary>
  /// Sorts by the PP difference of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort PPDifferenceDesc { get; } = new("pp_diff", false, "PP Difference (Descending)");

  /// <summary>
  /// Sorts by the PP difference of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort PPDifferenceAsc { get; } = new("pp_diff", true, "PP Difference (Ascending)");

  /// <summary>
  /// Sorts by the weighted Aim PP of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort AimPPDesc { get; } = new("aim_pp", false, "Weighted Aim PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Aim PP of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort AimPPAsc { get; } = new("aim_pp", true, "Weighted Aim PP (Ascending)");

  /// <summary>
  /// Sorts by the weighted Tap PP of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort TapPPDesc { get; } = new("tap_pp", false, "Weighted Tap PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Tap PP of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort TapPPAsc { get; } = new("tap_pp", true, "Weighted Tap PP (Ascending)");

  /// <summary>
  /// Sorts by the weighted FL PP of the score in descending order.
  /// </summary>
  public static HuisScoreRankingSort FLPPDesc { get; } = new("fl_pp", false, "Weighted FL PP (Descending)");

  /// <summary>
  /// Sorts by the weighted FL PP of the score in ascending order.
  /// </summary>
  public static HuisScoreRankingSort FLPPAsc { get; } = new("fl_pp", true, "Weighted FL PP (Ascending)");

  /// <summary>
  /// An identifier to uniquely identify the combination of sort code and order.
  /// </summary>
  public string Id => $"{Code}_{(IsAscending ? "asc" : "desc")}";

  /// <summary>
  /// The code used in the Huis API parameter for this sort option.
  /// </summary>
  public string Code { get; }

  /// <summary>
  /// Bool whether the sort order is ascending or not.
  /// </summary>
  public bool IsAscending { get; }

  /// <summary>
  /// The display name of the sort option.
  /// </summary>
  public string DisplayName { get; }

  private HuisScoreRankingSort(string code, bool ascending, string displayName)
  {
    Code = code;
    IsAscending = ascending;
    DisplayName = displayName;
  }

  public override string ToString() => DisplayName;
}
