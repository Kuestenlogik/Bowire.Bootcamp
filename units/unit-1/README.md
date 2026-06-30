# Unit 1 — Workbench basics

> **What you're learning.** First contact with the Bowire workbench against your own services. The deployment shape — standalone CLI (`bowire --url …`) vs embedded host (`AddBowire()` / `MapBowire()`) — is a setup choice **inside** each lesson, not a separate path. Pick whichever matches your environment in the lesson's setup section; the workbench walkthrough after that is identical.

## When this unit fits

- You've finished Unit 0 and want to drive a workbench against a service of your own (rather than the public demo from Lesson 0.3).
- You want to see how Bowire renders REST + gRPC side-by-side in one sidebar.
- You want the mental model for everything that follows — Record / Replay / Mock (Unit 2), AI-Agent integration (Unit 3), plugin authoring (Unit 4), and CI integration (Unit 5) all assume you've already seen the workbench drive a real service.

## Lessons in this unit

| # | Lesson | Duration |
|---|---|---|
| 1.1 | [First call — REST](lesson-1/README.md) | 10 min (CLI shape) / 12 min (Embedded shape) |
| 1.2 | [Multi-protocol — REST + gRPC](lesson-2/README.md) | 10 min (CLI shape) / 12 min (Embedded shape) |

After Lesson 1.2 → continue to [Unit 2: Record, Replay, Mock](../unit-2/README.md).

## Mental models — pick the shape inside each lesson

### CLI shape (two processes)

```
┌─────────────────────────┐      ┌───────────────────────────┐
│  Your service           │      │  bowire CLI               │
│  (HelloApi on :5001)    │ ◀──▶ │  http://localhost:5080/   │
└─────────────────────────┘      │     bowire (workbench UI) │
                                 └───────────────────────────┘
   terminal 1                       terminal 2
```

One terminal for the service, one for the workbench. The workbench discovers the service through the URL you pass via `--url` and renders each method in the sidebar.

### Embedded shape (one process)

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

## Production note (Embedded shape)

In production you probably *don't* want `/bowire` mounted alongside the service. Two common patterns:

1. **Compile-time gate:** `#if DEBUG` around the `AddBowire()` / `MapBowire()` calls. Workbench available in dev builds only.
2. **Runtime gate:** wrap the registrations in `if (builder.Environment.IsDevelopment())`. Workbench available when `ASPNETCORE_ENVIRONMENT=Development`.

Both ship in the same single binary. The lessons here keep things ungated to keep the walkthrough simple.
