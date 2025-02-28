using huisbot.Services;
using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a player from the Huis API.
/// </summary>
public class HuisPlayer
{
  /// <summary>
  /// Returns a player object representing an outdated player.
  /// </summary>
  public static HuisPlayer Outdated => new() { IsOutdated = true };

  /// <summary>
  /// Bool whether the player is outdated or not. This property is used in <see cref="HuisApiService.GetPlayerAsync(int, HuisRework)"/>, which returns
  /// an object where this is true in order to report that the request was successful, but outdated player data was received back to the caller.
  /// </summary>
  public bool IsOutdated { get; init; } = false;

  /// <summary>
  /// The osu! user id of the player.
  /// </summary>
  [JsonProperty("user_id")]
  public int Id { get; private set; }

  /// <summary>
  /// The name of the player.
  /// </summary>
  [JsonProperty("name")]
  public string Name { get; private set; } = null!;

  /// <summary>
  /// The live PP of the player.
  /// </summary>
  [JsonProperty("old_pp")]
  public double OldPP { get; private set; }

  /// <summary>
  /// The PP of the player in the rework the player object is from.
  /// </summary>
  [JsonProperty("new_pp_incl_bonus")]
  public double NewPP { get; private set; }

  /// <summary>
  /// The current global rank of the player. This is null if Huis is not able to know the rank of the player.
  /// </summary>
  [JsonProperty("old_global_rank")]
  public int? Rank { get; private set; }

  /// <summary>
  /// The bonus PP of the player.
  /// </summary>
  [JsonProperty("bonus_pp")]
  public double BonusPP { get; private set; }

  /// <summary>
  /// The weighted accuracy PP of the player in the rework the player object is from.
  /// </summary>
  [JsonProperty("weighted_acc_pp")]
  public double AccPP { get; private set; }

  /// <summary>
  /// The weighted aim PP of the player in the rework the player object is from.
  /// </summary>
  [JsonProperty("weighted_aim_pp")]
  public double AimPP { get; private set; }

  /// <summary>
  /// The weighted tapping PP of the player in the rework the player object is from.
  /// </summary>
  [JsonProperty("weighted_tap_pp")]
  public double TapPP { get; private set; }

  /// <summary>
  /// The weighted flashlight PP of the player in the rework the player object is from.
  /// </summary>
  [JsonProperty("weighted_fl_pp")]
  public double FLPP { get; private set; }

  /// <summary>
  /// The last time the player got updated on Huismetbenen.
  /// </summary>
  [JsonProperty("last_updated")]
  public DateTime LastUpdated { get; private set; }

  /// <summary>
  /// The most recent algorithm version the player was calculated in, used to determine whether a player is up-to-date or not.
  /// </summary>
  [JsonProperty("pp_version")]
  public int PPVersion { get; private set; }

  public override string ToString()
  {
    return $"{Id} #{Rank} {Name} - {OldPP} -> {NewPP}pp (Aim: {AimPP}, Tap: {TapPP}, Acc: {AccPP}, FL: {FLPP})";
  }
}
