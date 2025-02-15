using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Persistence;

/// <summary>
/// Represents an alias to be used instead of a beatmap ID in order to provide easier access.
/// </summary>
/// <remarks>
/// Creates a new <see cref="BeatmapAlias"/> object with the specified beatmap ID, alias for it and display name.
/// </remarks>
/// <param name="alias">The alias.</param>
/// <param name="beatmapId">The beatmap ID the alias represents.</param>
/// <param name="displayName">The display name of the beatmap.</param>
public class BeatmapAlias(string alias, int beatmapId, string displayName)
{
  /// <summary>
  /// The unique alias.
  /// </summary>
  [Key]
  public string Alias { get; private set; } = alias;

  /// <summary>
  /// The beatmap ID the alias represents.
  /// </summary>
  public int BeatmapId { get; private set; } = beatmapId;

  /// <summary>
  /// The display name of the object associated with the beatmap ID, used for display purposes.
  /// </summary>
  public string DisplayName { get; private set; } = displayName;
}
