namespace Cirreum.ResultMonad.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.Json;

[TestClass]
public class PaginationSerializationTests {

	private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

	[TestMethod]
	public void PagedResult_RoundTrips_AndRecomputesDerivedState() {
		var original = new PagedResult<int>([1, 2, 3], 10, 3, 1);

		var back = JsonSerializer.Deserialize<PagedResult<int>>(JsonSerializer.Serialize(original, Web), Web)!;

		Assert.AreEqual(3, back.Items.Count);
		Assert.AreEqual(1, back.Items[0]);
		Assert.AreEqual(10, back.TotalCount);
		Assert.AreEqual(3, back.PageSize);
		Assert.AreEqual(1, back.PageNumber);
		// Derived state recomputes correctly from the round-tripped data.
		Assert.AreEqual(4, back.TotalPages);
		Assert.IsTrue(back.HasNextPage);
	}

	[TestMethod]
	public void PagedResult_RejectsInvalidArguments() {
		Assert.ThrowsExactly<ArgumentNullException>(() => new PagedResult<int>(null!, 0, 10, 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PagedResult<int>([], -1, 10, 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PagedResult<int>([], 0, -1, 1));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PagedResult<int>([], 0, 10, 0));
	}

	[TestMethod]
	public void CursorResult_RoundTrips_PreservingInitProperties() {
		var original = new CursorResult<string>(["a", "b"], "next", true) {
			PreviousCursor = "prev",
			TotalCount = 42
		};

		var back = JsonSerializer.Deserialize<CursorResult<string>>(JsonSerializer.Serialize(original, Web), Web)!;

		Assert.AreEqual("next", back.NextCursor);
		Assert.IsTrue(back.HasNextPage);
		Assert.AreEqual("prev", back.PreviousCursor);
		Assert.AreEqual(42, back.TotalCount);
		Assert.IsTrue(back.HasPreviousPage);
	}

	[TestMethod]
	public void SliceResult_NullItems_Throws() {
		Assert.ThrowsExactly<ArgumentNullException>(() => new SliceResult<int>(null!, false));
	}

	[TestMethod]
	public void ComputedProperties_AreOmittedFromJson() {
		AssertOmits(JsonSerializer.Serialize(new SliceResult<int>([1], true), Web),
			"count", "isEmpty");
		AssertOmits(JsonSerializer.Serialize(new CursorResult<int>([1], "c", true), Web),
			"count", "isEmpty", "hasPreviousPage");
		AssertOmits(JsonSerializer.Serialize(new PagedResult<int>([1], 1, 10, 1), Web),
			"count", "isEmpty", "totalPages", "hasNextPage", "hasPreviousPage");
	}

	private static void AssertOmits(string json, params string[] forbidden) {
		using var doc = JsonDocument.Parse(json);
		var names = new HashSet<string>();
		foreach (var p in doc.RootElement.EnumerateObject()) {
			names.Add(p.Name);
		}
		foreach (var name in forbidden) {
			Assert.IsFalse(names.Contains(name), $"'{name}' should be omitted from the wire. JSON: {json}");
		}
	}

}
