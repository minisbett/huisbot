using Discord.Net;
using huisbot.Models.Huis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace huisbot.Services;

/// <summary>
/// The Huis API service is responsible for communicating with the Huis API @ https://pp-api.huismetbenen.nl/.
/// </summary>
public class HuisApiService
{
  private readonly HttpClient _http;
  private readonly ILogger<HuisApiService> _logger;

  /// <summary>
  /// The cached reworks from the API.
  /// </summary>
  private readonly Cached<HuisRework[]> _reworks = new Cached<HuisRework[]>(TimeSpan.FromMinutes(5));

  public HuisApiService(IHttpClientFactory httpClientFactory, ILogger<HuisApiService> logger)
  {
    _http = httpClientFactory.CreateClient("huisapi");
    _logger = logger;
  }

  /// <summary>
  /// Returns a bool whether a connection to the Huis API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsAvailableAsync()
  {
    try
    {
      // Try to send a request to the base URL of the Huis API.
      HttpResponseMessage response = await _http.GetAsync("");

      // Check whether it returns the expected result.
      if (!(await response.Content.ReadAsStringAsync()).Contains("<pre>Cannot GET /</pre>"))
        throw new Exception("Result does not contain \"<pre>Cannot GET /</pre>\".");

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("IsAvailable() returned false: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns an array of all reworks from the API.
  /// </summary>
  /// <returns></returns>
  public async Task<HuisRework[]?> GetReworksAsync()
  {
    // If the cached reworks are not expired, return them.
    if (!_reworks.IsExpired)
      return _reworks.Value;

    try
    {
      // Get the reworks from the API.
      string json = await _http.GetStringAsync("reworks/list");
      HuisRework[]? reworks = JsonConvert.DeserializeObject<HuisRework[]>(json);

      // Check whether the deserialized json is valid.
      if (reworks is null || reworks.Length == 0)
      {
        _logger.LogError("Failed to deserialize the reworks from the Huis API. https://pp-api.huismetbenen.nl/reworks/list");
        return null;
      }

      // Update the cached reworks and return it.
      _reworks.Value = reworks;
      return reworks;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the list of reworks from the Huis API: {Message}", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the player with the specified id in the specified rework from the API.
  /// </summary>
  /// <param name="playerId">The osu! id of the player.</param>
  /// <param name="playerId">The id of the rework.</param>
  /// <returns></returns>
  public async Task<HuisPlayer?> GetPlayerAsync(int playerId, int reworkId)
  {
    // TODO: Implement caching

    try
    {
      // Get the player from the API.
      string json = await _http.GetStringAsync($"player/userdata/{playerId}/{reworkId}");
      HuisPlayer? player = JsonConvert.DeserializeObject<HuisPlayer>(json);

      // Check whether the deserialized json is valid.
      if (player is null)
      {
        _logger.LogError("Failed to deserialize the player from the Huis API. https://pp-api.huismetbenen.nl/player/userdata/{playerId}/{reworkId}",
          playerId, reworkId);
        return null;
      }

      return player;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the player from the Huis API: {Message} https://pp-api.huismetbenen.nl/player/userdata/{playerId}/{reworkId}",
        ex.Message, playerId, reworkId);
      return null;
    }
  }

  /// <summary>
  /// Calculates the specified score via the Huis API and returns the result.
  /// </summary>
  /// <param name="request">The score request.</param>
  /// <returns>The calculation result.</returns>
  public async Task<HuisCalculationResult?> CalculateAsync(HuisCalculationRequest request)
  {
    // TODO: Implement caching

    try
    {
      // Get the player from the API.
      HttpResponseMessage response = await _http.PatchAsync("calculate-score", new StringContent(request.ToJson(), Encoding.UTF8, "application/json"));
      string json = await response.Content.ReadAsStringAsync();
      HuisCalculationResult? result = JsonConvert.DeserializeObject<HuisCalculationResult>(await response.Content.ReadAsStringAsync());

      // Check whether the deserialized json is valid.
      if (response is null)
      {
        _logger.LogError("Failed to deserialize the calculation response from the Huis API.");
        return null;
      }

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the calculation response from the Huis API: {Message}", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the statistic with the specified ID in the specified rework from the Huis API.
  /// </summary>
  /// <param name="statisticId">The statistic ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The statistic.</returns>
  private async Task<HuisStatistic?> GetStatisticAsync(string statisticId, int reworkId, bool all = false)
  {
    // TODO: Implement caching

    try
    {
      // Get the statistic data from the API.
      string json = await _http.GetStringAsync($"/statistics/{statisticId}/{reworkId}{(all ? "/all" : "")}");
      HuisStatistic? statistic = JsonConvert.DeserializeObject<HuisStatistic>(json);

      // Check whether the deserialized json is valid.
      if (statistic is null)
      {
        _logger.LogError("Failed to deserialize the statistic response from the Huis API. https://pp-api.huismetbenen.nl/statistics/{StatisticId}/{ReworkId}{All}",
          statisticId, reworkId, all ? "all" : "");
        return null;
      }

      return statistic;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the statistic response from the Huis API: {Message}\nhttps://pp-api.huismetbenen.nl/statistics/{StatisticId}/{ReworkId}{All}",
        statisticId, reworkId, ex.Message, all ? "all" : "");
      return null;
    }
  }

  /// <summary>
  /// Returns the top-player statistic in the specified rework from the Huis API.
  /// </summary>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The top-player statistic in the specified rework.</returns>
  public Task<HuisStatistic?> GetTopPlayerStatisticAsync(int reworkId) => GetStatisticAsync("topplayers", reworkId);

  /// <summary>
  /// Returns the top-scores statistic in the specified rework from the Huis API.
  /// </summary>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The top-scores statistic in the specified rework.</returns>
  public Task<HuisStatistic?> GetTopScoresStatisticAsync(int reworkId) => GetStatisticAsync("topscores", reworkId, true);
}
