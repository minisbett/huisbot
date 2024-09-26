using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a user from the osu! API v1.
/// </summary>
public class OsuUser
{
  /// <summary>
  /// The ID of the user.
  /// </summary>
  [JsonProperty("user_id")]
  public int Id { get; private set; }

  /// <summary>
  /// The name of the user.
  /// </summary>
  [JsonProperty("username")]
  public string? Name { get; private set; }
}
