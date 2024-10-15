using Newtonsoft.Json;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents an osu!lazer API Mod from the osu! API (<see cref="OsuScore"/>) or Huismetbenen score simulation (<see cref="Huis.HuisSimulationResponse"/>).
/// </summary>
public class OsuMod
{
  /// <summary>
  /// The mod acronym. (eg. "DT")
  /// </summary>
  [JsonProperty("acronym")]
  public string Acronym { get; private set; } = "";
}