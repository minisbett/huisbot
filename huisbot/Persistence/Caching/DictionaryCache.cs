namespace huisbot.Persistence.Caching;

/// <summary>
/// A dictionary cache, caching non-expiring values by their unique ke.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">Thet ype of the value.</typeparam>
public class DictionaryCache<TKey, TValue> where TKey : ICacheableKey
{
  /// <summary>
  /// The cache dictionary, consisting of the <see cref="ICacheableKey.CacheUID"/> for the key and the value.
  /// </summary>
  private readonly Dictionary<string, TValue> _cache = new Dictionary<string, TValue>();

  /// <summary>
  /// Returns whether the cache contains an entry with the specified key.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <returns>Bool whether the cache contains an entry with the specified key.</returns>
  public bool Has(TKey key) => _cache.ContainsKey(key.CacheUID);

  /// <summary>
  /// Gets or sets the cached value for the specified key.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <returns>The cached value for the key.</returns>
  public TValue this[TKey key]
  {
    get => _cache[key.CacheUID];
    set => _cache[key.CacheUID] = value;
  }
}