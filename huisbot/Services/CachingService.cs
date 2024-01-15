﻿using huisbot.Models.Huis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Services;

/// <summary>
/// The caching service handles all kind of caching of values in both memory and database.
/// </summary>
public class CachingService
{
  private readonly PersistenceService _persistence;
  private readonly ILogger<CachingService> _logger;

  public CachingService(PersistenceService persistence, ILogger<CachingService> logger)
  {
    _persistence = persistence;
    _logger = logger;
  }

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
    _logger.LogInformation("Updated reworks cache, expiring on {DateTime}.", _reworksCacheExpiryDate);
  }

  /// <summary>
  /// Returns the cached simulated score of the specified simulation request. If no score is cached, null is returned instead.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <returns>The simulated score or null, if no score is cached.</returns>
  public Task<HuisSimulatedScore?> GetCachedScoreSimulationAsync(HuisSimulationRequest request)
  {
    return _persistence.GetCachedScoreSimulationAsync(request);
  }

  /// <summary>
  /// Adds a new cache entry for the score simulation request and it's corresponding simulated score.
  /// </summary>
  /// <param name="request">The score simulation request.</param>
  /// <param name="score">The simulated score.</param>
  public async Task AddCachedScoreSimulationAsync(HuisSimulationRequest request, HuisSimulatedScore score)
  {
    await _persistence.AddCachedScoreSimulationAsync(request, score);
  }
}
