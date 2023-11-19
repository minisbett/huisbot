using huisbot.Services;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a player from the Huis API.
/// </summary>
public class HuisPlayer
{
  /// <summary>
  /// Returns a player object representing an uncalculated player.
  /// </summary>
  public static HuisPlayer Uncalculated => new HuisPlayer() { IsCalculated = false };

  /// <summary>
  /// Bool whether the player is calculated or not. This property is used in <see cref="HuisApiService.GetPlayerAsync(int, int)"/>, which returns
  /// an object where this is false in order to report that the request was successful, but no player data was received back to the caller.
  /// </summary>
  public bool IsCalculated { get; init; } = true;

  [JsonProperty("user_id")]
  /// <summary>
  /// The osu! user id of the player.
  /// </summary>
  public int Id { get; set; }

  [JsonProperty("name")]
  /// <summary>
  /// The name of the player.
  /// </summary>
  public string? Name { get; private set; }

  [JsonProperty("old_pp")]
  /// <summary>
  /// The live pp of the player.
  /// </summary>
  public double OldPP { get; private set; }

  [JsonProperty("new_pp_incl_bonus")]
  /// <summary>
  /// The pp of the player in the rework the player object is from.
  /// </summary>
  public double NewPP { get; private set; }

  [JsonProperty("bonus_pp")]
  /// <summary>
  /// The bonus pp of the player.
  /// </summary>
  public double BonusPP { get; private set; }

  [JsonProperty("weighted_acc_pp")]
  /// <summary>
  /// The weighted accuracy pp of the player in the rework the player object is from.
  /// </summary>
  public double AccPP { get; private set; }

  [JsonProperty("weighted_aim_pp")]
  /// <summary>
  /// The weighted aim pp of the player in the rework the player object is from.
  /// </summary>
  public double AimPP { get; private set; }

  [JsonProperty("weighted_tap_pp")]
  /// <summary>
  /// The weighted tapping pp of the player in the rework the player object is from.
  /// </summary>
  public double TapPP { get; private set; }

  [JsonProperty("weighted_fl_pp")]
  /// <summary>
  /// The weighted flashlight pp of the player in the rework the player object is from.
  /// </summary>
  public double FLPP { get; private set; }

  [JsonProperty("last_updated")]
  /// <summary>
  /// The last time the player got updated on Huismetbenen.
  /// </summary>
  public DateTime LastUpdated { get; private set; }

  [JsonProperty("global_rank_real")]
  /// <summary>
  /// The actual rank of the player.
  /// Might be null, indiucating the rank is not known.
  /// </summary>
  public int? Rank { get; private set; }

  public override string ToString()
  {
    return $"{Id} #{Rank} {Name} - {OldPP} -> {NewPP}pp (Aim: {AimPP}, Tap: {TapPP}, Acc: {AccPP}, FL: {FLPP})";
  }
}
