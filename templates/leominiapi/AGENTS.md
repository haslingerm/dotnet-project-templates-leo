# LeoMiniApi development rules

These instructions apply to this project. Preserve its small, explicit minimal-API design.

## Architecture

- `Program.cs` is the composition root. Keep it limited to configuration, DI, middleware, and startup.
- `Setup.cs` owns service registration and top-level endpoint mapping. Register every resource explicitly; do not add reflection-based discovery.
- `Endpoints/` owns routes, transport validation, typed HTTP results, and request/response DTOs. Keep each resource in its own endpoint class and route group.
- `Util/` contains narrowly scoped, transport-level cross-cutting helpers such as JSON configuration, validation, and exception handling.
- Do not introduce controllers, mediator abstractions, repositories, or extra projects unless the application actually needs the corresponding complexity.

## API and implementation conventions

- Keep nullable reference types and warnings-as-errors enabled, and satisfy the configured analyzers instead of suppressing them.
- Always use Allman braces: every block-opening brace belongs on its own line, including methods, properties, conditionals, loops, `try`/`catch`, lambdas with blocks, and local functions.
- Use file-scoped namespaces, four-space indentation, and the naming/layout already established by nearby code.
- Never use the null-forgiving operator (`!`). This template has no EF entities, so there is no permitted exception. Model nullability accurately and use guards, pattern matching, `required`, or explicit failure paths.
- Prefer sealed types when inheritance is not intended. Keep public surface area minimal.
- Name asynchronous methods with `Async` and use `Task`/`ValueTask` consistently with adjacent APIs. Do not block on asynchronous work.
- Use `IClock` and NodaTime for time-dependent behavior; do not call `DateTime.Now`, `DateTime.UtcNow`, or `SystemClock.Instance` inside application behavior.
- Keep JSON behavior centralized in `JsonConfig`. New DTOs must respect the existing camel-case, enum, NodaTime, required-constructor, and nullable-annotation settings.
- Return `TypedResults` and declare all normal HTTP outcomes in `Results<...>`. Validate route/body input before doing work and return the existing RFC 9457-compatible validation/problem shapes.
- Use `Validation.Failure` for simple parameter checks. Add a focused validator only when validation is genuinely complex; validation must remain explicit at the endpoint boundary.
- Expected outcomes belong in typed return values, not exceptions. If a service layer with several domain outcomes is introduced, represent them with `OneOf<...>` and exhaustively `Match` them at the endpoint. Do not encode expected failure as `null`, magic values, or broad exceptions.
- Let `GlobalExceptionHandler` handle unexpected failures. Do not expose exception details to clients.

## Transactions and persistence

If database-backed writes are added, the endpoint owns the explicit transaction boundary: validate first, begin immediately before the write workflow, and commit only on a successful result branch. An exception or unsuccessful branch must remain uncommitted so scoped disposal rolls it back. Services and repositories must not begin or commit transactions.

## Tests and coverage

- Every behavior change needs tests for the successful path and relevant validation, failure, and edge paths. Coverage must exercise behavior, not merely execute lines.
- Keep unit and integration tests separate. If test projects are introduced, use `LeoMiniApi.Test` for unit tests and `LeoMiniApi.TestInt` for integration tests.
- Unit tests cover pure helpers and isolated business behavior. They must not start the web host or use network, filesystem, or database infrastructure; substitute dependencies such as `IClock`.
- Integration tests exercise the public HTTP contract through a test host, including routing, binding, serialization, status codes, and problem details. Replace nondeterministic dependencies through DI. If persistence is later added, use the real database provider in an isolated container and reset state between tests.
- Use xUnit, AwesomeAssertions, NSubstitute, and coverlet consistently with the larger API templates. Keep tests deterministic and pass `TestContext.Current.CancellationToken` to cancellable operations.

Before finishing, build the project and run every affected test project. Do not weaken analyzers, nullability, warnings-as-errors, or tests to make a change pass.
