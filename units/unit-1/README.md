# Unit 1: Workbench Basics

*Time: ~15 minutes • Lessons: 2 • Previous: [Unit 0](../unit-0/README.md)*

Drive Bowire's two foundational mechanics — protocol auto-discovery and the unified invoke form — across two wires at once.

## Prerequisites

- [Unit 0](../unit-0/README.md) complete (Bowire CLI installed + verified).
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — the bootcamp's `HelloApi` (REST, Lesson 1.1) and `HelloGrpc` (gRPC, Lesson 1.2) sample servers run on it.
- A second free TCP port — by default the workbench listens on `5080`, `HelloApi` on `5001`, `HelloGrpc` on `5002`. Override via `--port` if any of those are taken.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [1.1](lesson-1/README.md) | First call (REST + OpenAPI) | A REST sample API, discovered through its OpenAPI doc, invoked from the workbench |
| [1.2](lesson-2/README.md) | Multi-protocol session (REST + gRPC) | A gRPC sample side-by-side with REST, both discovered automatically, server-streaming demo |

## Why this unit

Bowire's headline claim is "one workbench, every wire". Unit 1 proves it: by the end you'll have a REST API and a gRPC service running side by side, both surfaced as identical-looking trees in the workbench sidebar, and you'll have invoked both unary and streaming methods through the same UI.

Everything after this unit (recording, mocking, plugins, CI, AI) builds on the discovery + invoke surface you wire up here.

---

**Next:** → [Unit 2: Record, Replay, Mock](../unit-2/README.md)
