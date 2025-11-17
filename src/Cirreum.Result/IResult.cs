namespace Cirreum;

/// <summary>
/// Represents the result of an operation, providing information about its success or failure.
/// </summary>
/// <remarks>
/// This interface provides a common abstraction for both <see cref="Result"/> and <see cref="Result{T}"/>,
/// enabling runtime-agnostic handling of operation outcomes across different hosting environments
/// (Server, WASM, Functions).
/// </remarks>
public interface IResult {

	/// <summary>
	/// Gets a value indicating whether the operation completed successfully.
	/// </summary>
	bool IsSuccess { get; }

	/// <summary>
	/// Gets a value indicating whether the operation failed.
	/// </summary>
	bool IsFailure { get; }

	/// <summary>
	/// Gets the error that caused the failure, if any.
	/// Returns <see langword="null"/> if the operation was successful.
	/// </summary>
	Exception? Error { get; }

	/// <summary>
	/// Gets the underlying value if this is a <see cref="Result{T}"/>, otherwise returns <see langword="null"/>.
	/// For non-generic <see cref="Result"/>, this always returns <see langword="null"/>.
	/// </summary>
	/// <returns>
	/// The boxed value for <see cref="Result{T}"/>, or <see langword="null"/> for non-generic <see cref="Result"/>.
	/// </returns>
	object? GetValue();

	/// <summary>
	/// Executes the appropriate action based on success or failure state.
	/// </summary>
	/// <param name="onSuccess">Action to execute if successful.</param>
	/// <param name="onFailure">Action to execute with the error if failed.</param>
	/// <param name="onCallbackError">
	/// Optional action invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// action is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
	void Switch(
		Action onSuccess,
		Action<Exception> onFailure,
		Action<Exception>? onCallbackError = null);

	/// <summary>
	/// Asynchronously executes the appropriate function based on the success or failure state
	/// of the result.
	/// </summary>
	/// <param name="onSuccess">
	/// A function to invoke when the result is successful.  
	/// </param>
	/// <param name="onFailure">
	/// A function to invoke when the result represents a failure.  
	/// The function receives the associated <see cref="Exception"/>.
	/// </param>
	/// <param name="onCallbackError">
	/// Optional func invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// funcs is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <returns>
	/// A <see cref="ValueTask"/> that completes when the invoked function has completed.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <c>null</c>.
	/// </exception>
	/// <remarks>
	/// This method provides a way to attach asynchronous side-effect processors.  
	/// If the result is successful, <paramref name="onSuccess"/> is invoked with the value.  
	/// If the result is a failure, <paramref name="onFailure"/> is invoked with the error.  
	/// Any exception thrown by either function is allowed to propagate to the caller.
	/// </remarks>
	ValueTask SwitchAsync(
		Func<ValueTask> onSuccess,
		Func<Exception, ValueTask> onFailure,
		Func<Exception, ValueTask>? onCallbackError = null);

	/// <summary>
	/// Asynchronously executes the appropriate function based on the success or failure state
	/// of the result.
	/// </summary>
	/// <param name="onSuccess">
	/// A function to invoke when the result is successful.  
	/// </param>
	/// <param name="onFailure">
	/// A function to invoke when the result represents a failure.  
	/// The function receives the associated <see cref="Exception"/>.
	/// </param>
	/// <param name="onCallbackError">
	/// Optional func invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// funcs is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> that completes when the invoked function has completed.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <c>null</c>.
	/// </exception>
	/// <remarks>
	/// This method provides a way to attach asynchronous side-effect processors.  
	/// If the result is successful, <paramref name="onSuccess"/> is invoked with the value.  
	/// If the result is a failure, <paramref name="onFailure"/> is invoked with the error.  
	/// Any exception thrown by either function is allowed to propagate to the caller.
	/// </remarks>
	Task SwitchAsyncTask(
		Func<Task> onSuccess,
		Func<Exception, Task> onFailure,
		Func<Exception, Task>? onCallbackError = null);

}

/// <summary>
/// Represents the result of an operation that may produce a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the value returned when the operation is successful.
/// </typeparam>
/// <remarks>
/// This interface extends <see cref="IResult"/> with a strongly-typed value accessor and
/// value-aware variants of the <c>Switch</c> and async-switch methods.
/// It is implemented by <see cref="Result{T}"/>.
/// </remarks>
public interface IResult<out T> : IResult {

