# Unit 3: CLI & operations

*Time: ~70 minutes • Lessons: 7 • Modality: CLI*

Everything Bowire does from the command line — install and drive it, replay and test in CI, expose it to an AI agent, front an upstream as a reverse-proxy, deploy it, observe it, and keep workspaces tidy. This unit is **CLI-only**: where the workbench UI is the natural surface (the invoke walkthrough, the Intercept rail, Flow authoring) it *links* to the UI unit rather than opening it inline.

Drive the CLI against the single-plugin demos in [`Bowire.Samples/protocols/`](https://github.com/Kuestenlogik/Bowire.Samples) (`Rest.PetStore`, `Grpc.Greeter`, …) — they don't embed Bowire, so they're the natural `bowire --url` targets. The richer Harbor domain works too.

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [3.1](lesson-1/README.md) | Install & first call | `dotnet tool install`, `bowire --url`, `list` / `describe` / `call`, repeatable `--url` |
| [3.2](lesson-2/README.md) | Mock & test in CI | `bowire mock`, `bowire test` (recording + Flow), `--junit` / `--report`, GitHub Actions |
| [3.3](lesson-3/README.md) | AI agents over MCP | `bowire mcp serve` (stdio), the tool surface, Claude Desktop wiring, the security gate |
| [3.4](lesson-4/README.md) | Reverse-proxy interception | `bowire interceptor` fronting an upstream; feeds the Intercept rail; TLS + CA |
| [3.5](lesson-5/README.md) | Deployment patterns | Container / systemd, layered config (`appsettings` → `BOWIRE_*` → flags), reverse-proxy in front |
| [3.6](lesson-6/README.md) | Observability & operations | `--telemetry` + OTLP, Bowire-domain metrics, plugin health, backup, per-plugin disable |
| [3.7](lesson-7/README.md) | Workspace hygiene | Soft vs hard deletion, Trash retention, undo semantics |

## Why this unit

The CLI is what turns Bowire from a laptop debugger into a headless, scriptable, deployable tool — the surface every integrator, DevOps and platform engineer lives in.

---

**Next:** → [Lesson 3.1: Install & first call](lesson-1/README.md)
