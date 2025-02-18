using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;
using Microsoft.Extensions.Configuration;

namespace huisbot.Modules;

/// <summary>
/// The interaction module for the alias group & add, remove and list subcommand, listing and modifying the different aliases.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[Group("alias", "Commands for adding, removing and listing aliases.")]
public class AliasGroupModule : InteractionModuleBase<SocketInteractionContext>
{
  [Group("beatmap", "Commands for adding, removing and listing beatmap aliases.")]
  public class BeatmapAliasCommandModule(IServiceProvider services, IConfiguration configuration, PersistenceService persistence)
    : ModuleBase(services, configuration)
  {
    [SlashCommand("list", "Lists all existing beatmap aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      // Return the list of beatmap aliases in an embed.
      await FollowupAsync(embed: Embeds.BeatmapAliases(await persistence.GetBeatmapAliasesAsync()));
    }

    [SlashCommand("add", "Adds a beatmap alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("beatmapId", "The ID of the beatmap.")] int beatmapId)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias already exists.
      BeatmapAlias? alias = await persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` already exists.\n[{alias.DisplayName}](https://osu.ppy.sh/b/{alias.BeatmapId})"));
        return;
      }

      // Get the beatmap.
      OsuBeatmap? beatmap = await GetBeatmapAsync(beatmapId.ToString());
      if (beatmap is null)
        return;

      // Add the beatmap alias.
      string displayName = $"{beatmap.Title} [{beatmap.Version}]";
      await persistence.AddBeatmapAliasAsync(aliasText, beatmapId, displayName);
      await FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` was successfully added.\n[{displayName}](https://osu.ppy.sh/b/{beatmapId})"));
    }

    [SlashCommand("remove", "Removes a beatmap alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The beatmap alias to remove.")] string aliasText)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias exists.
      BeatmapAlias? alias = await persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` does not exist."));
        return;
      }

      // Remove the beatmap alias.
      await persistence.RemoveBeatmapAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a beatmap alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The beatmap alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the beatmap alias.")] string newName)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the beatmap alias exists.
      BeatmapAlias? alias = await persistence.GetBeatmapAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{alias}` does not exist."));
        return;
      }

      // Check whether the new name is already taken.
      BeatmapAlias? _alias = await persistence.GetBeatmapAliasAsync(newName);
      if (_alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/b/{_alias.BeatmapId})"));
        return;
      }

      // Remove the beatmap alias and add the new one.
      await persistence.RemoveBeatmapAliasAsync(alias);
      await persistence.AddBeatmapAliasAsync(newName, alias.BeatmapId, alias.DisplayName);
      await base.FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }

  [Group("score", "Commands for adding, removing and listing score aliases.")]
  public class ScoreAliasCommandModule(IServiceProvider services, IConfiguration configuration, PersistenceService persistence)
    : ModuleBase(services, configuration)
  {
    [SlashCommand("list", "Lists all existing score aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      // Return the list of score aliases in an embed.
      await FollowupAsync(embed: Embeds.ScoreAliases(await persistence.GetScoreAliasesAsync()));
    }

    [SlashCommand("add", "Adds a score alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("scoreId", "The ID of the beatmap.")] long scoreId)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias already exists.
      ScoreAlias? alias = await persistence.GetScoreAliasAsync(aliasText);
      if (alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` already exists.\n[{alias.DisplayName}](https://osu.ppy.sh/scores/{alias.ScoreId})"));
        return;
      }

      // Get the score.
      OsuScore? score = await GetScoreAsync(scoreId.ToString());
      if (score is null)
        return;

      // Add the score alias. 
      string displayName = $"{score.User.Name} on {score.BeatmapSet.Title} [{score.Beatmap.Version}]";
      await persistence.AddScoreAliasAsync(aliasText, scoreId, displayName);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` was successfully added.\n[{displayName}](https://osu.ppy.sh/scores/osu/{scoreId})"));
    }

    [SlashCommand("remove", "Removes a score alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The score alias to remove.")] string aliasText)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias exists.
      ScoreAlias? alias = await persistence.GetScoreAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` does not exist."));
        return;
      }

      // Remove the score alias.
      await persistence.RemoveScoreAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a score alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The score alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the score alias.")] string newName)
    {
      await DeferAsync();

      // Make sure the user is part of the PP team.
      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      // Check whether the score alias exists.
      ScoreAlias? alias = await persistence.GetScoreAliasAsync(aliasText);
      if (alias is null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{alias}` does not exist."));
        return;
      }

      // Check whether the new name is already taken.
      ScoreAlias? _alias = await persistence.GetScoreAliasAsync(newName);
      if (_alias is not null)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/scores/osu/{_alias.ScoreId})"));
        return;
      }

      // Remove the score alias and add the new one.
      await persistence.RemoveScoreAliasAsync(alias);
      await persistence.AddScoreAliasAsync(newName, alias.ScoreId, alias.DisplayName);
      await base.FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }
}
