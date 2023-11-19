using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a rework from the Huis API.
/// </summary>
public class HuisRework
{
  /// <summary>
  /// The internal id of the rework. 1 is always the live pp system.
  /// </summary>
  [JsonProperty("id")]
  public int Id { get; private set; }

  /// <summary>
  /// An internal safename identifier for the rework. (lowercase + _)
  /// </summary>
  [JsonProperty("code")]
  public string? Code { get; private set; }

  /// <summary>
  /// A human readable name for the rework.
  /// </summary>
  [JsonProperty("name")]
  public string? Name { get; private set; }

  /// <summary>
  /// The type of rework. (LIVE, REWORK_PUBLIC_ACTIVE, REWORK_PUBLIC_INACTIVE, HISTORIC, MASTER)
  /// </summary>
  [JsonProperty("rework_type_code")]
  public string? ReworkType { get; private set; }

  /// <summary>
  /// The URL to the rework on GitHub. This may or may not be targetting the correct branch directly.
  /// </summary>
  [JsonProperty("url")]
  public string? Url { get; private set; }

  /// <summary>
  /// The ID of the current commit. The corresponding GitHub repository can be found in <see cref="Url"/>.
  /// </summary>
  [JsonProperty("commit")]
  public string? Commit { get; private set; }

  /// <summary>
  /// The ID of the ruleset/gamemode the rework is for.
  /// </summary>
  [JsonProperty("gamemode")]
  public int RulesetId { get; private set; }

  /// <summary>
  /// The description of the rework, as displayed in the banner text on the website.
  /// </summary>
  [JsonProperty("banner_text")]
  public string? Description { get; private set; }

  /// <summary>
  /// Bool whether the rework is the live pp system.
  /// </summary>
  public bool IsLive => Id == LiveId;

  /// <summary>
  /// Bool whether the rework is public or not.
  /// </summary>
  public bool IsPublic => ReworkType?.StartsWith("REWORK_PUBLIC") ?? false;

  /// <summary>
  /// Bool whether the rework is active or not.
  /// </summary>
  public bool IsActive => !ReworkType?.EndsWith("INACTIVE") ?? false;

  /// <summary>
  /// Bool whether the rework is historic.
  /// </summary>
  public bool IsHistoric => ReworkType == "HISTORIC";

  /// <summary>
  /// Bool whether the rework is the one containung changes confirmed for next deploy.
  /// </summary>
  public bool IsConfirmed => ReworkType == "MASTER";

  /// <summary>
  /// The rework ID of the live pp system.
  /// </summary>
  public static int LiveId => 1;

  public override string ToString()
  {
    return $"{Id} {Name} ({Code})";
  }
}
