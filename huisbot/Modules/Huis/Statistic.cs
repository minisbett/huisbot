using Discord.Interactions;
using huisbot.Models.Huis;
using huisbot.Modules.Autocompletes;
using huisbot.Services;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace huisbot.Modules.Huis;

/// <summary>
/// The interaction module for the statistic command, displaying graphs for the top-statistics of a rework.
/// </summary>
public class StatisticCommandModule : ModuleBase
{
  public StatisticCommandModule(HuisApiService huis) : base(huis) { }

  [SlashCommand("statistic", "Displays the specific top-statistic in the specified rework.")]
  public async Task HandleAsync(
    [Summary("statistic", "The top-statistic to display.")]
    [Choice("Top Scores", "topscores")] [Choice("Top Players", "topplayers")] string statisticId,
    [Summary("rework", "An identifier for the rework. This can be it's ID, internal code or autocompleted name.")]
    [Autocomplete(typeof(ReworkAutocompleteHandler))] string reworkId,
    [Summary("amount", "Amount of entries to include, up to 500. Default is 500.") ][MinValue(1)] [MaxValue(500)] int amount = 500)
  {
    await DeferAsync();
    bool topscores = statisticId == "topscores"; // True = topscores, False = topplayers

    // Get the matching rework for the specified rework identifier.
    HuisRework? rework = await GetReworkAsync(reworkId);
    if (rework is null)
      return;

    // Get the statistic.
    HuisStatistic? statistic = await GetStatisticAsync(statisticId, rework.Id);
    if (statistic is null)
      return;

    // Configure the plot with a size of 1000x600, colors, a legend in the upper center, the axis labels and an extra axis on the right for the difference.
    Plot liveLocal = new Plot(1000, 600);
    liveLocal.Style(Color.FromArgb(63, 66, 66), Color.FromArgb(63, 66, 66), Color.FromArgb(57, 59, 59), null, null, Color.White);
    liveLocal.Title($"{(topscores ? "Top Scores" : "Top Players")} - Live vs {rework.Name} @ {DateTime.UtcNow.ToShortDateString()} " +
                    $"{DateTime.UtcNow.ToLongTimeString()} (commit {rework.Commit})");
    liveLocal.LeftAxis.Label("Performance Points");
    liveLocal.LeftAxis.Color(Color.LightGray);
    liveLocal.BottomAxis.Label($"No. # Top {(topscores ? "Score" : "Player")}");
    liveLocal.BottomAxis.Color(Color.LightGray);
    liveLocal.AddAxis(Edge.Right, 2, "Difference", Color.LightGray);
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

    // Add the scatter lines of the old and new values and the difference.
    double[] xs = Enumerable.Range(0, oldValues.Length).Select(x => (double)x).ToArray();
    ScatterPlot diff = liveLocal.AddScatterLines(xs, difference, Color.FromArgb(103, 106, 106), 1, LineStyle.Solid, "Difference");
    diff.YAxisIndex = liveLocal.RightAxis.AxisIndex; // Make sure the difference is plotted on the right axis.
    liveLocal.AddScatterLines(xs, oldValues, Color.FromArgb(244, 122, 31), 2, LineStyle.Solid, "Live");
    liveLocal.AddScatterLines(xs, newValues, Color.FromArgb(0, 124, 195), 2, LineStyle.Solid, "Local");

    // Render the plot to a bitmap and send it.
    using MemoryStream ms = new MemoryStream();
    liveLocal.Render().Save(ms, ImageFormat.Png);
    await FollowupWithFileAsync(ms, "plot.png");
  }
}
