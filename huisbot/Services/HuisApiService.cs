using huisbot.Models.Huis;
using huisbot.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace huisbot.Services;

/// <summary>
/// The Huis API service is responsible for communicating with the Huis API @ https://api.pp.huismetbenen.nl/.
/// </summary>
public class HuisApiService(IHttpClientFactory httpClientFactory, CachingService caching, ILogger<HuisApiService> logger)
{
  private readonly HttpClient _http = httpClientFactory.CreateClient("huisapi");

  /// <summary>
  /// Returns a bool whether a connection to the Huis API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsAvailableAsync()
  {
    try
    {
      // Try to send a request to the ping endpoint of the Huis API and return whether a success was reported.
      return (await _http.GetAsync("ping")).IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
      logger.LogError("IsAvailable() returned false: {Message}", ex.Message);
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
    if (caching.GetReworks() is HuisRework[] r)
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
      caching.SetReworks(reworks);
      return reworks;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the list of reworks from the Huis API: {Message} https://api.pp.huismetbenen.nl/reworks/list", ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the player calculation queue of the specified rework from the API.
  /// </summary>
  /// <param name="reworkId">The ID of the rework.</param>
  /// <returns>The player calculation queue.</returns>
  public async Task<int[]?> GetQueueAsync(int reworkId)
  {
    try
    {
      // Get the json from the Huis API and parse the objects in the queue property.
      string json = await _http.GetStringAsync($"queue/list?rework={reworkId}");
      JObject[]? entries = JObject.Parse(json)["queue"]?.ToObject<JObject[]>();

      // Check whether the deserialized json is valid and select the user id of each entry.
      return entries?.Select(x => x["user_id"]!.Value<int>()).ToArray() ?? throw new Exception("Deserialization of JSON returned null.");
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the player calculation queue from the Huis API: {Message} https://api.pp.huismetbenen.nl/queue/list",
        ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Queues the specified player in the rework with the specified ID.<br/><br/>
  /// A requester identifier needs to be provided and will be passed to Huismetbenen in order to provide ratelimits
  /// based on the person invoking the queuing, rather than the Onion-key associated with this application.
  /// This is only of relevance if the application is using an Onion-key and is therefore an authenticated 3rd-party app.
  /// </summary>
  /// <param name="playerId">The osu! user ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="discordId">The Discord ID of the requester.</param>
  /// <returns>True when queuing was successful, false when the requester is being ratelimited, null if an error ocurred.</returns>
  public async Task<bool?> QueuePlayerAsync(int playerId, int reworkId, ulong discordId)
  {
    try
    {
      // Send the queue request to the API.
      var request = new { user_id = playerId, rework = reworkId, discord_id = discordId };
      HttpResponseMessage response = await _http.PatchAsync("/queue/add-to-queue", new StringContent(JsonConvert.SerializeObject(request),
        Encoding.UTF8, "application/json"));

      // Check whether the requester is being ratelimited.
      if (response.StatusCode == HttpStatusCode.TooManyRequests)
        return false;

      // Make sure the request was successful by checking whether the json contains the "queue" property.
      string json = await response.Content.ReadAsStringAsync();
      if (JsonConvert.DeserializeObject<dynamic>(json)?.queue is null)
        throw new Exception("No \"queue\" property found in the response.");

      return true;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the player calculation queue from the Huis API: {Message}", ex.Message);
      return null;
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
      HuisPlayer? player = JsonConvert.DeserializeObject<HuisPlayer>(json) ?? throw new Exception("Deserialization of JSON returned null.");

      // Check whether the player is outdated. In that case, return an outdated player object.
      if (player.PPVersion != rework.PPVersion)
        return HuisPlayer.Outdated;

      return player;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the player from the Huis API: {Message} https://api.pp.huismetbenen.nl{Url}", ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Calculates the specified score calculation request via the Huis API and returns the result.
  /// </summary>
  /// <param name="request">The score request.</param>
  /// <returns>The calculation result.</returns>
  public async Task<HuisCalculationResponse?> CalculateAsync(HuisCalculationRequest request)
  {
    // Check whether a score for the score calculation request is cached.
    if (await caching.GetCachedScoreCalcuationAsync(request) is HuisCalculationResponse s)
      return s;

    try
    {
      // Send the score calculation request to the server and parse the response.
      HttpResponseMessage response = await _http.PostAsync("/calculate-score", new StringContent(request.ToJson(),
        Encoding.UTF8, "application/json"));
      string json = await response.Content.ReadAsStringAsync();
      HuisCalculationResponse? simResponse = JsonConvert.DeserializeObject<HuisCalculationResponse>(json)
        ?? throw new Exception("Deserialization of JSON returned null.");

      // Check whether the json contains an error.
      string? error = JsonConvert.DeserializeObject<dynamic>(json)?.error;
      if (error is not null)
        throw new Exception($"API returned {error}");

      // Cache the calculation response and return it.
      await caching.AddCachedScoreCalculationAsync(request, simResponse);
      return simResponse;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to process the calculation on the Huis API: {Message}", ex.Message);
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
      logger.LogError("Failed to get the statistic from the Huis API: {Message} https://api.pp.huismetbenen.nl{Url}", ex.Message, url);
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
      return scores is null ? throw new Exception("Deserialization of JSON returned null.") : scores;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the global score leaderboard from the Huis API: {Message} https://api.pp.huismetbenen.nl{Url}",
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
                 $"&onlyUpToDate={onlyUpToDate.ToString().ToLower()}&hideUnranked={hideUnranked.ToString().ToLower()}";
    try
    {
      // Get the ranking data from the API.
      string json = await _http.GetStringAsync(url);
      HuisPlayer[]? players = JsonConvert.DeserializeObject<HuisPlayer[]>(json);

      // Check whether the deserialized json is valid.
      return players is null ? throw new Exception("Deserialization of JSON returned null.") : players;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the global player leaderboard from the Huis API: {Message} https://api.pp.huismetbenen.nl{Url}",
        ex.Message, url);
      return null;
    }
  }

  /// <summary>
  /// Returns the top plays of the specified player in the specified rework from the Huis API.
  /// </summary>
  /// <param name="playerId">The player ID.</param>
  /// <param name="reworkId">The rework ID.</param>
  /// <param name="scoreType">The type of scores (topranks, flashlight or pinned).</param>
  /// <returns>The top plays of the specified player in the specified rework.</returns>
  public async Task<HuisScore[]?> GetTopPlaysAsync(int playerId, int reworkId, string scoreType)
  {
    string url = $"/player/scores/{playerId}/{reworkId}/{scoreType}";
    try
    {
      // Get the top plays data from the API.
      string json = await _http.GetStringAsync(url);
      HuisScore[]? scores = JsonConvert.DeserializeObject<HuisScore[]>(json);

      // Check whether the deserialized json is valid.
      return scores is null ? throw new Exception("Deserialization of JSON returned null.") : scores;
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the top plays from the Huis API: {Message} https://api.pp.huismetbenen.nl{Url}", ex.Message, url);
      return null;
    }
  }
}
