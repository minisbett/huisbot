using Discord;
using Discord.Interactions;
using huisbot.Models.Utility;
using huisbot.Services;
using huisbot.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;

namespace huisbot.Modules.Utility;

/// <summary>
/// This module introduces a "csharprepl" command which allows the owner of the application to execute C# code.
/// Microsoft's script engine is used to execute the code, with a global context containing important objects.
/// </summary>
public class CSharpReplModule : InteractionModuleBase<SocketInteractionContext>
{
  /// <summary>
  /// A list of namespaces to import in the script context.
  /// </summary>
  private readonly string[] _imports = new string[]
  {
    "System",
    "System.Linq",
    "System.IO",
    "System.Collections.Generic",
    "System.Threading",
    "System.Threading.Tasks",
    "Microsoft.Extensions.Configuration",
    "Discord",
    "Discord.Rest",
    "Discord.WebSocket",
    "huisbot",
    "huisbot.Enums",
    "huisbot.Models",
    "huisbot.Models.Huis",
    "huisbot.Models.Osu",
    "huisbot.Models.Utility",
    "huisbot.Modules",
    "huisbot.Modules.Huis",
    "huisbot.Modules.Utility",
    "huisbot.Persistence",
    "huisbot.Services",
    "huisbot.Utils",
    "huisbot.Utils.Extensions"
  };

  /// <summary>
  /// The references of the entry assembly of the application. This will be loaded on command execution
  /// and the array will be maintained through-out the applications' lifespan. This list is used to
  /// let the script engine know which assemblies to load when executing the code.
  /// </summary>
  private static Assembly[]? _references = null;

  private readonly IServiceProvider _provider;
  private readonly IConfiguration _config;
  private readonly HuisApiService _huis;

  public CSharpReplModule(IServiceProvider provider, IConfiguration config, HuisApiService huis)
  {
    _provider = provider;
    _config = config;
    _huis = huis;
  }

  [SlashCommand("csharprepl", "Runs C# code in the runtime context of the bot client application.")]
  public async Task CSharpReplAsync(
    [Summary("code", "The C# code to execute.")] string code)
  {
    // Make sure that the user is the owner of the application.
    if (Context.User.Id != (await Context.Client.GetApplicationInfoAsync()).Owner.Id)
    {
      await RespondAsync(embed: Embeds.Error("Only the owner of the application is permitted to use this command."));
      return;
    }

    // If the code does not end with a semicolon, add one.
    if (!code.EndsWith(";"))
      code += ";";

    // If the references array has not been initialized yet, initialize it with all assemblies referenced by the entry assembly.
    if (_references is null)
    {
      AssemblyName[] refAssemblies = Assembly.GetEntryAssembly()!.GetReferencedAssemblies();
      Assembly[] references = refAssemblies.Select(Assembly.Load).Concat(new Assembly[] { Assembly.GetEntryAssembly()! }).ToArray();
      _references = references;
    }

    // Construct the script options using the loaded references and the specified namespaces to import.
    ScriptOptions options = ScriptOptions.Default.AddReferences(_references).AddImports(_imports);

    // Construct the script globals, which contains variables for the script to be accessable.
    ScriptGlobals globals = new ScriptGlobals()
    {
      Client = Context.Client,
      User = Context.User,
      Channel = Context.Channel,
      Guild = Context.Guild,
      ServiceProvider = _provider,
      Config = _config,
      Huis = _huis,
    };

    // Respond to the interaction because the script might take more than the 3 second timeout on interaction responses.
    await RespondAsync(embed: Embeds.Neutral("Executing code..."));

    ScriptState<object> state;
    try
    {
      // Try to run the specified code and save the resulting ScriptState object.
      state = await CSharpScript.RunAsync(code, options, globals);
    }
    catch (Exception ex)
    {
      // If an error occured, notify the user.
      await ModifyOriginalResponseAsync(msg => msg.Embed = Embeds.Error($"```cs\n{ex.Message}```"));
      return;
    }

    // If the resulting object is null, send a message indicating that the code has been executed.
    // This happens if the specified code does not return anything and simply executes some code.
    if (state.ReturnValue is null)
    {
      await ModifyOriginalResponseAsync(msg => msg.Embed = Embeds.Success("Action performed successfully."));

      return;
    }

    // Inspect the resulting object (or exception if one exists) and save the string representation.
    string str = Inspector.Inspect(state.Exception ?? state.ReturnValue);

    // As a safety measure, replace secrets from the config with a placeholder.
    foreach (string secret in new string[] { "BOT_TOKEN", "OSU_API_KEY", "HUIS_ONION_KEY" })
      str = str.Replace(_config.GetValue<string>(secret), "<censored>");

    // If the string representation is too long, send a file containing it.
    if (str.Length > 2000)
    {
      await ModifyOriginalResponseAsync(msg =>
      {
        msg.Embed = null;
        msg.Content = "The result was too long to be displayed in a message.";
        msg.Attachments = new Optional<IEnumerable<FileAttachment>>(new List<FileAttachment>
        {
          new FileAttachment(new MemoryStream(Encoding.UTF8.GetBytes(str)), "result.txt")
        });
      });

      return;
    }

    // Edit the original response and replace the content with the string representation.
    await ModifyOriginalResponseAsync(msg =>
    {
      msg.Embed = null;
      msg.Content = $"```\n{str}\n```";
    });
  }
}
