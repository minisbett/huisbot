using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using huisbot.Helpers;
using huisbot.Models.Options;
using huisbot.Persistence;
using huisbot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace huisbot;

// TODO: (after notfoundor<> replaced with results) proper internal error handling with error logging channel
// TODO: refactor pagination sometime

public class Program
{
  /// <summary>
  /// The version of the application.
  /// </summary>
  public const string VERSION = "2.8.2";

  public static async Task Main(string[] args)
  {
    // Run the host in a try-catch block to catch any unhandled exceptions.
    try
    {
      await MainAsync(args);
    }
    catch (Exception ex) when (ex is not HostAbortedException)
    {
      Environment.ExitCode = 727;
      Console.WriteLine(ex);
    }
  }

  public static async Task MainAsync(string[] args)
  {
    // Ensure a consistent culture for parsing and formatting.
    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

    IHost host = Host.CreateDefaultBuilder()
#if DEVELOPMENT
      .UseEnvironment("Development")
#endif
      .ConfigureLogging(logging =>
      {
        logging.AddSimpleConsole(options =>
        {
          options.TimestampFormat = "[HH:mm:ss] ";
          options.UseUtcTimestamp = true;
          options.ColorBehavior = LoggerColorBehavior.Enabled;
        });

        // Exclude HttpClients and DB commands from logging, as they spam the logs.
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
      })

      .ConfigureServices((context, services) =>
      {
        // Register and immediately validate the options from the appsettings.json.
        services.AddOptionsWithValidateOnStart<AppOptions>()
          .BindConfiguration("")
          .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<OsuApiOptions>()
          .BindConfiguration("Osu")
          .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<HuisApiOptions>()
          .BindConfiguration("Huis")
          .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<DiscordIdOptions>()
          .BindConfiguration("DiscordIds")
          .ValidateDataAnnotations();

        services.AddDiscordHost((config, services) =>
        {
          config.SocketConfig = new DiscordSocketConfig()
          {
            LogLevel = LogSeverity.Verbose,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 125,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
          };

          config.Token = services.GetRequiredService<IOptions<AppOptions>>().Value.BotToken;
        });

        services.AddInteractionService((config, _) =>
        {
          config.LogLevel = LogSeverity.Verbose;
          config.UseCompiledLambda = true;
        });

        services.AddHostedService<InteractionHandler>();

        services.AddSingleton<ILoggerProvider, DiscordLogger>();

        // Add the Discord service, responsible for retrieving Discord-related information.
        // The service is first registered as a singleton as hosted services themselves cannot be injected.
        services.AddSingleton<DiscordService>();
        services.AddHostedService(services => services.GetRequiredService<DiscordService>());

        // Register all services (API, database, ...)
        services.AddSingleton<EmbedService>();
        services.AddScoped<OsuApiService>();
        services.AddScoped<HuisApiService>();
        services.AddScoped<PersistenceService>();
        services.AddScoped<CachingService>();

        // Add an http client for communicating with the Huis API.
        services.AddHttpClient("huisapi", (services, client) =>
        {
          client.BaseAddress = new Uri("https://api.pp.huismetbenen.nl/");
          client.DefaultRequestHeaders.Add("User-Agent", $"huisbot/{VERSION}");

          // The onion key is optional, allowing access to onion-level reworks.
          string onionKey = services.GetRequiredService<IOptions<HuisApiOptions>>().Value.OnionKey;
          if (onionKey != "")
            client.DefaultRequestHeaders.Add("x-onion-key", onionKey);
        });

        // Add an http client for communicating with the osu! API. 
        services.AddTransient<OsuOAuthDelegatingHandler>();
        services.AddKeyedSingleton<ExpiringAccessToken>(nameof(OsuApiService));
        services.AddHttpClient(nameof(OsuApiService), (services, client) =>
        {
          client.BaseAddress = new Uri("https://osu.ppy.sh/");
          client.DefaultRequestHeaders.Add("User-Agent", $"huisbot/{VERSION}");
          client.DefaultRequestHeaders.Add("x-api-version", "20220705");
        }).AddHttpMessageHandler<OsuOAuthDelegatingHandler>();

        // Register our data context for accessing our database.
        services.AddDbContext<Database>(options =>
        {
          options.UseSqlite("Data Source=database.db");
          options.UseSnakeCaseNamingConvention();
        });
      })
      .Build();

    // Run migrations on the database.
    using (IServiceScope scope = host.Services.CreateScope())
      await scope.ServiceProvider.GetRequiredService<Database>().Database.MigrateAsync();

    // Run the application.
    await host.RunAsync();
  }
}
