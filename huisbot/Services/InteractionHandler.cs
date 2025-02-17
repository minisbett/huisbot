using Discord;
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
public class InteractionHandler(DiscordSocketClient client, ILogger<InteractionHandler> logger, InteractionService service, IServiceProvider provider)
  : DiscordClientService(client, logger)
{
  protected override async Task ExecuteAsync(CancellationToken cts)
  {
    // Handle interactions with the bot client.
    Client.InteractionCreated += OnInteractionCreated;
    Client.SlashCommandExecuted += OnSlashCommandExecuted;

    // Add the modules in this assembly to the interaction service and wait for the bot client to be ready.
    await service.AddModulesAsync(Assembly.GetExecutingAssembly(), provider);
    await Client.WaitForReadyAsync(cts);

    // Register the commands in the added modules to all guilds.
    await service.RegisterCommandsGloballyAsync();

    Logger.LogInformation("{Modules} modules have been loaded.", service.Modules.Count);
    Logger.LogInformation("{Commands} commands have been registered globally.", service.SlashCommands.Count);
  }

  private async Task OnInteractionCreated(SocketInteraction interaction)
  {
    // Create a context for the interaction with the bot client and execute the command using the interaction service.
    SocketInteractionContext context = new(Client, interaction);
    await service.ExecuteCommandAsync(context, provider);
  }

  private Task OnSlashCommandExecuted(SocketSlashCommand command)
  {
    // Go through all slash command data options and combine them to an argument string.
    static string parse(IReadOnlyCollection<SocketSlashCommandDataOption> data, string str = "")
    {
      foreach (var i in data)
        str += " " + (i.Type == ApplicationCommandOptionType.SubCommand ? $"{i.Name}{parse(i.Options, str)}" : $"{i.Name}:{i.Value}");

      return str;
    }

    // Log the command execution.
    string guild = command.GuildId is null ? "Direct Message" : $"{Client.GetGuild(command.GuildId.Value)} ({command.GuildId})";
    string user = $"{command.User.Username} [{command.User.GlobalName}] ({command.User.Id})";
    string cmd = $"/{command.CommandName}{parse(command.Data.Options)}";
    Logger.LogInformation("Guild: {Guild}\n      User: {User}\n      Command: {Command}", guild, user, cmd);

    return Task.CompletedTask;
  }
}
