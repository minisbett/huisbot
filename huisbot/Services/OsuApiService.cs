﻿using Discord.Commands;
using huisbot.Helpers;
using huisbot.Models.Options;
using huisbot.Models.Osu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace huisbot.Services;

// TODO: get rid of api v1

/// <summary>
/// The osu! API service is responsible for communicating with the osu! API.
/// </summary>
public class OsuApiService
{
  private readonly ExpiringAccessToken _accessToken;
  private readonly HttpClient _http;
  private readonly OsuApiOptions _options;
  private readonly ILogger<OsuApiService> _logger;

  public OsuApiService([FromKeyedServices("osuapi")] ExpiringAccessToken accessToken, IHttpClientFactory httpClientFactory,
                       IOptions<OsuApiOptions> options, ILogger<OsuApiService> logger)
  {
    _accessToken = accessToken;
    _http = httpClientFactory.CreateClient("osuapi");
    _http.DefaultRequestHeaders.Add("Authorization", accessToken.ToString());
    _options = options.Value;
    _logger = logger;
  }

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
      _logger.LogError("IsV1Available() returned false: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns a bool whether a connection to the osu! v2 API can be established.
  /// </summary>
  /// <returns>Bool whether a connection can be established.</returns>
  public async Task<bool> IsV2AvailableAsync()
  {
    if (!await EnsureAccessTokenAsync())
      return false;

    try
    {
      // Try to send a request to the base URL of the osu! v2 API.
      HttpResponseMessage response = await _http.GetAsync("api/v2");

      // Check whether it returns the expected result.
      if (response.StatusCode != HttpStatusCode.NotFound)
        throw new Exception($"API returned status code {response.StatusCode}. Expected: NotFound (404)."); // Gives 401 Unauthorized if invalid client credentials.

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("IsV2Available() returned false: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Ensures that the current osu! API v2 access token is valid and returns whether it is valid or was successfully refreshed.
  /// </summary>
  /// <returns>Bool whether the access token is valid or was successfully refreshed.</returns>
  public async Task<bool> EnsureAccessTokenAsync()
  {
    if (!_accessToken.IsExpired)
      return true;

    _logger.LogInformation("The osu! API v2 access token has expired. Requesting a new one...");

    try
    {
      HttpResponseMessage response = await _http.PostAsync($"oauth/token",
        new FormUrlEncodedContent(new Dictionary<string, string>()
        {
        { "client_id", _options.ClientId.ToString() },
        { "client_secret", _options.ClientSecret },
        { "grant_type", "client_credentials"},
        { "scope", "public" }
        }));

      OsuAccessToken? token = JsonConvert.DeserializeObject<OsuAccessToken>(await response.Content.ReadAsStringAsync());

      if (response.StatusCode is HttpStatusCode.Unauthorized)
        throw new Exception("Unauthorized.");
      if (token?.Token is null)
        throw new Exception("The access token is null.");

      _accessToken.Renew(token.Token, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn - 10));
      _http.DefaultRequestHeaders.Remove("Authorization");
      _http.DefaultRequestHeaders.Add("Authorization", _accessToken.ToString());

      _logger.LogInformation("The osu! API v2 access token has been updated and expires at {Date}.", _accessToken.ExpiresAt);
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to request an osu! API v2 access token: {Message}", ex.Message);
      return false;
    }

    return true;
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
      _logger.LogError("Failed to get the user with identifier \"{Identifier}\" from the osu! API: {Message}", identifier, ex.Message);
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
      string json = await _http.GetStringAsync($"api/get_beatmaps?b={id}&k={_options.ApiKey}");
      OsuBeatmap? beatmap = JsonConvert.DeserializeObject<OsuBeatmap[]>(json)?.FirstOrDefault(x => x.Id == id);

      // Check whether the deserialized json is null/an empty array. If so, the beatmap could not be found. The API returns "[]" when the beatmap could not be found.
      if (beatmap is null)
        return NotFoundOr<OsuBeatmap>.NotFound;

      // Return the beatmap.
      return beatmap.WasFound();
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the beatmap with ID {Id} from the osu! API: {Message}", id, ex.Message);
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
    // Make sure a valid access token exists. If not, return null.
    if (!await EnsureAccessTokenAsync())
      return null;

    try
    {
      // Get the score from the API and check whether a 404 was returned. If so, the score was not found.
      HttpResponseMessage response = await _http.GetAsync($"api/v2/scores/{scoreId}");
      if (response.StatusCode == HttpStatusCode.NotFound)
        return NotFoundOr<OsuScore>.NotFound;

      // Parse the score object.
      string json = await response.Content.ReadAsStringAsync();
      OsuScore? score = JsonConvert.DeserializeObject<OsuScore>(json);

      // If the score is non-standard, reject it as only standard is supported.
      return score?.RulesetId > 0 ? NotFoundOr<OsuScore>.NotFound : score?.WasFound();
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the score with ID {Id} from the osu! API: {Message}", scoreId, ex.Message);
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
    // Make sure a valid access token exists. If not, return null.
    if (!await EnsureAccessTokenAsync())
      return null;

    try
    {
      // Get the score from the API and check whether a 404 was returned. If so, the score was not found.
      HttpResponseMessage response = await _http.GetAsync($"api/v2/users/{userId}/scores/{type.ToString().ToLower()}?mode=osu&limit=1&offset={index - 1}");
      if (response.StatusCode == HttpStatusCode.NotFound)
        return NotFoundOr<OsuScore>.NotFound;

      // Parse the response.
      string json = await response.Content.ReadAsStringAsync();
      OsuScore[]? scores = JsonConvert.DeserializeObject<OsuScore[]>(json);

      return scores?.Length > 0 ? scores[0].WasFound() : null;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the {Index}-th {Type} score of {userId} from the osu! API: {Message}", index, type, userId, ex.Message);
      return null;
    }
  }
}

/// <summary>
/// Represents a type of score for fetching user scores via <see cref="OsuApiService.GetUserScoreAsync(int, int, string)"/>.
/// </summary>
public enum ScoreType
{
  Best,
  Recent
}