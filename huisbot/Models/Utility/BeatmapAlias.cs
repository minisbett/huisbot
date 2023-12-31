using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents an alias to be used instead of a beatmap ID in order to provide easier access.
/// </summary>
public class BeatmapAlias
{
  /// <summary>
  /// The unique alias.
  /// </summary>
  [Key]
  public string Alias { get; set; }

  /// <summary>
  /// The beatmap ID the alias represents.
  /// </summary>
  public long BeatmapId { get; set; }

  /// <summary>
  /// The display name of the object associated with the beatmap ID, used for display purposes.
  /// </summary>
  public string DisplayName { get; set; }

  /// <summary>
  /// Creates a new <see cref="BeatmapAlias"/> object with the specified beatmap ID, alias for it and display name.
  /// </summary>
  /// <param name="alias">The alias.</param>
  /// <param name="beatmapId">The beatmap ID the alias represents.</param>
  /// <param name="displayName">The display name of the beatmap.</param>
  public BeatmapAlias(string alias, long beatmapId, string displayName)
  {
    Alias = alias;
    BeatmapId = beatmapId;
    DisplayName = displayName;
  }
}
