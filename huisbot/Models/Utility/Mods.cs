using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents a mod combination, allowing proper parsing.
/// </summary>
public class Mods
{
  private Mods() { }

  /// <summary>
  /// An array of valid mods in their community-agreed order.
  /// </summary>
  private static readonly string[] VALID_MODS = "RXEZFLHRHDHTDTNCNF".Chunk(2).Select(x => new string(x)).ToArray();

  /// <summary>
  /// The mods of this <see cref="Mods"/> instance.
  /// </summary>
  private string[] _mods = new string[0];

  /// <summary>
  /// An array of the mods in this instance.
  /// </summary>
  public string[] Array => (string[])_mods.Clone();

  /// <summary>
  /// The clock rate resulting from the mod combination.
  /// </summary>
  public double ClockRate => IsHalfTime ? 3 / 4d : IsDoubleTime ? 3 / 2d : 1;

  /// <summary>
  /// Bool whether the mod combination does not include any mods.
  /// </summary>
  public bool IsNoMod => _mods.Length == 0;

  /// <summary>
  /// Bool whether the mod combination includes halftime.
  /// </summary>
  public bool IsHalfTime => _mods.Contains("HT");

  /// <summary>
  /// Bool whether the mod combination includes doubletime/nightcore.
  /// </summary>
  public bool IsDoubleTime => _mods.Contains("DT") || _mods.Contains("NC");

  /// <summary>
  /// Bool whether the mod combination includes hardrock.
  /// </summary>
  public bool IsHardRock => _mods.Contains("HR");

  /// <summary>
  /// Bool whether the mod combination includes easy.
  /// </summary>
  public bool IsEasy => _mods.Contains("EZ");

  /// <summary>
  /// The mods as a string prefixed with " +". If nomod, this string is empty instead.<br/>
  /// This string is meant to be used to use the mods in a UI text.
  /// </summary>
  public string PlusString => IsNoMod ? "" : $" +{ToString()}";

  public override string ToString()
  {
    return string.Join("", _mods);
  }

  /// <summary>
  /// Parses the mods from the specified mod string.
  /// </summary>
  /// <param name="modsStr">The mod string.</param>
  /// <returns>The parsed mods.</returns>
  public static Mods Parse(string modsStr)
  {
    // Split the mods string into chunks, ensuring consistent capitalization and return the parsed mods array.
    return Parse(modsStr.ToUpper().Chunk(2).Select(x => new string(x)).ToArray());
  }

  /// <summary>
  /// Parses the mods from the specified mod array.
  /// </summary>
  /// <param name="mods">The mod array.</param>
  /// <returns>The parsed mods.</returns>
  public static Mods Parse(string[] mods)
  {
    // Return the parsed mods.
    return new Mods()
    {
      // Go through all valid mods, check for their existence and compose them into an array.
      _mods = VALID_MODS.Where(x => mods.Contains(x)).ToArray()
    };
  }
}
