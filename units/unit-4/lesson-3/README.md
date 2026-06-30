# Lesson 4.3: Interceptor middleware — capture every request your host serves

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Unit 1.1](../../unit-1/README.md) (Embedded shape), .NET 10 SDK, an ASP.NET host you control

## Overview

Drop one line into your ASP.NET host and Bowire's workbench starts showing every request flowing through the process — method, path, headers, request body, response status, response headers, response body, end-to-end latency. No client cert trust, no separate proxy process, no per-route attribute. This lesson covers the `Kuestenlogik.Bowire.Interceptor` package: what it captures, how to turn it on, the option surface you can tune, and the rail it surfaces in.

By the end you'll have the Sample.Embedded host running with the interceptor mounted, you'll have driven traffic through it from `curl`, and you'll have watched the captured flows appear in the workbench's Intercepted rail (and the `/api/intercepted/flows` JSON endpoint).

> **One package, two roles.** `Kuestenlogik.Bowire.Interceptor` is the v2.1 consolidation of the old Proxy / Intercepted / Traffic rails — it ships both the `UseBowireInterceptor()` middleware AND the rail-side UI that surfaces what the middleware captured. Reference it once; both halves light up.

## Steps

### 1. Reference the package

The interceptor ships as its own NuGet. From your embedded host project root:

```bash
dotnet add package Kuestenlogik.Bowire.Interceptor
```

If you're following along against `samples/Kuestenlogik.Bowire.Sample.Embedded` from the main Bowire repo, the reference is already there — skip this step.

### 2. Mount the middleware

Open the host's `Program.cs` and add a `using` + a `app.UseBowireInterceptor()` call between `var app = builder.Build()` and your endpoints:

```csharp
using Kuestenlogik.Bowire;
using Kuestenlogik.Bowire.Interceptor;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBowire();

var app = builder.Build();

app.UseBowireInterceptor();        // ← every request from here on is captured

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapBowire("/bowire");          // workbench mounted last
app.Run();
```

Order matters. The interceptor only sees requests that pass through it; anything mapped *before* `UseBowireInterceptor` (typically `UseRouting` is fine — auth / static-file middleware that short-circuits is not) bypasses the capture.

> **Self-traffic exclusion.** Out of the box `IgnoredPathPrefixes` contains `"/bowire"`, so the rail doesn't observe itself. If you mount the workbench at a non-default path (`MapBowire("/devtools")`), extend the list — see Step 4.

### 3. Drive some traffic + watch the Intercepted rail

Start the host:

```bash
dotnet run --project samples/Kuestenlogik.Bowire.Sample.Embedded --urls http://localhost:5181
```

Then in another terminal:

```bash
curl http://localhost:5181/api/health
curl http://localhost:5181/api/users
curl -X POST http://localhost:5181/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"Ada Lovelace","role":"Pioneer"}'
```

Open `http://localhost:5181/bowire`. The workbench's rail strip now shows an **Intercepted** rail with three entries — `GET /api/health`, `GET /api/users`, `POST /api/users`. Click one: the detail pane shows the captured request headers + body, the response status / headers / body, and the latency.

You can also hit the JSON surface directly:

```bash
curl http://localhost:5181/api/intercepted/flows | jq '.[0]'
```

Same data, raw. The rail's live-feed pane uses `/api/intercepted/stream` (SSE) — open it in a browser tab and it stays current as you fire more `curl`s.

### 4. Tune the options surface

`UseBowireInterceptor` takes an optional `Action<BowireInterceptorOptions>` so you can adjust behaviour without re-deploying configuration:

```csharp
app.UseBowireInterceptor(options =>
{
    options.MaxBodyBytes = 4 * 1024 * 1024;        // raise the per-side body cap from 1 MiB to 4 MiB
    options.MaxRetainedFlows = 5000;               // ring-buffer capacity (FIFO eviction)
    options.IgnoredPathPrefixes.Add("/healthz");   // mute a noisy probe route
    options.Enabled = true;                        // master kill-switch
    options.MocksEnabled = true;                   // honour InterceptorMockStore rules
});
```

Every property above exists on `BowireInterceptorOptions` in `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorOptions.cs` — defaults are 1 MiB / 1000 flows / `["/bowire"]` / on / on.

A few non-obvious points:

- **Bodies over `MaxBodyBytes`** are still captured up to the cap; the flow's `RequestBodyTruncated` / `ResponseBodyTruncated` flag flips so the rail can label them.
- **`IgnoredPathPrefixes` is a `List<string>`**, not a record — mutate it in place (`Add`, `Remove`). Matching is case-insensitive, prefix-based: `/bowire` covers `/bowire/api/foo`.
- **Streaming responses** (SSE, chunked, WebSocket upgrade, gRPC bi-di) are detected and stored with an empty body. The rail shows a "streaming" badge instead of attempting to buffer the open stream.
- **`MocksEnabled = false`** turns off the `InterceptorMockStore` short-circuit path entirely — useful for a production host that loaded the package for read-only observability and wants to be sure no workbench-defined rule can change response bytes.

