using huisbot.Models.Huis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents a user from the osu! API v1.
/// </summary>
public class OsuUser
{
  /// <summary>
  /// Returns a user object representing no user was found.
  /// </summary>
  public static OsuUser NotFound => new OsuUser() { WasFound = false };

  /// <summary>
  /// Bool whether the user could be found or not. This property is used in <see cref="OsuApiService.GetUserAsync(string)"/>, which returns
  /// an object where this is false in order to report that the request was successful, but no user was found back to the caller.
  /// </summary>
  public bool WasFound { get; init; } = true;

  /// <summary>
  /// The ID of the user.
  /// </summary>
  [JsonProperty("user_id")]
  public int Id { get; private set; }

  /// <summary>
  /// The name of the user.
  /// </summary>
  [JsonProperty("username")]
  public string Name { get; private set; }
}