	/// <summary>
	/// Gets the underlying value if the result represents a successful outcome.
	/// </summary>
	/// <returns>
	/// The value of type <typeparamref name="T"/> when the result is successful;
	/// otherwise, <see langword="null"/>.
	/// </returns>
	/// <remarks>
	/// This method hides <see cref="IResult.GetValue"/> and returns the value as
	/// a strongly-typed <typeparamref name="T"/> instead of a boxed <see cref="object"/>.
	/// For failure results, this method returns <see langword="null"/>.
	/// </remarks>
	new T? GetValue();

	/// <summary>
	/// Executes one of the provided actions depending on whether the result
	/// represents success or failure, without modifying the result.
	/// </summary>
	/// <param name="onSuccess">
	/// The action to invoke when the result is successful.
	/// Receives the value of type <typeparamref name="T"/>.
	/// </param>
	/// <param name="onFailure">
	/// The action to invoke when the result represents a failure.
	/// Receives the associated <see cref="IResult.Error"/>.
	/// </param>
	/// <param name="onCallbackError">
	/// Optional action invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// action is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/>
	/// is <c>null</c>.
	/// </exception>
	void Switch(
		Action<T> onSuccess,
		Action<Exception> onFailure,
		Action<Exception>? onCallbackError = null);

	/// <summary>
	/// Asynchronously executes the appropriate function based on the success or failure state
	/// of the result.
	/// </summary>
	/// <param name="onSuccess">
	/// A function to invoke when the result is successful.  
	/// The function receives the result value of type <typeparamref name="T"/>.
	/// </param>
	/// <param name="onFailure">
	/// A function to invoke when the result represents a failure.  
	/// The function receives the associated <see cref="Exception"/>.
	/// </param>
	/// <param name="onCallbackError">
	/// Optional func invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// funcs is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <returns>
	/// A <see cref="ValueTask"/> that completes when the invoked function has completed.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <c>null</c>.
	/// </exception>
	/// <remarks>
	/// This method provides a way to attach asynchronous side-effect processors.  
	/// If the result is successful, <paramref name="onSuccess"/> is invoked with the value.  
	/// If the result is a failure, <paramref name="onFailure"/> is invoked with the error.  
	/// Any exception thrown by either function is allowed to propagate to the caller.
	/// </remarks>
	ValueTask SwitchAsync(
		Func<T, ValueTask> onSuccess,
		Func<Exception, ValueTask> onFailure,
		Func<Exception, ValueTask>? onCallbackError = null);

	/// <summary>
	/// Asynchronously executes the appropriate function based on the success or failure state
	/// of the result.
	/// </summary>
	/// <param name="onSuccess">
	/// A function to invoke when the result is successful.  
	/// The function receives the result value of type <typeparamref name="T"/>.
	/// </param>
	/// <param name="onFailure">
	/// A function to invoke when the result represents a failure.  
	/// The function receives the associated <see cref="Exception"/>.
	/// </param>
	/// <param name="onCallbackError">
	/// Optional func invoked if <paramref name="onSuccess"/> or
	/// <paramref name="onFailure"/> throws.  
	/// If this parameter is <c>null</c>, any exception thrown by the selected
	/// funcs is rethrown to the caller.  
	/// If it is non-null, the exception is passed to this handler and is
	/// not rethrown.
	/// </param>
	/// <returns>
	/// A <see cref="Task"/> that completes when the invoked function has completed.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="onSuccess"/> or <paramref name="onFailure"/> is <c>null</c>.
	/// </exception>
	/// <remarks>
	/// This method provides a way to attach asynchronous side-effect processors.  
	/// If the result is successful, <paramref name="onSuccess"/> is invoked with the value.  
	/// If the result is a failure, <paramref name="onFailure"/> is invoked with the error.  
	/// Any exception thrown by either function is allowed to propagate to the caller.
	/// </remarks>
	Task SwitchAsyncTask(
		Func<T, Task> onSuccess,
		Func<Exception, Task> onFailure,
		Func<Exception, Task>? onCallbackError = null);

}
