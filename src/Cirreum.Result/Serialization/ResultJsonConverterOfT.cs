namespace Cirreum.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json converter for <see cref="Result{T}"/>.
/// </summary>
/// <remarks>
/// Wire shape: <c>{ "isSuccess": true, "value": &lt;T&gt; }</c> for success, or
/// <c>{ "isSuccess": false, "error": { "type": "...", "message": "..." } }</c> for failure. The value is
/// (de)serialized through the supplied <see cref="JsonSerializerOptions"/>; a failure read back carries a
/// <see cref="SurrogateResultException"/> (exceptions do not round-trip; see that type).
/// </remarks>
/// <typeparam name="T">The success value type.</typeparam>
public sealed class ResultJsonConverter<T> : JsonConverter<Result<T>> {

	/// <inheritdoc />
	[UnconditionalSuppressMessage("Trimming", "IL2026",
		Justification = "Result<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Result<T> types it (de)serializes.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Result<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Result<T> types it (de)serializes.")]
	public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.StartObject) {
			throw new JsonException("Expected the start of a Result object.");
		}

		bool? isSuccess = null;
		var hasValue = false;
		T? value = default;
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
				case "value":
					value = JsonSerializer.Deserialize<T>(ref reader, options);
					hasValue = true;
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

		if (isSuccess.Value) {
			// A successful Result<T> is always constructed with a non-null value (Result<T>.Success guards
			// against null); a success payload missing its value is therefore corrupt.
			if (!hasValue || value is null) {
				throw new JsonException("A successful Result must carry a non-null value.");
			}
			return Result<T>.Success(value);
		}

		return Result<T>.Fail(error ?? new SurrogateResultException(null, "Unspecified error."));
	}

	/// <inheritdoc />
	[UnconditionalSuppressMessage("Trimming", "IL2026",
		Justification = "Result<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Result<T> types it (de)serializes.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Result<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Result<T> types it (de)serializes.")]
	public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options) {
		writer.WriteStartObject();
		writer.WriteBoolean("isSuccess", value.IsSuccess);
		if (value.IsSuccess) {
			writer.WritePropertyName("value");
			JsonSerializer.Serialize(writer, value.Value, options);
		} else {
			writer.WritePropertyName("error");
			ResultErrorJson.Write(writer, value.Error);
		}
		writer.WriteEndObject();
	}

}
