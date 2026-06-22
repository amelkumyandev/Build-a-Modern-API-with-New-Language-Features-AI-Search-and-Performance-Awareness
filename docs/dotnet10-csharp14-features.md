# .NET 9, .NET 10, C# 13, and C# 14 Feature Usage

This document records which recent platform and language features were reviewed, which ones are used in this codebase, and why some were not applied.

References:

- [.NET 9 overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview)
- [.NET 10 overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
- [C# 13 overview](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)
- [C# 14 overview](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [ASP.NET Core OpenAPI overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview)
- [ASP.NET Core 10 release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0)

## Project Configuration

All backend projects target .NET 10 and compile with C# 14:

```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>14.0</LangVersion>
```

That means the project can use .NET 9, .NET 10, C# 13, and C# 14 features where they fit.

## Features Applied in This Codebase

| Version | Feature | Where used | Improvement |
| --- | --- | --- | --- |
| .NET 10 | `net10.0` target framework | All backend projects | Uses the .NET 10 LTS runtime, SDK, libraries, and ASP.NET Core stack. |
| .NET 10 | `JsonSerializerOptions.AllowDuplicateProperties = false` | API JSON options in `Program.cs` | Rejects ambiguous duplicate JSON properties during request deserialization, reducing input-confusion risk. |
| .NET 10 | ASP.NET Core 10 packages | API project package references | Keeps authentication, OpenAPI, hosting, and Minimal API infrastructure aligned with .NET 10. |
| .NET 10 | EF Core 10/Npgsql 10 package line | Infrastructure project package references | Keeps persistence and PostgreSQL access on the .NET 10-compatible provider stack. |
| .NET 9+ | First-party ASP.NET Core OpenAPI generation | `AddOpenApi()` and `MapOpenApi()` in `Program.cs` | Adds built-in OpenAPI JSON generation alongside the existing Swagger UI in development. |
| .NET 9+ | Runtime and SDK improvements | Inherited by targeting `net10.0` | Benefits from runtime performance, SDK, build, and library improvements without app-specific code. |
| C# 14 | Field-backed properties using `field` | Strongly typed option classes in `ApplicationCore.cs` | Adds validation and normalization to option setters without hand-written backing fields. |
| C# 13 | `System.Threading.Lock` with `lock` statement | `RuntimeSettings` in `ApplicationServices.cs` | Uses the newer lock object and compiler semantics for runtime settings synchronization. |
| C# 13 | `params` collections | `ValidationFailureException.For` in `ApplicationCore.cs` | Allows concise single-field validation errors without manual array creation at call sites. |
| Modern C# | Records | API contracts and value-like response models | Keeps DTOs immutable and concise. |
| Modern C# | Primary constructors | Application services, repositories, middleware, clients | Keeps dependency-injected services compact. |
| Modern C# | Collection expressions | Domain models, tests, validation helpers | Improves readability for empty and copied collections. |
| Modern C# | File-scoped namespaces | Backend source files | Reduces nesting and boilerplate. |
| Modern C# | Nullable reference types | Backend project configuration | Makes nullability part of the implementation and contract design. |
| Modern C# | Pattern matching and switch expressions | Validation and search-mode selection | Keeps branching explicit and readable. |

## .NET 9 Features Reviewed

| Feature area | Applied? | Notes |
| --- | --- | --- |
| Runtime performance improvements | Yes, inherited | Targeting .NET 10 includes the later runtime line and benefits from these optimizations. |
| Dynamic GC adaptation | Yes, inherited | Runtime behavior, no app code required. |
| Feature switches with trimming support | Not directly | No trimming feature switches are defined by this app. |
| `System.Text.Json` improvements | Partially | The app now uses duplicate-property rejection from the newer JSON stack. Other schema/streaming APIs are not needed. |
| LINQ `CountBy` / `AggregateBy` | Not applied | Current grouping logic needs full grouped values, not only aggregate counts. |
| `PriorityQueue.Remove` | Not applicable | The app does not use priority queues. |
| Cryptography additions | Not applicable | JWT signing and password hashing use existing focused APIs. |
| `PersistedAssemblyBuilder` | Not applicable | The app does not emit assemblies. |
| New `TimeSpan.From*` overloads | Inherited | Existing `TimeSpan.FromSeconds(2)` and `FromMinutes(1)` compile against the newer runtime. |
| SDK workload sets, terminal logger, build checks | Tooling only | Useful for development, but not source-code changes. |
| `Microsoft.Extensions.AI` / `VectorData` | Not applied | The current OpenAI and pgvector abstractions are already implemented behind local interfaces. Introducing new abstractions now would be a broader refactor. |
| ASP.NET Core built-in OpenAPI | Applied | `MapOpenApi()` exposes the built-in document in development. |
| MAUI, WPF, Windows Forms | Not applicable | This is a web API plus Next.js app. |

## .NET 10 Features Reviewed

| Feature area | Applied? | Notes |
| --- | --- | --- |
| Runtime JIT, devirtualization, stack allocation, loop, AVX, NativeAOT improvements | Yes, inherited | Targeting `net10.0` gives the app these runtime improvements. |
| New library APIs in cryptography, globalization, numerics, serialization, collections, diagnostics, ZIP | Partially | JSON duplicate-property rejection is applied. Other APIs do not match current requirements. |
| `WebSocketStream` and TLS/process improvements | Not applicable | The app does not use WebSockets or process-group control. |
| .NET 10 SDK MTP, CLI, tooling, container improvements | Tooling only | Useful for local/build workflow, not source-code changes in this app. |
| ASP.NET Core 10 Minimal API validation | Not applied | The app already has custom validation and ProblemDetails behavior. Adding built-in validation now could alter public error responses. |
| ASP.NET Core 10 authentication/authorization metrics | Inherited | The app uses ASP.NET Core auth; metrics can be collected by the platform when observability is configured. |
| ASP.NET Core 10 OpenAPI enhancements | Applied through current package stack | The app exposes built-in OpenAPI and keeps Swagger UI for local development. |

## C# 13 Features Reviewed

| Feature | Applied? | Notes |
| --- | --- | --- |
| `params` collections | Applied | Used by `ValidationFailureException.For`. |
| New `System.Threading.Lock` type and semantics | Applied | Used by `RuntimeSettings`. |
| `\e` escape sequence | Not applicable | No terminal escape/control sequence strings are needed. |
| Method group natural type improvements | Compiler improvement | No explicit source change required. |
| Implicit indexer access in object initializers | Not applied | Existing object initializers do not benefit meaningfully. |
| `ref` locals and unsafe contexts in iterators/async methods | Not applicable | The app avoids unsafe and low-level span-heavy code. |
| `ref struct` interfaces and generic arguments | Not applicable | No custom `ref struct` types are used. |
| Partial properties and indexers | Not applicable | No source-generator or partial-member scenario exists. |
| Overload resolution priority | Not applicable | This app does not publish competing overload families needing priority. |
| `field` preview | Superseded by C# 14 usage | The project uses the released C# 14 field-backed property feature. |

## C# 14 Features Reviewed

| Feature | Applied? | Notes |
| --- | --- | --- |
| Field-backed properties using `field` | Applied | Used in `JwtOptions`, `OpenAiOptions`, `ChunkingOptions`, and `SearchOptions`. |
| `nameof` with unbound generic types | Not applicable | The app does not need generic type names in diagnostics. |
| Implicit `Span<T>` and `ReadOnlySpan<T>` conversions | Not directly | The app is not span-heavy; forcing span APIs would hurt readability. |
| Lambda parameter modifiers | Not applicable | Existing lambdas do not need `ref`, `in`, `out`, or `scoped`. |
| Partial constructors and events | Not applicable | No generator-driven partial constructor/event pattern exists. |
| Extension blocks and extension properties | Not applied | Existing classic extension methods remain clearer for simple mapping and normalization helpers. |
| Null-conditional assignment | Not applied | There is no natural nullable receiver assignment path in the current code. |
| User-defined compound assignment/increment/decrement operators | Not applicable | The domain model does not need custom arithmetic operators. |

## Practical Conclusion

Applied features are intentionally conservative:

- C# 13 improves synchronization and validation helper ergonomics.
- C# 14 improves strongly typed option normalization without backing-field boilerplate.
- .NET 10 improves JSON request safety.
- ASP.NET Core's built-in OpenAPI support improves development-time API discoverability.

Features that do not match the current codebase were left out to avoid novelty-driven refactoring.

