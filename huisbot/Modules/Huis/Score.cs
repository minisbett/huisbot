using Discord;
using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the score command, calculating the existing score of a user.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
[Group("score", "Calculates a score in a rework based on the specified parameters.")]
public class ScoreCommandModule(IServiceProvider services) : ModuleBase(services)
{
  private async Task HandleAsync(string reworkId, string? refReworkId, Task<OsuScore?> scoreTask, OsuUser? user = null)
  {
    refReworkId ??= HuisRework.LiveId.ToString();

    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetReworkAsync(refReworkId) is not HuisRework refRework) return;
    if (await scoreTask is not OsuScore score) return;
    if ((user ??= await GetOsuUserAsync(score.User.Id.ToString())) is null) return; // Only fetch if not passed to this method already
    if (await GetBeatmapAsync(score.Beatmap.Id.ToString()) is not OsuBeatmap beatmap) return;

    IUserMessage msg = await FollowupAsync(embed: Embeds.Calculating(rework, rework == refRework ? null : refRework, false));

    if (await CalculateScoreAsync(new(beatmap, rework, score.Mods, score.MaxCombo, score.Statistics)) is not HuisCalculationResponse localScore) return;

    // If the requested rework is the same as the reference, set the scores equal and don't perform another calculation.
    HuisCalculationResponse? refScore = localScore;
    if (rework != refRework)
    {
      await ModifyOriginalResponseAsync(x => x.Embed = Embeds.Calculating(rework, refRework, true));

      if ((refScore = await CalculateScoreAsync(new(beatmap, refRework, score.Mods, score.MaxCombo, score.Statistics))) is null) return;
    }

    await ModifyOriginalResponseAsync(x => x.Embed = Embeds.CalculatedScore(localScore, refScore, rework, refRework, beatmap, score, user));
  }

  [SlashCommand("id", "Calculates a score in a rework based on the specified ID.")]
  public async Task HandleIdAsync(
    [Summary("score", "The ID, URL or alias of the score.")] string scoreId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

    await HandleAsync(reworkId, referenceReworkId, GetScoreAsync(scoreId));
  }

  [SlashCommand("best", "Calculates the X-th best score of the specified user in a rework.")]
  public async Task HandleBestAsync(
    [Summary("user", "The osu! ID or name of the user. Optional, defaults to your linked osu! user.")] string? userId = null,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("index", "The index of the score. Defaults to 1.")][MinValue(1)][MaxValue(100)] int index = 1,
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

    // If no user identifier was specified, try to get one from an osu-discord link.
    if (userId is null)
      if (await GetOsuDiscordLinkAsync() is OsuDiscordLink link)
        userId = link.OsuId.ToString();
      else
        return;

    if (await GetOsuUserAsync(userId) is not OsuUser user) return;

    await HandleAsync(reworkId, referenceReworkId, GetUserScoreAsync(user.Id, index, ScoreType.Best), user);
  }

  [SlashCommand("recent", "Calculates the X-th recent score of you or the specified user in a rework.")]
  public async Task HandleRecentAsync(
    [Summary("player", "The osu! ID or name of the player. Optional, defaults to your linked osu! user.")] string? userId = null,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId = "master",
    [Summary("index", "The index of the score. Defaults to 1.")][MinValue(1)][MaxValue(100)] int index = 1,
    [Summary("referenceRework", "The reference rework to compare the score to. Defaults to the live PP system.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string? referenceReworkId = null)
  {
    await DeferAsync();

    // If no user identifier was specified, try to get one from an osu-discord link.
    if (userId is null)
      if (await GetOsuDiscordLinkAsync() is OsuDiscordLink link)
        userId = link.OsuId.ToString();
      else
        return;

    if (await GetOsuUserAsync(userId) is not OsuUser user) return;

    await HandleAsync(reworkId, referenceReworkId, GetUserScoreAsync(user.Id, index, ScoreType.Recent), user);
  }
}