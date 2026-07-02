# Unit 5: Extend Bowire

*Time: ~55 minutes • Lessons: 4 • Modality: extension coding*

Ship *new* capability on top of Bowire — a protocol plugin (in .NET or any language), a UI extension, and the lifecycle controls that manage them. This unit is **coding**; it never drops into the CLI or UI mid-lesson (it links to Units 1/3 where you *run* what you built).

Real, shipped reference throughout: [`Bowire.Protocol.Akka`](https://github.com/Kuestenlogik/Bowire.Protocol.Akka) — a production `IBowireProtocol` plugin (`src/`, `tests/`, `samples/`) that streams an Akka.NET actor system into the workbench. Read it as the "completed" example; scaffold your own with `dotnet new bowire-plugin`.

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [5.1](lesson-1/README.md) | .NET protocol plugin | `IBowireProtocol`, `dotnet new bowire-plugin`, pack, `bowire plugin install` |
| [5.2](lesson-2/README.md) | Polyglot sidecar plugin | Ship a plugin in Python / Rust / Node / Go via the sidecar SDK + `.zip` |
| [5.3](lesson-3/README.md) | UI extension — semantic kinds | `[BowireExtension]`, `IBowireUiExtension`, auto-mounting a widget on `coordinate.wgs84` |
| [5.4](lesson-4/README.md) | Plugin lifecycle | Load / unload / restart / reset-storage / health, without a process restart |

## Why this unit

The plugin model is what makes Bowire *multi-protocol by design* rather than by a fixed list. This unit is for protocol authors and core contributors shipping new wires and UI surfaces.

---

**Next:** → [Lesson 5.1: .NET protocol plugin](lesson-1/README.md)
