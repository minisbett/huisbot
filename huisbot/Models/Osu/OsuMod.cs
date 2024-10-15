using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents an osu!lazer API Mod from the osu! API or Huismetbenen's score simulation.
/// </summary>
public class OsuMod
{

  /// <summary>
  /// The mod acronym. (eg. "DT")
  /// </summary>
  [JsonProperty("acronym")]
  public string Acronym { get; private set; } = "";
}