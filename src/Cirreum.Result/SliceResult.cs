namespace Cirreum;

/// <summary>
/// Represents a slice of results with an indicator for whether more items exist.
/// </summary>
/// <remarks>
/// <para>
/// Use this when you need a simple "load more" pattern without full pagination metadata.
/// Typically implemented by fetching N+1 items and checking if more exist.
/// </para>
/// <para>
/// This is ideal for scenarios where you don't need cursor stability or page numbers,
/// such as loading initial data with a "Show More" button, or batch processing.
/// </para>
/// <para>
/// For stable pagination across requests (e.g., infinite scroll with inserts/deletes),
/// consider <see cref="CursorResult{T}"/>. For full pagination metadata with
/// page numbers and total counts, consider <see cref="PagedResult{T}"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Items">The items for the current slice.</param>
/// <param name="HasMore">A value indicating whether additional items exist beyond this slice.</param>
public sealed record SliceResult<T>(
	IReadOnlyList<T> Items,
	bool HasMore) {

	/// <summary>
	/// Gets an empty slice with no additional items.
	/// </summary>
	public static SliceResult<T> Empty => new([], false);

	/// <summary>
	/// Gets the number of items contained in the current slice.
	/// </summary>
	public int Count => this.Items.Count;

	/// <summary>
	/// Gets a value indicating whether the slice contains no items.
	/// </summary>
	public bool IsEmpty => this.Items.Count == 0;

	/// <summary>
	/// Projects each item in the slice to a new form while preserving pagination metadata.
	/// </summary>
	/// <typeparam name="TResult">The type of items in the resulting slice.</typeparam>
	/// <param name="selector">A transform function to apply to each item.</param>
	/// <returns>A new <see cref="SliceResult{TResult}"/> containing the transformed items.</returns>
	public SliceResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
		new([.. this.Items.Select(selector)], this.HasMore);
}