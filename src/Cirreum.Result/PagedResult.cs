namespace Cirreum;

/// <summary>
/// Represents a paginated result set using offset-based pagination.
/// </summary>
/// <remarks>
/// <para>
/// Use offset pagination when clients need to jump to arbitrary pages or display total counts.
/// This approach works well for smaller datasets and traditional paged interfaces with numbered pages.
/// </para>
/// <para>
/// <strong>Trade-offs:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Typically requires two queries (count + data)</description></item>
///   <item><description>Performance degrades on deep pages due to offset scanning</description></item>
///   <item><description>Results can shift if data is inserted or deleted between requests</description></item>
/// </list>
/// <para>
/// For large datasets, real-time data, or infinite scroll interfaces where consistency matters,
/// consider <see cref="CursorResult{T}"/>. For simple "load more" without counts,
/// consider <see cref="SliceResult{T}"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Items">The items for the current page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <param name="PageNumber">The current page number (1-based).</param>
public sealed record PagedResult<T>(
	IReadOnlyList<T> Items,
	int TotalCount,
	int PageSize,
	int PageNumber) {

	/// <summary>
	/// Gets an empty paged result with the specified page size.
	/// </summary>
	/// <param name="pageSize">The page size to use. Defaults to 25.</param>
	/// <returns>An empty <see cref="PagedResult{T}"/> on page 1 with zero total count.</returns>
	public static PagedResult<T> Empty(int pageSize = 25) =>
		new([], 0, pageSize, 1);

	/// <summary>
	/// Gets the number of items contained in the current page.
	/// </summary>
	public int Count => this.Items.Count;

	/// <summary>
	/// Gets a value indicating whether the result contains no items.
	/// </summary>
	public bool IsEmpty => this.Items.Count == 0;

	/// <summary>
	/// Gets the total number of pages available.
	/// </summary>
	public int TotalPages => this.PageSize > 0
		? (int)Math.Ceiling((double)this.TotalCount / this.PageSize)
		: 0;

	/// <summary>
	/// Gets a value indicating whether there is a subsequent page.
	/// </summary>
	public bool HasNextPage => this.PageNumber < this.TotalPages;

	/// <summary>
	/// Gets a value indicating whether there is a preceding page.
	/// </summary>
	public bool HasPreviousPage => this.PageNumber > 1;

	/// <summary>
	/// Projects each item in the result to a new form while preserving pagination metadata.
	/// </summary>
	/// <typeparam name="TResult">The type of items in the resulting set.</typeparam>
	/// <param name="selector">A transform function to apply to each item.</param>
	/// <returns>A new <see cref="PagedResult{TResult}"/> containing the transformed items.</returns>
	public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) =>
		new([.. this.Items.Select(selector)], this.TotalCount, this.PageSize, this.PageNumber);

}