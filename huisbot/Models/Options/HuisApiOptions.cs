namespace huisbot.Models.Options;

/// <summary>
/// Represents options for the Huismetbenen API (<see cref="Services.HuisApiService"/>).
/// </summary>
public class HuisApiOptions
{
  /// <summary>
  /// The Onion key for acceessing Onion-level reworks via the Huis API.
  /// </summary>
  public string OnionKey { get; set; } = "";
}
