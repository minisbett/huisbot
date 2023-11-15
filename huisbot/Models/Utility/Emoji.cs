using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents a Discord emoji with a name and ID.
/// </summary>
public class Emoji
{
  /// <summary>
  /// The name of the emoji.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// The snowflake ID of the emoji.
  /// </summary>
  public ulong Id { get; }

  /// <summary>
  /// Returns the asset url of this emoji.
  /// </summary>
  public string Url => $"https://cdn.discordapp.com/emojis/{Id}.webp";

  /// <summary>
  /// Returns the emoji string representation of this emoji.
  /// </summary>
  public override string ToString() => $"<:{Name}:{Id}>";

  /// <summary>
  /// Creates a new <see cref="Emoji"/> object with the name and ID of the custom emoji.
  /// </summary>
  /// <param name="name">The name of the emoji.</param>
  /// <param name="id">The ID of the emoji.</param>
  public Emoji(string name, ulong id)
  {
    Name = name;
    Id = id;
  }
}
