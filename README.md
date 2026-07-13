# Marky.Framework

A monorepo housing foundational .NET infrastructure library for distributed systems that I use to fast-track projects.

## Core Architectural Modules

### Persistence Engine

- **Universal Schema Migration & Orchestration Engine(`Toolkit.Migration`)** A provider-agnostic execution runner that decouples environment-specific data initializing loops from core application runtimes. It handles zero-allocation pre-flight TCP socket connectivity validation, automated database setup strategies (DropAndRecreate sandboxes vs. sequential production ApplyMigrations), and executes type-safe, reflection-driven data seeding pipelines sequentially within atomic transaction boundaries.
- **Multi-Context Transaction Engine:** Layered on top of `Entity Framework Core` to coordinate atomic, single-pass persistence synchronization across multi-database environments and distinct context boundaries.
- **Domain Lifecycle Core:** A set of lightweight, base domain contracts (`IAuditable`, `ISoftDelete`) that automate record chronological auditing and global read-filtering logic.
- **Transactional Memory Broker:** An extension of `StackExchange.Redis` that interfaces directly with the transaction engine to provide high-velocity write-behind caching pipelines and distributed locking mechanism wrappers.
- **Virtualized CDC Outbox Redirection:** A custom Change Data Capture (CDC) mechanism that intercepts entity tracking graphs *pre-save*. When enabled, it suppresses original table writes, flattens entity mutations to JSON payloads, and appends them to an immutable outbox log table inside the primary database boundary—allowing a background worker (`BackgroundService`) to rehydrate and distribute state changes asynchronously. This completely eliminates the dual-write anti-pattern and guarantees eventual consistency across multiple bounded contexts.

### Enterprise Web Toolkit

This category encapsulates cross-cutting enterprise patterns and transport utilities standard in domain-driven Web applications.

- **Permission-Based Access Control:** An explicit, claim-centric security infrastructure utilizing custom authorization requirements and handlers (`PermissionAuthorizationHandler`). It provides a lightweight `[HasPermission]` attribute policy mapping to secure endpoints directly against user identity scopes.
- **Automated OpenAPI/NSwag Schema Enrichment:** A compiler-aware operation processor (`AutoErrorResponseProcessor`) that automatically scans metadata and route attributes to inject deterministic HTTP status codes (400, 401, 403, 500) and error envelope schemas straight into the generated Swagger documentation.
- **Unified Domain Error Mapping:** A centralized controller foundation (`BaseApiController`) that transforms polymorphic `ErrorOr` domain results into standardized JSON error envelopes (`ErrorResponse`) with explicit HTTP status alignment.
- **Fail-Fast Pre-Processing Pipeline:** A MediatR pipeline behavior middleware (`ValidationBehavior<,>`) integrated with `FluentValidation` that intercepts incoming commands and queries, automatically mapping validation failures to domain-specific error objects and short-circuiting requests before they touch the application layer.

> **Target Runtime:** `.NET 8.0`
