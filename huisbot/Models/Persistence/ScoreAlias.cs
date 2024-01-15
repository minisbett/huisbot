using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Persistence;

/// <summary>
/// Represents an alias to be used instead of a score ID in order to provide easier access.
/// </summary>
public class ScoreAlias
{
  /// <summary>
  /// The unique alias.
  /// </summary>
  [Key]
  public string Alias { get; private set; }

  /// <summary>
  /// The score ID the alias represents.
  /// </summary>
  public long ScoreId { get; private set; }

  /// <summary>
  /// The display name of the object associated with the score ID, used for display purposes.
  /// </summary>
  public string DisplayName { get; private set; }

  /// <summary>
  /// Creates a new <see cref="ScoreAlias"/> object with the specified score ID, alias for it and display name.
  /// </summary>
  /// <param name="alias">The alias.</param>
  /// <param name="scoreId">The score ID the alias represents.</param>
  /// <param name="displayName">The display name of the score.</param>
  public ScoreAlias(string alias, long scoreId, string displayName)
  {
    Alias = alias;
    ScoreId = scoreId;
    DisplayName = displayName;
  }
}
