namespace Cirreum.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Creates the converter for <see cref="Optional{T}"/>. Referenced by the <see cref="JsonConverterAttribute"/>
/// on <see cref="Optional{T}"/>, so it applies under <em>any</em> <see cref="JsonSerializerOptions"/> with no
/// explicit registration.
/// </summary>
/// <remarks>
/// Serializing <see cref="Optional{T}"/> for an arbitrary <c>T</c> is reflection-based, like System.Text.Json's
/// own reflection serializer. A Native-AOT or fully-trimmed application must supply a source-generated converter
/// for the closed <c>Optional&lt;T&gt;</c> types it serializes; this factory targets the reflection-based serializer.
/// </remarks>
public sealed class OptionalJsonConverterFactory : JsonConverterFactory {

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

	/// <inheritdoc />
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Optional<T> serialization is reflection-based; a Native-AOT app must supply a " +
			"source-generated converter for the closed Optional<T> types it (de)serializes.")]
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
		var valueType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);
		return (JsonConverter?)Activator.CreateInstance(converterType);
	}

}
