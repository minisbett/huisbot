using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Rest;
using Discord.WebSocket;
using huisbot.Helpers;
using Microsoft.Extensions.Logging;

namespace huisbot.Services;

/// <summary>
/// Provides with Discord bot-related information (eg. application information or emojis).
/// </summary>
public class DiscordService(DiscordSocketClient client, ILogger<DiscordService> logger) : DiscordClientService(client, logger)
{
  /// <summary>
  /// The application-specific emotes.
  /// </summary>
  public Emote[] ApplicationEmotes { get; private set; } = null!;

  /// <summary>
  /// The ID of the owner of the Discord bot application.
  /// </summary>
  public ulong BotOwnerId { get; private set; }

  /// <summary>
  /// The up-time of the bot client.
  /// </summary>
  public TimeSpan Uptime => _startTime == DateTime.MinValue ? TimeSpan.Zero : DateTime.UtcNow - _startTime;

  /// <summary>
  /// The date and time at which the bot client was deemed ready.
  /// </summary>
  private DateTime _startTime = DateTime.MinValue;

  protected override async Task ExecuteAsync(CancellationToken cts)
  {
    Embeds.Discord = this;
    await Client.WaitForReadyAsync(cts);
    _startTime = DateTime.UtcNow;

    // Gather some information once on startup.
    ApplicationEmotes = [.. await Client.GetApplicationEmotesAsync()];
    RestApplication app = await Client.GetApplicationInfoAsync();
    BotOwnerId = app.Team is null ? app.Owner.Id : app.Team.OwnerUserId;

    Logger.LogInformation("Fetched {Emotes} application emotes.", ApplicationEmotes.Length);
    Logger.LogInformation("Fetched bot owner ID: {Id}.", BotOwnerId);
  }

  /// <summary>
  /// Returns the guild and user installation counts of the bot application.
  /// </summary>
  public async Task<(int GuildInstalls, int UserInstalls)> GetInstallCountsAsync()
  {
    await Client.WaitForReadyAsync(new CancellationToken());
    RestApplication app = await Client.GetApplicationInfoAsync();
    return (app.ApproximateGuildCount ?? -1, app.ApproximateUserInstallCount ?? -1);
  }
}