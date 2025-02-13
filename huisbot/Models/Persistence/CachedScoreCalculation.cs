using huisbot.Models.Huis;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace huisbot.Models.Persistence;

/// <summary>
/// Represents a cached score calculation from Huismetbenen in the database.
/// </summary>
public class CachedScoreCalculation
{
  /// <summary>
  /// A unique JSON identifier resembling the calculation request.
  /// </summary>
  [Key]
  public string RequestIdentifier { get; private set; } = "";

  /// <summary>
  /// The JSON string resembling the calculation response.
  /// </summary>
  public string ResponseJson { get; private set; } = "";

  /// <summary>
  /// The deserialized <see cref="ResponseJson"/>.
  /// </summary>
  [NotMapped]
  public HuisCalculationResponse Response => JsonConvert.DeserializeObject<HuisCalculationResponse>(ResponseJson)!;

  /// <summary>
  /// Creates a new <see cref="CachedScoreCalculation"/> instance with the specified request identifier and calculation data.
  /// </summary>
  public CachedScoreCalculation(string requestIdentifier, string responseJson)
  {
    RequestIdentifier = requestIdentifier;
    ResponseJson = responseJson;
  }

  /// <summary>
  /// Creates a new <see cref="CachedScoreCalculation"/> instance with the specified calculation request and response.
  /// </summary>
  /// <param name="request">The calculation request.</param>
  /// <param name="response">The calculation response.</param>
  public CachedScoreCalculation(HuisCalculationRequest request, HuisCalculationResponse response)
  {
    // The rework ID is included in case another rework with the same code exists sometime in the future.
    // The algorithm version is included in order to "forget"/ignore scores of an older state of the rework.
    RequestIdentifier = GetRequestIdentifier(request);
    ResponseJson = JsonConvert.SerializeObject(response);
  }

  /// <summary>
  /// Returns the unique identifier for a score calculation request.
  /// </summary>
  /// <param name="request">The score calculation request.</param>
  /// <returns>The unique identifier for the score calculation request.</returns>
  public static string GetRequestIdentifier(HuisCalculationRequest request)
  {
    return $"rework {request.Rework.Id}, algorithm version {request.Rework.PPVersion}\n{request.ToJson()}";
  }
}
