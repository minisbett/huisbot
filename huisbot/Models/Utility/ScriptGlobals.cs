using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using huisbot.Services;

namespace huisbot.Models.Utility;

/// <summary>
/// Represents the globals that are available to the script executed through the <see cref="CSharpRepl"/> module.
/// </summary>
public class ScriptGlobals
{
  /// <summary>
  /// The bot client.
  /// </summary>
  public required DiscordSocketClient Client { get; init; }

  /// <summary>
  /// The user that executed the command.
  /// </summary>
  public required SocketUser User { get; init; }

  /// <summary>
  /// The channel in which the command was executed.
  /// </summary>
  public required ISocketMessageChannel Channel { get; init; }

  /// <summary>
  /// The guild in which the command was executed.
  /// </summary>
  public required SocketGuild Guild { get; init; }

  /// <summary>
  /// The service provider of the application.
  /// </summary>
  public required IServiceProvider ServiceProvider { get; init; }

  /// <summary>
  /// The configuration of the application.
  /// </summary>
  public required IConfiguration Config { get; init; }

  /// <summary>
  /// The osu! API service.
  /// </summary>
  public required OsuApiService Osu { get; init; }

  /// <summary>
  /// The Huis API service.
  /// </summary>
  public required HuisApiService Huis { get; init; }
}
