using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace huisbot.Services;

/// <summary>
/// Handles interactions (slash commands, components, ...) with the application.
/// </summary>
public class InteractionHandler : DiscordClientService
{
  private readonly InteractionService _interactionService;
  private readonly IServiceProvider _provider;

  public InteractionHandler(DiscordSocketClient client, ILogger<InteractionHandler> logger, InteractionService service, IServiceProvider provider)
    : base(client, logger)
  {
    _interactionService = service;
    _provider = provider;
  }

  protected override async Task ExecuteAsync(CancellationToken cts)
  {
    // Handle interactions with the bot client.
    Client.InteractionCreated += OnInteractionCreated;

    // Add the modules in this assembly to the interaction service and wait for the bot client to be ready.
    await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _provider);
    await Client.WaitForReadyAsync(cts);

    // Register the commands in the added modules to all guilds.
    await _interactionService.RegisterCommandsGloballyAsync();

    Logger.LogInformation("{Modules} modules have been loaded.", _interactionService.Modules.Count);
    Logger.LogInformation("{Commands} commands have been registered globally.", _interactionService.SlashCommands.Count);
  }

  private async Task OnInteractionCreated(SocketInteraction interaction)
  {
    // Create a context for the interaction with the bot client and execute the command using the interaction service.
    SocketInteractionContext context = new SocketInteractionContext(Client, interaction);
    await _interactionService.ExecuteCommandAsync(context, _provider);
  }
}
