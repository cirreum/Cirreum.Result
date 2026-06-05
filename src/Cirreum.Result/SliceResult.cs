namespace Cirreum;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a slice of results with an indicator for whether more items exist.
/// </summary>
public sealed record SliceResult<T> {

	/// <summary>
	/// Gets the items for the current slice.
	/// </summary>
	/// <param name="items">The items for the current slice.</param>
	/// <param name="hasMore">A value indicating whether additional items exist beyond this slice.</param>
	public SliceResult(IReadOnlyList<T> items, bool hasMore) {
		ArgumentNullException.ThrowIfNull(items, nameof(items));

		this.Items = [.. items];
		this.HasMore = hasMore;
	}

	/// <summary>
	/// Gets an empty slice with no additional items.
	/// </summary>
	public static SliceResult<T> Empty => new([], false);

	/// <summary>
	/// Gets the items for the current slice.
	/// </summary>
	public IReadOnlyList<T> Items { get; }

	/// <summary>
	/// Gets a value indicating whether additional items exist beyond this slice.
	/// </summary>
	public bool HasMore { get; }

	/// <summary>
	/// Gets the number of items contained in the current slice.
	/// </summary>
	[JsonIgnore]
	public int Count => this.Items.Count;

	/// <summary>
	/// Gets a value indicating whether the slice contains no items.
	/// </summary>
	[JsonIgnore]
	public bool IsEmpty => this.Items.Count == 0;

	/// <summary>
	/// Projects each item in the slice to a new form while preserving pagination metadata.
	/// </summary>
	public SliceResult<TResult> Map<TResult>(Func<T, TResult> selector) {
		ArgumentNullException.ThrowIfNull(selector);

		return new([.. this.Items.Select(selector)], this.HasMore);
	}

}