using huisbot.Models.Huis;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
  /// The JSON string resembling the simulation response.
  /// </summary>
  public string ResponseJson { get; private set; } = "";

  /// <summary>
  /// The deserialized <see cref="ResponseJson"/>.
  /// </summary>
  [NotMapped]
  public HuisSimulationResponse Response => JsonConvert.DeserializeObject<HuisSimulationResponse>(ResponseJson)!;

  /// <summary>
  /// Creates a new <see cref="CachedScoreSimulation"/> instance with the specified request identifier and simulation data.
  /// </summary>
  public CachedScoreSimulation(string requestIdentifier, string responseJson)
  {
    RequestIdentifier = requestIdentifier;
    ResponseJson = responseJson;
  }

  /// <summary>
  /// Creates a new <see cref="CachedScoreSimulation"/> instance with the specified simulation request and response.
  /// </summary>
  /// <param name="request">The simulation request.</param>
  /// <param name="response">The simulation response.</param>
  public CachedScoreSimulation(HuisSimulationRequest request, HuisSimulationResponse response)
  {
    // The rework ID is included in case another rework with the same code exists sometime in the future.
    // The algorithm version is included in order to "forget"/ignore scores of an older state of the rework.
    RequestIdentifier = GetRequestIdentifier(request);
    ResponseJson = JsonConvert.SerializeObject(response);
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
