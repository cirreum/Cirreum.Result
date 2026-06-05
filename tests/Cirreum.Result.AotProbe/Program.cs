using Cirreum;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("== Cirreum.Result AOT probe ==");

// (A) Monad core — no reflection, fully AOT-safe. Expect: no analyzer warnings, works at runtime.
var mapped = Result<int>.Success(41).Map(x => x + 1);
var failed = Result<int>.Fail(new InvalidOperationException("nope"));
Console.WriteLine(
	$"A monad      : IsSuccess={mapped.IsSuccess}, Value={mapped.Value}, " +
	$"HasError<IOE>={failed.HasError<InvalidOperationException>()}");

// (B) Reflection-based System.Text.Json — the AOT-unsafe path.
//     JsonSerializer.Serialize/Deserialize (no JsonTypeInfo) are themselves
//     [RequiresDynamicCode]/[RequiresUnreferencedCode], so the warning surfaces HERE at the
//     consumer call site — independent of Cirreum.Result's own (suppressed) converter internals.
//     Under a Native-AOT image (reflection serialization disabled by default) these throw at runtime.
try {
	var json = JsonSerializer.Serialize(Result<int>.Success(5));
	var back = JsonSerializer.Deserialize<Result<int>>(json);
	Console.WriteLine($"B reflection : json={json}, back.IsSuccess={back.IsSuccess}, back.Value={back.Value}");
} catch (Exception ex) {
	Console.WriteLine($"B reflection : THREW {ex.GetType().Name}: {ex.Message}");
}

// (C) Source-generated System.Text.Json via a JsonSerializerContext — the AOT-intended path.
//     Uses the JsonTypeInfo<T> overloads (not [RequiresDynamicCode]). This reveals whether our
//     JsonConverterFactory (which calls Type.MakeGenericType) is reachable AOT-cleanly.
try {
	var json = JsonSerializer.Serialize(Result<int>.Success(7), ProbeJsonContext.Default.ResultInt);
	var back = JsonSerializer.Deserialize(json, ProbeJsonContext.Default.ResultInt);
	Console.WriteLine($"C source-gen : json={json}, back.IsSuccess={back.IsSuccess}, back.Value={back.Value}");
} catch (Exception ex) {
	Console.WriteLine($"C source-gen : THREW {ex.GetType().Name}: {ex.Message}");
}

// Source-gen requires BOTH the Result<T> AND its inner T: the converter (de)serializes the inner value
// through the options resolver, so T must also be registered or the inner serialize throws under AOT.
[JsonSerializable(typeof(Result<int>), TypeInfoPropertyName = "ResultInt")]
[JsonSerializable(typeof(int))]
internal partial class ProbeJsonContext : JsonSerializerContext;
