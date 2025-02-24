using huisbot.Helpers;
using huisbot.Models.Options;
using huisbot.Models.Osu;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace huisbot.Services;

// TODO: get rid of api v1

/// <summary>
/// The osu! API service is responsible for communicating with the osu! API.
/// </summary>
public class OsuApiService(IHttpClientFactory httpClientFactory, IOptions<OsuApiOptions> options, ILogger<OsuApiService> logger)
{
  private readonly HttpClient _http = httpClientFactory.CreateClient(nameof(OsuApiService));

  /// <summary>
  /// Returns a bool whether a connection to the osu! v1 API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsV1AvailableAsync()
  {
    try
    {
      HttpResponseMessage response = await _http.GetAsync("api");

      // Check whether it returns the expected result, being a redirect response.
      if (response.StatusCode != HttpStatusCode.Redirect)
        throw new Exception($"API returned status code {response.StatusCode}. Expected: Redirect (302).");

      return true;
    }
    catch (Exception ex)
    {
      logger.LogError("IsV1Available() returned false: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns a bool whether a connection to the osu! v2 API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsV2AvailableAsync()
  {
    try
    {
      // Try to send a request to the base URL of the osu! v2 API, which should return a NotFound (Unauthorized if invalid client credentials).
      HttpResponseMessage response = await _http.GetAsync("api/v2");
      if (response.StatusCode != HttpStatusCode.NotFound)
        throw new Exception($"API returned status code {response.StatusCode}. Expected: NotFound (404).");

      return true;
    }
    catch (Exception ex)
    {
      logger.LogError("IsV2Available() returned false: {Message}", ex.Message);
      return false;
    }
  }

  

  /// <summary>
  /// Returns the the osu! user by the specified ID or name. Returns <see cref="OsuUser.NotFound"/> if the user could not be found.
  /// </summary>
  /// <param name="identifier">The identifier the user.</param>
  /// <returns>The user.</returns>
  public async Task<NotFoundOr<OsuUser>?> GetUserAsync(string identifier)
  {
    try
    {
      HttpResponseMessage response = await _http.GetAsync($"api/v2/users/{(identifier.All(char.IsDigit) ? identifier : "@" + identifier)}");
      if (response.StatusCode == HttpStatusCode.NotFound)
        return NotFoundOr<OsuUser>.NotFound;

      string json = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<OsuUser>(json)?.WasFound();
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the user with identifier \"{Identifier}\" from the osu! API: {Message}", identifier, ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the beatmap set ID for the specified beatmap ID.
  /// </summary>
  /// <returns>The Beatmap Set ID of the specified beatmap.</returns>
  public async Task<NotFoundOr<OsuBeatmap>?> GetBeatmapAsync(int id)
  {
    try
    {
      // Get the user from the API.
      string json = await _http.GetStringAsync($"api/get_beatmaps?b={id}&k={options.Value.ApiKey}");
      OsuBeatmap? beatmap = JsonConvert.DeserializeObject<OsuBeatmap[]>(json)?.FirstOrDefault(x => x.Id == id);

      // Check whether the deserialized json is null/an empty array. If so, the beatmap could not be found. The API returns "[]" when the beatmap could not be found.
      if (beatmap is null)
        return NotFoundOr<OsuBeatmap>.NotFound;

      // Return the beatmap.
      return beatmap.WasFound();
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the beatmap with ID {Id} from the osu! API: {Message}", id, ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the score with the specified ID.
  /// </summary>
  /// <param name="scoreId">The score ID.</param>
  /// <returns>The score with the specified ID./returns>
  public async Task<NotFoundOr<OsuScore>?> GetScoreAsync(long scoreId)
  {
    try
    {
      HttpResponseMessage response = await _http.GetAsync($"api/v2/scores/{scoreId}");
      if (response.StatusCode == HttpStatusCode.NotFound)
        return NotFoundOr<OsuScore>.NotFound;

      string json = await response.Content.ReadAsStringAsync();
      OsuScore? score = JsonConvert.DeserializeObject<OsuScore>(json);

      // If the score is non-standard, reject it as only standard is supported.
      return score?.RulesetId > 0 ? NotFoundOr<OsuScore>.NotFound : score?.WasFound();
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the score with ID {Id} from the osu! API: {Message}", scoreId, ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the X-th best score of the specified user.
  /// </summary>
  /// <param name="userId">The ID of the osu! user.</param>
  /// <param name="index">The one-based index of the X-th best score.</param>
  /// <param name="type">The type of score.</param>
  /// <returns>The X-th best score.</returns>
  public async Task<NotFoundOr<OsuScore>?> GetUserScoreAsync(int userId, int index, ScoreType type)
  {
    try
    {
      HttpResponseMessage response = await _http.GetAsync($"api/v2/users/{userId}/scores/{type.ToString().ToLower()}?limit=1&offset={index - 1}");
      if (response.StatusCode == HttpStatusCode.NotFound)
        return NotFoundOr<OsuScore>.NotFound;

      string json = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<OsuScore[]>(json)?.FirstOrDefault()?.WasFound();
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to get the {Index}-th {Type} score of {userId} from the osu! API: {Message}", index, type, userId, ex.Message);
      return null;
    }
  }
}

/// <summary>
/// Represents a type of score for fetching user scores via <see cref="OsuApiService.GetUserScoreAsync(int, int, ScoreType)"/>.
/// </summary>
public enum ScoreType
{
  Best,
  Recent
}