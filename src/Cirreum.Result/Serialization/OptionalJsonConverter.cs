namespace Cirreum.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json converter for <see cref="Optional{T}"/>. Serializes transparently: a present optional writes
/// its value; an empty optional writes <c>null</c>. Reading <c>null</c> (or a value that deserializes to null)
/// yields <see cref="Optional{T}.Empty"/>. This makes an <see cref="Optional{T}"/> property indistinguishable on
/// the wire from a plain nullable, while round-tripping presence correctly.
/// </summary>
/// <typeparam name="T">The optional value type.</typeparam>
public sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>> {

	// Optional<T> is a struct, so System.Text.Json would not otherwise invoke this converter for a JSON null
	// token. Opt in so that an empty optional round-trips through null rather than throwing.
	/// <inheritdoc />
	public override bool HandleNull => true;

	/// <inheritdoc />
	[UnconditionalSuppressMessage("Trimming", "IL2026",
		Justification = "Optional<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Optional<T> types it (de)serializes.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Optional<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Optional<T> types it (de)serializes.")]
	public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType == JsonTokenType.Null) {
			return Optional<T>.Empty;
		}

		var value = JsonSerializer.Deserialize<T>(ref reader, options);
		return value is null ? Optional<T>.Empty : Optional<T>.For(value);
	}

	/// <inheritdoc />
	[UnconditionalSuppressMessage("Trimming", "IL2026",
		Justification = "Optional<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Optional<T> types it (de)serializes.")]
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Optional<T> serialization is reflection-based; a trimmed/Native-AOT app must supply a " +
			"source-generated converter for the closed Optional<T> types it (de)serializes.")]
	public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options) {
		if (value.HasValue) {
			JsonSerializer.Serialize(writer, value.Value, options);
		} else {
			writer.WriteNullValue();
		}
	}

}
