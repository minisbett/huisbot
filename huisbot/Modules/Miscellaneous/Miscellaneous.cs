using Discord.Interactions;
using huisbot.Services;

namespace huisbot.Modules.Miscellaneous;

/// <summary>
/// The partial interaction module for the misc group & various subcommands, providing miscellaneous utility commands.
/// </summary>
[Group("misc", "Miscellaneous utility commands.")]
public partial class MiscellaneousCommandModule(HuisApiService huis, OsuApiService osu, PersistenceService persistence)
  : ModuleBase(huis, osu, persistence);