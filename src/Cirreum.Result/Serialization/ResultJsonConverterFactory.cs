namespace Cirreum.Serialization;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Creates the appropriate converter for <see cref="Result"/> and <see cref="Result{T}"/>. Referenced by the
/// <see cref="JsonConverterAttribute"/> on those types, so it applies under <em>any</em>
/// <see cref="JsonSerializerOptions"/> — including the options used by distributed/hybrid caches and message
/// transports — with no explicit registration.
/// </summary>
/// <remarks>
/// Serializing <see cref="Result{T}"/> for an arbitrary <c>T</c> is reflection-based, like System.Text.Json's
/// own reflection serializer. A Native-AOT or fully-trimmed application must supply a source-generated converter
/// for the closed <c>Result&lt;T&gt;</c> types it caches; this factory targets the reflection-based serializer.
/// </remarks>
public sealed class ResultJsonConverterFactory : JsonConverterFactory {

	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert == typeof(Result) ||
		(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>));

	/// <inheritdoc />
	[UnconditionalSuppressMessage("AOT", "IL3050",
		Justification = "Result<T> serialization is reflection-based; a Native-AOT app must supply a " +
			"source-generated converter for the closed Result<T> types it (de)serializes.")]
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
		if (typeToConvert == typeof(Result)) {
			return new ResultJsonConverter();
		}

		var valueType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(ResultJsonConverter<>).MakeGenericType(valueType);
		return (JsonConverter?)Activator.CreateInstance(converterType);
	}

}
