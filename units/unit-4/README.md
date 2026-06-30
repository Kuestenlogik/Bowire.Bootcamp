# Unit 4: Extending Bowire

*Time: ~30 minutes • Lessons: 2 • Previous: [Unit 1](../unit-1/README.md)*

Author your own protocol plugin — once in .NET (in-process `IBowireProtocol` implementation, ships as a NuGet package), once in Python (sidecar plugin, ships as a zip with `sidecar.json` at its root). Same workbench, same install command, different language under the hood.

## Prerequisites

- [Unit 1](../unit-1/README.md) complete in either shape (CLI or Embedded — the setup tab inside each Unit 1 lesson) — you need a feel for how the host renders a discovered protocol before writing one.
- [.NET 10 SDK](https://dotnet.microsoft.com/download) + Bowire CLI.
- `dotnet new bowire-plugin` template installed:
  ```bash
  dotnet new install Kuestenlogik.Bowire.Templates
  ```
- **Python 3.10+** with `pip` — only for Lesson 4.2 (the sidecar half). Lesson 4.1 needs nothing beyond the .NET SDK.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [4.1](lesson-1/README.md) | .NET protocol plugin | A Pirate-Speak `IBowireProtocol` packaged via `dotnet pack` → `bowire plugin install` |
| [4.2](lesson-2/README.md) | Python sidecar plugin | A Yoda-Speak `BowirePlugin` subclass packaged as a sidecar `.zip` → `bowire plugin install --file` |

## Why this unit

Bowire's bundled plugin set (REST, gRPC, GraphQL, MQTT, WebSocket, SignalR, MCP, NATS, SOAP, Pulsar, &c) covers most protocols you'll encounter. Unit 4 covers the case where it doesn't.

The .NET path (4.1) is the canonical extension story — a NuGet package, an `IBowireProtocol` implementation, full type-safe access to the host's services. The Python path (4.2) is the polyglot escape hatch — when your protocol's reference implementation is `paho-mqtt`, `pulsar-python`, or some bespoke broker SDK, you ship the plugin in the language the SDK already lives in.

By the end you'll have both shapes in hand and a clear feel for when to reach for which.

---

**Next:** → [Unit 5: CI Integration](../unit-5/README.md)
