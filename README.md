# Cirreum Result: High-Performance Railway-Oriented Programming for C#

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Result.svg?style=flat-square)](https://www.nuget.org/packages/Cirreum.Result/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Result.svg?style=flat-square)](https://www.nuget.org/packages/Cirreum.Result/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Result?style=flat-square)](https://github.com/cirreum/Cirreum.Result/releases)

## Overview

A lightweight, allocation‑free, struct‑based `Result` and `Optional` monad library designed for high‑performance .NET applications.
Provides a complete toolkit for functional, exception‑free control flow with **full async support**, **validation**, **inspection**, and **monadic composition**.

---

## ✨ Features

- **Struct-based**: No heap allocations on the success path.
- **Unified Success/Failure Model** using `Result` and `Result<T>`.
- **Optional Values** with `Optional<T>` for explicit presence/absence modeling.
- **Full async support**
  - `ValueTask` + `Task`
  - Async variants of `Map`, `Then`, `Ensure`, `Inspect`, failure handlers, etc.
- **Validation pipeline** with `Ensure` (sync + async).
- **Inspection helpers**: `Inspect`, `InspectTry`.
- **Composable monad API**:
  `Map`, `Then`, `Match`, `Switch`, `TryGetValue`, `TryGetError`, and more.
- **Ergonomic extension methods** for async workflows.
- **Zero exceptions for control flow**—exceptions are captured as failures.

---

## 📦 Installation

```bash
dotnet add package Cirreum.Result
```

---
🧱 Target Frameworks

Result is built for modern, high-performance .NET, with first-class support for:

| TFM              | Status       | Notes                                                           |
| ---------------- | ------------ | --------------------------------------------------------------- |
| **.NET 10**      | ✔️ Primary   | Latest runtime/JIT optimizations. Best performance.             |
| **.NET 9**       | ✔️ Supported | Fully compatible. Same performance envelope for most scenarios. |
| **.NET 8 (LTS)** | ✔️ Supported | Ideal for production stability; fully compatible.               |

---

## 🚀 Quick Start

### Basic Success / Failure

```csharp
Result SaveUser(User user)
{
    if (user is null)
        return Result.Fail(new ArgumentNullException(nameof(user)));

    return Result.Success;
}

Result<User> CreateUser(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<User>.Fail(new ArgumentException("Name is required"));

    return Result<User>.Success(new User(name));
}
```

---

## 🔗 Chaining With `Then` and `Map`

```csharp
var result =
    CreateUser("Alice")
        .Map(user => user.Id)
        .Then(LogCreation);
```

---

## 🎯 Optional Values

Use `Optional<T>` when a value may be absent without implying an error:

```csharp
Optional<User> FindUser(int id)
{
    var user = _db.Users.Find(id);
    return Optional.From(user);  // Returns Empty if null
}

// Chaining
var displayName = FindUser(id)
    .Map(u => u.DisplayName)
    .GetValueOrDefault("Unknown");

// Pattern matching
FindUser(id).Switch(
    onValue: user => Console.WriteLine($"Found: {user.Name}"),
    onEmpty: () => Console.WriteLine("User not found"));

// Convert to Result when absence is an error
var result = FindUser(id)
    .ToResult(new NotFoundException("User not found"));
```

### When to Use `Optional<T>` vs `Result<T>`

| Use Case | Type |
| -------- | ---- |
| Operation that might fail with a reason | `Result<T>` |
| Value that may or may not exist | `Optional<T>` |
| Dictionary/cache lookup | `Optional<T>` |
| Database query that might return null | `Optional<T>` |
| Validation or business rule failure | `Result<T>` |
| API call that might error | `Result<T>` |

---

## ✅ Validation With `Ensure`

```csharp
var result =
    GetOrder(id)
        .Ensure(o => o.Amount > 0, "Amount must be positive")
        .Ensure(o => o.Items.Any(), o => new InvalidOperationException("Order has no items"));
```

---

## 👀 Side‑Effects With `Inspect`

```csharp
result
    .Inspect(r => logger.LogInformation("Result: {State}", r.IsSuccess ? "OK" : "FAIL"));
```

---

## ⚡ Async Support (`ValueTask` + `Task`)

Every operation has async variants:

```csharp
var result =
    await GetUserAsync(id)
        .OnSuccessAsync(user => logger.LogInformation("Loaded {Id}", user.Id))
        .OnFailureAsync(ex => logger.LogError(ex, "Failed to load user"));
```

Or with async lambdas:

```csharp
await SaveAsync(entity)
    .OnSuccessTryAsync(async () => await NotifyAsync(entity));
```

---

## 🧩 Pattern Matching

```csharp
var message = result.Match(
    onSuccess: () => "OK",
    onFailure: ex => $"Error: {ex.Message}");
```

---

## 🏗️ API Overview

### `Result` (non-generic)

- `IsSuccess`, `IsFailure`, `Error`
- `OnSuccess`, `OnFailure`, `Inspect`, `TryGetError`
- `Map`, `Then`, `Match`
- Async: `OnSuccessAsync`, `OnFailureAsync`, `SwitchAsync`, etc.

### `Result<T>`

All of the above, plus:

- `Value` / `TryGetValue`
- `Map<TOut>(...)`
- `Ensure(...)` validation helpers

### `Optional<T>`

- `HasValue`, `Value`, `TryGetValue`
- `From(T?)`, `Empty` factory methods
- `Map`, `Then`, `Match`, `Switch`, `Where`
- `GetValueOrDefault(T)`, `GetValueOrDefault(Func<T>)`
- `ToResult(Exception)` for converting to `Result<T>`

### Async Extensions
(From `ResultAsyncExtensions`)
Supports async versions of:

- `OnSuccess`
- `OnSuccessTry`
- `OnFailure`
- `OnFailureTry`
- `Inspect`
- `Ensure`
- `Map`
- `Then`
- `Match`

All support both `ValueTask` and `Task`.

---

## 🧪 Example: End‑to‑End Pipeline

```csharp
var result =
    await Validate(request)
        .Then(() => LoadUser(request.UserId))
        .Ensure(u => u.IsActive, "User must be active")
        .Map(user => user.Profile)
        .OnSuccessAsync(profile => Cache(profile))
        .OnFailureAsync(ex => LogFailure(ex));
```

No exceptions. No branches. Pure railway flow.

---

## 🛠️ Why Struct‑Based?

- Avoids heap allocations
- No capturing of closures on success path
- More predictable performance in hot loops
- Perfect for high‑volume messaging, pipelines, middleware, and game engines

---

## 📜 License

This project is licensed under the **MIT License**.

---

## 🤝 Contributing

Pull requests are welcome! If you have ideas for improvements—performance tweaks, new helpers, analyzers—feel free to open an issue or contribute directly.

---

## 🧭 Project Files

- Core interfaces: `IResult`, `IResult<T>`
- Implementations: `Result`, `Result<T>`, `Optional<T>`
- Async pipeline operators: `ResultAsyncExtensions`
- Monadic composition, validation, and inspection APIs
- Designed to support any .NET hosting model (Server, WASM, Azure Functions)

---

Made with ❤️ for clean, predictable, exception‑free flow control.

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*
