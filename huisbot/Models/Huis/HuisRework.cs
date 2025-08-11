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
  /// The category of the rework. (LIVE, HISTORIC, CONFIRMED, ABANDONED, WIP, PROPOSED)
  /// </summary>
  [JsonProperty("category")]
  private string Category { get; set; } = "";

  /// <summary>
  /// The visibility of the rework. (PUBLIC, ONION)
  /// </summary>
  [JsonProperty("visibility")]
  private string Visibility { get; set; } = "";

  /// <summary>
  /// The URL to the rework on GitHub. This may or may not be targetting the correct branch directly.
  /// </summary>
  [JsonProperty("url")]
  private string? GitHubUrl { get; set; }

  /// <summary>
  /// The ID of the current commit. The corresponding GitHub repository can be found in <see cref="GitHubUrl"/>.
  /// </summary>
  [JsonProperty("commit")]
  private string? Commit { get; set; }

  /// <summary>
  /// The ID of the ruleset/gamemode the rework is for.
  /// </summary>
  [JsonProperty("gamemode")]
  public int RulesetId { get; private set; }

  /// <summary>
  /// The description of the rework, as displayed in the banner text on the website.
  /// </summary>
  [JsonProperty("banner_text")]
  public string Description { get; private set; } = "";

  /// <summary>
  /// The version of the rework, used to determine whether a player is up-to-date or not or cache values for a rework state.
  /// </summary>
  [JsonProperty("algorithm_version")]
  public int PPVersion { get; private set; }

  /// <summary>
  /// Bool whether the rework is the live pp system.
  /// </summary>
  public bool IsLive => Category == "LIVE";

  /// <summary>
  /// Bool whether the rework is public and accessible to everyone.
  /// </summary>
  public bool IsPublic => Visibility == "PUBLIC";

  /// <summary>
  /// Bool whether the rework is only accessible for onion-level authorization.
  /// </summary>
  public bool IsOnionOnly => Visibility == "ONION";

  /// <summary>
  /// Bool whether the rework is considered abandoned.
  /// </summary>
  public bool IsAbandoned => Category == "ABANDONED";

  /// <summary>
  /// Bool whether the rework is proposed for the next deploy.
  /// </summary>
  public bool IsProposed => Category == "PROPOSED";

  /// <summary>
  /// Bool whether the rework is work in progress.
  /// </summary>
  public bool IsWIP => Category == "WIP";

  /// <summary>
  /// Bool whether the rework is confirmed for the next deploy.
  /// </summary>
  public bool IsConfirmed => Category == "CONFIRMED";

  /// <summary>
  /// Bool whether the rework is considered historic.
  /// </summary>
  public bool IsHistoric => Category == "HISTORIC";

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
  /// Example: 🔒 Onion-only • 💀 Abandoned
  /// </summary>
  public string ReworkTypeString
  {
    get
    {
      if (IsLive)
        return "🔴 Live";
      else if (IsHistoric)
        return "📜 Historic";
      else if (IsConfirmed)
        return "✅ Confirmed";
      else if (IsProposed)
        return "💍 Proposed";

      string str = IsPublic ? "🌐 Public" : IsOnionOnly ? "🔒 Onion-only" : Visibility;
      str += " • ";
      str += IsAbandoned ? "💀 Abandoned" : IsWIP ? "⌛ WIP" : Category;
      return str;
    }
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
