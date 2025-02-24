using huisbot.Helpers;
using huisbot.Models.Options;
using huisbot.Models.Osu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace huisbot.Services;

public class OsuOAuthDelegatingHandler([FromKeyedServices(nameof(OsuApiService))] ExpiringAccessToken accessToken,
                                       IOptions<OsuApiOptions> options, ILogger<OsuOAuthDelegatingHandler> logger) : DelegatingHandler
{
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    // Only enforce an oauth token for osu! API v2 calls.
    if (request.RequestUri?.AbsolutePath.StartsWith("/api/v2") ?? false)
    {
      if (accessToken.IsExpired)
        await RenewAccessTokenAsync(cancellationToken);

      request.Headers.Authorization = new("Bearer", accessToken.Token);
    }

    return await base.SendAsync(request, cancellationToken);
  }

  /// <summary>
  /// Renews the osu! API v2 access token and stores it in the <see cref="accessToken"/>.
  /// </summary>
  private async Task RenewAccessTokenAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("The osu! API v2 access token has expired, refreshing...");

    try
    {
      HttpResponseMessage response = await base.SendAsync(new(HttpMethod.Post, $"https://osu.ppy.sh/oauth/token")
      {
        Content = new FormUrlEncodedContent(new Dictionary<string, string>()
        {
          ["client_id"] = options.Value.ClientId.ToString(),
          ["client_secret"] = options.Value.ClientSecret,
          ["grant_type"] = "client_credentials",
          ["scope"] = "public"
        })
      }, cancellationToken);

      string json = await response.Content.ReadAsStringAsync(cancellationToken);
      OsuAccessToken? token = JsonConvert.DeserializeObject<OsuAccessToken>(json) ?? throw new Exception("The access token response is null.");

      if (token.ErrorDescription is not null)
        throw new Exception(token.ErrorDescription);
      else if (response.StatusCode is HttpStatusCode.Unauthorized) // If not authorized but no error message provided
        throw new Exception("Unauthorized.");
      else if (token?.Token is null)
        throw new Exception("The access token is null.");
      else if (token?.ExpiresIn is null)
        throw new Exception("The expiration date is null.");

      accessToken.Renew(token.Token, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn.Value - 10));
      logger.LogInformation("The osu! API v2 access token has been updated and expires at {Date}.", accessToken.ExpiresAt);
    }
    catch (Exception ex)
    {
      logger.LogError("Failed to refresh the osu! API v2 access token: {Message}", ex.Message);
      throw new Exception($"Failed to refresh the osu! API v2 token.");
    }
  }
}