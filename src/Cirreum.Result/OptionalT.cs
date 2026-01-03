namespace Cirreum;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents an optional value that may or may not be present.
/// </summary>
/// <remarks>
/// Use <see cref="Optional{T}"/> to model values that may be absent without implying an error.
/// Unlike <see cref="Result{T}"/>, which represents success or failure with error information,
/// <see cref="Optional{T}"/> simply indicates presence or absence of a value. This type is
/// thread-safe for read-only usage and is commonly used for lookups, searches, and nullable
/// domain modeling.
/// </remarks>
/// <typeparam name="T">The type of the value when present.</typeparam>
public readonly struct Optional<T> : IEquatable<Optional<T>> {

	private readonly T? _value;
	private readonly bool _hasValue;

	/// <summary>
	/// Creates an optional containing the specified value.
	/// </summary>
	/// <remarks>
	/// If <paramref name="value"/> is <see langword="null"/>, returns <see cref="Empty"/>
	/// instead of wrapping the null value.
	/// </remarks>
	/// <param name="value">The value to wrap.</param>
	/// <returns>An <see cref="Optional{T}"/> containing the value, or <see cref="Empty"/> if null.</returns>
	public static Optional<T> From(T? value) =>
		value is null ? Empty : new(value);

	/// <summary>
	/// Represents an empty optional with no value.
	/// </summary>
	public static Optional<T> Empty => default;

	private Optional(T value) {
		this._value = value;
		this._hasValue = true;

		Debug.Assert(value is not null, "Optional value must be non-null when HasValue is true.");
	}

	/// <summary>
	/// Gets a value indicating whether this optional contains a value.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Value))]
	[MemberNotNullWhen(true, nameof(_value))]
	public bool HasValue => this._hasValue;

	/// <summary>
	/// Gets a value indicating whether this optional is empty (has no value).
	/// </summary>
	[MemberNotNullWhen(false, nameof(Value))]
	[MemberNotNullWhen(false, nameof(_value))]
	public bool IsEmpty => !this._hasValue;

	/// <summary>
	/// Gets the value if present.
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when accessing Value on an empty optional.</exception>
	public T Value => this._hasValue
		? this._value!
		: throw new InvalidOperationException("Optional has no value.");

	/// <summary>
	/// Attempts to retrieve the value if present.
	/// </summary>
	/// <param name="value">When this method returns <see langword="true"/>, contains the value;
	/// otherwise, the default value for <typeparamref name="T"/>.</param>
	/// <returns><see langword="true"/> if this optional contains a value; otherwise, <see langword="false"/>.</returns>
	public bool TryGetValue([NotNullWhen(true)] out T? value) {
		if (this._hasValue) {
			value = this._value!;
			return true;
		}
		value = default;
		return false;
	}

	/// <summary>
	/// Deconstructs the <see cref="Optional{T}"/> into its components.
	/// </summary>
	/// <param name="hasValue">When this method returns, contains the presence flag.</param>
	/// <param name="value">When this method returns, contains the value if present; otherwise, the default.</param>
	public void Deconstruct(out bool hasValue, out T? value) {
		hasValue = this._hasValue;
		value = this._value;
	}

	/// <summary>
	/// Transforms the value if present using the specified selector function.
	/// </summary>
	/// <typeparam name="TResult">The type of the transformed value.</typeparam>
	/// <param name="selector">The transformation function.</param>
	/// <returns>An <see cref="Optional{TResult}"/> containing the transformed value if present;
	/// otherwise, <see cref="Optional{TResult}.Empty"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is null.</exception>
	public Optional<TResult> Map<TResult>(Func<T, TResult> selector) {
		ArgumentNullException.ThrowIfNull(selector);
		return this._hasValue
			? Optional<TResult>.From(selector(this._value!))
			: Optional<TResult>.Empty;
	}

	/// <summary>
	/// Chains another optional-returning operation if the value is present.
	/// </summary>
	/// <typeparam name="TResult">The type of the value in the chained optional.</typeparam>
	/// <param name="selector">The function that returns the next optional.</param>
	/// <returns>The result of the chained operation if present; otherwise, <see cref="Optional{TResult}.Empty"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is null.</exception>
	public Optional<TResult> Then<TResult>(Func<T, Optional<TResult>> selector) {
		ArgumentNullException.ThrowIfNull(selector);
		return this._hasValue
			? selector(this._value!)
			: Optional<TResult>.Empty;
	}

	/// <summary>
	/// Projects the optional into a value by invoking the appropriate function.
	/// </summary>
	/// <typeparam name="TResult">The type of the value produced by the projection.</typeparam>
	/// <param name="onValue">Function invoked when the optional has a value.</param>
	/// <param name="onEmpty">Function invoked when the optional is empty.</param>
	/// <returns>The value produced by either <paramref name="onValue"/> or <paramref name="onEmpty"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="onValue"/> or <paramref name="onEmpty"/> is null.</exception>
	public TResult Match<TResult>(Func<T, TResult> onValue, Func<TResult> onEmpty) {
		ArgumentNullException.ThrowIfNull(onValue);
		ArgumentNullException.ThrowIfNull(onEmpty);
		return this._hasValue
			? onValue(this._value!)
			: onEmpty();
	}

	/// <summary>
	/// Executes one of the provided actions based on whether the optional has a value.
	/// </summary>
	/// <param name="onValue">Action invoked when the optional has a value.</param>
	/// <param name="onEmpty">Action invoked when the optional is empty.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="onValue"/> or <paramref name="onEmpty"/> is null.</exception>
	public void Switch(Action<T> onValue, Action onEmpty) {
		ArgumentNullException.ThrowIfNull(onValue);
		ArgumentNullException.ThrowIfNull(onEmpty);

		if (this._hasValue) {
			onValue(this._value!);
			return;
		}

		onEmpty();
	}

	/// <summary>
	/// Filters the optional based on a predicate.
	/// </summary>
	/// <param name="predicate">The condition to test the value against.</param>
	/// <returns>This optional if present and the predicate returns true; otherwise, <see cref="Empty"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
	public Optional<T> Where(Func<T, bool> predicate) {
		ArgumentNullException.ThrowIfNull(predicate);
		return this._hasValue && predicate(this._value!)
			? this
			: Empty;
	}

	/// <summary>
	/// Returns the value if present; otherwise, returns the specified default value.
	/// </summary>
	/// <param name="defaultValue">The value to return if this optional is empty.</param>
	/// <returns>The contained value if present; otherwise, <paramref name="defaultValue"/>.</returns>
	public T GetValueOrDefault(T defaultValue) =>
		this._hasValue ? this._value! : defaultValue;

	/// <summary>
	/// Returns the value if present; otherwise, invokes the factory and returns its result.
	/// </summary>
	/// <param name="defaultFactory">A function that produces a default value.</param>
	/// <returns>The contained value if present; otherwise, the result of <paramref name="defaultFactory"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="defaultFactory"/> is null.</exception>
	public T GetValueOrDefault(Func<T> defaultFactory) {
		ArgumentNullException.ThrowIfNull(defaultFactory);
		return this._hasValue ? this._value! : defaultFactory();
	}

	/// <summary>
	/// Returns the value if present; otherwise, returns <see langword="null"/>.
	/// </summary>
	/// <returns>The contained value if present; otherwise, <see langword="null"/>.</returns>
	public T? GetValueOrNull() =>
		this._hasValue ? this._value : default;

	/// <summary>
	/// Converts this optional to a <see cref="Result{T}"/>, using the specified error if empty.
	/// </summary>
	/// <param name="errorIfEmpty">The exception to use if this optional is empty.</param>
	/// <returns>A successful <see cref="Result{T}"/> if present; otherwise, a failed result.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="errorIfEmpty"/> is null.</exception>
	public Result<T> ToResult(Exception errorIfEmpty) {
		ArgumentNullException.ThrowIfNull(errorIfEmpty);
		return this._hasValue
			? Result<T>.Success(this._value!)
			: Result<T>.Fail(errorIfEmpty);
	}

	/// <summary>
	/// Converts this optional to a <see cref="Result{T}"/>, using the factory to create an error if empty.
	/// </summary>
	/// <param name="errorFactory">A function that creates the exception if this optional is empty.</param>
	/// <returns>A successful <see cref="Result{T}"/> if present; otherwise, a failed result.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="errorFactory"/> is null.</exception>
	public Result<T> ToResult(Func<Exception> errorFactory) {
		ArgumentNullException.ThrowIfNull(errorFactory);
		return this._hasValue
			? Result<T>.Success(this._value!)
			: Result<T>.Fail(errorFactory());
	}

	/// <inheritdoc />
	public bool Equals(Optional<T> other) =>
		this._hasValue == other._hasValue &&
		EqualityComparer<T>.Default.Equals(this._value, other._value);

	/// <inheritdoc />
	public override bool Equals(object? obj) =>
		obj is Optional<T> other && this.Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() =>
		HashCode.Combine(this._hasValue, this._value);

	/// <inheritdoc />
	public override string ToString() =>
		this._hasValue ? $"HasValue({this._value})" : "IsEmpty";

	/// <summary>
	/// Determines whether two <see cref="Optional{T}"/> instances are equal.
	/// </summary>
	/// <param name="left">The first optional to compare.</param>
	/// <param name="right">The second optional to compare.</param>
	/// <returns><see langword="true"/> if both optionals are equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

	/// <summary>
	/// Determines whether two <see cref="Optional{T}"/> instances are not equal.
	/// </summary>
	/// <param name="left">The first optional to compare.</param>
	/// <param name="right">The second optional to compare.</param>
	/// <returns><see langword="true"/> if the optionals are not equal; otherwise, <see langword="false"/>.</returns>
	public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

}