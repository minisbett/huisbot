namespace huisbot.Persistence.Caching;

/// <summary>
/// A wrapper class around a string in order to use it as a cache key.
/// </summary>
public class StringCacheKey : ICacheableKey
{
  /// <inheritdoc/>
  public string CacheUID { get; set; } = "";

  /// <summary>
  /// Creates a new <see cref="StringCacheKey"/> with the specified string.
  /// </summary>
  /// <param name="str">The string.</param>
  public StringCacheKey(string str)
  {
    CacheUID = str;
  }
}