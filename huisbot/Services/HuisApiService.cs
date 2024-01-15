﻿using huisbot.Models.Huis;
using huisbot.Utilities;
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
  private readonly CachingService _caching;
  private readonly ILogger<HuisApiService> _logger;

  // TEMPORARY MONITORING
  private static List<DateTime> _lastSimulations = new List<DateTime>();
  private static List<DateTime> _lastQueues = new List<DateTime>();
  public static int GetSimulationsLastHour()
  {
    _lastSimulations.RemoveAll(x => x.AddHours(1) < DateTime.UtcNow);
    return _lastSimulations.Count;
  }
  public static int GetQueuesLastHour()
  {
    _lastQueues.RemoveAll(x => x.AddHours(1) < DateTime.UtcNow);
    return _lastQueues.Count;
  }
  // TEMPORARY MONITORING

  public HuisApiService(IHttpClientFactory httpClientFactory, CachingService caching, ILogger<HuisApiService> logger)
  {
    _http = httpClientFactory.CreateClient("huisapi");
    _caching = caching;
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
  /// <returns>The reworks.</returns>
  public async Task<HuisRework[]?> GetReworksAsync()
  {
    // Check whether the cached reworks are valid. If so, return them.
    if (_caching.GetReworks() is HuisRework[] r)
      return r;

    try
    {
      // Get the reworks from the API.
      string json = await _http.GetStringAsync("reworks/list");
      HuisRework[]? reworks = JsonConvert.DeserializeObject<HuisRework[]>(json)?.Where(x => x.RulesetId == 0).ToArray();

      // Check whether the deserialized json is valid.
      if (reworks is null || reworks.Length == 0)
        throw new Exception("Deserialization of JSON returned null.");

      // Cache the reworks and return them.
      _caching.SetReworks(reworks);
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
      _logger.LogError("Failed to get the player calculation queue from the Huis API: {Message} https://pp-api.huismetbenen.nl/queue/list",
        ex.Message);
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
    // TEMPORARY MONITORING
    _lastQueues.Add(DateTime.UtcNow);
    // TEMPORARY MONITORING

    try
    {
      // Send the queue request to the API.
      var request = new { user_id = playerId, rework = reworkId };
      HttpResponseMessage response = await _http.PatchAsync("/queue/add-to-queue", new StringContent(JsonConvert.SerializeObject(request),
        Encoding.UTF8, "application/json"));

      // Make sure the request was successful by checking whether the json contains the "queue" property.
      string json = await response.Content.ReadAsStringAsync();
      if (JsonConvert.DeserializeObject<dynamic>(json)?.queue is null)
        throw new Exception("No \"queue\" property found in the response.");

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the player calculation queue from the Huis API: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns the player with the specified ID in the specified rework from the API.
  /// </summary>
  /// <param name="playerId">The osu! ID of the player.</param>
  /// <param name="playerId">The the rework.</param>
  /// <returns>The player with the specified ID in the specified rework.</returns>
  public async Task<HuisPlayer?> GetPlayerAsync(int playerId, HuisRework rework)
  {
    string url = $"/player/userdata/{playerId}/{rework.Id}";
    try
    {
      // Get the json from the API.
      string json = await _http.GetStringAsync(url);

      // Check whether the json matches "{}". If so, no player data is available. In that case, return an outdated player object.
      if (json == "{}")
        return HuisPlayer.Outdated;

      // Otherwise, deserialize the json.
      HuisPlayer? player = JsonConvert.DeserializeObject<HuisPlayer>(json);

      // Check whether the deserialized json is valid.
      if (player is null)
        throw new Exception("Deserialization of JSON returned null.");

      // Check whether the player is outdated. In that case, return an outdated player object.
      if (player.PPVersion != rework.PPVersion)
        return HuisPlayer.Outdated;

      return player;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the player from the Huis API: {Message} https://pp-api.huismetbenen.nl{Url}", ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Simulates the specified score simulation request via the Huis API and returns the result.
  /// </summary>
  /// <param name="request">The score request.</param>
  /// <returns>The calculation result.</returns>
  public async Task<HuisSimulatedScore?> SimulateAsync(HuisSimulationRequest request)
  {
    // Check whether a score for the score simulation request is cached.
    if (await _caching.GetCachedScoreSimulationAsync(request) is HuisSimulatedScore s)
      return s;

    // TEMPORARY MONITORING
    _lastSimulations.Add(DateTime.UtcNow);
    // TEMPORARY MONITORING

    try
    {
      // Send the score calculation request to the server and parse the response.
      HttpResponseMessage response = await _http.PatchAsync("/calculate-score", new StringContent(request.ToJson(),
        Encoding.UTF8, "application/json"));
      string json = await response.Content.ReadAsStringAsync();
      HuisSimulatedScore? score = JsonConvert.DeserializeObject<HuisSimulatedScore>(json);

      // Check whether the deserialized json is valid.
      if (score is null)
        throw new Exception("Deserialization of JSON returned null.");

      // Check whether the json contains an error.
      string? error = JsonConvert.DeserializeObject<dynamic>(json)?.error;
      if (error is not null)
        throw new Exception($"API returned {error}");

      // Cache the score and return it.
      await _caching.AddCachedScoreSimulationAsync(request, score);
      return score;
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
  /// <returns>The statistic comparing the specified rework with the live pp system.</returns>
  public async Task<HuisStatistic?> GetStatisticAsync(string statisticId, int reworkId)
  {
    string url = $"/statistics/{statisticId}/{reworkId}{(statisticId == "topscores" ? "/all" : "")}";
    try
    {
      // Get the statistic data from the API.
      string json = await _http.GetStringAsync(url);
      HuisStatistic? statistic = JsonConvert.DeserializeObject<HuisStatistic>(json);

      // Check whether the deserialized json is valid.
      if (statistic is null || statistic.Old is null || statistic.New is null || statistic.Difference is null)
        throw new Exception("Deserialization of JSON returned null.");

      return statistic;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the statistic from the Huis API: {Message} https://pp-api.huismetbenen.nl{Url}", ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Returns the global score leaderboard in the specified rework from the Huis API.
  /// </summary>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="sort">The sort option.</param>
  /// <returns>The global score rankings in the specified rework.</returns>
  public async Task<HuisScore[]?> GetScoreRankingsAsync(int reworkId, Sort sort)
  {
    string url = $"/rankings/topscores/{reworkId}?sort={sort.Code}&order={(sort.IsAscending ? "asc" : "desc")}";
    try
    {
      // Get the ranking data from the API.
      string json = await _http.GetStringAsync(url);
      HuisScore[]? scores = JsonConvert.DeserializeObject<HuisScore[]>(json);

      // Check whether the deserialized json is valid.
      if (scores is null)
        throw new Exception("Deserialization of JSON returned null.");

      return scores;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the global score leaderboard from the Huis API: {Message} https://pp-api.huismetbenen.nl{Url}",
        ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Returns the global player leaderboard in the specified rework from the Huis API.
  /// </summary>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="sort">The sorting option.</param>
  /// <param name="onlyUpToDate">Bool whether only calculated, up-to-date players should be included.</param>
  /// <param name="hideUnranked">Bool whether unranked players (inactivity) should be hidden.</param>
  /// <returns>The global player rankings in the specified rework.</returns>
  public async Task<HuisPlayer[]?> GetPlayerRankingsAsync(int reworkId, Sort sort, bool onlyUpToDate, bool hideUnranked)
  {
    string url = $"/rankings/players/{reworkId}?sort={sort.Code}&order={(sort.IsAscending ? "asc" : "desc")}" +
                 $"&onlyUpToDate={onlyUpToDate.ToString().ToLower()}&hideUnranked={onlyUpToDate.ToString().ToLower()}";
    try
    {
      // Get the ranking data from the API.
      string json = await _http.GetStringAsync(url);
      HuisPlayer[]? players = JsonConvert.DeserializeObject<HuisPlayer[]>(json);

      // Check whether the deserialized json is valid.
      if (players is null)
        throw new Exception("Deserialization of JSON returned null.");

      return players;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the global player leaderboard from the Huis API: {Message} https://pp-api.huismetbenen.nl{Url}",
        ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Returns the top plays of the specified player in the specified rework from the Huis API.
  /// </summary>
  /// <param name="playerId">The player ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <returns>The top plays of the specified player in the specified rework.</returns>
  public async Task<HuisScore[]?> GetTopPlaysAsync(int playerId, int reworkId)
  {
    string url = $"/player/topscores/{playerId}/{reworkId}";
    try
    {
      // Get the top plays data from the API.
      string json = await _http.GetStringAsync(url);
      HuisScore[]? scores = JsonConvert.DeserializeObject<HuisScore[]>(json);

      // Check whether the deserialized json is valid.
      if (scores is null)
        throw new Exception("Deserialization of JSON returned null.");

      return scores;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the top plays from the Huis API: {Message} https://pp-api.huismetbenen.nl{Url}", ex.Message, url);
      return null;
    }
  }
}
