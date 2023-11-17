namespace huisbot.Enums;

/// <summary>
/// A "fake enum" representing the sorting and order options for the global player rankings returned by the Huis API.
/// </summary>
public class HuisPlayerSort
{
  private static HuisPlayerSort[] _all = null!;

  /// <summary>
  /// Returns all the available sort options.
  /// </summary>
  public static HuisPlayerSort[] All
  {
    get
    {
      // Lazy load the array since the property values are not known at compile time.
      if (_all is null)
        _all = new HuisPlayerSort[]
        {
          OldPPDesc, OldPPAsc, NewPPDesc, NewPPAsc, PPDifferenceDesc, PPDifferenceAsc, AimPPDesc, AimPPAsc, TapPPDesc, TapPPAsc, AccPPDesc, AccPPAsc,
          FLPPDesc, FLPPAsc, BonusPPDesc, BonusPPAsc, NewPPExclBonusDesc, NewPPExclBonusAsc
        };

      return _all;
    }
  }

  /// <summary>
  /// Sorts by the old PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort OldPPDesc { get; } = new("old_pp", false, "Old PP (Descending)");

  /// <summary>
  /// Sorts by the old PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort OldPPAsc { get; } = new("old_pp", true, "Old PP (Ascending)");

  /// <summary>
  /// Sorts by the new PP inclusive bonus of the player in descending order.
  /// </summary>
  public static HuisPlayerSort NewPPDesc { get; } = new("new_pp_incl_bonus", false, "New PP (Descending)");

  /// <summary>
  /// Sorts by the new PP inclusive bonus of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort NewPPAsc { get; } = new("new_pp_incl_bonus", true, "New PP (Ascending)");

  /// <summary>
  /// Sorts by the PP difference of the player in descending order.
  /// </summary>
  public static HuisPlayerSort PPDifferenceDesc { get; } = new("pp_change", false, "PP Difference (Descending)");

  /// <summary>
  /// Sorts by the PP difference of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort PPDifferenceAsc { get; } = new("pp_change", true, "PP Difference (Ascending)");

  /// <summary>
  /// Sorts by the weighted Aim PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort AimPPDesc { get; } = new("weighted_aim_pp", false, "Weighted Aim PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Aim PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort AimPPAsc { get; } = new("weighted_aim_pp", true, "Weighted Aim PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Tap PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort TapPPDesc { get; } = new("weighted_tap_pp", false, "Weighted Tap PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Tap PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort TapPPAsc { get; } = new("weighted_tap_pp", true, "Weighted Tap PP (Ascending)");

  /// <summary>
  /// Sorts by the weighted Acc PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort AccPPDesc { get; } = new("weighted_acc_pp", false, "Weighted Acc PP (Descending)");

  /// <summary>
  /// Sorts by the weighted Acc PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort AccPPAsc { get; } = new("weighted_acc_pp", true, "Weighted Acc PP (Ascending)");

  /// <summary>
  /// Sorts by the weighted FL PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort FLPPDesc { get; } = new("weighted_fl_pp", false, "Weighted FL PP (Descending)");

  /// <summary>
  /// Sorts by the weighted FL PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort FLPPAsc { get; } = new("weighted_fl_pp", true, "Weighted FL PP (Ascending)");

  /// <summary>
  /// Sorts by the Bonus PP of the player in descending order.
  /// </summary>
  public static HuisPlayerSort BonusPPDesc { get; } = new("bonus_pp", false, "Bonus PP (Descending)");

  /// <summary>
  /// Sorts by the Bonus PP of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort BonusPPAsc { get; } = new("bonus_pp", true, "Bonus PP (Ascending)");

  /// <summary>
  /// Sorts by the new PP with bonus pp exclusive of the player in descending order.
  /// </summary>
  public static HuisPlayerSort NewPPExclBonusAsc { get; } = new("new_pp_excl_bonus", false, "New PP Excl. Bonus (Descending)");

  /// <summary>
  /// Sorts by the new PP with bonus pp exclusive of the player in ascending order.
  /// </summary>
  public static HuisPlayerSort NewPPExclBonusDesc { get; } = new("new_pp_excl_bonus", true, "New PP Excl. Bonus (Ascending)");

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

  private HuisPlayerSort(string code, bool ascending, string displayName)
  {
    Code = code;
    IsAscending = ascending;
    DisplayName = displayName;
  }

  public override string ToString() => DisplayName;
}
