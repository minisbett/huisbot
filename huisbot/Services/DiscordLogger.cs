using Discord;
using Discord.WebSocket;
using huisbot.Models.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace huisbot.Services;

internal class DiscordLogger(IServiceProvider services) : ILoggerProvider, ILogger
{
  private string _categoryName = null!;

  public ILogger CreateLogger(string categoryName)
  {
    _categoryName = categoryName;
    return this;
  }

  public void Dispose() { }

  IDisposable ILogger.BeginScope<TState>(TState state) => null!;

  public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
  {
    if(!IsEnabled(logLevel))
      return;

    EmbedService embeds = services.GetRequiredService<EmbedService>();
    DiscordSocketClient discordClient = services.GetRequiredService<DiscordSocketClient>();
    IOptions<DiscordIdOptions> options = services.GetRequiredService<IOptions<DiscordIdOptions>>();

    SocketTextChannel? channel = discordClient.GetGuild(options.Value.LoggingGuild)?.GetTextChannel(options.Value.LoggingChannel);
    if (channel is null)
      return;

    Embed embed = embeds.Log(logLevel, _categoryName, formatter(state, exception));
    _ = Task.Run(async () => await channel.SendMessageAsync(embed: embed));
  }
}
