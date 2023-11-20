using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Enums;

/// <summary>
/// A "fake enum" representing an osu! mod.
/// </summary>
/// </summary>
public class OsuMod
{
  private static OsuMod[] _all = null!;

  /// <summary>
  /// Returns all the available osu! mods.
  /// </summary>
  public static OsuMod[] All
  {
    get
    {
      // Lazy load the array since the property values are not known at compile time.
      if (_all is null)
        _all = new OsuMod[]
        {
          NoFail, Easy, TouchDevice, Hidden, HardRock, SuddenDeath, DoubleTime, Relax, HalfTime, Nightcore, Flashlight, Auto, SpunOut, AutoPilot
        };

      return _all;
    }
  }

  /// <summary>
  /// The NoFail osu! mod.
  /// </summary>
  public static OsuMod NoFail { get; } = new("NF");

  /// <summary>
  /// The Easy osu! mod.
  /// </summary>
  public static OsuMod Easy { get; } = new("EZ");

  /// <summary>
  /// The TouchDevice osu! mod.
  /// </summary>
  public static OsuMod TouchDevice { get; } = new("TD");

  /// <summary>
  /// The Hidden osu! mod.
  /// </summary>
  public static OsuMod Hidden { get; } = new("HD");

  /// <summary>
  /// The HardRock osu! mod.
  /// </summary>
  public static OsuMod HardRock { get; } = new("HR");

  /// <summary>
  /// The SuddenDeath osu! mod.
  /// </summary>
  public static OsuMod SuddenDeath { get; } = new("SD");

  /// <summary>
  /// The DoubleTime osu! mod.
  /// </summary>
  public static OsuMod DoubleTime { get; } = new("DT");

  /// <summary>
  /// The Relax osu! mod.
  /// </summary>
  public static OsuMod Relax { get; } = new("RX");

  /// <summary>
  /// The HalfTime osu! mod.
  /// </summary>
  public static OsuMod HalfTime { get; } = new("HT");

  /// <summary>
  /// The Nightcore osu! mod.
  /// </summary>
  public static OsuMod Nightcore { get; } = new("NC");

  /// <summary>
  /// The Flashlight osu! mod.
  /// </summary>
  public static OsuMod Flashlight { get; } = new("FL");

  /// <summary>
  /// The Auto osu! mod.
  /// </summary>
  public static OsuMod Auto { get; } = new("AT");

  /// <summary>
  /// The SpunOut osu! mod.
  /// </summary>
  public static OsuMod SpunOut { get; } = new("SO");

  /// <summary>
  /// The AutoPilot osu! mod.
  /// </summary>
  public static OsuMod AutoPilot { get; } = new("AP");

  [JsonProperty("acronym")]
  /// <summary>
  /// The acronym of the osu! mod.
  /// </summary>
  public string Acronym { get; }

  private OsuMod(string acronym)
  {
    Acronym = acronym;
  }

  public override string ToString()
  {
    return Acronym;
  }

  /// <summary>
  /// Returns the OsuMod array for the specified mod string. All invalid mods are being ignored.
  /// </summary>
  /// <param name="modsStr">The mod string.</param>
  /// <returns>The parsed mods.</returns>
  public static OsuMod[] Parse(string modsStr)
  {
    // Allow some variation in the way mods are specified.
    modsStr = modsStr.ToUpper().TrimStart('+').Replace(" ", "");

    // Try to parse the mods by chunking the string into 2-character strings and then parsing them.
    List<OsuMod> mods = new List<OsuMod>();
    foreach (string acronym in modsStr.Chunk(2).Select(x => new string(x)))
      if (All.Any(x => x.Acronym == acronym))
        mods.Add(All.First(x => x.Acronym == acronym));

    return mods.ToArray();
  }
}
