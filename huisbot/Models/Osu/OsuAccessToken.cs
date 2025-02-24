using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents an osu! API access token response from the API.
/// </summary>
internal class OsuAccessToken
{
  /// <summary>
  /// The access token.
  /// </summary>
  [JsonProperty("access_token")]
  public string? Token { get; private set; }

  /// <summary>
  /// The amount of seconds in which the token expires.
  /// </summary>
  [JsonProperty("expires_in")]
  public int? ExpiresIn { get; private set; }

  /// <summary>
  /// The description of the authorization error. If null, no error occurred.
  /// </summary>
  [JsonProperty("error_description")]
  public string? ErrorDescription { get; private set; }
}
