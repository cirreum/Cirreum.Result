namespace Cirreum.ResultMonad.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

[TestClass]
public class ResultSerializationTests {

	private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

	[TestMethod]
	public void ResultOfT_Success_RoundTrips_ThroughString() {
		// The exact scenario that previously came back as a failure: a serialized success must
		// deserialize as a success carrying the original value.
		var json = JsonSerializer.Serialize(Result<int>.Success(5));
		var back = JsonSerializer.Deserialize<Result<int>>(json);

		Assert.IsTrue(back.IsSuccess, "A round-tripped success must remain a success.");
		Assert.AreEqual(5, back.Value);
		Assert.IsNull(back.Error);
	}

	[TestMethod]
	public void ResultOfT_Success_RoundTrips_ThroughUtf8Bytes() {
		// Mirrors the IDistributedCache path: SerializeToUtf8Bytes -> Deserialize(bytes).
		var bytes = JsonSerializer.SerializeToUtf8Bytes(Result<int>.Success(5));
		var back = JsonSerializer.Deserialize<Result<int>>(bytes);

		Assert.IsTrue(back.IsSuccess);
		Assert.AreEqual(5, back.Value);
	}

	[TestMethod]
	public void ResultOfT_Success_WithComplexValue_RoundTrips() {
		var original = Result<SampleDto>.Success(new SampleDto("Ada", 36));

		var json = JsonSerializer.Serialize(original);
		var back = JsonSerializer.Deserialize<Result<SampleDto>>(json);

		Assert.IsTrue(back.IsSuccess);
		Assert.AreEqual(original.Value, back.Value);
	}

	[TestMethod]
	public void ResultOfT_RoundTrips_UnderWebOptions() {
		// The inner value is (de)serialized through the supplied options; round-trip must hold under them.
		var json = JsonSerializer.Serialize(Result<string>.Success("hi"), Web);
		var back = JsonSerializer.Deserialize<Result<string>>(json, Web);

		Assert.IsTrue(back.IsSuccess);
		Assert.AreEqual("hi", back.Value);
	}

	[TestMethod]
	public void ResultOfT_Failure_RoundTrips_PreservingTypeAndMessage() {
		var original = Result<int>.Fail(new InvalidOperationException("Boom"));

		var json = JsonSerializer.Serialize(original);
		var back = JsonSerializer.Deserialize<Result<int>>(json);

		Assert.IsTrue(back.IsFailure);
		Assert.IsInstanceOfType(back.Error, typeof(SurrogateResultException));
		var error = (SurrogateResultException)back.Error!;
		Assert.AreEqual("System.InvalidOperationException", error.OriginalTypeFullName);
		Assert.AreEqual("Boom", error.Message);
	}

	[TestMethod]
	public void Result_Success_RoundTrips() {
		var back = JsonSerializer.Deserialize<Result>(JsonSerializer.Serialize(Result.Success));

		Assert.IsTrue(back.IsSuccess);
		Assert.IsNull(back.Error);
	}

	[TestMethod]
	public void Result_Failure_RoundTrips_PreservingMessage() {
		var back = JsonSerializer.Deserialize<Result>(
			JsonSerializer.Serialize(Result.Fail(new InvalidOperationException("Nope"))));

		Assert.IsTrue(back.IsFailure);
		Assert.IsInstanceOfType(back.Error, typeof(SurrogateResultException));
		Assert.AreEqual("Nope", back.Error!.Message);
	}

	public sealed record SampleDto(string Name, int Age);

}
