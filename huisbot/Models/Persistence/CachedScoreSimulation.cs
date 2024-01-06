using huisbot.Models.Huis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Models.Persistence;

/// <summary>
/// Represents a cached score simulation from Huismetbenen in the database.
/// </summary>
public class CachedScoreSimulation
{
  /// <summary>
  /// A unique JSON identifier resembling the simulation request.
  /// </summary>
  [Key]
  public string RequestIdentifier { get; private set; } = "";

  /// <summary>
  /// The JSON string resembling the simulated score.
  /// </summary>
  public string ScoreJson { get; private set; } = "";

  /// <summary>
  /// The deserialized <see cref="ScoreJson"/>.
  /// </summary>
  [NotMapped]
  public HuisSimulatedScore Score => JsonConvert.DeserializeObject<HuisSimulatedScore>(ScoreJson)!;

  /// <summary>
  /// Creates a new <see cref="CachedScoreSimulation"/> instance with the specified request identifier and score data.
  /// </summary>
  public CachedScoreSimulation(string requestIdentifier, string scoreJson)
  {
    RequestIdentifier = requestIdentifier;
    ScoreJson = scoreJson;
  }

  /// <summary>
  /// Creates a new <see cref="CachedScoreSimulation"/> instance with the specified simulation request and response.
  /// </summary>
  /// <param name="request">The simulation request.</param>
  /// <param name="score">The simulated score.</param>
  /// <returns>The cached score simulation.</returns>
  public CachedScoreSimulation(HuisSimulationRequest request, HuisSimulatedScore score)
  {
    // The rework ID is included in case another rework with the same code exists sometime in the future.
    // The algorithm version is included in order to "forget"/ignore scores of an older state of the rework.
    RequestIdentifier = GetRequestIdentifier(request);
    ScoreJson = JsonConvert.SerializeObject(score);
  }

  /// <summary>
  /// Returns the unique identifier for a score simulation request.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <returns>The unique identifier for the score simulation request.</returns>
  public static string GetRequestIdentifier(HuisSimulationRequest request)
  {
    return $"rework {request.Rework.Id}, algorithm version {request.Rework.PPVersion}\n{request.ToJson()}";
  }
}
