# Ckl.Libs.ResultPattern

**Exceptions are not a return channel.** This package provides the *Result
pattern*: a type-safe alternative to `throw`/`catch`, where every operation
carries its success or failure as a return value — including an error message,
an optional exception, and an automatically built call stack.

---

## Why Result instead of exceptions?

`try`/`catch` has its place — but as the primary means of error handling it
brings problems:

- **Invisible control flow.** A `throw` in a helper method can surprisingly
  terminate the caller without any hint in the signature.
- **Forced context switch.** The caller has to decide whether to catch or keep
  throwing — and if they forget, the exception propagates uncontrolled.
- **No structured call stack.** `Exception.StackTrace` shows the technical
  execution path, not the *business* call path.

The Result pattern turns errors into **first-class values**: the compiler
forces the caller to consider the failure case, and `Propagate()`
automatically builds up a readable call stack along the way.

---

## Quick start

```csharp
// Success
return Result.Success;
return Result<MyType>.Success(value);

// Failure
return Result.Fail("Something went wrong.");
return Result.Error("Invalid input.");          // idiomatic shorthand

// Catching an exception — always like this
catch (Exception ex) { return ex; }

// Propagating a failure up the call chain
var result = DoSomething();
if (!result.Succeeded) return Result.Propagate(result);

// Evaluating a result
if (!result.Succeeded)
{
    logger.Error(result.ErrorMessage);
    logger.Error(result.CallStackAsString);
}

var value = result.Value; // only access after Succeeded == true
```

---

## Core concepts

### `Result` and `Result<T>`

Two record types: `Result` for void operations, `Result<T>` for operations
that return a value. Both implement the same interface and behave
identically.

```csharp
Result         DoSomething()        { ... }
Result<string> LoadUserName(int id) { ... }
```

### `Error` — idiomatic return value

`Result.Error(...)` creates an `Error` value that implicitly converts to
`Result` *and* `Result<T>`. This allows a compact notation without explicit
casting:

```csharp
Result<int> Parse(string s) =>
    int.TryParse(s, out var n) ? n : Result.Error($"Not an integer: {s}");
```

### Implicit conversions

```csharp
// Value → Result<T>
Result<int> result = 42;

// Exception → Result (in a catch block)
catch (Exception ex) { return ex; }

// Error → Result or Result<T>
return Result.Error("message");
```

> **Ternary trap with `Result<string>`:** When `TValue = string`, C# resolves
> the common type of both ternary branches — **not** the method's return type.
> Always write the success branch explicitly as
> `Result<string>.Success(value)`:
>
> ```csharp
> // WRONG — silently becomes Error("INT"):
> Result<string> GetValue(string key) =>
>     dict.TryGetValue(key, out var v) ? v : Result.Error("not found");
>
> // CORRECT:
> Result<string> GetValue(string key) =>
>     dict.TryGetValue(key, out var v) ? Result<string>.Success(v) : Result.Error("not found");
> ```

### `Propagate()` — building the call stack

Every call to `Propagate()` adds the current caller to the chain. In the end,
`CallStackAsString` contains the full business call path — useful for logging
and debugging.

```csharp
private Result<string> ReadUserName()
{
    var result = LoadUserRecord();
    if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
    return result.Value.ToString();
}
```

### Switching between types

`ToResult()` and `ToResult<T>()` convert a *failed* result value into another
type without losing the error — needed when the call chain passes through
different `Result<T>` types.

```csharp
Result<int> ParseUserAge()
{
    var result = ReadUserName();                                    // Result<string>
    if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
    return int.Parse(result.Value);
}
```

---

## Full example

```csharp
// Deepest level: exception becomes a Result
Result<Guid> LoadUserRecord()
{
    try { return repository.GetUser(id); }
    catch (Exception ex) { return ex; }
}

// Middle levels: errors travel upward
Result<string> ReadUserName()
{
    var r = LoadUserRecord();
    if (!r.Succeeded) return Result<string>.Propagate(r.ToResult<string>());
    return r.Value.ToString();
}

Result<int> ParseUserAge()
{
    var r = ReadUserName();
    if (!r.Succeeded) return Result<int>.Propagate(r.ToResult<int>());
    return int.Parse(r.Value);
}

// Top level: evaluate the error
Result result = ParseUserAge().ToResult();
if (!result.Succeeded)
{
    Console.WriteLine(result.ErrorMessage);
    Console.WriteLine(result.CallStackAsString);
    // e.g.:
    //   at LoadUserRecord in UserRepository.cs, line 42
    //   at ReadUserName in UserService.cs, line 18
    //   at ParseUserAge in UserService.cs, line 27
}
```

---

## Reference

| Member | Description |
|---|---|
| `Result.Success` | Successful void result |
| `Result<T>.Success(value)` | Successful typed result |
| `Result.Fail(message)` | Failed result with a message |
| `Result.Fail(exception)` | Failed result from an exception |
| `Result.Error(message)` | Shorthand: creates an `Error` value for idiomatic return |
| `result.Succeeded` | `true` if successful; `false` guarantees `ErrorMessage != null` |
| `result.Value` | Only access after `Succeeded == true` |
| `result.ErrorMessage` | Error message |
| `result.Exception` | Original exception, if any |
| `result.Callers` | List of propagation frames |
| `result.CallStackAsString` | Human-readable call stack for logging |
| `result.ToString()` | `[Success]` / `[Success] {value}`, or error message + call stack |
| `Result.Propagate(result)` | Adds the current caller to the chain |
| `result.ToResult()` | `Result<T>` → `Result` (failures only) |
| `result.ToResult<T>()` | Converts a failure into another `Result<T>` type |

---

## For maintainers

### Versioning

`Major.Minor.Patch` following Semantic Versioning. The current state is
documented in `CHANGELOG.md`.

### Requirements

.NET 10 SDK, C# (latest)

### Project structure

```
Ckl.Libs.ResultPattern\
├── Result.cs                   # void Result
├── Result.generic.cs           # Result<T>
├── Support\
│   ├── CallerInfo.cs           # One frame in the propagation call stack
│   ├── Error.cs                # Helper type for idiomatic error return
│   ├── ResultHelper.cs         # Internal helper functions
│   └── ResultSuccess.cs        # Sentinel type for Result.Success
└── Contracts\
    ├── IResult.cs               # API contract, void Result
    ├── IResult.generic.cs       # API contract, Result<T>
    └── IError.cs                # Shared failure contract
```

### Dependencies

None.

### Notes

- `Result` and `Result<T>` are C# records. Record equality compares all
  properties — two `Fail` results with the same message are **not**
  record-equal, since their `Callers` lists are different instances.
- The `Debug.Assert` in `Propagate()` fires if this method is called on a
  successful result — visible only in Debug builds.
  `ToResult<T>()` always throws an `InvalidOperationException` if called on a
  successful result.

---

*Ckl.Libs.ResultPattern — © 2026 ckluth — MIT License*
