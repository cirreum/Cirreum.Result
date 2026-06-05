namespace Cirreum;

/// <summary>
/// The stand-in exception a deserialized failed <see cref="Result"/> or <see cref="Result{T}"/> carries when the
/// original exception cannot be reconstructed. Exceptions do not round-trip through serialization — type identity,
/// stack trace, and custom state are lost — so this surrogate preserves only the original error's full type name and
/// message.
/// </summary>
/// <remarks>
/// <c>is</c> checks, <c>catch</c> filters, and equality against the original concrete exception type will NOT match
/// a surrogate. The original type full name is preserved on <see cref="OriginalTypeFullName"/> for diagnostics, logging, and
/// status mapping.
/// </remarks>
/// <param name="originalTypeFullName">The full name of the original exception type, if known.</param>
/// <param name="message">The original error message.</param>
public sealed class SurrogateResultException(
	string? originalTypeFullName,
	string message
) : Exception(message), IErrorState {

	private readonly Dictionary<string, string> _state = [];

	/// <summary>
	/// The full name of the exception type that produced the original failure
	/// (for example <c>System.InvalidOperationException</c>), or <see langword="null"/> if it was not recorded.
	/// </summary>
	public string? OriginalTypeFullName { get; } = originalTypeFullName;

	/// <summary>
	/// The state carried across the serialization boundary for this error. Mutable — a producer may populate it
	/// before serializing, and the converter populates it on deserialization. Empty by default.
	/// </summary>
	public IDictionary<string, string> State => this._state;

	IReadOnlyDictionary<string, string> IErrorState.State => this._state;

	/// <summary>
	/// Returns the original exception type name (when known) followed by the message.
	/// </summary>
	public override string ToString() =>
		this.OriginalTypeFullName is null
			? base.ToString()
			: $"{this.OriginalTypeFullName}: {this.Message}";

}