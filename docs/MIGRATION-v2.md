# Migrating to Cirreum.Result v2.0.0 (from v1.x)

## Why v2

v2 makes `Result`, `Result<T>`, and `Optional<T>` **System.Text.Json round-trippable** — a foundational addition, since these types now cross caches, message buses, and HTTP payloads correctly. (Previously a `Result<T>` serialized through a serializing provider silently deserialized as a *failure*, because the struct's get-only properties left System.Text.Json nothing to populate.)

That serialization work is purely additive. The **only breaking change** is a hardening of the three pagination result types — `SliceResult<T>`, `CursorResult<T>`, `PagedResult<T>` — which moved from positional records to explicit-constructor records so they can validate their arguments. The major bump is for that change alone.

## Breaking Changes — Find/Replace Table

All three pagination types are affected identically. Property **reads** and **positional construction** are unchanged; only the record-generated members and parameter *names* changed.

| v1.x | v2.0.0 | Why |
|---|---|---|
| `var (items, hasMore) = slice;` | `var items = slice.Items; var hasMore = slice.HasMore;` | `Deconstruct` is no longer generated (not a positional record). |
| `slice with { HasMore = false }` | `new SliceResult<T>(slice.Items, hasMore: false)` | `with` is no longer supported (properties are get-only). |
| `new PagedResult<T>(Items: x, TotalCount: n, PageSize: s, PageNumber: p)` | `new PagedResult<T>(items: x, totalCount: n, pageSize: s, pageNumber: p)` | Constructor parameter names are now camelCase. **Positional calls need no change.** |
| `new CursorResult<T>(items, null!, true)` (null items tolerated) | items must be non-null | The constructor now throws `ArgumentNullException` on null `items`. |

Cursor/Paged init-only members (`PreviousCursor`, `TotalCount` on `CursorResult<T>`) are **unchanged** — they remain `{ get; init; }` and still work with object-initializer syntax.

## New Capabilities

- **Serialize/deserialize results directly** — no converter registration:
  ```csharp
  var json = JsonSerializer.Serialize(Result<Order>.Success(order));
  var back = JsonSerializer.Deserialize<Result<Order>>(json);   // round-trips as a success
  ```
- **`Optional<T>`** serializes transparently (present → value, empty → `null`).
- **Branch on a failure's type safely across serialization** with `HasError`:
  ```csharp
  if (result.HasError<NotFoundException>()) { /* works for live AND deserialized failures */ }
  ```
- See the README's *JSON Serialization* section for the failure surrogate model and Native AOT guidance.

## Migration Walkthrough

1. Update the package reference to `2.0.0`.
2. Build. The compiler flags every break — all three are in pagination-type usage:
   - Replace any **deconstruction** of `SliceResult`/`CursorResult`/`PagedResult` with explicit property reads.
   - Replace any **`with` expression** on those types with a `new(...)` call.
   - Change any **named-argument** constructor calls from PascalCase to camelCase parameter names (positional calls are unaffected).
3. Ensure you never pass `null` items to a pagination constructor — pass an empty collection (or use the `Empty` factory) instead.
4. If you serialize `Result`/`Result<T>` and inspect failures by exception type, switch `error is TException` checks to `result.HasError<TException>()` so they survive a serialization round-trip.

## What Didn't Change

- The entire `Result` / `Result<T>` / `Optional<T>` monad surface — `IsSuccess`, `IsFailure`, `Value`, `Error`, `Map`, `Then`, `Match`, `Switch`, `Ensure`, `TryGetValue`/`TryGetError`, and the full async-extension surface — is unchanged.
- Pagination **property reads** (`Items`, `Count`, `HasMore`, `TotalPages`, …) and **positional construction** are unchanged.
- Target frameworks (`net8.0`, `net9.0`, `net10.0`) are unchanged.

## Downstream Package Impact

`Cirreum.Result` is a foundational package, so consumers that pin it must move to `2.0.0`. Within the Cirreum foundation this is a coordinated re-pin (`Cirreum.Contracts` → the new `Cirreum.Result`, flowing transitively to the rest). No code changes are required in those consumers unless they use the pagination types in one of the broken forms above.
