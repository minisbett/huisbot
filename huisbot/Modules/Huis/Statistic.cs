using Discord;
using Discord.Interactions;
using huisbot.Helpers;
using huisbot.Models.Huis;
using Microsoft.Extensions.Configuration;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the statistic command, displaying graphs for the top-statistics of a rework.
/// </summary>
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
public class StatisticCommandModule(IServiceProvider services, IConfiguration configuration) : ModuleBase(services, configuration)
{
  [SlashCommand("statistic", "Displays the specific top-statistic in the specified rework.")]
  public async Task HandleAsync(
    [Summary("statistic", "The top-statistic to display.")]
    [Choice("Top Scores", "topscores")] [Choice("Top Players", "topplayers")] string statisticId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("amount", "Amount of entries to include, up to 500. Default is 500.")][MinValue(1)][MaxValue(500)] int amount = 500)
  {
    await DeferAsync();
    string target = $"{statisticId.Substring(3, 1)}{statisticId[4..].TrimEnd('s')}"; // topscores or topplayers => Score or Player

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Exclude the live rework, as it has no statistics to show.
    if (rework.Id == HuisRework.LiveId)
    {
      await FollowupAsync(embed: Embeds.Error("The live rework has no statistics."));
      return;
    }

    // Get the statistic.
    HuisStatistic? statistic = await GetStatisticAsync(statisticId, rework.Id);
    if (statistic is null)
      return;

    // Configure the plot with a size of 1000x600 and it's colors.
    Plot liveLocal = new(1000, 600);
    liveLocal.Style(Color.FromArgb(63, 66, 66), Color.FromArgb(63, 66, 66), Color.FromArgb(57, 59, 59), null, null, Color.White);
    liveLocal.Title($"Top {target}s - Live vs {rework.Name} @ {DateTime.UtcNow.ToShortDateString()} " +
                    $"{DateTime.UtcNow.ToLongTimeString()} (commit {rework.Commit})");

    // Configure the left, bottom and right axis.
    liveLocal.LeftAxis.Label("Performance Points");
    liveLocal.LeftAxis.Color(Color.LightGray);
    liveLocal.BottomAxis.Label($"No. # Top {target}");
    liveLocal.BottomAxis.Color(Color.LightGray);
    liveLocal.RightAxis.Label("Difference");
    liveLocal.RightAxis.Color(Color.LightGray);
    liveLocal.RightAxis.Ticks(true);

    // Configure the legend.
    Legend legend = liveLocal.Legend(true, Alignment.UpperCenter);
    legend.Orientation = Orientation.Horizontal;
    legend.FillColor = Color.FromArgb(63, 66, 66);
    legend.FontColor = Color.White;
    legend.OutlineColor = Color.Transparent;
    legend.ShadowColor = Color.Transparent;

    // Get the old, new and difference values and limit them to the amount requested.
    double[] oldValues = statistic.Old!.Take(amount).ToArray();
    double[] newValues = statistic.New!.Take(amount).ToArray();
    double[] difference = statistic.Difference!.Take(amount).ToArray();
    double[] xs = Enumerable.Range(0, oldValues.Length).Select(x => (double)x).ToArray();

    // Add the scatter lines of the old and new values and the difference.
    ScatterPlot diff = liveLocal.AddScatterLines(xs, difference, Color.FromArgb(103, 106, 106), 1, LineStyle.Solid, "Difference");
    liveLocal.AddScatterLines(xs, oldValues, Color.FromArgb(244, 122, 31), 2, LineStyle.Solid, "Live");
    liveLocal.AddScatterLines(xs, newValues, Color.FromArgb(0, 124, 195), 2, LineStyle.Solid, "Local");
    diff.YAxisIndex = liveLocal.RightAxis.AxisIndex; // Make sure the difference is plotted on the right axis.

    // Render the plot to a bitmap and send it.
    using MemoryStream ms = new();
    liveLocal.Render().Save(ms, ImageFormat.Png);
    await FollowupWithFileAsync(ms, "plot.png");
  }
}
