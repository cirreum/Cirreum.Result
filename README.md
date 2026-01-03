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
- **Comprehensive validation pipeline** with `Ensure`:
  - Three overload patterns: error message, direct exception, error factory
  - Full async support for async predicates
  - Chainable validation with fail-fast semantics
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

## 🏭 Static Factory Methods

The non-generic `Result` class provides convenient factory methods:

```csharp
// Create a successful Result<T>
var success = Result.From(42);  // Result<int>

// Create a failed Result<T>
var failure = Result.Fail<string>(new InvalidOperationException("Error"));

// Convert Optional<T> to Result<T>
var optional = Optional<User>.From(user);

// With direct exception
var result1 = Result.FromOptional(optional, new NotFoundException("User not found"));

// With error message (creates InvalidOperationException)
var result2 = Result.FromOptional(optional, "User not found");

// With error factory for lazy evaluation
var result3 = Result.FromOptional(optional, () => new NotFoundException($"User {id} not found"));
```

---

## 🎯 Optional Values

Use `Optional<T>` when a value may be absent without implying an error:

### Creating Optionals

```csharp
// When you know the value is not null
var name = Optional<string>.For("John");

// When the value might be null (null-safe)
var user = _db.Users.Find(id);
var userOptional = Optional.From(user);  // Returns Empty if null

// Explicit empty
var empty = Optional<int>.Empty;
```

### Using Optionals

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

// Or use the static factory method
var result2 = Result.FromOptional(FindUser(id), new NotFoundException("User not found"));
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

The `Ensure` method provides a fluent way to add validation to your `Result<T>` pipeline. If the predicate returns false, the success result is converted to a failure.

### Synchronous Ensure

```csharp
// With error message (creates InvalidOperationException)
var result = GetOrder(id)
    .Ensure(o => o.Amount > 0, "Amount must be positive");

// With exception factory for custom error types
var result = GetOrder(id)
    .Ensure(o => o.Amount > 0, o => new ValidationException($"Order {o.Id} has invalid amount: {o.Amount}"));

// With direct exception
var result = GetOrder(id)
    .Ensure(o => o.Amount > 0, new ValidationException("Amount must be positive"));

// Chain multiple validations - stops on first failure
var result = GetOrder(id)
    .Ensure(o => o.Amount > 0, "Amount must be positive")
    .Ensure(o => o.Items.Any(), "Order must have items")
    .Ensure(o => o.CustomerId != null, o => new InvalidOperationException($"Order {o.Id} has no customer"));
```

### Async Ensure