### 5. Verify auto-recording (Phase B)

The middleware is wired to `BowireRecordingSession`. Start a Capture-mode recording from the workbench's Recording rail, drive a few more `curl`s, then stop. The recording now has one step per intercepted flow — no client-side recorder, no SDK call, no proxy. Replay it like any other Bowire recording.

## Exercise — capture your own route

1. In `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs`, add a new `POST /api/orders` endpoint that takes a JSON `{ "sku": "...", "qty": ... }` body and returns `{ "orderId": "...", "status": "queued" }`.
2. Restart the host.
3. Fire a request:
   ```bash
   curl -X POST http://localhost:5181/api/orders \
     -H "Content-Type: application/json" \
     -d '{"sku":"WIDGET-001","qty":3}'
   ```
4. Open the Intercepted rail. Confirm:
   - The flow appears with method `POST`, path `/api/orders`.
   - The detail pane shows your request body verbatim.
   - The response pane shows the `{ "orderId": "...", "status": "queued" }` payload your handler produced.
   - The latency is recorded (typically a few ms for a local handler).

If the flow doesn't appear, the most common cause is endpoint ordering — `MapBowire("/bowire")` or your endpoint mapping running *before* `UseBowireInterceptor` will hide that request from the capture path.

## Key Takeaways

1. **Two lines of host change.** `dotnet add package Kuestenlogik.Bowire.Interceptor` + `app.UseBowireInterceptor()`. Everything else — the rail UI, the JSON API, the SSE live feed — is in the package.
2. **Order matters.** `UseBowireInterceptor` only captures what flows through it. Mount it before the endpoints you want observed; the workbench's own surface is excluded by default via `IgnoredPathPrefixes`.
3. **The options surface is small and explicit.** Five properties: `MaxBodyBytes`, `MaxRetainedFlows`, `IgnoredPathPrefixes`, `Enabled`, `MocksEnabled`. No global config file, no environment-variable plumbing — `Action<BowireInterceptorOptions>` is the whole API.
4. **Recording integration is free.** When a Capture-mode recording session is active, every intercepted flow auto-appends as a step. No extra wiring on the host side.

## Knowledge Assessment

1. **Middleware ordering.** Your host calls `app.UseBowireInterceptor()` AFTER `app.MapGet("/api/users", ...)` and then `app.MapBowire("/bowire")`. A `GET /api/users` request comes in. Does the Intercepted rail capture it?  
   *Answer:* No — `MapGet` registers the endpoint, but endpoint *execution* runs through the pipeline that was built up to that point. In ASP.NET, middleware ordering is by `Use*` call order. Since `UseBowireInterceptor` is added after the endpoint mapping, it still wraps the endpoint pipeline correctly **as long as `UseRouting` / `UseEndpoints` (or the minimal-API equivalents) come after it**. The right rule of thumb: put `UseBowireInterceptor` between `var app = builder.Build()` and any `Map*` call you want observed.

2. **Self-traffic.** You change `MapBowire("/bowire")` to `MapBowire("/devtools")`. The Intercepted rail now floods with its own SSE / API traffic. Why, and what's the one-line fix?  
   *Answer:* `IgnoredPathPrefixes` defaults to `["/bowire"]` — the new mount path no longer matches. Fix: `options.IgnoredPathPrefixes.Add("/devtools")` inside the `UseBowireInterceptor` configure callback.

3. **Body cap.** A client sends a 5 MiB JSON payload. With default options, what does the captured flow contain in `requestBody`?  
   *Answer:* The first 1 MiB (the `MaxBodyBytes` default) and `RequestBodyTruncated = true` on the flow record. The downstream handler still receives the full body — the middleware tees, it does not gate.

4. **Mocks off.** A production host pins `MocksEnabled = false`. What still works, and what doesn't?  
   *Answer:* Capture, the Intercepted rail, the SSE live feed, recording auto-append — all still work. The `InterceptorMockStore` short-circuit path is disabled, so no workbench-defined mock rule can replace a real response. Useful when you only want read-only observability.

## What's Next

You've extended Bowire on the request-capture axis. Lesson 4.4 covers the other extension axis: **UI extensions** — what happens when your response payload carries a `coordinate.wgs84` semantic kind, and how Bowire mounts a map widget over it.

**Continue:** → [Lesson 4.4: Map widget + semantic kinds](../lesson-4/README.md)

## Reference

- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorApplicationBuilderExtensions.cs` — the `UseBowireInterceptor` entry point + service registration.
- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorOptions.cs` — every public option, with default values.
- `src/Kuestenlogik.Bowire.Interceptor/BowireInterceptorMiddleware.cs` — the capture path; Phase A pass-through, Phase B recording integration, Phase D mock injection.
- `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` — the canonical embedded host wiring used in this lesson.
