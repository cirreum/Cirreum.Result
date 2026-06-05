namespace Cirreum;

/// <summary>
/// Extension methods for working with <see cref="SurrogateResultException"/> instances on <see cref="IResult"/>.
/// </summary>
public static class ResultSurrogateExceptionExtensions {

	/// <summary>
	/// Returns true when the result failed with an error matching <typeparamref name="TException"/>.
	/// </summary>
	/// <remarks>
	/// For live exceptions, normal runtime type assignability is used. For deserialized failures,
	/// only the preserved original exception type name is available, so matching is by exact
	/// full type name. This method does not reconstruct or restore the original exception.
	/// </remarks>
	/// <typeparam name="TException">The type of the exception to look for.</typeparam>
	/// <param name="result">The result to check.</param>
	/// <returns>A value indicating whether the result has an error of the specified type.</returns>
	public static bool HasError<TException>(this IResult result)
		where TException : Exception =>
		result.HasError(typeof(TException));

	/// <summary>
	/// Determines whether the specified <see cref="IResult"/> has an error of the given type, including surrogate exceptions that match by original type name.
	/// </summary>
	/// <param name="result">The result to check.</param>
	/// <param name="exceptionType">The type of the exception to look for.</param>
	/// <returns>A value indicating whether the result has an error of the specified type.</returns>
	/// <exception cref="ArgumentException">Thrown when the supplied type does not derive from Exception.</exception>
	public static bool HasError(this IResult result, Type exceptionType) {

		ArgumentNullException.ThrowIfNull(result);
		ArgumentNullException.ThrowIfNull(exceptionType);

		if (!typeof(Exception).IsAssignableFrom(exceptionType)) {
			throw new ArgumentException("The supplied type must derive from Exception.", nameof(exceptionType));
		}

		if (result.IsSuccess || result.Error is null) {
			return false;
		}

		if (exceptionType.IsInstanceOfType(result.Error)) {
			return true;
		}

		if (result.Error is SurrogateResultException deserialized) {
			return StringComparer.Ordinal.Equals(
				deserialized.OriginalTypeFullName,
				exceptionType.FullName
			);
		}

		return false;

	}

}