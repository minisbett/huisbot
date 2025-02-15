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

/// <summary>
/// Represents a collection of <see cref="OsuMod"/> and provides utility methods and properties.
/// </summary>
public class OsuMods : List<OsuMod>
{
  /// <summary>
  /// The clock rate of the mods.
  /// </summary>
  public double ClockRate
  {
    get
    {
      if (GetMod("DT", "NC") is OsuMod dtMod)
        return (double)(dtMod.GetSetting("speed_change") ?? 1.5d);

      if (GetMod("HT", "DC") is OsuMod htMod)
        return (double)(htMod.GetSetting("speed_change") ?? 0.75d);

      return 1.0d;
    }
  }

  /// <summary>
  /// Boolean indicating whether the mods contain the Easy mod.
  /// </summary>
  public bool IsEasy => GetMod("EZ") is not null;

  /// <summary>
  /// Bool indicating whether the mods contain the HardRock mod.
  /// </summary>
  public bool IsHardRock => GetMod("HR") is not null;

  /// <summary>
  /// The string representation of the mods, with a " +" in front.
  /// </summary>
  public string PlusString => this.Any() ? $" +{this}" : "";

  /// <summary>
  /// Sets the clockrate of the mods and ensures the correct mod ("DT"/"HT") is enabled.
  /// </summary>
  /// <param name="clockRate">The clock rate.</param>
  public void SetClockRate(double clockRate)
  {
    clockRate = Math.Round(clockRate, 2);

    if (clockRate == 1.0d && GetMod("DT, NC", "HT", "DC") is OsuMod mod)
      Remove(mod);
    else if (clockRate >= 1.01)
    {
      if (GetMod("DT", "NC") is OsuMod dtMod)
        dtMod.Settings["speed_change"] = clockRate;
      else
        Add(new OsuMod("DT") { Settings = { { "speed_change", clockRate } } });
    }
    else if (clockRate <= 0.99)
    {
      if (GetMod("HT", "DC") is OsuMod htMod)
        htMod.Settings["speed_change"] = clockRate;
      else
        Add(new OsuMod("HT") { Settings = { { "speed_change", clockRate } } });
    }
  }

  /// <summary>
  /// Returns the mod by the specified acronyms, or null if the mod does not exist.
  /// </summary>
  /// <param name="acronyms">The acronyms.</param>
  /// <returns>The mod or null if the mod does not exist.</returns>
  private OsuMod? GetMod(params string[] acronyms) => this.FirstOrDefault(x => acronyms.Contains(x.Acronym));

  public override string ToString()
  {
    string modsStr = "";

    foreach (OsuMod mod in this)
    {
      modsStr += mod.Acronym;

      // If a custom clock rate is set for DT/NC (usually 1.5) or HT/DC (usually 0.75), append it to the mod string.
      if (mod.Acronym is "DT" or "NC" or "HT" or "DC" && mod.GetSetting("speed_change") is double speedChange && speedChange is not 1.5 or 0.75)
        modsStr += $"({speedChange}x)";
    }

    return modsStr;
  }

  /// <summary>
  /// Parses a <see cref="OsuMods"/> object from the specified mods string. (eg. "HDDT" or "HDDT(1.3x)")
  /// </summary>
  /// <param name="modsStr">The mods string.</param>
  /// <returns>The parsed OsuMods object.</returns>
  public static OsuMods FromString(string modsStr)
  {
    OsuMods mods = [];

    // Go through the mod string with two characters at once, and optionally the mod settings in () after the acronym.
    foreach (Match match in Regex.Matches(modsStr, @"([A-Z]{2})(?:\((.*?)\))?"))
    {
      OsuMod mod = new(match.Groups[1].Value);
      string extra = match.Groups[2].Value;

      // If the DT/NC or HT/DC mod contains extra information, there is a custom clock rate.
      if (mod.Acronym is "DT" or "NC" or "HT" or "DC" && extra != "")
        mod.Settings["speed_change"] = double.Parse(extra.TrimEnd('x'));

      mods.Add(mod);
    }

    return mods;
  }
}