namespace Cirreum.ResultMonad.Tests;

using Cirreum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

[TestClass]
public class ResultOfTTests {

	[TestMethod]
	public void Success_ResultT_HasValue_AndNoError() {
		// Arrange
		var result = Result<string>.Success("hello");

		// Act & Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse(result.IsFailure);
		Assert.AreEqual("hello", result.Value);
		Assert.IsNull(result.Error);
	}

	[TestMethod]
	public void Fail_ResultT_HasError_AndNoValue() {
		// Arrange
		var ex = new InvalidOperationException("Boom");
		var result = Result<string>.Fail(ex);

		// Act & Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsTrue(result.IsFailure);
		Assert.IsNull(result.Value);
		Assert.AreSame(ex, result.Error);
	}

	[TestMethod]
	public void TryGetValue_ReturnsTrue_ForSuccess_AndOutputsValue() {
		// Arrange
		var result = Result<int>.Success(42);

		// Act
		var success = result.TryGetValue(out var value);

		// Assert
		Assert.IsTrue(success);
		Assert.AreEqual(42, value);
	}

	[TestMethod]
	public void TryGetValue_ReturnsFalse_ForFailure_AndOutputsDefault() {
		// Arrange
		var result = Result<int>.Fail(new Exception("Oops"));

		// Act
		var success = result.TryGetValue(out var value);

		// Assert
		Assert.IsFalse(success);
		Assert.AreEqual(default, value);
	}

	[TestMethod]
	public void TryGetError_ReturnsTrue_ForFailure_AndOutputsError() {
		// Arrange
		var ex = new InvalidOperationException("Oops");
		var result = Result<int>.Fail(ex);

		// Act
		var hasError = result.TryGetError(out var error);

		// Assert
		Assert.IsTrue(hasError);
		Assert.AreSame(ex, error);
	}

	[TestMethod]
	public void TryGetError_ReturnsFalse_ForSuccess_AndOutputsNull() {
		// Arrange
		var result = Result<int>.Success(1);

		// Act
		var hasError = result.TryGetError(out var error);

		// Assert
		Assert.IsFalse(hasError);
		Assert.IsNull(error);
	}

	[TestMethod]
	public void IResultT_Switch_Invokes_Success_ForSuccessfulResult() {
		// Arrange
		IResult<int> result = Result<int>.Success(10);
		var successCalled = false;
		var failureCalled = false;
		var received = 0;

		// Act
		result.Switch(
			onSuccess: v => {
				successCalled = true;
				received = v;
			},
			onFailure: _ => failureCalled = true);

		// Assert
		Assert.IsTrue(successCalled);
		Assert.IsFalse(failureCalled);
		Assert.AreEqual(10, received);
	}

	[TestMethod]
	public void IResultT_Switch_Invokes_Failure_ForFailedResult() {
		// Arrange
		var ex = new InvalidOperationException("Oops");
		IResult<int> result = Result<int>.Fail(ex);
		var successCalled = false;
		var failureCalled = false;
		Exception? received = null;

		// Act
		result.Switch(
			onSuccess: _ => successCalled = true,
			onFailure: e => {
				failureCalled = true;
				received = e;
			});

		// Assert
		Assert.IsFalse(successCalled);
		Assert.IsTrue(failureCalled);
		Assert.AreSame(ex, received);
	}

	[TestMethod]
	public async Task IResultT_SwitchAsync_ValueTask_Invokes_Success_ForSuccessfulResult() {
		// Arrange
		IResult<int> result = Result<int>.Success(99);
		var successCalled = false;
		var failureCalled = false;
		var received = 0;

		// Act
		await result.SwitchAsync(
			onSuccess: v => {
				successCalled = true;
				received = v;
				return ValueTask.CompletedTask;
			},
			onFailure: _ => {
				failureCalled = true;
				return ValueTask.CompletedTask;
			});

		// Assert
		Assert.IsTrue(successCalled);
		Assert.IsFalse(failureCalled);
		Assert.AreEqual(99, received);
	}

	[TestMethod]
	public async Task IResultT_SwitchAsync_Task_Invokes_Failure_ForFailedResult() {
		// Arrange
		var ex = new InvalidOperationException("Oops");
		IResult<int> result = Result<int>.Fail(ex);
		var successCalled = false;
		var failureCalled = false;
		Exception? received = null;

		// Act
		await result.SwitchAsyncTask(
			onSuccess: _ => {
				successCalled = true;
				return Task.CompletedTask;
			},
			onFailure: e => {
				failureCalled = true;
				received = e;
				return Task.CompletedTask;
			});

		// Assert
		Assert.IsFalse(successCalled);
		Assert.IsTrue(failureCalled);
		Assert.AreSame(ex, received);
	}

