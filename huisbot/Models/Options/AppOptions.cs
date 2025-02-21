using System.ComponentModel.DataAnnotations;

namespace huisbot.Models.Options;

/// <summary>
/// Represents the top-level options in the appsettings for configuring the application.
/// </summary>
public class AppOptions
{
  /// <summary>
  /// The Discord bot token.
  /// </summary>
  [Required]
  public string BotToken { get; set; } = "";
}
