using huisbot.Models.Huis;
using huisbot.Models.Osu;

namespace huisbot.Helpers;

/// <summary>
/// Modifies <see cref="HuisCalculationRequest"/> according to a specified <see cref="Modifier"/>.
/// </summary>
public static class ScoreModifier
{
  /// <summary>
  /// Modifies <see cref="HuisCalculationRequest"/> according to the specified <see cref="Modifier"/>.
  /// </summary>
  public static void Modify(HuisCalculationRequest request, OsuBeatmap beatmap, Modifier modifier)
  {
    if (modifier >= Modifier.FC)
    {
      request.Misses = 0;
      request.LargeTickMisses = request.LargeTickMisses.HasValue ? 0 : null;
      request.Combo = beatmap.MaxCombo;
    }
    
    if (modifier >= Modifier.PFC)
      request.SliderTailMisses = request.SliderTailMisses.HasValue ? 0 : null;
    
    if (modifier is Modifier.SS)
    {
      request.Count100 = 0;
      request.Count50 = 0;
    }
  }
  
  /// <summary>
  /// The way in which the score will be modified before performing calculation.
  /// </summary>
  public enum Modifier
  {
    /// <summary>
    /// The score will be turned into a full combo.
    /// </summary>
    FC,

    /// <summary>
    /// The score will be turned into a perfect full combo.
    /// </summary>
    PFC,

    /// <summary>
    /// The score will be turned into an SS.
    /// </summary>
    SS,
  }
}