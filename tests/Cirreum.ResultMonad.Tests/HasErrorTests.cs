namespace Cirreum.ResultMonad.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

[TestClass]
public class HasErrorTests {

	[TestMethod]
	public void LiveError_ExactType_Matches() {
		var result = Result<int>.Fail(new InvalidOperationException("x"));

		Assert.IsTrue(result.HasError<InvalidOperationException>());
		Assert.IsFalse(result.HasError<ArgumentException>());
	}

	[TestMethod]
	public void LiveError_BaseType_Matches_ViaAssignability() {
		var result = Result.Fail(new DerivedError("x"));

		Assert.IsTrue(result.HasError<BaseError>(), "A live error matches base types via runtime assignability.");
	}

	[TestMethod]
	public void Success_NeverHasError() {
		Assert.IsFalse(Result<int>.Success(5).HasError<InvalidOperationException>());
		Assert.IsFalse(Result.Success.HasError(typeof(Exception)));
	}

	[TestMethod]
	public void Surrogate_ExactType_Matches_AfterRoundTrip() {
		var back = RoundTrip(Result<int>.Fail(new InvalidOperationException("boom")));

		Assert.IsInstanceOfType(back.Error, typeof(SurrogateResultException));
		Assert.IsTrue(back.HasError<InvalidOperationException>(),
			"HasError bridges the surrogate by matching the preserved original type name.");
		Assert.IsFalse(back.HasError<ArgumentException>());
	}

	[TestMethod]
	public void Surrogate_BaseType_DoesNotMatch_ByDesign() {
		// Live: a DerivedError matches its BaseError via runtime assignability. After a serializing
		// round-trip the surrogate preserves only the exact original full type name, so the base-type
		// check no longer matches — a documented asymmetry (matching the original requires the exact type).
		var live = Result<int>.Fail(new DerivedError("boom"));
		Assert.IsTrue(live.HasError<BaseError>());

		var back = RoundTrip(live);
		Assert.IsInstanceOfType(back.Error, typeof(SurrogateResultException));
		Assert.IsTrue(back.HasError<DerivedError>(), "The exact original type still matches across serialization.");
		Assert.IsFalse(back.HasError<BaseError>(), "Base-type assignability is lost across serialization.");
	}

	[TestMethod]
	public void NonGeneric_MirrorsGeneric() {
		var result = Result.Fail(new InvalidOperationException("x"));

		Assert.IsTrue(result.HasError(typeof(InvalidOperationException)));
		Assert.IsFalse(result.HasError(typeof(ArgumentException)));
	}

	[TestMethod]
	public void NonGeneric_NonExceptionType_Throws() {
		var result = Result.Fail(new InvalidOperationException("x"));

		Assert.ThrowsExactly<ArgumentException>(() => result.HasError(typeof(string)));
	}

	private static Result<int> RoundTrip(Result<int> result) =>
		JsonSerializer.Deserialize<Result<int>>(JsonSerializer.Serialize(result));

	private class BaseError(string message) : Exception(message);

	private sealed class DerivedError(string message) : BaseError(message);

}
