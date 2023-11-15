using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Utils;

/// <summary>
/// Provides a method to inspect an object and its properties and display them in a human-readable format.
/// Source: https://github.com/discord-net/Discord.Net/blob/0f37677c59d84061b1df2c740f2494c1bbb29a30/samples/idn/Inspector.cs
///         (with minor modifications)
/// </summary>
public static class Inspector
{
  /// <summary>
  /// Inspects the specified object and returns a string representation of it.
  /// </summary>
  /// <param name="value">The object to inspect.</param>
  /// <returns>A string representation of the specified object.</returns>
  public static string Inspect(object value)
  {
    var builder = new StringBuilder();

    if (value != null)
    {
      // Get the type of the object and append it to the string builder.
      builder.AppendLine($"[{value.GetType().Namespace}.{value.GetType().Name}]");

      // Inspect the properties of the object and append them to the string builder.
      builder.AppendLine($"{InspectProperty(value)}");

      // If the object is an enumerable and not blacklisted, append each item in it's inspected version to the string builder.
      // Certain types are blacklisted to prevent useless listing of items, for example when inspecting strings which are IEnumerable<char>.
      if (value is IEnumerable enumerable)
      {
        if (value is not string)
        {
          // Cast the enumerable to an array to prevent multiple enumerations.
          var items = enumerable.Cast<object>().ToArray();

          // If the array is not empty, append each item in it's inspected version to the string builder.
          if (items.Length > 0)
          {
            builder.AppendLine();
            foreach (var item in items)
              builder.AppendLine($"- {InspectProperty(item)}");
          }
        }
      }
      // If the object is not an enumerable, inspect each property of the object.
      else
      {
        // Get all non-inherited properties of the object.
        PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetIndexParameters().Length == 0).ToArray();

        // If properties are present, append them to the string builder.
        if (properties.Length > 0)
        {
          builder.AppendLine();
          foreach (PropertyInfo property in properties)
            builder.AppendLine($"{property.Name.PadRight(properties.Max(x => x.Name.Length), ' ')} {InspectProperty(property.GetValue(value)!)}");
        }
      }
    }
    // If the value is null, return "null".
    else
      builder.AppendLine("null");

    // Return the built string.
    return builder.ToString();
  }

  private static string InspectProperty(object obj)
  {
    // If the specified propety is null, return "null".
    if (obj == null)
      return "null";

    var type = obj.GetType();

    // Check whether a DebuggerDisplay property is present on the type and if so, use it for a string representation.
    PropertyInfo? debuggerDisplay = type.GetProperty("DebuggerDisplay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (debuggerDisplay != null)
      return debuggerDisplay.GetValue(obj)!.ToString()!;

    // Otherwise, check whether a ToString method not inherited from the object type is present on the type and if so, use it for
    // a string representation. It would mean that the type overrides the ToString method and thus has a custom string representation.
    MethodInfo? toString = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Where(x => x.Name == "ToString" && x.DeclaringType != typeof(object)).FirstOrDefault();
    if (toString != null)
      return obj.ToString()!;

    // Otherwise, check whether a Count property is present and if so, return the amount of items in the collection.
    var count = type.GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    if (count != null)
      return $"[{count.GetValue(obj)} Items]";

    // If none of the above is present, return the default string representation of the object.
    return obj.ToString()!;
  }
}
