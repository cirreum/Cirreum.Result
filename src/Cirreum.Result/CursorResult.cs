namespace Cirreum;

/// <summary>
/// Represents a paginated result set using cursor-based (keyset) pagination.
/// </summary>
/// <remarks>
/// <para>
/// Cursor pagination provides stable results when data is being inserted or deleted, and performs
/// consistently regardless of how deep into the result set the client navigates. This makes it
/// ideal for large datasets, real-time data, and infinite scroll interfaces.
/// </para>
/// <para>
/// The cursor is an opaque token encoding the position within a sorted result set. Clients should
/// treat cursors as opaque strings and not attempt to parse or construct them directly. The format
/// and contents of cursor strings are determined by the persistence implementation that produces them.
/// </para>
/// <para>
/// For scenarios requiring arbitrary page jumps or total counts, consider <see cref="PagedResult{T}"/>.
/// For simple "load more" without cursor stability, consider <see cref="SliceResult{T}"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Items">The items for the current page.</param>
/// <param name="NextCursor">The cursor to fetch the next page, or null if there are no more items.</param>
/// <param name="HasNextPage">A value indicating whether there is a subsequent page.</param>
public sealed record CursorResult<T>(
	IReadOnlyList<T> Items,
	string? NextCursor,
	bool HasNextPage) {

	/// <summary>
	/// Gets an empty cursor result with no items or pagination.
	/// </summary>
	public static CursorResult<T> Empty => new([], null, false);

	/// <summary>
	/// Gets the number of items contained in the current page.
	/// </summary>
	public int Count => this.Items.Count;

	/// <summary>
	/// Gets a value indicating whether the result contains no items.
	/// </summary>
	public bool IsEmpty => this.Items.Count == 0;

	/// <summary>
	/// Gets the cursor to fetch the previous page, or null if this is the first page.
	/// </summary>
	public string? PreviousCursor { get; init; }

	/// <summary>
	/// Gets a value indicating whether there is a preceding page.
	/// </summary>
	public bool HasPreviousPage => this.PreviousCursor is not null;

	/// <summary>
	/// Gets the total number of items across all pages, if known.
	/// </summary>
	/// <remarks>
	/// This value is optional and may be null if computing the total count is too expensive.
	/// </remarks>
	public int? TotalCount { get; init; }

	/// <summary>
	/// Projects each item in the result to a new form while preserving pagination metadata.
	/// </summary>
	/// <typeparam name="TResult">The type of items in the resulting set.</typeparam>
	/// <param name="selector">A transform function to apply to each item.</param>
	/// <returns>A new <see cref="CursorResult{TResult}"/> containing the transformed items.</returns>
	public CursorResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
		new([.. this.Items.Select(selector)], this.NextCursor, this.HasNextPage) {
			PreviousCursor = this.PreviousCursor,
			TotalCount = this.TotalCount
		};

}