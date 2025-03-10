﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using huisbot.Persistence;
using huisbot.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using System.Reflection;
using System.Text;

namespace huisbot.Modules;

/// <summary>
/// This module introduces a "csharprepl" command which allows the owner of the application to execute C# code.
/// Microsoft's script engine is used to execute the code, with a global context containing important objects.
/// </summary>

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
#pragma warning disable CS9107
public class CSharpReplCommandModule(IServiceProvider services, Database database) : ModuleBase(services)
#pragma warning restore CS9107
{
  /// <summary>
  /// A list of namespaces to import in the script context.
  /// </summary>
  private readonly string[] _imports =
  [
    "System",
    "System.Linq",
    "System.IO",
    "System.Threading",
    "System.Threading.Tasks",
    "System.Collections.Generic",
    "Microsoft.EntityFrameworkCore",
    "Microsoft.Extensions.Configuration",
    "Microsoft.Extensions.DependencyInjection",
    "Discord",
    "Discord.Rest",
    "Discord.WebSocket",
    "huisbot",
    "huisbot.Helpers",
    "huisbot.Models",
    "huisbot.Models.Huis",
    "huisbot.Models.Osu",
    "huisbot.Models.Persistence",
    "huisbot.Modules",
    "huisbot.Modules.Huis",
    "huisbot.Modules.Miscellaneous",
    "huisbot.Persistence",
    "huisbot.Services",
  ];

  /// <summary>
  /// The references of the entry assembly of the application. This will be loaded on command execution
  /// and the array will be maintained through-out the applications' lifespan. This list is used to
  /// let the script engine know which assemblies to load when executing the code.
  /// </summary>
  private static readonly Assembly[] _references = null!;

  static CSharpReplCommandModule()
  {
    // Load all assemblies referenced by the entry assembly once on program start (static constructor).
    AssemblyName[] refAssemblies = Assembly.GetEntryAssembly()!.GetReferencedAssemblies();
    Assembly[] references = refAssemblies.Select(Assembly.Load).Concat([Assembly.GetEntryAssembly()!]).ToArray();
    _references = references;
  }

  [SlashCommand("csharprepl", "Runs C# code in the runtime context of the bot client application.")]
  public async Task CSharpReplAsync(
    [Summary("code", "The C# code to execute.")] string code)
  {
    await DeferAsync();

    // Ensure that the user is the owner of the application.
    if (Context.User.Id != Discord.BotOwnerId)
    {
      await FollowupAsync(embed: Embeds.Error("Only the owner of the application is permitted to use this command."));
      return;
    }

    if (!code.EndsWith(';'))
      code += ";";

    ScriptOptions options = ScriptOptions.Default.AddReferences(_references).AddImports(_imports);
    ScriptGlobals globals = new()
    {
      Client = Context.Client,
      User = Context.User,
      Channel = Context.Channel,
      Guild = Context.Guild,
      Services = services,
      OsuApi = OsuApi,
      HuisApi = HuisApi,
      Database = database
    };

    await FollowupAsync(embed: Embeds.Neutral("Executing code..."));

    ScriptState<object> state;
    try
    {
      state = await CSharpScript.RunAsync(code, options, globals);
    }
    catch (Exception ex)
    {
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

    string str = Inspect(state.Exception ?? state.ReturnValue);

    // If the string representation is too long, send a file containing it.
    if (str.Length > 2000 - 8 /* ```\n\n``` */)
    {
      await ModifyOriginalResponseAsync(msg =>
      {
        msg.Embed = null;
        msg.Content = "The result was too long to be displayed in a message.";
        msg.Attachments = new Optional<IEnumerable<FileAttachment>>(
        [
          new FileAttachment(new MemoryStream(Encoding.UTF8.GetBytes(str)), "result.txt")
        ]);
      });

      return;
    }

    await ModifyOriginalResponseAsync(msg =>
    {
      msg.Embed = null;
      msg.Content = $"```\n{str}\n```";
    });
  }

  /// <summary>
  /// Inspects the specified object and returns a string representation of it.<br/>
  /// Source: <a href="https://github.com/discord-net/Discord.Net/blob/0f37677c59d84061b1df2c740f2494c1bbb29a30/samples/idn/Inspector.cs"/>
  ///         (with minor modifications)
  /// </summary>
  /// <param name="value">The object to inspect.</param>
  /// <returns>A string representation of the specified object.</returns>
  public static string Inspect(object value)
  {
    static string InspectProperty(object obj)
    {
      if (obj == null)
        return "null";

      Type type = obj.GetType();

      // Check whether a DebuggerDisplay property is present on the type and if so, use it for a string representation.
      PropertyInfo? debuggerDisplay = type.GetProperty("DebuggerDisplay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (debuggerDisplay != null)
        return debuggerDisplay.GetValue(obj)!.ToString()!;

      // Otherwise, check whether a ToString method not inherited from the object type is present on the type and if so, use it for
      // a string representation. It means that the type overrides the ToString method and thus has a custom string representation.
      MethodInfo? toString = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          .Where(x => x.Name == "ToString" && x.DeclaringType != typeof(object)).FirstOrDefault();
      if (toString != null)
        return obj.ToString()!;

      // Otherwise, check whether a Count property is present and if so, return the amount of items in the collection.
      PropertyInfo? count = type.GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (count != null)
        return $"[{count.GetValue(obj)} Items]";

      // If none of the above is present, return the default string representation of the object.
      return obj.ToString()!;
    }

    StringBuilder builder = new();

    if (value != null)
    {
      // Get the type of the object and append it to the string builder.
      builder.AppendLine($"[{value.GetType().Namespace}.{value.GetType().Name}]");

      // Inspect the properties of the object and append them to the string builder.
      builder.AppendLine($"{InspectProperty(value)}");

      // If the object is an enumerable and not blacklisted, append each item in it's inspected version to the string builder.
      // Certain types are blacklisted to prevent useless listing of items, for example when inspecting strings which are IEnumerable<char>.
      if (value is IEnumerable enumerable)
      {
        if (value is not string)
        {
          // Cast the enumerable to an array to prevent multiple enumerations.
          object[] items = enumerable.Cast<object>().ToArray();

          // If the array is not empty, append each item in it's inspected version to the string builder.
          if (items.Length > 0)
          {
            builder.AppendLine();
            foreach (object? item in items)
              builder.AppendLine($"- {InspectProperty(item)}");
          }
        }
      }
      // If the object is not an enumerable, inspect each property of the object.
      else
      {
        // Get all non-inherited properties of the object.
        PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetIndexParameters().Length == 0).ToArray();

        // If properties are present, append them to the string builder.
        if (properties.Length > 0)
        {
          builder.AppendLine();
          foreach (PropertyInfo property in properties)
            builder.AppendLine($"{property.Name.PadRight(properties.Max(x => x.Name.Length), ' ')} {InspectProperty(property.GetValue(value)!)}");
        }
      }
    }
    // If the value is null, return "null".
    else
      builder.AppendLine("null");

    // Return the built string.
    return builder.ToString();
  }

  /// <summary>
  /// Represents the globals that are available to the script.
  /// </summary>
  public class ScriptGlobals
  {
    /// <summary>
    /// The bot client.
    /// </summary>
    public required DiscordSocketClient Client { get; init; }

    /// <summary>
    /// The user that executed the command.
    /// </summary>
    public required SocketUser User { get; init; }

    /// <summary>
    /// The channel in which the command was executed.
    /// </summary>
    public required ISocketMessageChannel Channel { get; init; }

    /// <summary>
    /// The guild in which the command was executed.
    /// </summary>
    public required SocketGuild Guild { get; init; }

    /// <summary>
    /// The service provider of the application.
    /// </summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>
    /// The osu! api service.
    /// </summary>
    public required OsuApiService OsuApi { get; init; }

    /// <summary>
    /// The huis api service.
    /// </summary>
    public required HuisApiService HuisApi { get; init; }

    /// <summary>
    /// The EF MySQL database object.
    /// </summary>
    public required Database Database { get; init; }
  }
}