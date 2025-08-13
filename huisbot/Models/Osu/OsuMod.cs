using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace huisbot.Models.Osu;

/*
 * Notes about mod-handling for myself:
 * 
 * User Input
 * - User-entered mods are parsed via OsuMods.FromString
 * - The extra-string (in parentheses) is parsed into the correct APIMod Settings dictionary keys
 * - Explicit parameters (eg. on commands) are set via methods (eg. SetClockRate, SetCS), also set in the Settings dictionary
 * 
 * Huismetbenen
 * - Huis wants the mod settings as properties, so there's ClockRate, AdjustedCircleSize, ..., fetching from the Settings dictionary
 * - Those are used in the creation of a HuisCalculationRequest
 * - In the response, Huis returns the mods in the APIMod[] format, which is deserialized into an OsuMods object
 * 
 * Display
 * - The embeds can access a user-facing mods string via ToString() of the OsuMods object, constructing a string in the same
 *   way as what the user input mods-string would expect (eg. "DT(1.3x)", "DA(cs:5,ar:9,od:7)"), parsed from the *received* Settings dictionary
 * - Custom beatmap stats (CS, AR, OD) for display are accessed via methods on OsuBeatmap, but AdjustedCircleSize etc. overwrite the return values
 * 
 * 
 * user-entered + Set methods -> OsuMods -> provides properties for huis request -> huis response -> parsed into OsuMods via JSON
 * for human input, ToString() is the opposite of FromString()
 */


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
  public Dictionary<string, object> Settings
  {
    get => field;
    set
    {
      field = [];

      // Ensure that all number values are doubles for better handling of object-unboxing.
      foreach (KeyValuePair<string, object> setting in value)
        field[setting.Key] = setting.Value switch
        {
          int @int => (double)@int,
          long @long => (double)@long,
          _ => setting.Value
        };
    }
  } = [];

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
public partial class OsuMods : List<OsuMod>
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
  /// The adjusted circle size of the mods. This will be null if Difficulty Adjust does not modify the circle size.
  /// </summary>
  public double? AdjustedCircleSize => GetMod("DA") is OsuMod daMod && daMod.Settings.GetValueOrDefault("circle_size") is double cs ? cs : null;

  /// <summary>
  /// The adjusted approach rate of the mods. This will be null if Difficulty Adjust does not modify the approach rate.
  /// </summary>
  public double? AdjustedApproachRate => GetMod("DA") is OsuMod daMod && daMod.Settings.GetValueOrDefault("approach_rate") is double ar ? ar : null;

  /// <summary>
  /// The adjusted overall difficulty of the mods. This will be null if Difficulty Adjust does not modify the overall difficulty.
  /// </summary>
  public double? AdjustedOverallDifficulty => GetMod("DA") is OsuMod daMod && daMod.Settings.GetValueOrDefault("overall_difficulty") is double od ? od : null;

  /// <summary>
  /// Boolean indicating whether the mods contain the Classic mod.
  /// </summary>
  public bool IsClassic => GetMod("CL") is not null;

  /// <summary>
  /// Boolean indicating whether the mods contain the Easy mod.
  /// </summary>
  public bool IsEasy => GetMod("EZ") is not null;

  /// <summary>
  /// Bool indicating whether the mods contain the HardRock mod.
  /// </summary>
  public bool IsHardRock => GetMod("HR") is not null;

  /// <summary>
  /// Bool indicating whether the mods contain the Flashlight mod.
  /// </summary>
  public bool IsFlashlight => GetMod("FL") is not null;

  /// <summary>
  /// The string representation of the mods, with a " +" in front.
  /// </summary>
  public string PlusString => this.Any() ? $" +{this}" : "";

  /// <summary>
  /// Sets the clock rate of the mods and ensures the correct mod ("DT"/"HT") is enabled.
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
  /// Sets the circle size of the mods mods and ensures Difficulty Adjust mod is enabled.
  /// </summary>
  /// <param name="cs">The circle size.</param>
  public void SetCS(double cs)
  {
    cs = Math.Round(cs, 1);

    if (GetMod("DA") is OsuMod daMod)
      daMod.Settings["circle_size"] = cs;
    else
      Add(new OsuMod("DA") { Settings = { { "circle_size", cs } } });
  }

  /// <summary>
  /// Sets the approach rate of the mods mods and ensures Difficulty Adjust mod is enabled.
  /// </summary>
  /// <param name="ar">The approach rate.</param>
  public void SetAR(double ar)
  {
    ar = Math.Round(ar, 1);

    if (GetMod("DA") is OsuMod daMod)
      daMod.Settings["approach_rate"] = ar;
    else
      Add(new OsuMod("DA") { Settings = { { "approach_rate", ar } } });
  }

  /// <summary>
  /// Sets the overall difficulty of the mods mods and ensures Difficulty Adjust mod is enabled.
  /// </summary>
  /// <param name="od">The overall difficulty.</param>
  public void SetOD(double od)
  {
    od = Math.Round(od, 1);

    if (GetMod("DA") is OsuMod daMod)
      daMod.Settings["overall_difficulty"] = od;
    else
      Add(new OsuMod("DA") { Settings = { { "overall_difficulty", od } } });
  }

  /// <summary>
  /// Returns the mod matching one of the specified acronyms, or null if the mod does not exist.
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

      if (mod.Acronym is "DA" && mod.Settings.Count > 0)
        modsStr += $"({string.Join(",", mod.Settings.Select(kvp => $"{string.Join("", kvp.Key.Split('_').Select(x => x[0]))}:{kvp.Value}"))})";
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
    foreach (Match match in ModsRegex().Matches(modsStr))
    {
      OsuMod mod = new(match.Groups[1].Value);
      string extra = match.Groups[2].Value;

      // If the DT/NC or HT/DC mod contains extra information, there is a custom clock rate.
      if (mod.Acronym is "DT" or "NC" or "HT" or "DC" && extra != "")
        mod.Settings["speed_change"] = double.Parse(extra.TrimEnd('x'));

      // If the DA mod contains extra information, there are custom settings (cs, ar, od).
      if (mod.Acronym is "DA" && extra != "")
        foreach (string setting in extra.Split(','))
        {
          string[] split = setting.Split(':', 2);
          string key = split[0] switch
          {
            "cs" => "circle_size",
            "ar" => "approach_rate",
            "od" => "overall_difficulty",
            _ => setting.Split(':')[0]
          };

          if (double.TryParse(split[1], out double value))
            mod.Settings[key] = value;
        }

      mods.Add(mod);
    }

    return mods;
  }

  [GeneratedRegex(@"([A-Za-z]{2})(?:\((.*?)\))?")]
  private static partial Regex ModsRegex();
}
