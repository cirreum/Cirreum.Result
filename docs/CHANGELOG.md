# Changelog

All notable changes to **Cirreum.Result** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For detailed migration steps on major version bumps, see the per-version migration
guides linked at the bottom of each entry.

---

## [Unreleased]

### Added

- **System.Text.Json round-trip support** for `Result`, `Result<T>`, and `Optional<T>` via a built-in `[JsonConverter]` — no registration required; it applies under any `JsonSerializerOptions`. A success serializes as `{ "isSuccess": true, "value": <T> }` and a failure as `{ "isSuccess": false, "error": { "type": "...", "message": "..." } }`. `Optional<T>` serializes transparently — a present value writes the bare value, an empty optional writes `null`.
- **`SurrogateResultException`** — the carrier a deserialized failure's `Error` becomes. Exceptions cannot round-trip through serialization, so it preserves the original error's full type name (`OriginalTypeFullName`), message, and an optional `State` bag (see `IErrorState`).
- **`IErrorState`** — an opt-in contract (`IReadOnlyDictionary<string, string> State`) that any exception can implement to have its serializable state captured into the error wire-format and round-tripped onto `SurrogateResultException.State`. A `string`→`string`, lossless, AOT-safe escape hatch (empty state is omitted from the wire); the implementing exception owns what it exposes.
- **`HasError<TException>()`** and **`HasError(this IResult, Type)`** extension methods — serialization-safe failure-type checks. They match a live error by runtime assignability and a deserialized (surrogate) error by exact original type name, so they behave identically before and after a serialization round-trip.

### Changed

- **Pagination result types** (`SliceResult<T>`, `CursorResult<T>`, `PagedResult<T>`) now validate their constructor arguments — `ArgumentNullException` for a null `items`, and `ArgumentOutOfRangeException` for a negative `totalCount`/`pageSize` or a `pageNumber` less than 1 — and defensively copy `Items`. Their computed properties (`Count`, `IsEmpty`, `TotalPages`, `HasNextPage`, `HasPreviousPage`) are now annotated `[JsonIgnore]`, so the serialized payload carries only the stored data.

### Removed

- **The pagination result types are no longer positional records.** The compiler-generated `Deconstruct` method and `with`-expression support are removed, and the constructor parameter names changed to camelCase (`Items` → `items`, `HasMore` → `hasMore`, `TotalCount` → `totalCount`, …). Reading the properties and positional construction (`new PagedResult<T>(items, total, size, page)`) are unchanged. See [MIGRATION-v2.md](MIGRATION-v2.md).
