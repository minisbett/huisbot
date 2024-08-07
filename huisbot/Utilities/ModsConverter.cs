﻿using Newtonsoft.Json;

namespace huisbot.Utilities;

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
