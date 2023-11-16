using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Huis;

/// <summary>
/// Represents a statistic from the Data tab on Huismetbenen with it's old and new values.
/// </summary>
public class HuisStatistic
{
  /// <summary>
  /// The new values.
  /// </summary>
  [JsonProperty("new")]
  public double[]? New { get; private set; }

  /// <summary>
  /// The old values.
  /// </summary>
  [JsonProperty("old")]
  public double[]? Old { get; private set; }

  /// <summary>
  /// The differences between the old and new values.
  /// </summary>
  [JsonProperty("diff")]
  public double[]? Difference { get; private set; }

  /// <summary>
  /// The average change between the old and new values.
  /// </summary>
  [JsonProperty("avgChange")]
  public double AverageChange { get; private set; }
}
