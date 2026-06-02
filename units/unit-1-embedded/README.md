# Unit 1 — Embedded Setup Track (Path B)

> **You've chosen the embedded path.** Single-process model: the Bowire workbench mounts *inside* your ASP.NET host via `AddBowire()` + `MapBowire()`, sharing the host's port, DI container, lifecycle, and config. The tracks merge again at [Unit 2](../unit-2/README.md) once setup is done — recording, mocking, AI integration, plugin work all use the same workbench UI regardless of which setup track you walked.

## When this path fits

- You own the host and can `dotnet add package Kuestenlogik.Bowire` into it.
- You want the workbench available at `/bowire` on the same `<scheme>://<host>:<port>` your service already runs on — no second port, no second process, no `--url` flag for your dev team to memorise.
- You want the workbench to see *everything the host's DI container sees*: gRPC services registered through `AddGrpc`, SignalR hubs, OpenAPI documents, custom protocols, all in one pass — no per-URL probing.
- You want a single Dockerfile / deploy bundle for the service + workbench.

If your service isn't yours to modify (or isn't .NET), or you'd rather not couple the workbench to the host's lifecycle, look at [Unit 1 — CLI Setup Track](../unit-1-cli/README.md) instead.

## Lessons in this track

| # | Lesson | Duration |
|---|---|---|
| 1.1 | [Mount the workbench in your host — REST](lesson-1/README.md) | 12 min |
| 1.2 | [Add a second protocol — gRPC](lesson-2/README.md) | 12 min |

After Lesson 1.2 → continue to [Unit 2: Record, Replay, Mock](../unit-2/README.md).

## Mental model

```
┌─────────────────────────────────────────────────┐
│  Your ASP.NET host (port 5001)                  │
│                                                 │
│   ├─ /hello/{name}              (your routes)   │
│   ├─ /openapi/v1.json           (MapOpenApi)    │
│   └─ /bowire                    (MapBowire)     │
└─────────────────────────────────────────────────┘
   one terminal, one process
```

The workbench is a regular ASP.NET endpoint inside your host. Your existing routes, auth, middleware, and OpenAPI metadata stay exactly where they were — Bowire reads the same DI registrations they do, then renders the workbench UI at `/bowire`.

## Production note

In production you probably *don't* want `/bowire` mounted alongside the service. Two common patterns:

1. **Compile-time gate:** `#if DEBUG` around the `AddBowire()` / `MapBowire()` calls. Workbench available in dev builds only.
2. **Runtime gate:** wrap the registrations in `if (builder.Environment.IsDevelopment())`. Workbench available when `ASPNETCORE_ENVIRONMENT=Development`.

Both ship in the same single binary. We'll keep things ungated through this track to keep the walkthrough simple.
