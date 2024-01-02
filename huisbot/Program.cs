using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using dotenv.net;
using huisbot.Persistence;
using huisbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Globalization;

public class Program
{
  /// <summary>
  /// The version of the application.
  /// </summary>
  public const string VERSION = "1.9.0";

  /// <summary>
  /// The startup time of the application.
  /// </summary>
  public static readonly DateTime STARTUP_TIME;

  static Program()
  {
    STARTUP_TIME = DateTime.UtcNow;
  }

  public static async Task Main(string[] args)
  {
    // Run the host in a try-catch block to catch any unhandled exceptions.
    try
    {
      await MainAsync(args);
    }
    catch (Exception ex)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(ex);
      Console.ForegroundColor = ConsoleColor.Gray;
      Environment.ExitCode = 727;
    }
  }

  public static async Task MainAsync(string[] args)
  {
    // Load the .env file. (Only useful when debugging locally, not when running it via e.g. Docker)
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

      // Configure the logging to have timestamps.
      .ConfigureLogging(logging =>
      {
        logging.AddSimpleConsole(options =>
        {
          options.TimestampFormat = "[HH:mm:ss] ";
          options.UseUtcTimestamp = true;
          options.ColorBehavior = LoggerColorBehavior.Enabled;
        });
      })

      // Configure the Discord host (bot token, log level, bot behavior etc.)
      .ConfigureDiscordHost((context, config) =>
      {
        config.SocketConfig = new DiscordSocketConfig()
        {
          LogLevel = LogSeverity.Verbose,
          AlwaysDownloadUsers = true,
          MessageCacheSize = 100,
          GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
        };

        config.Token = context.Configuration["BOT_TOKEN"]
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

        // Add the osu! API service for communicating with the osu! API.
        services.AddSingleton<OsuApiService>();

        // Add the Huis API service for communicating with the Huis API.
        services.AddSingleton<HuisApiService>();

        // Register the persistence service, responsible for providing logic for accessing the persistence database.
        services.AddScoped<PersistenceService>();

        // Add an http client for communicating with the Huis API.
        services.AddHttpClient("huisapi", client =>
        {
          client.BaseAddress = new Uri("https://pp-api.huismetbenen.nl/");
          client.DefaultRequestHeaders.Add("User-Agent", $"huisbot/{VERSION}");

          // The onion key is optional, allowing access to onion-level reworks.
          string? onionKey = context.Configuration["HUIS_ONION_KEY"];
          if (onionKey is not null)
            client.DefaultRequestHeaders.Add("x-onion-key", onionKey);
        });

        // Add an http client for communicating with the osu! API.
        services.AddHttpClient("osuapi", client =>
        {
          client.BaseAddress = new Uri("https://osu.ppy.sh/");
          client.DefaultRequestHeaders.Add("User-Agent", $"huisbot/{VERSION}");
        });

        // Register our data context for accessing our database.
        services.AddDbContext<Database>(options =>
        {
          options.UseSqlite("Data Source=database.db");
          options.UseSnakeCaseNamingConvention();
        });
      })
      .Build();

    // Run migrations on the database.
    await host.Services.GetRequiredService<Database>().Database.MigrateAsync();

    // Ensure that all APIs are available.
    OsuApiService osu = host.Services.GetRequiredService<OsuApiService>();
    HuisApiService huis = host.Services.GetRequiredService<HuisApiService>();
    if (!await osu.IsV1AvailableAsync())
      throw new Exception("The osu! v1 API was deemed unavailable at startup.");
    if (!await osu.IsV2AvailableAsync())
      throw new Exception("The osu! v2 API was deemed unavailable at startup.");
    if (!await huis.IsAvailableAsync())
      throw new Exception("The Huis API was deemed unavailable at startup.");


    // Try to initially load the reworks for a faster use after startup.
    HuisApiService huisApi = host.Services.GetRequiredService<HuisApiService>();
    await huisApi.GetReworksAsync();


    // Run the host.
    await host.RunAsync();
  }
}