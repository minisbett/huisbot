using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents an alias to be used instead of an ID in order to provide easier access.
/// </summary>
public class IDAlias
{
  /// <summary>
  /// The unique alias.
  /// </summary>
  [Key]
  public string Alias { get; set; }

  /// <summary>
  /// The ID the alias represents.
  /// </summary>
  public long Id { get; set; }

  /// <summary>
  /// The display name of the object associated with the ID, used for display purposes.
  /// </summary>
  public string DisplayName { get; set; }

  /// <summary>
  /// Creates a new <see cref="IDAlias"/> object with the specified ID, alias for it and display name.
  /// </summary>
  /// <param name="alias">The alias.</param>
  /// <param name="id">The ID the alias represents.</param>
  /// <param name="displayName">The display name of the beatmap.</param>
  public IDAlias(string alias, long id, string displayName)
  {
    Alias = alias;
    Id = id;
    DisplayName = displayName;
  }
}
