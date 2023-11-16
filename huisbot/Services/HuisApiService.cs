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
      HuisRework[]? reworks = JsonConvert.DeserializeObject<HuisRework[]>(json)?.Where(x => x.RulesetId == 0).ToArray() /* std only for now */;

      // Check whether the deserialized json is valid.
      if (reworks is null || reworks.Length == 0)
        throw new Exception("Deserialization of JSON returned null.");

      // Update the cached reworks and return it.
      _reworks.Value = reworks;
      return reworks;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the list of reworks from the Huis API: {Message} https://pp-api.huismetbenen.nl/reworks/list", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the player calculation queue of all of Huismetbenen from the API.
  /// </summary>
  /// <returns>The player calculation queue.</returns>
  public async Task<HuisQueue?> GetQueueAsync()
  {
    try
    {
      // Get the reworks from the API.
      string json = await _http.GetStringAsync("queue/list");
      HuisQueue? queue = JsonConvert.DeserializeObject<HuisQueue>(json);

      // Check whether the deserialized json is valid.
      if (queue is null || queue.Entries is null)
        throw new Exception("Deserialization of JSON returned null.");

      return queue;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the player calculation queue from the Huis API: {Message} https://pp-api.huismetbenen.nl/queue/list", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Queues the specified player in the rework with the specified ID.
  /// </summary>
  /// <param name="playerId">The osu! user ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>Bool whether queuing was successful.</returns>
  public async Task<bool> QueuePlayerAsync(int playerId, int reworkId)
  {
    try
    {
      // Send the queue request to the API.
      var request = new { user_id = playerId, rework = reworkId };
      HttpResponseMessage response = await _http.PatchAsync("queue/add-to-queue", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

      // Make sure the request was successful by checking whether the json contains the "queue" property.
      string json = await response.Content.ReadAsStringAsync();
      if(JsonConvert.DeserializeObject<dynamic>(json)?.queue is null)
        throw new Exception("No \"queue\" property found in the response.");

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the player calculation queue from the Huis API: {Message} https://pp-api.huismetbenen.nl/queue/list", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns the player with the specified id in the specified rework from the API.
  /// </summary>
  /// <param name="playerId">The osu! id of the player.</param>
  /// <param name="playerId">The id of the rework.</param>
  /// <returns>The player with the specified id in the specified rework.</returns>
  public async Task<HuisPlayer?> GetPlayerAsync(int playerId, int reworkId)
  {
    // TODO: Implement caching

    try
    {
      // Get the json from the API.
      string json = await _http.GetStringAsync($"player/userdata/{playerId}/{reworkId}");

      // Check whether the json matches "{}". If so, no player data is available. In that case, return an uncalculated player object.
      if (json == "{}")
        return HuisPlayer.Uncalculated;

      // Otherwise, deserialize the json.
      HuisPlayer? player = JsonConvert.DeserializeObject<HuisPlayer>(json);

      // Check whether the deserialized json is valid.
      if (player is null)
        throw new Exception("Deserialization of JSON returned null.");

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
        throw new Exception("Deserialization of JSON returned null.");

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
      if (statistic is null || statistic.Old is null || statistic.New is null || statistic.Difference is null)
        throw new Exception("Deserialization of JSON returned null.");

      return statistic;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the statistic response from the Huis API: {Message} https://pp-api.huismetbenen.nl/statistics/{StatisticId}/{ReworkId}{All}",
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
