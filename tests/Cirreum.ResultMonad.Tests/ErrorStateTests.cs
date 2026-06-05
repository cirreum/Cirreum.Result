namespace Cirreum.ResultMonad.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.Json;

[TestClass]
public class ErrorStateTests {

	[TestMethod]
	public void Surrogate_State_RoundTrips_AndPreservesOriginalType() {
		var surrogate = new SurrogateResultException("Some.Original.Type", "boom");
		surrogate.State["keys"] = "42,99";

		var back = RoundTrip(Result<int>.Fail(surrogate));

		var error = (SurrogateResultException)back.Error!;
		Assert.AreEqual("Some.Original.Type", error.OriginalTypeFullName, "Re-serialization preserves the original type.");
		Assert.AreEqual("42,99", error.State["keys"]);
	}

	[TestMethod]
	public void IErrorState_Exception_StateIsCaptured_OnWrite() {
		var back = RoundTrip(Result<int>.Fail(new StatefulError("boom")));

		var error = (SurrogateResultException)back.Error!;
		Assert.AreEqual("v", error.State["k"]);
		// The original type name is captured even though the live exception was not a surrogate.
		Assert.IsTrue(error.OriginalTypeFullName!.EndsWith("StatefulError"));
	}

	[TestMethod]
	public void EmptyState_IsOmittedFromTheWire() {
		using var doc = JsonDocument.Parse(JsonSerializer.Serialize(Result<int>.Fail(new InvalidOperationException("x"))));

		var error = doc.RootElement.GetProperty("error");
		Assert.IsFalse(error.TryGetProperty("state", out _), "An error with no state must omit the 'state' property.");
	}

	[TestMethod]
	public void Surrogate_WithNoState_RoundTrips_StateEmpty() {
		var back = RoundTrip(Result<int>.Fail(new InvalidOperationException("x")));

		var error = (SurrogateResultException)back.Error!;
		Assert.AreEqual(0, error.State.Count);
	}

	private static Result<int> RoundTrip(Result<int> result) =>
		JsonSerializer.Deserialize<Result<int>>(JsonSerializer.Serialize(result));

	private sealed class StatefulError(string message) : Exception(message), IErrorState {
		public IReadOnlyDictionary<string, string> State { get; } = new Dictionary<string, string> { ["k"] = "v" };
	}

}
