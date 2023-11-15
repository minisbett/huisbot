using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a rework from the Huis API.
/// </summary>
public class Rework
{
  /// <summary>
  /// The internal id of the rework. 1 is always the current live changes.
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
}
