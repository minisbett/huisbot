using huisbot.Models.Huis;
using Microsoft.Extensions.Logging;

namespace huisbot.Services;

/// <summary>
/// The caching service handles all kind of caching of values in both memory and database.
/// </summary>
public class CachingService(PersistenceService persistence, ILogger<CachingService> logger)
{
  /// <summary>
  /// The cache for the reworks. This is cached as reworks are frequently accessed, e.g. via autocompletes.
  /// </summary>
  private static HuisRework[] _reworksCache = null!;

  /// <summary>
  /// The expiry date of the reworks cache.
  /// </summary>
  private static DateTime _reworksCacheExpiryDate = DateTime.MinValue;

  /// <summary>
  /// Returns the cached reworks. If the cache expired, null is returned instead.
  /// </summary>
  /// <returns>The cached reworks or null, if the cache expired.</returns>
  public HuisRework[]? GetReworks()
  {
    return _reworksCacheExpiryDate > DateTime.UtcNow ? _reworksCache : null;
  }

  /// <summary>
  /// Sets the cached reworks and renews the cache expiration.
  /// </summary>
  /// <param name="reworks">The reworks.</param>
  public void SetReworks(HuisRework[] reworks)
  {
    _reworksCache = reworks;
    _reworksCacheExpiryDate = DateTime.UtcNow.AddMinutes(5);
    logger.LogInformation("Updated reworks cache, expiring on {DateTime}.", _reworksCacheExpiryDate);
  }

  /// <summary>
  /// Returns the cached simulation response of the specified simulation request. If no score is cached, null is returned instead.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <returns>The simulation response or null, if no score is cached.</returns>
  public Task<HuisSimulationResponse?> GetCachedScoreSimulationAsync(HuisSimulationRequest request)
  {
    return persistence.GetCachedScoreSimulationAsync(request);
  }

  /// <summary>
  /// Adds a new cache entry for the score simulation request and it's corresponding simulated score.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <param name="response">The simulation response.</param>
  public async Task AddCachedScoreSimulationAsync(HuisSimulationRequest request, HuisSimulationResponse response)
  {
    await persistence.AddCachedScoreSimulationAsync(request, response);
  }
}
