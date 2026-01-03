namespace Cirreum;

/// <summary>
/// Provides factory methods for creating <see cref="Optional{T}"/> instances.
/// </summary>
public static class Optional {

	/// <summary>
	/// Creates an optional containing the specified value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value to wrap.</param>
	/// <returns>An <see cref="Optional{T}"/> containing the value, or empty if null.</returns>
	public static Optional<T> From<T>(T? value) => Optional<T>.From(value);

	/// <summary>
	/// Returns an empty optional of the specified type.
	/// </summary>
	/// <typeparam name="T">The type of the optional.</typeparam>
	/// <returns>An empty <see cref="Optional{T}"/>.</returns>
	public static Optional<T> Empty<T>() => Optional<T>.Empty;

}