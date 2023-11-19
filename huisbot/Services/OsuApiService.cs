using huisbot.Models.Osu;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace huisbot.Services;

/// <summary>
/// The osu! API service is responsible for communicating with the osu! v1 API @ https://pp-api.huismetbenen.nl/.
/// </summary>
public class OsuApiService
{
  private readonly HttpClient _http;
  private readonly string _apikey;
  private readonly ILogger<OsuApiService> _logger;

  public OsuApiService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<OsuApiService> logger)
  {
    _http = httpClientFactory.CreateClient("osuapi");
    _apikey = config["OSU_API_KEY"] ?? throw new InvalidOperationException("The environment variable 'OSU_API_KEY' is not set.");
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
      HttpResponseMessage response = await _http.GetAsync("");

      // Check whether it returns the expected result.
      if (response.StatusCode != HttpStatusCode.Redirect)
        throw new Exception($"API returned status code {response.StatusCode}. Expected: Redirect (302).");

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError("IsAvailable() returned false: {Message}", ex.Message);
      return false;
    }
  }

  /// <summary>
  /// Returns the the osu! user by the specified ID or name. Returns <see cref="OsuUser.NotFound"/> if the user could not be found.
  /// </summary>
  /// <param name="identifier">The identifier the user.</param>
  /// <returns>The user.</returns>
  public async Task<OsuUser?> GetUserAsync(string identifier)
  {
    try
    {
      // Get the user from the API.
      string json = await _http.GetStringAsync($"get_user?u={identifier}&k={_apikey}");
      OsuUser? user = JsonConvert.DeserializeObject<OsuUser[]>(json)?.FirstOrDefault();

      // Check whether the deserialized json is null/an empty array. If so, the user could not be found. The API returns "[]" when the user could not be found.
      if (user is null)
        return OsuUser.NotFound;

      return user;
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
  public async Task<OsuBeatmap?> GetBeatmapAsync(int id)
  {
    try
    {
      // Get the user from the API.
      string json = await _http.GetStringAsync($"get_beatmaps?b={id}&k={_apikey}");
      OsuBeatmap? beatmap = JsonConvert.DeserializeObject<OsuBeatmap[]>(json)?.FirstOrDefault(x => x.Id == id);

      // Check whether the deserialized json is null/an empty array. If so, the beatmap could not be found. The API returns "[]" when the beatmap could not be found.
      if (beatmap is null)
        return OsuBeatmap.NotFound;

      return beatmap;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the beatmap with ID {Id} from the osu! API: {Message}", id, ex.Message);
      return null;
    }
  }

  /// <summary>
  /// Returns the difficulty rating of the specified beatmap in the specified ruleset with the specified mods.
  /// </summary>
  /// <param name="rulesetId">The ruleset ID.</param>
  /// <param name="beatmapId">The beatmap ID.</param>
  /// <param name="mods">The mods.</param>
  /// <returns>The difficulty rating.</returns>
  public async Task<double?> GetDifficultyRatingAsync(int rulesetId, int beatmapId, string mods)
  {
    try
    {
      // Get the difficulty rating from the API.
      var request = new { ruleset_id = rulesetId, beatmap_id = beatmapId, mods = mods.Chunk(2).Select(x => $"{{\"acronym\": \"{x}\"}}") };
      string s = JsonConvert.SerializeObject(request);
      HttpResponseMessage response = await _http.PostAsync($"https://osu.ppy.sh/difficulty-rating",
        new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

      // Try to parse the difficulty rating from the response.
      string result = await response.Content.ReadAsStringAsync();
      if (!double.TryParse(result, out double rating))
        throw new Exception("Failed to parse the difficulty rating from the response.");

      return rating;
    }
    catch (Exception ex)
    {
      _logger.LogError("Failed to get the difficulty rating for beatmap in ruleset {Ruleset} with ID {Id} and mods {Mods} from the osu! API: {Message}",
        rulesetId, beatmapId, mods, ex.Message);
      return null;
    }
  }
}
