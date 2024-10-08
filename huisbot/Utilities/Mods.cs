using Newtonsoft.Json;

namespace huisbot.Utilities;

/// <summary>
/// Represents a mod combination, allowing proper parsing.
/// </summary>
public class Mods
{
  private Mods() { }

  /// <summary>
  /// An array of valid mods in their community-agreed order.
  /// </summary>
  private static readonly string[] VALID_MODS = "RXEZFLHRHDHTDTNCNFCL".Chunk(2).Select(x => new string(x)).ToArray();

  /// <summary>
  /// The mods of this <see cref="Mods"/> instance.
  /// </summary>
  private string[] _mods = [];

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

/// <summary>
/// A <see cref="JsonConverter"/> for converting between <see cref="string"/> and <see cref="Mods"/>.
/// </summary>
public class ModsConverter : JsonConverter
{
  public override bool CanConvert(Type objectType)
  {
    return objectType.Equals(typeof(Mods));
  }

  public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
  {
    // If the value is null, default to an empty mods object.
    if (reader.TokenType == JsonToken.Null)
      return Mods.Parse("");

    // Make sure the token type is a string and the value not null.
    if (reader.TokenType != JsonToken.String || reader.Value is not string str)
      throw new JsonSerializationException($"Unable to convert '{reader.Value}' ({reader.TokenType}) into a Mods object.");

    // Parse the mods and return them.
    return Mods.Parse(str);
  }

  public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
  {
    // Make sure the value is not null.
    if (value is not Mods mods)
      throw new JsonSerializationException($"Unable to convert '{value}' to a string.");

    // Write the mods string to the json writer.
    writer.WriteValue(mods.ToString());
  }
}
