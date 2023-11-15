using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using dotenv.net;
using huisbot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using System.Globalization;

public class Program
{
  /// <summary>
  /// The version of the application.
  /// </summary>
  public const string VERSION = "1.0.0";

  public static async Task Main(string[] args)
  {
    // Load the .env file.
    DotEnv.Load();

    // Ensure a consistent culture for parsing & formatting.
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

    // Build the generic host.
    IHost host = Host.CreateDefaultBuilder()
      // Configure the host to use environment variables for the config.
      .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())

      // Configure the Discord host (bot token, log level, bot behavior etc.)
      .ConfigureDiscordHost((context, config) =>
      {
        config.SocketConfig = new DiscordSocketConfig()
        {
          LogLevel = LogSeverity.Verbose,
          AlwaysDownloadUsers = true,
          MessageCacheSize = 100,
          GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        config.Token = context.Configuration.GetValue<string>("BOT_TOKEN")
          ?? throw new InvalidOperationException("The environment variable 'BOT_TOKEN' is not set.");
      })

      // Configure Discord.NET's interaction service.
      .UseInteractionService((context, config) =>
      {
        config.LogLevel = LogSeverity.Verbose;
        config.UseCompiledLambda = true;
      })
      
      // Configure further services necessary in the application's lifetime.
      .ConfigureServices((context, services) =>
      {
        // Add the handler for Discord interactions.
        services.AddHostedService<InteractionHandler>();

        // Add the Huis API service for communicating with the Huis API.
        services.AddSingleton<HuisApiService>();

        // Add an http client for communicating with the Huis API.
        services.AddHttpClient("huisapi", client =>
        {
          client.BaseAddress = new Uri("https://pp-api.huismetbenen.nl/");
          client.DefaultRequestHeaders.Add("User-Agent", $"huisbot/{VERSION}");
        });
      })
      .Build();

    // Run the host.
    await host.RunAsync();
  }
}