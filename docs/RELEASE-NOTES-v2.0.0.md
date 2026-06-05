# Cirreum.Result 2.0.0 — Serializable Results

## Why this release exists

`Result`, `Result<T>`, and `Optional<T>` are struct-based monads with get-only properties and private constructors — which means System.Text.Json had nothing to populate on deserialization, and a `Result<T>` round-tripped through a *serializing* provider came back as a **failure with a null error**. The bug stayed hidden because the in-memory cache stores the live object and never serializes; it only surfaced once a distributed/hybrid cache (or a message bus, or an API payload) actually serialized a result — i.e. in production.

v2 fixes this at the type's home, so a result serializes correctly **everywhere, with no registration**.

## What's new

**Built-in JSON round-tripping.** `Result`, `Result<T>`, and `Optional<T>` each carry a `[JsonConverter]`, so they work under any `JsonSerializerOptions`:

```csharp
var json = JsonSerializer.Serialize(Result<int>.Success(5));     // {"isSuccess":true,"value":5}
var back = JsonSerializer.Deserialize<Result<int>>(json);        // back.IsSuccess == true, back.Value == 5

JsonSerializer.Serialize(Optional<int>.For(5));                  // 5
JsonSerializer.Serialize(Optional<int>.Empty);                   // null
```

**Honest failure handling.** Exceptions can't round-trip (type identity, stack, and custom state are lost), so a deserialized failure's `Error` is a `SurrogateResultException` that preserves the original exception's full type name and message. To branch on the failure type in a way that works identically for live *and* deserialized errors, use the new `HasError`:

```csharp
if (result.HasError<NotFoundException>()) { /* ... */ }
```

The type-agnostic surface — `IsSuccess`, `IsFailure`, `Value`, `Match`, `Switch`, `Error.Message` — is unaffected by the surrogate and behaves identically before and after serialization.

**Carrying error state across the wire.** Because exceptions are flattened to type + message, any structured state would otherwise be lost. An exception can opt in by implementing **`IErrorState`** — a `string`→`string` `State` map the serializer captures and round-trips onto `SurrogateResultException.State`:

```csharp
public class NotFoundException : Exception, IErrorState {
    public object[] Keys { get; }
    public IReadOnlyDictionary<string, string> State => new Dictionary<string, string> {
        ["keys"] = string.Join(",", Keys)
    };
}
// after a round-trip:  ((SurrogateResultException)result.Error).State["keys"]  →  "42,99"
```

It's a deliberate, lossless escape hatch (empty state is omitted from the wire), not typed reconstruction — think of it like the way a response DTO carries an error's shape across a boundary. The implementing exception owns what it exposes.

## Breaking changes

The three pagination result types (`SliceResult<T>`, `CursorResult<T>`, `PagedResult<T>`) moved from positional records to explicit-constructor records so they can validate their arguments (non-null items; non-negative counts; `pageNumber >= 1`) and defensively copy their items. That removes the compiler-generated `Deconstruct` and `with` support and renames constructor parameters to camelCase. **Property reads and positional construction are unchanged.** See [MIGRATION-v2.md](MIGRATION-v2.md) for the find/replace table — it's a small, mechanical change confined to pagination-type usage.

## Native AOT

Using `Result` / `Result<T>` / `Optional<T>` as monads is fully Native-AOT-compatible — referencing the package introduces no trim/AOT warnings on its own. Serializing a result via **reflection** (`JsonSerializer.Serialize(result)` with no `JsonTypeInfo`) is not AOT-safe — that is System.Text.Json's intrinsic limitation — and the analyzers flag it at your call site. Under AOT, serialize via **source generation**, registering **both** the `Result<T>` and its inner `T` in your `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(Result<Order>))]
[JsonSerializable(typeof(Order))]
internal partial class AppJsonContext : JsonSerializerContext;
```

## Compatibility

- Target frameworks unchanged: `net8.0`, `net9.0`, `net10.0`.
- The entire monad and async-extension surface is unchanged; the only source-breaking change is the pagination-type constructors (see above).
- As a foundational package, consumers that pin `Cirreum.Result` move to `2.0.0`; within the Cirreum foundation this is a coordinated re-pin with no code changes unless the pagination types are used in a broken form.

## See also

- [MIGRATION-v2.md](MIGRATION-v2.md)
- [CHANGELOG.md](CHANGELOG.md)
- The README's *JSON Serialization* section — wire shapes, the surrogate model, and AOT guidance.
