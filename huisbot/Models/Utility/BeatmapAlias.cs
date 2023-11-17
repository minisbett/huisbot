using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents a beatmap alias to be used instead of a beatmap id in order to provide easier access.
/// </summary>
public class BeatmapAlias
{
  /// <summary>
  /// The beatmap ID of the alias.
  /// </summary>
  public int Id { get; set; }

  /// <summary>
  /// The unique alias.
  /// </summary>
  [Key]
  public string Alias { get; set; }

  /// <summary>
  /// Creates a new <see cref="BeatmapAlias"/> object with the specified beatmap ID and alias for it.
  /// </summary>
  /// <param name="id">Th beatmap ID of the alias.</param>
  /// <param name="alias">The alias.</param>
  public BeatmapAlias(int id, string alias)
  {
    Id = id;
    Alias = alias;
  }
}
