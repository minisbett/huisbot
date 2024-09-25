using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a rework from the Huis API.
/// </summary>
public class HuisRework
{
  /// <summary>
  /// The rework ID of the live pp system.
  /// </summary>
  public const int LiveId = 1;

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
  public string? GitHubUrl { get; private set; }

  /// <summary>
  /// The ID of the current commit. The corresponding GitHub repository can be found in <see cref="GitHubUrl"/>.
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
  /// The version of the rework, used to determine whether a player is up-to-date or not or cache values for a rework state.
  /// </summary>
  [JsonProperty("algorithm_version")]
  public int PPVersion { get; private set; }

  /// <summary>
  /// Bool whether this rework is only accessible with Onion-level authorization.
  /// </summary>
  [JsonProperty("for_onions")]
  public bool IsOnionLevel { get; private set; }

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
  /// The URL to the rework on the Huismetbenen website.
  /// </summary>
  public string Url => $"https://pp.huismetbenen.nl/rankings/info/{Code}";

  /// <summary>
  /// The GitHub URL to the commit of the rework.
  /// </summary>
  public string? CommitUrl
  {
    get
    {
      // If the GitHub URL or commit is missing (eg. on the historic 2016 rework where no source code is available), return null.
      if (GitHubUrl is null || Commit is null)
        return null;

      // Get the base URL of the GitHub repository and append the commit hash.
      return string.Join('/', GitHubUrl.Split('/').Take(5)) + $"/tree/{Commit}";
    }
  }

  /// <summary>
  /// A human readable string for the rework status.<br/>
  /// Example: 🔒 Private • ✅ Active
  /// </summary>
  public string ReworkTypeString
  {
    get => this switch
    {
      { IsLive: true } => "🔴 Live",
      { IsHistoric: true } => "📜 Historic",
      { IsConfirmed: true } => "✅ Confirmed for next deploy",
      { IsPublic: true, IsActive: true } => "🌐 Public • ✅ Active",
      { IsPublic: true, IsActive: false } => "🌐 Public • 💀 Inactive",
      { IsPublic: false, IsActive: true } => "🔒 Onion-only • ✅ Active",
      { IsPublic: false, IsActive: false } => "🔒 Onion-only • 💀 Inactive",
      _ => ReworkType ?? "null"
    };
  }

  /// <summary>
  /// A human readable name for the ruleset targetted by the rework.
  /// </summary>
  public string RulesetName
  {
    get => RulesetId switch
    {
      0 => "osu!",
      1 => "osu!taiko",
      2 => "osu!catch",
      3 => "osu!mania",
      _ => "Unknown"
    };
  }

  public override string ToString()
  {
    return $"{Id} {Name} ({Code})";
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }

  public override bool Equals(object? obj)
  {
    return obj is HuisRework rework && Id == rework.Id;
  }
}
