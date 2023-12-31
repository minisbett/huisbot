using Discord.Interactions;
using huisbot.Services;

namespace huisbot.Modules.Utility.Miscellaneous;

/// <summary>
/// The partial interaction module for the misc group & various subcommands, providing miscellaneous utility commands.
/// </summary>
[Group("misc", "Miscellaneous utility commands.")]
public partial class MiscellaneousCommandModule : ModuleBase
{
  public MiscellaneousCommandModule(OsuApiService osu, PersistenceService persistence) : base(osu: osu, persistence: persistence) { }
}
