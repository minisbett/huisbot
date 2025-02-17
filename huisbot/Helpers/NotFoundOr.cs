namespace huisbot.Utilities;

/// <summary>
/// A wrapper around a type that can be considered "Not Found".
/// </summary>
public class NotFoundOr<T> where T : class
{
  /// <summary>
  /// Indicates the value was not found.
  /// </summary>
  public static NotFoundOr<T> NotFound => new NotFoundOr<T>();

  /// <summary>
  /// The possibly not found value.
  /// </summary>
  private readonly T? _value;

  /// <summary>
  /// Bool whether the value was found.
  /// </summary>
  public bool Found => _value != null;

  /// <summary>
  /// Creates a new instance of <see cref="NotFoundOr{T}"/> with the found value.
  /// </summary>
  /// <param name="value">The found value.</param>
  public NotFoundOr(T value)
  {
    _value = value;
  }

  /// <summary>
  /// Creates a new instance of <see cref="NotFoundOr{T}"/> without any constructor, indicating a "Not Found".
  /// </summary>
  private NotFoundOr() { }

  /// <summary>
  /// The value of the <see cref="NotFoundOr{T}"/> object.
  /// </summary>
  /// <param name="obj">The object.</param>
  public static implicit operator T(NotFoundOr<T> obj) => obj._value!;
}

/// <summary>
/// Provides the <see cref="WasFound{T}(T)"/> extension method.
/// </summary>
public static class NotFoundOrExtensions
{
  /// <summary>
  /// Returns a <see cref="NotFoundOr{T}"/> indicating the value was found.
  /// </summary>
  /// <typeparam name="T">The type of the value.</typeparam>
  /// <param name="value">The value.</param>
  /// <returns>The NotFoundOr instance.</returns>
  public static NotFoundOr<T> WasFound<T>(this T value) where T : class
  {
    return new NotFoundOr<T>(value);
  }
}