# LeoGRpcApi development rules

These instructions apply to this project. Preserve its contract-first gRPC design, layered backend, and MVVM client.

## Architecture and dependency direction

- `shared/contract.proto` is the single wire-contract source. `LeoGRpcApi.Shared.ApiContract` generates the C# contract and owns validation that can be shared by client and server. Never edit generated protobuf files.
- `backend/LeoGRpcApi.Api` is the server transport/composition layer: gRPC endpoint implementations, DTO mapping, status mapping, interceptors, and startup.
- `backend/LeoGRpcApi.Api.Services` owns business workflows and typed domain outcomes. It has no gRPC transport concerns.
- `backend/LeoGRpcApi.Api.Persistence` owns EF entities, repositories, `DatabaseContext`, and the unit of work/transaction provider. Repositories query or stage changes; the unit of work saves them.
- `backend/LeoGRpcApi.Api.Shared` contains small backend-wide primitives only.
- `frontend/LeoGRpcApi.Client.Core` wraps generated gRPC clients and exposes UI-independent services. `frontend/LeoGRpcApi.Client` is Avalonia MVVM: view models own state/commands, views own presentation, and code-behind stays limited to view concerns.
- Keep dependencies pointing inward along the existing project references. Do not let persistence or services depend on server endpoints, and do not move UI concerns into Client.Core or the shared contract.

## Protobuf and gRPC contract rules

- Treat every published field number and enum value as permanent. Never renumber or reuse one; make compatible changes by adding fields/RPCs and reserve removed numbers/names.
- Follow the existing proto naming: PascalCase messages/services, snake_case fields, and zero-valued `UNKNOWN_*` enum members. Validate unknown enum values at the boundary.
- Use `optional` when scalar/string presence has meaning, and use generated `HasX` properties when mapping. Use protobuf `oneof` for mutually exclusive fields or result payload variants; do not model alternatives as several independent optional fields.
- Use `repeated` for collections and empty request/response messages where the RPC shape requires them. Keep mapping between EF entities and generated DTOs explicit in `DtoExtensions`.
- Put reusable request validators in `Shared.ApiContract/Validation` and invoke them explicitly in both server endpoints and client service wrappers where applicable.
- Map invalid input to `InvalidArgument` and transport-level absence to `NotFound` via `RpcHelper`. When a negative result is normal application data, model it explicitly in the response (an enum or `oneof`) rather than throwing. Let `ExceptionInterceptor` sanitize and log unexpected failures.

## Backend flow, OneOf, and transactions

- Add one generated service implementation per resource under `Api/Endpoints` and map it explicitly in `Setup.MapApplicationEndpoints`.
- Endpoints own validation, transaction boundaries, DTO mapping, and translation to gRPC statuses/responses. Services own business rules and must not depend on `Grpc.Core`.
- Services represent expected alternatives with C# `OneOf<...>` and small named result types such as `MissionNotFound` or `NinjaBusy`. Do not replace these with `null`, booleans, magic values, or exceptions. Endpoints must exhaustively handle them with `Match`.
- Every mutating RPC validates first, calls `BeginTransactionAsync` immediately before its write workflow, and calls `CommitAsync` only in the successful result branch.
- The endpoint owns the transaction. Services and repositories must never begin, commit, or hide one. An exception or unsuccessful result stays uncommitted so scoped `UnitOfWork` disposal rolls it back. Use explicit rollback only if work must continue in the same scope.
- Services coordinate repositories and call `IUnitOfWork.SaveChangesAsync`; repositories do not save and do not return protobuf DTOs.
- Use no-tracking queries for read-only work and tracking queries only for entities being updated or removed. Configure relationships and constraints in `DatabaseContext`, and create a migration for each schema change.

## Client flow

- Client.Core services construct and validate requests, call generated clients, and return UI-independent contract/domain values. They do not show dialogs, toasts, or manipulate controls.
- View models use injected services, CommunityToolkit source generators (`ObservableProperty`, `RelayCommand`), and the messenger for cross-view coordination. Keep blocking calls and service location out of view models.
- Keep view code-behind thin. UI errors are logged and translated to user-facing notifications without exposing server internals.
- Maintain explicit design-time facades/view models when a view needs preview data; do not let design-only implementations enter runtime DI.

## C# conventions

- Preserve nullable reference types, latest analyzers, warnings-as-errors, and Leo analyzers. Fix warnings instead of suppressing them.
- Always use Allman braces: every block-opening brace is on its own line, including methods, properties, control flow, block lambdas, and local functions.
- Use file-scoped namespaces, four-space indentation, primary constructors where clear, and sealed types when inheritance is not intended.
- Do not use the null-forgiving operator (`!`) outside EF entity navigation properties where framework materialization makes it strictly unavoidable. Prefer `required`, accurate nullable types, guards, pattern matching, generated presence APIs, or explicit exceptions. Never use `!` merely to silence analysis.
- Use NodaTime and injected `IClock` for domain time, structured logging, and async APIs without blocking. Add `Async` suffixes except when overriding generated gRPC method names.

## Tests and coverage

- Every behavior change needs tests for the successful path and relevant invalid, not-found/conflict, alternate `OneOf`, transaction, mapping, and compatibility paths. Coverage must assert behavior, not merely execute lines.
- Backend unit tests belong in `LeoGRpcApi.Api.Test`. Isolate service/business logic with NSubstitute; do not start the host or access a database, network, or filesystem. Add a separate frontend unit-test project if substantial Client.Core or view-model logic needs testing; do not mix it into backend tests.
- Backend integration tests belong in `LeoGRpcApi.Api.TestInt`. Use the generated gRPC client against the in-process host and PostgreSQL via Testcontainers; verify protobuf presence, status codes, responses, persistence, and commit/rollback behavior.
- Derive integration tests from `ApiTestBase<TClient>`, seed through `ImportSeedDataAsync`/`ModifyDatabaseContentAsync`, and rely on the fixture's per-test schema reset. Tests must be deterministic and order-independent.
- Use xUnit and AwesomeAssertions, replace nondeterministic dependencies such as `IClock`, and pass `TestContext.Current.CancellationToken` to cancellable calls. Keep contract validation tests near the shared validator behavior they cover.

Before finishing, run `dotnet build LeoGRpcApi.slnx` and all affected unit and integration tests (integration tests require Docker and a current migration). Regenerate through the protobuf build after contract changes; never hand-edit generated output or weaken checks/tests to make a change pass.