	[TestMethod]
	public void Map_Propagates_Success_AndTransforms_Value() {
		// Arrange
		var result = Result<int>.Success(5);

		// Act
		var mapped = result.Map(x => x * 2);

		// Assert
		Assert.IsTrue(mapped.IsSuccess);
		Assert.AreEqual(10, mapped.Value);
	}

	[TestMethod]
	public void Map_Propagates_Failure_Without_Invoking_Selector() {
		// Arrange
		var ex = new InvalidOperationException("Oops");
		var result = Result<int>.Fail(ex);
		var selectorCalled = false;

		// Act
		var mapped = result.Map(x => {
			selectorCalled = true;
			return x * 2;
		});

		// Assert
		Assert.IsFalse(mapped.IsSuccess);
		Assert.AreSame(ex, mapped.Error);
		Assert.IsFalse(selectorCalled, "Selector should not be called on failure.");
	}

	[TestMethod]
	public void Then_Chains_Successful_Results() {
		// Arrange
		var result = Result<int>.Success(2);

		// Act
		var chained = result.Then(x => Result<string>.Success($"Value: {x}"));

		// Assert
		Assert.IsTrue(chained.IsSuccess);
		Assert.AreEqual("Value: 2", chained.Value);
	}

	[TestMethod]
	public void Then_ShortCircuits_On_Failure() {
		// Arrange
		var ex = new InvalidOperationException("Oops");
		var result = Result<int>.Fail(ex);
		var selectorCalled = false;

		// Act
		var chained = result.Then(x => {
			selectorCalled = true;
			return Result<string>.Success(x.ToString());
		});

		// Assert
		Assert.IsFalse(chained.IsSuccess);
		Assert.AreSame(ex, chained.Error);
		Assert.IsFalse(selectorCalled, "Chained selector should not run when initial result is failure.");
	}

	[TestMethod]
	public void ToResult_WhenSuccessful_ReturnsSuccessResult() {
		// Arrange
		var result = Result<int>.Success(42);

		// Act
		var nonGenericResult = result.ToResult();

		// Assert
		Assert.IsTrue(nonGenericResult.IsSuccess);
		Assert.IsNull(nonGenericResult.Error);
	}

	[TestMethod]
	public void ToResult_WhenFailed_ReturnsFailedResultWithSameError() {
		// Arrange
		var ex = new InvalidOperationException("test error");
		var result = Result<int>.Fail(ex);

		// Act
		var nonGenericResult = result.ToResult();

		// Assert
		Assert.IsFalse(nonGenericResult.IsSuccess);
		Assert.IsNotNull(nonGenericResult.Error);
		Assert.AreSame(ex, nonGenericResult.Error);
	}

	#region Ensure Tests

	[TestMethod]
	public void Ensure_WithErrorFactory_WhenPredicateReturnsTrue_ReturnsSuccess() {
		// Arrange
		var result = Result<int>.Success(42);

		// Act
		var ensuredResult = result.Ensure(
			x => x > 0,
			x => new InvalidOperationException($"{x} is not positive"));

		// Assert
		Assert.IsTrue(ensuredResult.IsSuccess);
		Assert.AreEqual(42, ensuredResult.Value);
	}

	[TestMethod]
	public void Ensure_WithErrorFactory_WhenPredicateReturnsFalse_ReturnsFailure() {
		// Arrange
		var result = Result<int>.Success(-5);

		// Act
		var ensuredResult = result.Ensure(
			x => x > 0,
			x => new InvalidOperationException($"{x} is not positive"));

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.IsInstanceOfType(ensuredResult.Error, typeof(InvalidOperationException));
		Assert.AreEqual("-5 is not positive", ensuredResult.Error!.Message);
	}

	[TestMethod]
	public void Ensure_WithErrorFactory_WhenAlreadyFailed_ReturnsOriginalFailure() {
		// Arrange
		var originalError = new InvalidOperationException("original error");
		var result = Result<int>.Fail(originalError);
		var factoryCalled = false;

		// Act
		var ensuredResult = result.Ensure(
			x => true,
			x => {
				factoryCalled = true;
				return new InvalidOperationException("new error");
			});

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(originalError, ensuredResult.Error);
		Assert.IsFalse(factoryCalled);
	}

