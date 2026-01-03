namespace Cirreum.ResultMonad.Tests;


[TestClass]
public sealed class ResultAsyncExtensionTests {

	#region OnSuccessAsync Tests

	[TestMethod]
	public async Task OnSuccessAsync_WithValueTask_WhenSuccessful_ShouldExecuteAction() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));
		var executed = false;
		var capturedValue = 0;

		// Act
		var result = await resultTask.OnSuccessAsync(x => {
			executed = true;
			capturedValue = x;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.AreEqual(42, capturedValue);
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(42, result.Value);
	}

	[TestMethod]
	public async Task OnSuccessAsync_WithValueTask_WhenFailed_ShouldNotExecuteAction() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("error")));
		var executed = false;

		// Act
		var result = await resultTask.OnSuccessAsync(x => executed = true);

		// Assert
		Assert.IsFalse(executed);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task OnSuccessAsync_WithTask_WhenSuccessful_ShouldExecuteAction() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(42));
		var executed = false;

		// Act
		var result = await resultTask.OnSuccessAsyncTask(x => executed = true);

		// Assert
		Assert.IsTrue(executed);
		Assert.IsTrue(result.IsSuccess);
	}

	#endregion

	#region OnFailureAsync Tests

	[TestMethod]
	public async Task OnFailureAsync_WithValueTask_WhenFailed_ShouldExecuteAction() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("async error")));
		var executed = false;
		var capturedMessage = "";

		// Act
		var result = await resultTask.OnFailureAsync(ex => {
			executed = true;
			capturedMessage = ex.Message;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.AreEqual("async error", capturedMessage);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task OnFailureAsync_WithTask_WhenFailed_ShouldExecuteAction() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Fail(new("async error")));
		var executed = false;

		// Act
		await resultTask.OnFailureAsyncTask(ex => executed = true);

		// Assert
		Assert.IsTrue(executed);
	}

	#endregion

	#region MapAsync Tests

	[TestMethod]
	public async Task MapAsync_WhenSuccessful_ShouldTransform() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var result = await resultTask.MapAsync(x => x.ToString());

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("42", result.Value);
	}

	[TestMethod]
	public async Task MapAsync_WhenFailed_ShouldPreserveFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("error")));

		// Act
		var result = await resultTask.MapAsync(x => x.ToString());

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("error", result.Error.Message);
	}

	#endregion

	#region ThenAsync Tests

	[TestMethod]
	public async Task ThenAsync_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Success($"Value: {x}");
		});

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Value: 42", result.Value);
	}

	[TestMethod]
	public async Task ThenAsync_WhenOriginalFailed_ShouldPropagateFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("async error")));

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Success("shouldn't reach");
		});

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("async error", result.Error.Message);
	}

	[TestMethod]
	public async Task ThenAsync_WhenChainedOperationFails_ShouldPropagateChainedFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Fail(new("chained error"));
		});

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("chained error", result.Error.Message);
	}

	#endregion

	#region EnsureAsync Tests

	[TestMethod]
	public async Task EnsureAsync_WithTruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsync(x => x > 5, _ => new Exception("Value too small"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsync_WithFalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsync(x => x > 5, _ => new Exception("Value too small"));

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("Value too small", result.Error.Message);
	}

	[TestMethod]
	public async Task EnsureAsync_OnAlreadyFailedResult_ShouldPreserveOriginalError() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new Exception("original error")));

		// Act
		var result = await resultTask.EnsureAsync(x => true, _ => new Exception("new error"));

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("original error", result.Error!.Message);
	}

	[TestMethod]
	public async Task EnsureAsync_WithStringOverload_WithTruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsync(x => x > 5, "Value too small");

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsync_WithStringOverload_WithFalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsync(x => x > 5, "Value too small");

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsInstanceOfType<InvalidOperationException>(result.Error);
		Assert.AreEqual("Value too small", result.Error!.Message);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithTruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsyncTask(x => x > 5, _ => new Exception("Value too small"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithFalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsyncTask(x => x > 5, _ => new Exception("Value too small"));

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("Value too small", result.Error.Message);
	}

	[TestMethod]
	public async Task EnsureAsync_WithAsyncPredicate_TruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			_ => new Exception("Value too small"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsync_WithAsyncPredicate_FalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			_ => new Exception("Value too small"));

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Value too small", result.Error!.Message);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithAsyncPredicate_TruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsyncTask(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			_ => new Exception("Value too small"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithAsyncPredicate_FalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsyncTask(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			_ => new Exception("Value too small"));

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Value too small", result.Error!.Message);
	}

	[TestMethod]
	public async Task EnsureAsync_WithAsyncPredicate_StringError_TruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			"Value too small");

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsync_WithAsyncPredicate_StringError_FalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			"Value too small");

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsInstanceOfType<InvalidOperationException>(result.Error);
		Assert.AreEqual("Value too small", result.Error!.Message);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithAsyncPredicate_StringError_TruePredicate_ShouldRemainSuccessful() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(10));

		// Act
		var result = await resultTask.EnsureAsyncTask(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			"Value too small");

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(10, result.Value);
	}

	[TestMethod]
	public async Task EnsureAsyncTask_WithAsyncPredicate_StringError_FalsePredicate_ShouldBecomeFailure() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(3));

		// Act
		var result = await resultTask.EnsureAsyncTask(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return x > 5;
			},
			"Value too small");

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsInstanceOfType<InvalidOperationException>(result.Error);
		Assert.AreEqual("Value too small", result.Error!.Message);
	}

	#endregion

	public TestContext TestContext { get; set; }

	#region ToResultAsync Tests

	[TestMethod]
	public async Task ToResultAsync_WithValueTask_WhenSuccessful_ShouldReturnSuccess() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var result = await resultTask.ToResultAsync();

		// Assert
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ToResultAsync_WithValueTask_WhenFailed_ShouldReturnFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("test error")));

		// Act
		var result = await resultTask.ToResultAsync();

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("test error", result.Error.Message);
	}

	[TestMethod]
	public async Task ToResultAsyncTask_WithTask_WhenSuccessful_ShouldReturnSuccess() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(42));

		// Act
		var result = await resultTask.ToResultAsyncTask();

		// Assert
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ToResultAsyncTask_WithTask_WhenFailed_ShouldReturnFailure() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Fail(new("test error")));

		// Act
		var result = await resultTask.ToResultAsyncTask();

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.IsNotNull(result.Error);
		Assert.AreEqual("test error", result.Error.Message);
	}

	#endregion

	#region ThenAsync Result<T> to Result Tests

	[TestMethod]
	public async Task ThenAsync_ResultTToResult_WithValueTask_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			executed = true;
			return Result.Success;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ThenAsync_ResultTToResult_WithValueTask_WhenOriginalFailed_ShouldPropagateFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Fail(new("original error")));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			executed = true;
			return Result.Success;
		});

		// Assert
		Assert.IsFalse(executed);
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("original error", result.Error!.Message);
	}

	[TestMethod]
	public async Task ThenAsync_ResultTToResult_WithValueTask_WhenChainedFails_ShouldPropagateChainedFailure() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var result = await resultTask.ThenAsync(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result.Fail(new Exception("chained error"));
		});

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("chained error", result.Error!.Message);
	}

	[TestMethod]
	public async Task ThenAsyncTask_ResultTToResult_WithTask_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(42));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsyncTask(async x => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			executed = true;
			return Result.Success;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ThenAsync_ResultTToResult_SyncSelector_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsync(x => {
			executed = true;
			return Result.Success;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ThenAsyncTask_ResultTToResult_SyncSelector_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(42));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsyncTask(x => {
			executed = true;
			return Result.Success;
		});

		// Assert
		Assert.IsTrue(executed);
		Assert.IsTrue(result.IsSuccess);
	}

	#endregion

	#region ThenAsync Result to Result<T> Tests

	[TestMethod]
	public async Task ThenAsync_ResultToResultT_WithValueTask_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = new ValueTask<Result>(Result.Success);

		// Act
		var result = await resultTask.ThenAsync(async () => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Success("chained value");
		});

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("chained value", result.Value);
	}

	[TestMethod]
	public async Task ThenAsync_ResultToResultT_WithValueTask_WhenOriginalFailed_ShouldPropagateFailure() {
		// Arrange
		var resultTask = new ValueTask<Result>(Result.Fail(new Exception("original error")));
		var executed = false;

		// Act
		var result = await resultTask.ThenAsync(async () => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			executed = true;
			return Result<string>.Success("shouldn't reach");
		});

		// Assert
		Assert.IsFalse(executed);
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("original error", result.Error!.Message);
	}

	[TestMethod]
	public async Task ThenAsync_ResultToResultT_WithValueTask_WhenChainedFails_ShouldPropagateChainedFailure() {
		// Arrange
		var resultTask = new ValueTask<Result>(Result.Success);

		// Act
		var result = await resultTask.ThenAsync(async () => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Fail(new Exception("chained error"));
		});

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("chained error", result.Error!.Message);
	}

	[TestMethod]
	public async Task ThenAsyncTask_ResultToResultT_WithTask_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = Task.FromResult(Result.Success);

		// Act
		var result = await resultTask.ThenAsyncTask(async () => {
			await Task.Delay(1, this.TestContext.CancellationToken);
			return Result<string>.Success("chained value");
		});

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("chained value", result.Value);
	}

	[TestMethod]
	public async Task ThenAsync_ResultToResultT_SyncSelector_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = new ValueTask<Result>(Result.Success);

		// Act
		var result = await resultTask.ThenAsync(() => Result<string>.Success("chained value"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("chained value", result.Value);
	}

	[TestMethod]
	public async Task ThenAsyncTask_ResultToResultT_SyncSelector_WhenSuccessful_ShouldChain() {
		// Arrange
		var resultTask = Task.FromResult(Result.Success);

		// Act
		var result = await resultTask.ThenAsyncTask(() => Result<string>.Success("chained value"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("chained value", result.Value);
	}

	#endregion

	#region MatchAsync Tests

	[TestMethod]
	public async Task MatchAsync_Result_WithValueTask_WhenSuccessful_ReturnsValueTaskResult() {
		// Arrange
		var result = Result.Success;

		// Act
		var matchTask = result.MatchAsync(
			async () => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "success";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("success", value);
	}

	[TestMethod]
	public async Task MatchAsync_Result_WithValueTask_WhenFailed_ReturnsValueTaskResult() {
		// Arrange
		var result = Result.Fail(new Exception("test error"));

		// Act
		var matchTask = result.MatchAsync(
			async () => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "success";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"failure: {ex.Message}";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("failure: test error", value);
	}

	[TestMethod]
	public async Task MatchAsync_ResultT_WithValueTask_WhenSuccessful_ReturnsValueTaskResult() {
		// Arrange
		var result = Result<int>.Success(42);

		// Act
		var matchTask = result.MatchAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"value: {x}";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("value: 42", value);
	}

	[TestMethod]
	public async Task MatchAsync_ResultT_WithValueTask_WhenFailed_ReturnsValueTaskResult() {
		// Arrange
		var result = Result<int>.Fail(new Exception("test error"));

		// Act
		var matchTask = result.MatchAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"value: {x}";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"failure: {ex.Message}";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("failure: test error", value);
	}

	[TestMethod]
	public async Task MatchAsync_ValueTaskResult_WhenSuccessful_ReturnsValueTaskResult() {
		// Arrange
		var resultTask = new ValueTask<Result>(Result.Success);

		// Act
		var matchTask = resultTask.MatchAsync(
			async () => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "success";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("success", value);
	}

	[TestMethod]
	public async Task MatchAsync_ValueTaskResultT_WhenSuccessful_ReturnsValueTaskResult() {
		// Arrange
		var resultTask = new ValueTask<Result<int>>(Result<int>.Success(42));

		// Act
		var matchTask = resultTask.MatchAsync(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"value: {x}";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("value: 42", value);
	}

	[TestMethod]
	public async Task MatchAsyncTask_TaskResult_WhenSuccessful_ReturnsTaskResult() {
		// Arrange
		var resultTask = Task.FromResult(Result.Success);

		// Act
		var matchTask = resultTask.MatchAsyncTask(
			async () => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "success";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("success", value);
	}

	[TestMethod]
	public async Task MatchAsyncTask_TaskResultT_WhenSuccessful_ReturnsTaskResult() {
		// Arrange
		var resultTask = Task.FromResult(Result<int>.Success(42));

		// Act
		var matchTask = resultTask.MatchAsyncTask(
			async x => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return $"value: {x}";
			},
			async ex => {
				await Task.Delay(1, this.TestContext.CancellationToken);
				return "failure";
			});

		var value = await matchTask;

		// Assert
		Assert.AreEqual("value: 42", value);
	}

	#endregion

}