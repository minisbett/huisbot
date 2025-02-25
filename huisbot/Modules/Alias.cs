﻿using Discord;
using Discord.Interactions;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;

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
  public class BeatmapAliasCommandModule(IServiceProvider services) : ModuleBase(services)
  {
    [SlashCommand("list", "Lists all existing beatmap aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      await FollowupAsync(embed: Embeds.BeatmapAliases(await Persistence.GetBeatmapAliasesAsync()));
    }

    [SlashCommand("add", "Adds a beatmap alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("beatmapId", "The ID of the beatmap.")] int beatmapId)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetBeatmapAliasAsync(aliasText) is BeatmapAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"""
                                                 The beatmap alias `{aliasText}` already exists.
                                                 [{alias.DisplayName}](https://osu.ppy.sh/b/{alias.BeatmapId})
                                                 """));
        return;
      }

      if (await GetBeatmapAsync(beatmapId.ToString()) is not OsuBeatmap beatmap) return;

      string displayName = $"{beatmap.Set.Title} [{beatmap.Version}]";
      await Persistence.AddBeatmapAliasAsync(aliasText, beatmapId, displayName);
      await FollowupAsync(embed: Embeds.Success($"""
                                                 The beatmap alias `{aliasText}` was successfully added.
                                                 [{displayName}](https://osu.ppy.sh/b/{beatmapId})
                                                 """));
    }

    [SlashCommand("remove", "Removes a beatmap alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The beatmap alias to remove.")] string aliasText)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetBeatmapAliasAsync(aliasText) is not BeatmapAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` does not exist."));
        return;
      }

      await Persistence.RemoveBeatmapAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a beatmap alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The beatmap alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the beatmap alias.")] string newName)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetBeatmapAliasAsync(aliasText) is not BeatmapAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The beatmap alias `{aliasText}` does not exist."));
        return;
      }

      if (await Persistence.GetBeatmapAliasAsync(newName) is BeatmapAlias _alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/b/{_alias.BeatmapId})"));
        return;
      }

      await Persistence.RemoveBeatmapAliasAsync(alias);
      await Persistence.AddBeatmapAliasAsync(newName, alias.BeatmapId, alias.DisplayName);
      await base.FollowupAsync(embed: Embeds.Success($"The beatmap alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }

  [Group("score", "Commands for adding, removing and listing score aliases.")]
  public class ScoreAliasCommandModule(IServiceProvider services)
    : ModuleBase(services)
  {
    [SlashCommand("list", "Lists all existing score aliases.")]
    public async Task HandleListAsync()
    {
      await DeferAsync();

      await FollowupAsync(embed: Embeds.ScoreAliases(await Persistence.GetScoreAliasesAsync()));
    }

    [SlashCommand("add", "Adds a score alias.")]
    public async Task HandleAddAsync(
      [Summary("alias", "The alias text.")] string aliasText,
      [Summary("scoreId", "The ID of the beatmap.")] long scoreId)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetScoreAliasAsync(aliasText) is ScoreAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"""
                                                 The score alias `{aliasText}` already exists.
                                                 [{alias.DisplayName}](https://osu.ppy.sh/scores/{alias.ScoreId})
                                                 """));
        return;
      }

      if (await GetScoreAsync(scoreId.ToString()) is not OsuScore score) return;

      string displayName = $"{score.User.Name} on {score.BeatmapSet.Title} [{score.Beatmap.Version}]";
      await Persistence.AddScoreAliasAsync(aliasText, scoreId, displayName);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` was successfully added.\n[{displayName}](https://osu.ppy.sh/scores/osu/{scoreId})"));
    }

    [SlashCommand("remove", "Removes a score alias.")]
    public async Task HandleRemoveAsync(
      [Summary("alias", "The score alias to remove.")] string aliasText)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetScoreAliasAsync(aliasText) is not ScoreAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` does not exist."));
        return;
      }

      await Persistence.RemoveScoreAliasAsync(alias);
      await FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` was successfully removed."));
    }

    [SlashCommand("rename", "Renames a score alias.")]
    public async Task HandleRenameAsync(
      [Summary("alias", "The score alias to rename.")] string aliasText,
      [Summary("newName", "The new name of the score alias.")] string newName)
    {
      await DeferAsync();

      if (!IsPPTeam)
      {
        await FollowupAsync(embed: Embeds.NotPPTeam);
        return;
      }

      if (await Persistence.GetScoreAliasAsync(aliasText) is not ScoreAlias alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{aliasText}` does not exist."));
        return;
      }

      if (await Persistence.GetScoreAliasAsync(newName) is ScoreAlias _alias)
      {
        await FollowupAsync(embed: Embeds.Error($"The score alias `{newName}` already exists.\n[{_alias.DisplayName}](https://osu.ppy.sh/scores/osu/{_alias.ScoreId})"));
        return;
      }

      await Persistence.RemoveScoreAliasAsync(alias);
      await Persistence.AddScoreAliasAsync(newName, alias.ScoreId, alias.DisplayName);
      await base.FollowupAsync(embed: Embeds.Success($"The score alias `{aliasText}` has been renamed to `{newName}`."));
    }
  }
}
