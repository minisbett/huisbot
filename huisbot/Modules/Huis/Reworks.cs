﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Models.Huis;
using huisbot.Services;
using huisbot.Utilities.Discord;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the reworks command, displaying info about all reworks.
/// </summary>
public class ReworksCommandModule : ModuleBase
{
  public ReworksCommandModule(HuisApiService huis) : base(huis) { }

  [SlashCommand("reworks", "Outputs a list of all existing reworks.")]
  public async Task HandleAsync()
  {
    await DeferAsync();

    // Get all available reworks from the Huis API.
    HuisRework[]? reworks = await GetReworksAsync();
    if (reworks is null)
      return;

    // If the user does not have Onion-level authorization, remove Onion-level reworks.
    if (!await IsOnionAsync(Context))
      reworks = reworks.Where(x => !x.IsOnionLevel).ToArray();

    // Construct the component for selecting a rework.
    MessageComponent component = new ComponentBuilder()
      .WithSelectMenu(new SelectMenuBuilder()
        .WithCustomId("rework")
        .WithPlaceholder("Select a rework...")
        .WithMaxValues(1)
        .WithOptions(reworks.Select(x => new SelectMenuOptionBuilder(x.Name, x.Code, $"{x.Code} ({x.ReworkTypeString} )", null, false)).ToList()))
      .Build();

    // Show the live "rework" by default and add the select menu to the reply.
    await FollowupAsync(embed: Embeds.Rework(reworks.First(x => x.Code == "live")), components: component);
  }
}

/// <summary>
/// The interaction module for the rework select menu from the <see cref="ReworksCommandModule"/> command.
/// </summary>
public class ReworksComponentModule : ModuleBase
{
  public ReworksComponentModule(HuisApiService huis) : base(huis) { }

  /// <summary>
  /// Callback for interactions with the "rework" select menu from the <see cref="ReworksAsync"/> command.
  /// </summary>
  /// <returns></returns>
  [ComponentInteraction("rework")]
  public async Task HandleAsync(string code)
  {
    SocketMessageComponent interaction = (SocketMessageComponent)Context.Interaction;

    // Get all reworks and check whether the request was successful. If not, notify the user.
    HuisRework? rework = (await GetReworksAsync(false))?.FirstOrDefault(x => x.Code == code);
    if (rework is null)
    {
      await interaction.UpdateAsync(msg => msg.Embed = Embeds.InternalError("Failed to get the reworks from the Huis API."));
      return;
    }

    // Block this interaction if the selected rework is Onion-level and the user does not have Onion-level authorization.
    if (rework.IsOnionLevel && !await IsOnionAsync(Context))
    {
      await Context.Interaction.RespondAsync(embed: Embeds.NotOnion, ephemeral: true);
      return;
    }

    // Show the selected rework.
    await interaction.UpdateAsync(msg => msg.Embed = Embeds.Rework(rework));
  }
}