# Unit 1: The Workbench — first contact

*Time: ~20 minutes • Lessons: 2 • Modality: UI (workbench)*

Your first hands-on time in the Bowire workbench: discover a service, invoke a method, and watch a live stream — all from the browser UI. This unit is **UI-only**. It doesn't teach how Bowire got launched: that's a one-line bootstrap you pick from your course's shape unit.

> **Get the workbench on screen first.** This unit assumes the workbench is open with at least one service discovered.
> - **CLI** (point at any URL): `bowire --url <service>` — full walkthrough in [Unit 3: CLI & operations](../unit-3/README.md).
> - **Embedded** (inside your ASP.NET host): `AddBowire()` + `MapBowire()` — [Unit 4: Embed Bowire](../unit-4/README.md).
>
> Everything below is identical regardless of which shape mounted the workbench — which is exactly why the UI walkthrough lives here, once, and the shape units link to it.

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [1.1](lesson-1/README.md) | First contact | The rail strip, Discover, the Compose invoke pane, invoking a unary method |
| [1.2](lesson-2/README.md) | Multi-protocol in one workbench | REST + gRPC side by side, server-streaming in the stream pane, switching services |

## Why this unit

The workbench UI is the surface every role touches. Learn it once here; the CLI, embedded and extension units all point back to these two lessons instead of re-teaching the invoke flow.

---

**Next:** → [Lesson 1.1: First contact](lesson-1/README.md)