```csharp
// Async predicate with error factory
var result = await GetOrderAsync(id)
    .EnsureAsync(async o => await IsValidCustomer(o.CustomerId), 
                 o => new ValidationException($"Invalid customer: {o.CustomerId}"));

// Async predicate with direct exception
var result = await GetOrderAsync(id)
    .EnsureAsync(async o => await HasSufficientStock(o.Items), 
                 new InsufficientStockException());

// Mix sync and async validations
var result = await GetOrderAsync(id)
    .EnsureAsync(o => o.Amount > 0, "Amount must be positive")  // sync predicate
    .EnsureAsync(async o => await IsValidCustomer(o.CustomerId), "Invalid customer")  // async predicate
    .EnsureAsync(o => o.Items.All(i => i.Quantity > 0), "All items must have positive quantity");
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
- Static factories:
  - `From<T>(T value)` - creates `Result<T>`
  - `Fail<T>(Exception error)` - creates failed `Result<T>`
  - `FromOptional<T>(Optional<T>, Exception/string/Func<Exception>)` - converts `Optional<T>` to `Result<T>`
- Async: `OnSuccessAsync`, `OnFailureAsync`, `SwitchAsync`, etc.

### `Result<T>`

All of the above, plus:

- `Value` / `TryGetValue`
- `Map<TOut>(...)`
- `Ensure(...)` validation helpers:
  - `Ensure(Func<T, bool> predicate, string errorMessage)`
  - `Ensure(Func<T, bool> predicate, Exception error)`
  - `Ensure(Func<T, bool> predicate, Func<T, Exception> errorFactory)`

### `Optional<T>`

- `HasValue`, `IsEmpty`, `Value`, `TryGetValue`
- `For(T non-null)` - creates Optional from non-null value
- `Empty` - static property for empty optional
- `Map`, `Then`, `Match`, `Switch`, `Where`
- `GetValueOrDefault(T)`, `GetValueOrDefault(Func<T>)`, `GetValueOrNull()`
- `ToResult(Exception)` for converting to `Result<T>`

### `Optional` (non-generic factory)

- `From<T>(T?)` - null-safe factory method

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

## 📚 Real-World Examples

### User Registration with Validation

```csharp
public async Task<Result<User>> RegisterUserAsync(RegistrationRequest request)
{
    return await Result<RegistrationRequest>.Success(request)
        .Ensure(r => !string.IsNullOrWhiteSpace(r.Email), "Email is required")
        .Ensure(r => IsValidEmail(r.Email), "Invalid email format")
        .Ensure(r => r.Password.Length >= 8, "Password must be at least 8 characters")
        .EnsureAsync(async r => !await UserExists(r.Email), 
                     r => new DuplicateUserException($"User with email {r.Email} already exists"))
        .Map(r => new User { Email = r.Email, PasswordHash = HashPassword(r.Password) })
        .ThenAsync(async user => await SaveUserAsync(user))
        .OnSuccessAsync(async user => await SendWelcomeEmailAsync(user.Email))
        .InspectAsync(result => 
            _logger.LogInformation("Registration attempt for {Email}: {Success}", 
                request.Email, result.IsSuccess));
}
```

### Order Processing with Stock Validation

```csharp
public async Task<Result<Order>> ProcessOrderAsync(OrderRequest request)
{
    return await LoadCustomerAsync(request.CustomerId)
        .Ensure(c => c.IsActive, c => new InactiveCustomerException($"Customer {c.Id} is inactive"))
        .Ensure(c => !c.HasOutstandingBalance, "Customer has outstanding balance")
        .Map(customer => CreateOrder(customer, request.Items))
        .EnsureAsync(async order => await CheckInventoryAsync(order.Items),
                     "Insufficient inventory for one or more items")
        .Ensure(order => order.Total >= 10m, "Minimum order amount is $10")
        .ThenAsync(async order => await SaveOrderAsync(order))
        .OnSuccessAsync(async order => {
            await ReserveInventoryAsync(order.Items);
            await NotifyWarehouseAsync(order);
        })
        .OnFailureAsync(async error => {
            await _logger.LogErrorAsync(error, "Order processing failed");
            if (error is InsufficientInventoryException)
                await NotifyInventoryTeamAsync(request);
        });
}
```

### API Response Handling with Optional

```csharp
public async Task<Result<UserProfile>> GetUserProfileAsync(int userId)
{
    // Find user returns Optional<User>
    var userOptional = await _repository.FindUserAsync(userId);
    
    return userOptional
        .ToResult(new NotFoundException($"User {userId} not found"))
        .EnsureAsync(async u => await IsAuthorizedToViewProfile(u),
                     new UnauthorizedException("Not authorized to view this profile"))
        .ThenAsync(async user => {
            var profile = await LoadProfileAsync(user.Id);
            var preferences = await LoadPreferencesAsync(user.Id);
            return BuildCompleteProfile(user, profile, preferences);
        });
}

// Alternative using static factory
public async Task<Result<Product>> GetProductAsync(string sku)
{
    var productOptional = await _cache.GetProductAsync(sku);
    
    // Convert optional to result using factory method
    return Result.FromOptional(productOptional, () => new ProductNotFoundException($"Product {sku} not found"))
        .Ensure(p => p.IsAvailable, "Product is not available")
        .Ensure(p => p.Stock > 0, p => new OutOfStockException($"Product {p.Name} is out of stock"))
        .Map(p => ApplyCurrentPricing(p));
}
```

## 💡 Best Practices

### 1. Use Specific Exception Types
```csharp
// Good - specific exception with context
.Ensure(order => order.Items.Any(), 
        order => new EmptyOrderException($"Order {order.Id} has no items"))

// Less ideal - generic exception
.Ensure(order => order.Items.Any(), "Order has no items")
```

### 2. Chain Validations from General to Specific
```csharp
// Good - fails fast on basic validation
result
    .Ensure(x => x != null, "Value cannot be null")
    .Ensure(x => x.Length > 0, "Value cannot be empty")
    .Ensure(x => x.Length <= 100, "Value too long")
    .EnsureAsync(async x => await IsUniqueAsync(x), "Value must be unique");
```

### 3. Use Optional<T> for Lookups, Result<T> for Operations
```csharp
// Good
Optional<User> FindUserByEmail(string email);
Result<User> CreateUser(CreateUserRequest request);

// Avoid
Result<User> FindUserByEmail(string email); // Finding nothing isn't an error
Optional<User> CreateUser(CreateUserRequest request); // Creation can fail
```

### 4. Keep Async and Sync Separate
```csharp
// Good - clear async boundaries
var result = ProcessSync()
    .Then(x => TransformSync(x))
    .ThenAsync(async x => await SaveAsync(x))
    .OnSuccessAsync(async x => await NotifyAsync(x));

// Avoid - mixing unnecessarily
var result = await ProcessSync()
    .ThenAsync(async x => TransformSync(x)) // Wrapping sync in async
```

### 5. Use Error Context in Factories
```csharp
// Good - provides context
.Ensure(u => u.Age >= 18, 
        u => new ValidationException($"User {u.Id} is underage: {u.Age}"))

// Less helpful
.Ensure(u => u.Age >= 18, new ValidationException("User is underage"))
```

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
