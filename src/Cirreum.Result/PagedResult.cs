namespace Cirreum;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a paginated result set using offset-based pagination.
/// </summary>
public sealed record PagedResult<T> {

	/// <summary>
	/// Gets the items for the current page.
	/// </summary>
	/// <param name="items">The items for the current page.</param>
	/// <param name="totalCount">The total number of items across all pages.</param>
	/// <param name="pageSize">The maximum number of items per page.</param>
	/// <param name="pageNumber">The current page number.</param>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public PagedResult(
		IReadOnlyList<T> items,
		int totalCount,
		int pageSize,
		int pageNumber) {

		ArgumentNullException.ThrowIfNull(items, nameof(items));

		if (totalCount < 0) {
			throw new ArgumentOutOfRangeException(nameof(totalCount), totalCount, "Total count cannot be negative.");
		}

		if (pageSize < 1) {
			throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");
		}

		if (pageNumber < 1) {
			throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be 1 or greater.");
		}

		this.Items = [.. items];
		this.TotalCount = totalCount;
		this.PageSize = pageSize;
		this.PageNumber = pageNumber;
	}

	/// <summary>
	/// Gets an empty paged result with the specified page size.
	/// </summary>
	public static PagedResult<T> Empty(int pageSize = 25) =>
		new([], 0, pageSize, 1);

	/// <summary>
	/// Gets the items for the current page.
	/// </summary>
	public IReadOnlyList<T> Items { get; }

	/// <summary>
	/// Gets the total number of items across all pages.
	/// </summary>
	public int TotalCount { get; }

	/// <summary>
	/// Gets the maximum number of items per page.
	/// </summary>
	public int PageSize { get; }

	/// <summary>
	/// Gets the current page number.
	/// </summary>
	public int PageNumber { get; }

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
	/// Gets the total number of pages available.
	/// </summary>
	[JsonIgnore]
	public int TotalPages => (int)Math.Ceiling((double)this.TotalCount / this.PageSize);

	/// <summary>
	/// Gets a value indicating whether there is a subsequent page.
	/// </summary>
	[JsonIgnore]
	public bool HasNextPage => this.PageNumber < this.TotalPages;

	/// <summary>
	/// Gets a value indicating whether there is a preceding page.
	/// </summary>
	[JsonIgnore]
	public bool HasPreviousPage => this.PageNumber > 1;

	/// <summary>
	/// Projects each item in the result to a new form while preserving pagination metadata.
	/// </summary>
	public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector) {
		ArgumentNullException.ThrowIfNull(selector);

		return new(
			[.. this.Items.Select(selector)],
			this.TotalCount,
			this.PageSize,
			this.PageNumber);
	}

}