	[TestMethod]
	public void Ensure_WithErrorFactory_WhenPredicateThrows_ReturnsFailureWithException() {
		// Arrange
		var result = Result<int>.Success(42);
		var predicateException = new InvalidOperationException("predicate failed");

		// Act
		var ensuredResult = result.Ensure(
			x => throw predicateException,
			x => new InvalidOperationException("should not be called"));

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(predicateException, ensuredResult.Error);
	}

	[TestMethod]
	public void Ensure_WithException_WhenPredicateReturnsTrue_ReturnsSuccess() {
		// Arrange
		var result = Result<int>.Success(42);
		var error = new InvalidOperationException("validation error");

		// Act
		var ensuredResult = result.Ensure(x => x > 0, error);

		// Assert
		Assert.IsTrue(ensuredResult.IsSuccess);
		Assert.AreEqual(42, ensuredResult.Value);
	}

	[TestMethod]
	public void Ensure_WithException_WhenPredicateReturnsFalse_ReturnsFailureWithProvidedError() {
		// Arrange
		var result = Result<int>.Success(-5);
		var error = new InvalidOperationException("value must be positive");

		// Act
		var ensuredResult = result.Ensure(x => x > 0, error);

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(error, ensuredResult.Error);
	}

	[TestMethod]
	public void Ensure_WithException_WhenAlreadyFailed_ReturnsOriginalFailure() {
		// Arrange
		var originalError = new InvalidOperationException("original error");
		var result = Result<int>.Fail(originalError);
		var newError = new InvalidOperationException("new error");

		// Act
		var ensuredResult = result.Ensure(x => true, newError);

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(originalError, ensuredResult.Error);
	}

	[TestMethod]
	public void Ensure_WithException_WhenPredicateThrows_ReturnsFailureWithException() {
		// Arrange
		var result = Result<int>.Success(42);
		var predicateException = new InvalidOperationException("predicate failed");
		var providedError = new InvalidOperationException("provided error");

		// Act
		var ensuredResult = result.Ensure(x => throw predicateException, providedError);

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(predicateException, ensuredResult.Error);
	}

	[TestMethod]
	public void Ensure_WithErrorMessage_WhenPredicateReturnsTrue_ReturnsSuccess() {
		// Arrange
		var result = Result<string>.Success("hello");

		// Act
		var ensuredResult = result.Ensure(
			x => x.Length > 3,
			"String must be longer than 3 characters");

		// Assert
		Assert.IsTrue(ensuredResult.IsSuccess);
		Assert.AreEqual("hello", ensuredResult.Value);
	}

	[TestMethod]
	public void Ensure_WithErrorMessage_WhenPredicateReturnsFalse_ReturnsFailureWithInvalidOperationException() {
		// Arrange
		var result = Result<string>.Success("hi");

		// Act
		var ensuredResult = result.Ensure(
			x => x.Length > 3,
			"String must be longer than 3 characters");

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.IsInstanceOfType(ensuredResult.Error, typeof(InvalidOperationException));
		Assert.AreEqual("String must be longer than 3 characters", ensuredResult.Error!.Message);
	}

	[TestMethod]
	public void Ensure_WithErrorMessage_WhenAlreadyFailed_ReturnsOriginalFailure() {
		// Arrange
		var originalError = new InvalidOperationException("original error");
		var result = Result<string>.Fail(originalError);

		// Act
		var ensuredResult = result.Ensure(x => true, "validation message");

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreSame(originalError, ensuredResult.Error);
	}

	[TestMethod]
	public void Ensure_CanChainMultipleValidations() {
		// Arrange
		var result = Result<int>.Success(15);

		// Act
		var ensuredResult = result
			.Ensure(x => x > 0, "Value must be positive")
			.Ensure(x => x < 100, "Value must be less than 100")
			.Ensure(x => x % 5 == 0, "Value must be divisible by 5");

		// Assert
		Assert.IsTrue(ensuredResult.IsSuccess);
		Assert.AreEqual(15, ensuredResult.Value);
	}

	[TestMethod]
	public void Ensure_ChainedValidations_StopOnFirstFailure() {
		// Arrange
		var result = Result<int>.Success(150);
		var thirdValidationCalled = false;

		// Act
		var ensuredResult = result
			.Ensure(x => x > 0, "Value must be positive")
			.Ensure(x => x < 100, "Value must be less than 100")
			.Ensure(x => {
				thirdValidationCalled = true;
				return x % 5 == 0;
			}, "Value must be divisible by 5");

		// Assert
		Assert.IsFalse(ensuredResult.IsSuccess);
		Assert.AreEqual("Value must be less than 100", ensuredResult.Error!.Message);
		Assert.IsFalse(thirdValidationCalled);
	}

	#endregion
}
