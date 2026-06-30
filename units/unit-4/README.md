# Unit 4: Extending Bowire

*Time: ~70 minutes • Lessons: 4 • Previous: [Unit 1](../unit-1/README.md)*

Four extension seams Bowire exposes to a host author: a .NET protocol plugin, a polyglot Python sidecar, the in-process HTTP interceptor, and a UI extension that mounts a widget over a semantic kind. Each ships as its own NuGet (or sidecar zip), each plugs into a different seam, all four are the same surface the bundled plugins use.

## Prerequisites

- [Unit 1](../unit-1/README.md) complete in either shape (CLI or Embedded — the setup tab inside each Unit 1 lesson) — you need a feel for how the host renders a discovered protocol before writing one.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI.
- `dotnet new bowire-plugin` template installed (Lessons 4.1 / 4.2 only):
  ```bash
  dotnet new install Kuestenlogik.Bowire.Templates
  ```
- **Python 3.10+** with `pip` — only for Lesson 4.2 (the sidecar half). Lesson 4.1 needs nothing beyond the .NET SDK.
- An ASP.NET host you control — for Lessons 4.3 / 4.4. The Bowire repo's `samples/Kuestenlogik.Bowire.Sample.Embedded` and `samples/Kuestenlogik.Bowire.Sample.TacticalApi` are both used as starter projects.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [4.1](lesson-1/README.md) | .NET protocol plugin | A Pirate-Speak `IBowireProtocol` packaged via `dotnet pack` → `bowire plugin install` |
| [4.2](lesson-2/README.md) | Python sidecar plugin | A Yoda-Speak `BowirePlugin` subclass packaged as a sidecar `.zip` → `bowire plugin install --file` |
| [4.3](lesson-3/README.md) | Interceptor middleware | `app.UseBowireInterceptor()` + a new `POST /api/orders` route observed in the Intercepted rail |
| [4.4](lesson-4/README.md) | Map widget / semantic kinds | A custom tactical entity rendered on the auto-mounted MapLibre viewer via the `coordinate.wgs84` kind |

## Why this unit

Bowire's bundled plugin set (REST, gRPC, GraphQL, MQTT, WebSocket, SignalR, MCP, NATS, SOAP, Pulsar, &c) covers most protocols you'll encounter; the bundled UI extensions (Map, soon others) cover most semantic kinds. Unit 4 covers the case where they don't — and the case where you want to extend the host's *behaviour* rather than its *protocol surface*.

- **4.1** is the canonical protocol-extension story — a NuGet package, an `IBowireProtocol` implementation, full type-safe access to the host's services.
- **4.2** is the polyglot escape hatch — ship the plugin in the language the SDK already lives in.
- **4.3** is the request-capture seam — `Kuestenlogik.Bowire.Interceptor` lets the workbench observe every HTTP flow through your host without a client cert or a separate proxy process.
- **4.4** is the UI-extension seam — `Kuestenlogik.Bowire.Map` is the dogfood example of `IBowireUiExtension` + `[BowireExtension]`, the same contract third-party widgets ship over.

By the end you'll have all four shapes in hand and a clear feel for when to reach for which.

---

**Next:** → [Unit 5: CI Integration](../unit-5/README.md)
