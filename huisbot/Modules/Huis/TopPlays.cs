using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using huisbot.Models.Osu;
using huisbot.Models.Persistence;
using huisbot.Services;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the topplays command, displaying the top plays of a user in a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class TopPlaysCommandModule(IServiceProvider services) : ModuleBase(services)
{
  /// <summary>
  /// Represents the cached values for providing pagination via Discord message components.
  /// This prevents those values from having to be fetched everytime the page is switched.
  /// </summary>
  private record PaginationCacheEntry(
    OsuUser User, HuisScore[] Scores, HuisScore[] SortedScores, HuisRework Rework, Sort Sort, string ScoreType);

  /// <summary>
  /// A dictionary of entries of cached values for providing pagination via Discord message components with their unique ID.
  /// </summary>
  private static readonly Dictionary<string, PaginationCacheEntry> _paginationCache = [];

  [SlashCommand("topplays", "Displays the top plays of you or the specified user in the specified rework.")]
  public async Task HandleScoreAsync(
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("user", "The osu! ID or name of the user. Optional, defaults to your linked osu! user.")] string? userId = null,
    [Summary("type", "The type of top scores to return (top scores, flashlight scores or pinned scores).")]
    [Choice("Top Scores", "topranks")] [Choice("Flashlight Scores", "flashlight")]
    [Choice("Pinned Scores", "pinned")] string scoreType = "topranks",
    [Summary("page", "The page of the scores. 1 page displays 10 scores.")][MinValue(1)] int page = 1,
    [Summary("sort", "The sorting for the scores. Defaults to sort by Local PP.")]
    [Autocomplete(typeof(ProfileScoresSortAutocomplete))] string sortId = "local_pp_desc",
    [Summary("filterTopRanks", "Bool whether only the top score on each beatmap should be returned. Defaults to true.")]
    bool filterTopRanks = true)
  {
    await DeferAsync();

    // If no user identifier was specified, try to get one from a link.
    if (userId is null)
      if (await GetOsuDiscordLinkAsync() is OsuDiscordLink link)
        userId = link.OsuId.ToString();
      else
        return;

    if (await GetSortAsync(sortId, Sort.ProfileScores) is not Sort sort) return;
    if (await GetReworkAsync(reworkId) is not HuisRework rework) return;
    if (await GetOsuUserAsync(userId) is not OsuUser user) return;
    if (await GetTopPlaysAsync(user, rework.Id, scoreType) is not HuisScore[] scores) return;

    // If filtering for top ranks is enabled, only include the best score on each beatmap.
    // For that, for every beatmap in those scores, pick the first occuring score with that beatmap when sorting by local PP. 
    if (filterTopRanks)
      scores = scores.Select(x => x.Beatmap.Id).Distinct()
                     .Select(id => scores.OrderByDescending(x => x.Values.LocalPP).First(x => x.Beatmap.Id == id))
                     .ToArray();

    // Apply the sorting to the scores, since this is done inside the browser on Huis and has no API parameter.
    Func<HuisScore, double> selector = sort.Code switch
    {
      "live_pp" => x => x.Values.LivePP,
      "pp_diff" => x => x.Values.LocalPP - x.Values.LivePP,
      _ => x => x.Values.LocalPP
    };
    HuisScore[] sortedScores = [.. sort.IsAscending ? scores.OrderBy(selector) : scores.OrderByDescending(selector)];

    // Cache the results and build a message component for pagination navigation.
    string cacheId = Guid.NewGuid().ToString();
    _paginationCache[cacheId] = new PaginationCacheEntry(user, scores, sortedScores, rework, sort, scoreType);
    int maxPage = (int)Math.Ceiling(scores.Length * 1d / EmbedService.SCORES_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"topplays:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"topplays:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    await FollowupAsync(embed: Embeds.TopPlays(user, scores, sortedScores, rework, sort, scoreType, page), components: builder.Build());
  }

  [ComponentInteraction("topplays:page:*,*")]
  public async Task HandlePageAsync(string cacheId, int page)
  {
    await DeferAsync();

    // Get the corresponding cache entry of the message.
    IUserMessage msg = (Context.Interaction as IComponentInteraction)!.Message;
    PaginationCacheEntry entry = _paginationCache[cacheId];

    // Re-build the message component for further pagination navigation.
    int maxPage = (int)Math.Ceiling(entry.Scores.Length * 1d / EmbedService.SCORES_PER_PAGE);
    ComponentBuilder builder = new ComponentBuilder()
      .WithButton("←", $"topplays:page:{cacheId},{page - 1}", ButtonStyle.Secondary, disabled: page == 1)
      .WithButton("→", $"topplays:page:{cacheId},{page + 1}", ButtonStyle.Secondary, disabled: page == maxPage);

    // Update the embed with the values of the requested page.
    await msg.ModifyAsync(x =>
    {
      x.Embed = Embeds.TopPlays(entry.User, entry.Scores, entry.SortedScores, entry.Rework, entry.Sort, entry.ScoreType, page);
      x.Components = builder.Build();
    });
  }

  /// <summary>
  /// Autocomplete for the sort parameter on the topplays command.
  /// </summary>
  private class ProfileScoresSortAutocomplete : AutocompleteHandler
  {
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction acInteraction,
      IParameterInfo pInfo, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(Sort.ProfileScores.Select(x => new AutocompleteResult(x.DisplayName, x.Id))));
  }
}
