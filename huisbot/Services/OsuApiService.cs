using huisbot.Models.Osu;
using huisbot.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace huisbot.Services;

/// <summary>
/// The osu! API service is responsible for communicating with the osu! API.
/// </summary>
public class OsuApiService
{
  private readonly HttpClient _http;
  private readonly ILogger<OsuApiService> _logger;

  /// <summary>
  /// The API key for the osu! v1 API.
  /// </summary>
  private readonly string _apiKey;

  /// <summary>
  /// The client ID for the osu! v2 API.
  /// </summary>
  private readonly string _clientId;

  /// <summary>
  /// The client secret for the osu! v2 API.
  /// </summary>
  private readonly string _clientSecret;

  /// <summary>
  /// The date when the API v2 access token expires.
  /// </summary>
  private DateTimeOffset _accessTokenExpiresAt = DateTimeOffset.MinValue;

  public OsuApiService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OsuApiService> logger)
  {
    _http = httpClientFactory.CreateClient("osuapi");
    _apiKey = config["OSU_API_KEY"] ?? throw new InvalidOperationException("The environment variable 'OSU_API_KEY' is not set.");
    _clientId = config["OSU_OAUTH_CLIENT_ID"] ?? throw new InvalidOperationException("The environment variable 'OSU_OAUTH_CLIENT_ID' is not set.");
    _clientSecret = config["OSU_OAUTH_CLIENT_SECRET"] ?? throw new InvalidOperationException("The environment variable 'OSU_OAUTH_CLIENT_SECRET' is not set.");
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
      // Try to send a request to the base URL of the osu! v1 API.
      HttpResponseMessage response = await _http.GetAsync("api");

      // Check whether it returns the expected result.
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
    // Make sure a valid access token exists. If not, v2 is unavailable.
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
    // Check whether the access token is still valid.
    if (DateTimeOffset.Now < _accessTokenExpiresAt)
      return true;

    _logger.LogInformation("The osu! API v2 access token has expired. Requesting a new one...");

    try
    {
      // Send the request.
      HttpResponseMessage response = await _http.PostAsync($"oauth/token",
        new FormUrlEncodedContent(new Dictionary<string, string>()
        {
        { "client_id", _clientId },
        { "client_secret", _clientSecret },
        { "grant_type", "client_credentials"},
        { "scope", "public" }
        }));

      // Parse the response object into a dynamic object.
      var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

      // Check whether the response was successful.
      if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.InternalServerError) // For some reason invalid client id = internal server error.
        throw new Exception("Unauthorized.");
      if (result?.access_token is null)
        throw new Exception("The oauth access token response did not contain an access token.");

      // Set the new access token and expiration date and return true.
      _http.DefaultRequestHeaders.Remove("Authorization");
      _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.access_token}");
      _accessTokenExpiresAt = DateTimeOffset.Now.AddSeconds((int)result.expires_in - 10);
      _logger.LogInformation("The osu! API v2 access token has been updated and expires at {date}.", _accessTokenExpiresAt);
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
      // Get the user from the API.
      string json = await _http.GetStringAsync($"api/get_user?u={identifier}&k={_apiKey}");
      OsuUser? user = JsonConvert.DeserializeObject<OsuUser[]>(json)?.FirstOrDefault();

      // Check whether the deserialized json is null/an empty array. If so, the user could not be found. The API returns "[]" when the user could not be found.
      if (user is null)
        return NotFoundOr<OsuUser>.NotFound;

      return user.WasFound();
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
      string json = await _http.GetStringAsync($"api/get_beatmaps?b={id}&k={_apiKey}");
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

      // Parse the score object and either return it or null.
      OsuScore? score = JsonConvert.DeserializeObject<OsuScore>(await response.Content.ReadAsStringAsync());
      return score?.WasFound();
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the score with ID {Id} from the osu! API: {Message}",
        scoreId, ex.Message);
      return null;
    }
  }
}