using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  public string Token { get; private set; } = null!;

  /// <summary>
  /// The amount of seconds in which the token expires.
  /// </summary>
  [JsonProperty("expires_in")]
  public int ExpiresIn { get; private set; }
}
