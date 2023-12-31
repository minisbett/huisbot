namespace huisbot.Utils;

/// <summary>
/// A wrapper around a dictionary to cache it's values for a certain time.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary key.</typeparam>
/// <typeparam name="TValue">The type of the dictionary value.</typeparam>
internal class CachedDictionary<TKey, TValue> where TKey : notnull
{
  /// <summary>
  /// The dictionary containing all keys and their corresponding values.
  /// </summary>
  private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

  /// <summary>
  /// The dictionary containing all keys and the last refresh date of the corresponding value.
  /// </summary>
  private readonly Dictionary<TKey, DateTime> _lastRefreshs = new Dictionary<TKey, DateTime>();

  /// <summary>
  /// The span after which the value expires.
  /// </summary>
  private readonly TimeSpan _expirationSpan;

  /// <summary>
  /// Returns whether the key exists and it's corresponding value has not expired yet.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <returns></returns>
  public bool IsValid(TKey key) => _dictionary.ContainsKey(key) && DateTime.UtcNow - _lastRefreshs[key] <= _expirationSpan;

  /// <summary>
  /// The value to the corresponding key.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <returns>The corresponding value of the key.</returns>
  public TValue this[TKey key]
  {
    get => _dictionary[key];
    set
    {
      _dictionary[key] = value;
      _lastRefreshs[key] = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Creates a new cached dictionary with the specified expiration span.
  /// </summary>
  /// <param name="expirationSpan">The expiration span.</param>
  public CachedDictionary(TimeSpan expirationSpan)
  {
    _expirationSpan = expirationSpan;
  }
}
