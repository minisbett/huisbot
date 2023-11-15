using huisbot.Models.Huis;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huisbot.Services;

/// <summary>
/// The osu! API service is responsible for communicating with the osu! v1 API @ https://pp-api.huismetbenen.nl/.
/// </summary>
public class OsuApiService
{
  private readonly HttpClient _http;
  private readonly ILogger<OsuApiService> _logger;

  public OsuApiService(IHttpClientFactory httpClientFactory, ILogger<OsuApiService> logger)
  {
    _http = httpClientFactory.CreateClient("osuapi");
    _logger = logger;
  }

  /// <summary>
  /// Returns a bool whether a connection to the osu! API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsAvailableAsync()
  {
    try
    {
      // Try to send a request to the base URL of the osu! API.
      HttpResponseMessage response = await _http.GetAsync("/");

      // Check whether it returns the expected result.
      if (!response.IsSuccessStatusCode)
        throw new Exception($"API returned status code {response.StatusCode}.");

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("IsAvailable() returned false: {Message}", ex.Message);
      return false;
    }
  }
}
