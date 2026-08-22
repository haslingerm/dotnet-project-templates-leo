# LeoWebApi development rules

These instructions apply to this project. Preserve the existing layered minimal-API architecture and its explicit control flow.

## Architecture and dependency direction

- `LeoWebApi` is the transport/composition layer: startup, endpoint groups, HTTP DTOs, explicit request validation, and HTTP result mapping.
- `LeoWebApi.Services` contains business workflows and logic. Services depend on persistence abstractions and return domain outcomes; they do not know HTTP.
- `LeoWebApi.Persistence` contains EF entities, `DatabaseContext`, repositories, and the unit of work/transaction provider. Repositories query or stage entity changes; the unit of work saves them.
- `LeoWebApi.Shared` contains small cross-layer primitives only. Do not turn it into a dumping ground or make it depend on higher layers.
- Dependencies flow inward from the host through services to persistence/shared code. Endpoints may use persistence entities for explicit DTO mapping and `ITransactionProvider` for their transaction boundary, as the template does, but must not query `DatabaseContext` or repositories directly. Do not introduce reverse references.
- `LeoWebApi.Test` is the isolated unit-test project. `LeoWebApi.TestInt` is the full HTTP/database integration-test project.

## HTTP and business flow

- Put each resource in its own static endpoint class and route group under `Endpoints/`. Add its mapping explicitly in `Setup.MapApplicationEndpoints`; do not use reflection or controllers.
- Endpoint methods own binding, authorization when added, validation, transaction boundaries, and conversion between service outcomes and typed HTTP results. Keep business rules in services.
- Keep API request/response DTOs out of persistence and do not expose EF entities as the HTTP contract. Map deliberately between them.
- Declare every expected response using `TypedResults` and `Results<...>`, with resource-specific status codes and RFC 9457 validation/problem details.
- Use FluentValidation for request objects, normally with the nested `Request.Validator` pattern. Invoke validation explicitly in the endpoint before starting a transaction: create the validator and call `validator.Check(request)`, which returns the `ValidationProblem` to return or null when the request is valid. Use `Validation.Failure` for simple route checks.
- Validators are database-less and compare against constants, configuration, or the clock. A validator needing extra information takes it through its constructor, and the endpoint passes it when creating the validator. Validators are not registered in DI, discovered by reflection, or run from an endpoint filter - an endpoint that can answer 400 must declare `ValidationProblem` in its own `Results<...>` union and return it itself.
- Services must represent expected alternatives with `OneOf<...>` (`Success`, `NotFound`, or small domain-specific result types). Do not use exceptions, `null`, booleans, or magic values for multi-outcome business operations. Endpoints must exhaustively translate outcomes with `Match`.
- Exceptions are for unexpected failures and are translated centrally by `GlobalExceptionHandler`; never leak their details to clients.

## Persistence and transactions

- Every write endpoint must validate first, call `BeginTransactionAsync` immediately before the write workflow, and call `CommitAsync` only in its successful result branch.
- The endpoint owns the transaction. Services and repositories must never begin, commit, or hide a transaction. Do not rely on EF's implicit `SaveChanges` transaction for an endpoint write workflow.
- Leave unsuccessful result branches and exceptions uncommitted; scoped `UnitOfWork` disposal intentionally rolls them back. Use explicit rollback only when execution must continue within the same scope after the failed transaction.
- Services coordinate repositories and call `IUnitOfWork.SaveChangesAsync`; repositories must not save or return transport DTOs.
- Make tracking intent explicit. Reads are no-tracking by default; request tracking only when an entity will be modified or removed in the same unit of work.
- Keep mappings and constraints in `DatabaseContext`. Create a migration for every schema change and ensure integration tests apply it.

## C# conventions

- Preserve nullable reference types, latest analyzers, warnings-as-errors, and the configured Leo analyzers. Fix warnings rather than suppressing them.
- Always use Allman braces: every block-opening brace is on its own line, including methods, properties, control flow, block lambdas, and local functions.
- Use file-scoped namespaces, four-space indentation, primary constructors where they improve clarity, and sealed types when inheritance is not intended.
- Do not use the null-forgiving operator (`!`) outside EF entity properties where framework materialization makes it strictly unavoidable. Prefer `required`, accurate nullable types, guards, pattern matching, or explicit exceptions. Never use `!` merely to silence analysis.
- Use NodaTime and injected `IClock` for domain time. Keep JSON behavior in `JsonConfig`, logging structured, and secrets/configuration out of source.
- Async methods use the `Async` suffix and do not block. Follow the established preference for explicit contract/result types and `var` only when the assigned type is obvious.

## Tests and coverage

- Every behavior change requires meaningful coverage of its success path plus relevant validation, not-found/conflict, rollback, and edge paths. Do not count generated code, trivial accessors, or line-only execution as useful coverage.
- Unit tests belong in `LeoWebApi.Test`. Test pure logic and services in isolation, substituting `IUnitOfWork`, repositories, clocks, and other collaborators with NSubstitute. Unit tests must not start ASP.NET or access a real database, network, or filesystem.
- Integration tests belong in `LeoWebApi.TestInt`. Exercise the public API through `HttpClient` and `WebApplicationFactory` against PostgreSQL via Testcontainers; verify routing, binding, JSON, status codes, response bodies, persistence, and transaction behavior.
- Derive integration tests from `WebApiTestBase`, seed through `ImportSeedDataAsync`/`ModifyDatabaseContentAsync`, and rely on the fixture's per-test schema reset. Never make integration tests order-dependent.
- Use xUnit and AwesomeAssertions, keep tests deterministic, replace `IClock` through the test factory, and pass `TestContext.Current.CancellationToken` to cancellable calls.

Before finishing, run `dotnet build LeoWebApi.slnx` and the affected unit and integration tests (integration tests require Docker and a current migration). Do not weaken production checks or tests to obtain a passing build.
