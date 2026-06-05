namespace Cirreum.Serialization;

using System.Text.Json;

/// <summary>
/// Shared read/write of the <c>error</c> object — <c>{ "type", "message", "state"? }</c> — used by the
/// non-generic and generic Result JSON converters. <c>state</c> is the optional <see cref="IErrorState"/>
/// string→string bag (omitted when empty).
/// </summary>
internal static class ResultErrorJson {

	public static void Write(Utf8JsonWriter writer, Exception error) {
		writer.WriteStartObject();

		// Preserve the ORIGINAL type identity when re-serializing an already-deserialized failure, rather than
		// writing "SurrogateResultException".
		var type = error is SurrogateResultException surrogate
			? surrogate.OriginalTypeFullName
			: error.GetType().FullName;
		writer.WriteString("type", type);
		writer.WriteString("message", error.Message);

		if (error is IErrorState { State.Count: > 0 } stateful) {
			writer.WriteStartObject("state");
			foreach (var entry in stateful.State) {
				writer.WriteString(entry.Key, entry.Value);
			}
			writer.WriteEndObject();
		}

		writer.WriteEndObject();
	}

	public static SurrogateResultException Read(ref Utf8JsonReader reader) {
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException("Expected the start of an error object.");
		}

		string? type = null;
		string? message = null;
		Dictionary<string, string>? state = null;

		while (reader.Read()) {
			if (reader.TokenType == JsonTokenType.EndObject) {
				break;
			}
			if (reader.TokenType != JsonTokenType.PropertyName) {
				throw new JsonException("Expected a property name inside the error object.");
			}

			var name = reader.GetString();
			reader.Read();

			switch (name) {
				case "type":
					type = reader.GetString();
					break;
				case "message":
					message = reader.GetString();
					break;
				case "state":
					state = ReadState(ref reader);
					break;
				default:
					reader.Skip();
					break;
			}
		}

		var surrogate = new SurrogateResultException(type, message ?? string.Empty);
		if (state is not null) {
			foreach (var entry in state) {
				surrogate.State[entry.Key] = entry.Value;
			}
		}
		return surrogate;
	}

	private static Dictionary<string, string> ReadState(ref Utf8JsonReader reader) {
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException("Expected the start of the error 'state' object.");
		}

		var state = new Dictionary<string, string>();

		while (reader.Read()) {
			if (reader.TokenType == JsonTokenType.EndObject) {
				break;
			}
			if (reader.TokenType != JsonTokenType.PropertyName) {
				throw new JsonException("Expected a property name inside the error 'state' object.");
			}

			var key = reader.GetString()!;
			reader.Read();
			state[key] = reader.GetString() ?? string.Empty;
		}

		return state;
	}

}
