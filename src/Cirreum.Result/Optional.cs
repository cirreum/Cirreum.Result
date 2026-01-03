namespace Cirreum;

/// <summary>
/// Provides factory methods for creating <see cref="Optional{T}"/> instances.
/// </summary>
public static class Optional {

	/// <summary>
	/// Creates an optional containing the specified value, or <see cref="Optional{T}.Empty"/> if null.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	/// <param name="value">The value to wrap.</param>
	/// <returns>An <see cref="Optional{T}"/> containing the value, or <see cref="Optional{T}.Empty"/> if null.</returns>
	public static Optional<T> From<T>(T? value) =>
		value is null ? default : Optional<T>.For(value);

}