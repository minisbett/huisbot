using Newtonsoft.Json;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a score calculation response received from Huismetbenen.
/// </summary>
public class HuisCalculationResponse
{
  /// <summary>
  /// The difficulty attributes of the calculated score.
  /// </summary>
  [JsonProperty("difficulty_attributes")]
  public HuisDifficultyAttributes DifficultyAttributes { get; private set; } = null!;

  /// <summary>
  /// The performance attributes of the calculated score.
  /// </summary>
  [JsonProperty("performance_attributes")]
  public HuisPerformanceAttributes PerformanceAttributes { get; private set; } = null!;

  /// <summary>
  /// The calculated score.
  /// </summary>
  [JsonProperty("score")]
  public HuisCalculationScore Score { get; private set; } = null!;
}