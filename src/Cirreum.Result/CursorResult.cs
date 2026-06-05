namespace Cirreum;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a paginated result set using cursor-based pagination.
/// </summary>
public sealed record CursorResult<T> {

	public CursorResult(
		IReadOnlyList<T> items,
		string? nextCursor,
		bool hasNextPage) {

		ArgumentNullException.ThrowIfNull(items, nameof(items));

		this.Items = [.. items];
		this.NextCursor = nextCursor;
		this.HasNextPage = hasNextPage;
	}

	/// <summary>
	/// Gets an empty cursor result with no items or pagination.
	/// </summary>
	public static CursorResult<T> Empty => new([], null, false);

	/// <summary>
	/// Gets the items for the current page.
	/// </summary>
	public IReadOnlyList<T> Items { get; }

	/// <summary>
	/// Gets the cursor to fetch the next page, or null if there are no more items.
	/// </summary>
	public string? NextCursor { get; }

	/// <summary>
	/// Gets a value indicating whether there is a subsequent page.
	/// </summary>
	public bool HasNextPage { get; }

	/// <summary>
	/// Gets the cursor to fetch the previous page, or null if this is the first page.
	/// </summary>
	public string? PreviousCursor { get; init; }

	/// <summary>
	/// Gets the total number of items across all pages, if known.
	/// </summary>
	public int? TotalCount { get; init; }

	/// <summary>
	/// Gets the number of items contained in the current page.
	/// </summary>
	[JsonIgnore]
	public int Count => this.Items.Count;

	/// <summary>
	/// Gets a value indicating whether the result contains no items.
	/// </summary>
	[JsonIgnore]
	public bool IsEmpty => this.Items.Count == 0;

	/// <summary>
	/// Gets a value indicating whether there is a preceding page.
	/// </summary>
	[JsonIgnore]
	public bool HasPreviousPage => this.PreviousCursor is not null;

	/// <summary>
	/// Projects each item in the result to a new form while preserving pagination metadata.
	/// </summary>
	public CursorResult<TResult> Map<TResult>(Func<T, TResult> selector) {
		ArgumentNullException.ThrowIfNull(selector);

		return new([.. this.Items.Select(selector)], this.NextCursor, this.HasNextPage) {
			PreviousCursor = this.PreviousCursor,
			TotalCount = this.TotalCount
		};
	}

}