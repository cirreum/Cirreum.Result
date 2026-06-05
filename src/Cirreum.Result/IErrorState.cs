namespace Cirreum;

/// <summary>
/// Implemented by an <see cref="Exception"/> that wants its serializable state to round-trip when a failed
/// <see cref="Result"/> or <see cref="Result{T}"/> carrying it is serialized. The state is captured into the
/// deserialized <see cref="SurrogateResultException"/> on the far side of a serialization boundary
/// (a distributed cache, a message bus, an API payload).
/// </summary>
/// <remarks>
/// State is intentionally <c>string</c>→<c>string</c> — an explicit, lossless, AOT-safe escape hatch, not typed
/// reconstruction. The implementing exception chooses what to expose (for example a key list or a status code);
/// the consumer reads it back from <see cref="SurrogateResultException.State"/>. Empty state writes nothing.
/// </remarks>
public interface IErrorState {

	/// <summary>
	/// The serializable state to carry across a serialization boundary. An empty map is omitted from the wire.
	/// </summary>
	IReadOnlyDictionary<string, string> State { get; }

}
