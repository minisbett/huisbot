using MathNet.Numerics.Financial;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace huisbot.Models.Osu;

/// <summary>
/// Represents an osu!lazer API Mod from the osu! API (<see cref="OsuScore"/>) or Huismetbenen score calculation (<see cref="Huis.HuisCalculationResponse"/>).
/// </summary>
public class OsuMod(string acronym)
{
  /// <summary>
  /// The mod acronym. (eg. "DT")
  /// </summary>
  [JsonProperty("acronym")]
  public string Acronym { get; private set; } = acronym;

  /// <summary>
  /// The mod settings.
  /// </summary>
  [JsonProperty("settings")]
  public Dictionary<string, object> Settings { get; private set; } = [];

  /// <summary>
  /// Returns the value by the specified settings key, or null if the setting does not exist.
  /// </summary>
  /// <param name="key">The key name.</param>
  /// <returns>The setting value or null.</returns>
  public object? GetSetting(string key) => Settings.TryGetValue(key, out object? value) ? value : null;
}