# Lesson 4.3: Interceptor middleware

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 4.1](../lesson-1/README.md); an ASP.NET host you control

## Overview

One line in your host and Bowire captures **every request the process serves** — method, path, headers, request/response bodies, status, latency — no proxy, no client-cert trust, no per-route attribute. This is the embedded counterpart to the standalone reverse-proxy from [Unit 3.4](../../unit-3/lesson-4/README.md); both feed the same **Intercept rail** you toured in [Unit 2.5](../../unit-2/lesson-5/README.md).

## Steps

### 1. Reference the package

```bash
dotnet add package Kuestenlogik.Bowire.Interceptor
```

One package, two halves: the `UseBowireInterceptor()` middleware **and** the rail-side UI that surfaces what it captured.

### 2. Mount the middleware

```csharp
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Interceptor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBowire();

var app = builder.Build();
app.UseBowireInterceptor();        // ← everything from here on is captured

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapBowire("/bowire");           // workbench mounted last
app.Run();
```

**Order matters** — the interceptor only sees requests that flow through it. Put `UseBowireInterceptor()` between `Build()` and the `Map*` calls you want observed. `/bowire` is excluded by default (`IgnoredPathPrefixes`) so the rail doesn't watch itself.

### 3. Drive traffic, watch the rail

Fire a few requests (`curl …/api/health`, a POST, …), then open `/bowire` → **Intercept → Captured**: each flow shows request/response headers + body, status and latency. Raw JSON at `GET /api/intercepted/flows`; a live SSE feed at `/api/intercepted/stream`.

### 4. Tune `BowireInterceptorOptions`

```csharp
app.UseBowireInterceptor(options =>
{
    options.MaxBodyBytes = 4 * 1024 * 1024;        // per-side body cap (default 1 MiB)
    options.MaxRetainedFlows = 5000;               // ring-buffer capacity (default 1000)
    options.IgnoredPathPrefixes.Add("/healthz");   // mute a noisy probe
    options.Enabled = true;                        // master kill-switch
    options.MocksEnabled = true;                   // honour InterceptorMockStore rules
});
```

Non-obvious points:
- Bodies over the cap are captured up to the limit; a `…BodyTruncated` flag flips so the rail can label them (the handler still gets the full body — the middleware **tees**, it doesn't gate).
- `IgnoredPathPrefixes` is a mutable `List<string>`; matching is case-insensitive prefix.
- Streaming responses (SSE, chunked, WebSocket upgrade, gRPC bi-di) are stored with an empty body + a "streaming" badge.
- `MocksEnabled = false` disables the mock short-circuit entirely — read-only observability for a production host.

### 5. Auto-recording is free

Start a Capture-mode recording from the Recordings rail, drive traffic, stop — every intercepted flow auto-appends as a step. No client-side recorder, no SDK call. Replay it like any recording ([Unit 2.1](../../unit-2/lesson-1/README.md)).

## Key Takeaways

1. **Two lines** — package + `app.UseBowireInterceptor()`; the rail UI, JSON API and SSE feed come with it.
2. **Order matters; self-traffic is excluded** via `IgnoredPathPrefixes` (default `["/bowire"]`).
3. **Small explicit option surface** — `MaxBodyBytes`, `MaxRetainedFlows`, `IgnoredPathPrefixes`, `Enabled`, `MocksEnabled`.
4. **Embedded interceptor vs standalone reverse-proxy** ([Unit 3.4](../../unit-3/lesson-4/README.md)) — same rail, different shape.

## What's Next

That closes the embedding unit. To ship *new protocols / extensions* on top of Bowire, continue to [Unit 5: Extend Bowire](../../unit-5/README.md).

## Reference

- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorOptions.cs`
- [Interceptor docs](https://bowire.io/docs/)
