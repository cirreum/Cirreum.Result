namespace Cirreum.ResultMonad.Tests;

using Cirreum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class OptionalTests {

	#region Factory Methods

	[TestMethod]
	public void From_WithValue_CreatesOptionalWithValue() {
		// Arrange & Act
		var optional = Optional.From("hello");

		// Assert
		Assert.IsTrue(optional.HasValue);
		Assert.IsFalse(optional.IsEmpty);
		Assert.AreEqual("hello", optional.Value);
	}

	[TestMethod]
	public void From_WithNull_CreatesEmptyOptional() {
		// Arrange & Act
		var optional = Optional.From<string>(null);

		// Assert
		Assert.IsFalse(optional.HasValue);
	}

	[TestMethod]
	public void Empty_CreatesOptionalWithNoValue() {
		// Arrange & Act
		var optional = Optional<int>.Empty;

		// Assert
		Assert.IsFalse(optional.HasValue);
		Assert.IsTrue(optional.IsEmpty);
	}

	[TestMethod]
	public void StaticFrom_WithValue_CreatesOptionalWithValue() {
		// Arrange & Act
		var optional = Optional.From("world");

		// Assert
		Assert.IsTrue(optional.HasValue);
		Assert.AreEqual("world", optional.Value);
	}

	[TestMethod]
	public void StaticFrom_WithNull_CreatesEmptyOptional() {
		// Arrange & Act
		var optional = Optional.From<string>(null);

		// Assert
		Assert.IsFalse(optional.HasValue);
	}

	[TestMethod]
	public void StaticEmpty_CreatesOptionalWithNoValue() {
		// Arrange & Act
		var optional = Optional<int>.Empty;

		// Assert
		Assert.IsFalse(optional.HasValue);
	}

	#endregion

	#region Value Access

	[TestMethod]
	public void Value_WhenHasValue_ReturnsValue() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var value = optional.Value;

		// Assert
		Assert.AreEqual(42, value);
	}

	[TestMethod]
	public void Value_WhenEmpty_ThrowsInvalidOperationException() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => _ = optional.Value);
	}

	[TestMethod]
	public void TryGetValue_WhenHasValue_ReturnsTrueAndOutputsValue() {
		// Arrange
		var optional = Optional.From("test");

		// Act
		var success = optional.TryGetValue(out var value);

		// Assert
		Assert.IsTrue(success);
		Assert.AreEqual("test", value);
	}

	[TestMethod]
	public void TryGetValue_WhenEmpty_ReturnsFalseAndOutputsDefault() {
		// Arrange
		var optional = Optional<string>.Empty;

		// Act
		var success = optional.TryGetValue(out var value);

		// Assert
		Assert.IsFalse(success);
		Assert.IsNull(value);
	}

	[TestMethod]
	public void Deconstruct_WhenHasValue_ReturnsComponents() {
		// Arrange
		var optional = Optional.From(99);

		// Act
		var (hasValue, value) = optional;

		// Assert
		Assert.IsTrue(hasValue);
		Assert.AreEqual(99, value);
	}

	[TestMethod]
	public void Deconstruct_WhenEmpty_ReturnsComponents() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var (hasValue, value) = optional;

		// Assert
		Assert.IsFalse(hasValue);
		Assert.AreEqual(default, value);
	}

	#endregion

	#region Map

	[TestMethod]
	public void Map_WhenHasValue_TransformsValue() {
		// Arrange
		var optional = Optional.From(5);

		// Act
		var mapped = optional.Map(x => x * 2);

		// Assert
		Assert.IsTrue(mapped.HasValue);
		Assert.AreEqual(10, mapped.Value);
	}

	[TestMethod]
	public void Map_WhenEmpty_ReturnsEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;
		var selectorCalled = false;

		// Act
		var mapped = optional.Map(x => {
			selectorCalled = true;
			return x * 2;
		});

		// Assert
		Assert.IsFalse(mapped.HasValue);
		Assert.IsFalse(selectorCalled);
	}

	[TestMethod]
	public void Map_WhenSelectorReturnsNull_ReturnsEmpty() {
		// Arrange
		var optional = Optional.From("hello");

		// Act
		var mapped = optional.Map<string?>(_ => null);

		// Assert
		Assert.IsFalse(mapped.HasValue);
	}

	[TestMethod]
	public void Map_ThrowsArgumentNullException_WhenSelectorIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => optional.Map<int>(null!));
	}

	#endregion

	#region Then

	[TestMethod]
	public void Then_WhenHasValue_ChainsOperation() {
		// Arrange
		var optional = Optional.From(2);

		// Act
		var chained = optional.Then(x => Optional.From($"Value: {x}"));

		// Assert
		Assert.IsTrue(chained.HasValue);
		Assert.AreEqual("Value: 2", chained.Value);
	}

	[TestMethod]
	public void Then_WhenEmpty_ReturnsEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;
		var selectorCalled = false;

		// Act
		var chained = optional.Then(x => {
			selectorCalled = true;
			return Optional.From(x.ToString());
		});

		// Assert
		Assert.IsFalse(chained.HasValue);
		Assert.IsFalse(selectorCalled);
	}

	[TestMethod]
	public void Then_WhenChainedOperationReturnsEmpty_ReturnsEmpty() {
		// Arrange
		var optional = Optional.From(5);

		// Act
		var chained = optional.Then(_ => Optional<string>.Empty);

		// Assert
		Assert.IsFalse(chained.HasValue);
	}

	[TestMethod]
	public void Then_ThrowsArgumentNullException_WhenSelectorIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => optional.Then<int>(null!));
	}

	#endregion

	#region Match

	[TestMethod]
	public void Match_WhenHasValue_InvokesOnValue() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var result = optional.Match(
			onValue: v => $"Got: {v}",
			onEmpty: () => "Empty");

		// Assert
		Assert.AreEqual("Got: 42", result);
	}

	[TestMethod]
	public void Match_WhenEmpty_InvokesOnEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var result = optional.Match(
			onValue: v => $"Got: {v}",
			onEmpty: () => "Empty");

		// Assert
		Assert.AreEqual("Empty", result);
	}

	[TestMethod]
	public void Match_ThrowsArgumentNullException_WhenOnValueIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.Match(null!, () => 0));
	}

	[TestMethod]
	public void Match_ThrowsArgumentNullException_WhenOnEmptyIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.Match(v => v, null!));
	}

	#endregion

	#region Switch

	[TestMethod]
	public void Switch_WhenHasValue_InvokesOnValue() {
		// Arrange
		var optional = Optional.From(10);
		var onValueCalled = false;
		var onEmptyCalled = false;
		var receivedValue = 0;

		// Act
		optional.Switch(
			onValue: v => {
				onValueCalled = true;
				receivedValue = v;
			},
			onEmpty: () => onEmptyCalled = true);

		// Assert
		Assert.IsTrue(onValueCalled);
		Assert.IsFalse(onEmptyCalled);
		Assert.AreEqual(10, receivedValue);
	}

	[TestMethod]
	public void Switch_WhenEmpty_InvokesOnEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;
		var onValueCalled = false;
		var onEmptyCalled = false;

		// Act
		optional.Switch(
			onValue: _ => onValueCalled = true,
			onEmpty: () => onEmptyCalled = true);

		// Assert
		Assert.IsFalse(onValueCalled);
		Assert.IsTrue(onEmptyCalled);
	}

	[TestMethod]
	public void Switch_ThrowsArgumentNullException_WhenOnValueIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.Switch(null!, () => { }));
	}

	[TestMethod]
	public void Switch_ThrowsArgumentNullException_WhenOnEmptyIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.Switch(_ => { }, null!));
	}

	#endregion

	#region Where

	[TestMethod]
	public void Where_WhenHasValueAndPredicateTrue_ReturnsSameOptional() {
		// Arrange
		var optional = Optional.From(10);

		// Act
		var filtered = optional.Where(x => x > 5);

		// Assert
		Assert.IsTrue(filtered.HasValue);
		Assert.AreEqual(10, filtered.Value);
	}

	[TestMethod]
	public void Where_WhenHasValueAndPredicateFalse_ReturnsEmpty() {
		// Arrange
		var optional = Optional.From(3);

		// Act
		var filtered = optional.Where(x => x > 5);

		// Assert
		Assert.IsFalse(filtered.HasValue);
	}

	[TestMethod]
	public void Where_WhenEmpty_ReturnsEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;
		var predicateCalled = false;

		// Act
		var filtered = optional.Where(x => {
			predicateCalled = true;
			return true;
		});

		// Assert
		Assert.IsFalse(filtered.HasValue);
		Assert.IsFalse(predicateCalled);
	}

	[TestMethod]
	public void Where_ThrowsArgumentNullException_WhenPredicateIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => optional.Where(null!));
	}

	#endregion

	#region GetValueOrDefault

	[TestMethod]
	public void GetValueOrDefault_WhenHasValue_ReturnsValue() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var value = optional.GetValueOrDefault(0);

		// Assert
		Assert.AreEqual(42, value);
	}

	[TestMethod]
	public void GetValueOrDefault_WhenEmpty_ReturnsDefaultValue() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var value = optional.GetValueOrDefault(99);

		// Assert
		Assert.AreEqual(99, value);
	}

	[TestMethod]
	public void GetValueOrDefault_WithFactory_WhenHasValue_ReturnsValue() {
		// Arrange
		var optional = Optional.From(42);
		var factoryCalled = false;

		// Act
		var value = optional.GetValueOrDefault(() => {
			factoryCalled = true;
			return 0;
		});

		// Assert
		Assert.AreEqual(42, value);
		Assert.IsFalse(factoryCalled);
	}

	[TestMethod]
	public void GetValueOrDefault_WithFactory_WhenEmpty_InvokesFactory() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var value = optional.GetValueOrDefault(() => 99);

		// Assert
		Assert.AreEqual(99, value);
	}

	[TestMethod]
	public void GetValueOrDefault_WithFactory_ThrowsArgumentNullException_WhenFactoryIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.GetValueOrDefault((Func<int>)null!));
	}

	#endregion

	#region GetValueOrNull

	[TestMethod]
	public void GetValueOrNull_WhenHasValue_ReturnsValue() {
		// Arrange
		var optional = Optional.From("hello");

		// Act
		var value = optional.GetValueOrNull();

		// Assert
		Assert.AreEqual("hello", value);
	}

	[TestMethod]
	public void GetValueOrNull_WhenEmpty_ReturnsNull() {
		// Arrange
		var optional = Optional<string>.Empty;

		// Act
		var value = optional.GetValueOrNull();

		// Assert
		Assert.IsNull(value);
	}

	[TestMethod]
	public void GetValueOrNull_WithValueType_WhenHasValue_ReturnsValue() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var value = optional.GetValueOrNull();

		// Assert
		Assert.AreEqual(42, value);
	}

	[TestMethod]
	public void GetValueOrNull_WithValueType_WhenEmpty_ReturnsDefault() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var value = optional.GetValueOrNull();

		// Assert
		Assert.AreEqual(default, value);
	}

	#endregion

	#region ToResult

	[TestMethod]
	public void ToResult_WhenHasValue_ReturnsSuccessResult() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var result = optional.ToResult(new InvalidOperationException("Should not be used"));

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(42, result.Value);
	}

	[TestMethod]
	public void ToResult_WhenEmpty_ReturnsFailedResult() {
		// Arrange
		var optional = Optional<int>.Empty;
		var ex = new InvalidOperationException("No value");

		// Act
		var result = optional.ToResult(ex);

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreSame(ex, result.Error);
	}

	[TestMethod]
	public void ToResult_WithFactory_WhenHasValue_ReturnsSuccessResult() {
		// Arrange
		var optional = Optional.From(42);
		var factoryCalled = false;

		// Act
		var result = optional.ToResult(() => {
			factoryCalled = true;
			return new InvalidOperationException("Should not be used");
		});

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(42, result.Value);
		Assert.IsFalse(factoryCalled);
	}

	[TestMethod]
	public void ToResult_WithFactory_WhenEmpty_InvokesFactoryAndReturnsFailedResult() {
		// Arrange
		var optional = Optional<int>.Empty;
		var ex = new InvalidOperationException("No value");

		// Act
		var result = optional.ToResult(() => ex);

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreSame(ex, result.Error);
	}

	[TestMethod]
	public void ToResult_ThrowsArgumentNullException_WhenExceptionIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.ToResult((Exception)null!));
	}

	[TestMethod]
	public void ToResult_WithFactory_ThrowsArgumentNullException_WhenFactoryIsNull() {
		// Arrange
		var optional = Optional.From(1);

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() =>
			optional.ToResult((Func<Exception>)null!));
	}

	#endregion

	#region Equality

	[TestMethod]
	public void Equals_TwoOptionals_WithSameValue_AreEqual() {
		// Arrange
		var optional1 = Optional.From(42);
		var optional2 = Optional.From(42);

		// Act & Assert
		Assert.AreEqual(optional1, optional2);
		Assert.IsTrue(optional1 == optional2);
		Assert.IsFalse(optional1 != optional2);
	}

	[TestMethod]
	public void Equals_TwoOptionals_WithDifferentValues_AreNotEqual() {
		// Arrange
		var optional1 = Optional.From(42);
		var optional2 = Optional.From(99);

		// Act & Assert
		Assert.AreNotEqual(optional1, optional2);
		Assert.IsFalse(optional1 == optional2);
		Assert.IsTrue(optional1 != optional2);
	}

	[TestMethod]
	public void Equals_TwoEmptyOptionals_AreEqual() {
		// Arrange
		var optional1 = Optional<int>.Empty;
		var optional2 = Optional<int>.Empty;

		// Act & Assert
		Assert.AreEqual(optional1, optional2);
		Assert.IsTrue(optional1 == optional2);
	}

	[TestMethod]
	public void Equals_OptionalWithValue_AndEmptyOptional_AreNotEqual() {
		// Arrange
		var optional1 = Optional.From(42);
		var optional2 = Optional<int>.Empty;

		// Act & Assert
		Assert.AreNotEqual(optional1, optional2);
		Assert.IsFalse(optional1 == optional2);
	}

	[TestMethod]
	public void Equals_WithObject_WhenSameValue_ReturnsTrue() {
		// Arrange
		var optional1 = Optional.From(42);
		object optional2 = Optional.From(42);

		// Act & Assert
		Assert.IsTrue(optional1.Equals(optional2));
	}

	[TestMethod]
	public void Equals_WithObject_WhenDifferentType_ReturnsFalse() {
		// Arrange
		var optional = Optional.From(42);

		// Act & Assert
		Assert.IsFalse(optional.Equals("not an optional"));
		Assert.IsFalse(optional.Equals(null));
	}

	[TestMethod]
	public void GetHashCode_TwoOptionals_WithSameValue_HaveSameHashCode() {
		// Arrange
		var optional1 = Optional.From(42);
		var optional2 = Optional.From(42);

		// Act & Assert
		Assert.AreEqual(optional1.GetHashCode(), optional2.GetHashCode());
	}

	[TestMethod]
	public void GetHashCode_TwoEmptyOptionals_HaveSameHashCode() {
		// Arrange
		var optional1 = Optional<int>.Empty;
		var optional2 = Optional<int>.Empty;

		// Act & Assert
		Assert.AreEqual(optional1.GetHashCode(), optional2.GetHashCode());
	}

	#endregion

	#region ToString

	[TestMethod]
	public void ToString_WhenHasValue_ReturnsHasValueWithValue() {
		// Arrange
		var optional = Optional.From(42);

		// Act
		var str = optional.ToString();

		// Assert
		Assert.AreEqual("HasValue(42)", str);
	}

	[TestMethod]
	public void ToString_WhenEmpty_ReturnsIsEmpty() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Act
		var str = optional.ToString();

		// Assert
		Assert.AreEqual("IsEmpty", str);
	}

	#endregion

	#region IsEmpty Property

	[TestMethod]
	public void IsEmpty_WhenHasValue_ReturnsFalse() {
		// Arrange
		var optional = Optional.From(42);

		// Assert
		Assert.IsFalse(optional.IsEmpty);
	}

	[TestMethod]
	public void IsEmpty_WhenEmpty_ReturnsTrue() {
		// Arrange
		var optional = Optional<int>.Empty;

		// Assert
		Assert.IsTrue(optional.IsEmpty);
	}

	[TestMethod]
	public void IsEmpty_IsOppositeOfHasValue() {
		// Arrange
		var withValue = Optional.From("test");
		var empty = Optional<string>.Empty;

		// Assert
		Assert.AreEqual(!withValue.HasValue, withValue.IsEmpty);
		Assert.AreEqual(!empty.HasValue, empty.IsEmpty);
	}

	#endregion

}
