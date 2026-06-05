namespace Cirreum.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json converter for the non-generic <see cref="Result"/>.
/// </summary>
/// <remarks>
/// Wire shape: <c>{ "isSuccess": true }</c> for success, or
/// <c>{ "isSuccess": false, "error": { "type": "...", "message": "..." } }</c> for failure. A failure read back
/// carries a <see cref="SurrogateResultException"/> (exceptions do not round-trip; see that type).
/// </remarks>
public sealed class ResultJsonConverter : JsonConverter<Result> {

	/// <inheritdoc />
	public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException("Expected the start of a Result object.");
		}

		bool? isSuccess = null;
		SurrogateResultException? error = null;

		while (reader.Read()) {
			if (reader.TokenType == JsonTokenType.EndObject) {
				break;
			}
			if (reader.TokenType != JsonTokenType.PropertyName) {
				throw new JsonException("Expected a property name inside the Result object.");
			}

			var name = reader.GetString();
			reader.Read();

			switch (name) {
				case "isSuccess":
					isSuccess = reader.GetBoolean();
					break;
				case "error":
					error = ResultErrorJson.Read(ref reader);
					break;
				default:
					reader.Skip();
					break;
			}
		}

		if (isSuccess is null) {
			throw new JsonException("Result JSON is missing the 'isSuccess' discriminator.");
		}

		return isSuccess.Value
			? Result.Success
			: Result.Fail(error ?? new SurrogateResultException(null, "Unspecified error."));
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		writer.WriteBoolean("isSuccess", value.IsSuccess);
		if (value.IsFailure) {
			writer.WritePropertyName("error");
			ResultErrorJson.Write(writer, value.Error);
		}
		writer.WriteEndObject();
	}

}
