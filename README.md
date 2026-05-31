# Bowire Bootcamp

A linear walkthrough that takes you from zero to "I can ship a new protocol plugin" with [Bowire](https://bowire.io) — Küstenlogik's multi-protocol API workbench.

The [reference docs](https://bowire.io/docs/) answer **what does X do**. This bootcamp answers **how do I go from zero to productive**, lesson by lesson, each one self-contained and runnable in 5–15 minutes.

## Lessons

| # | Lesson | Time | You'll learn |
|---|---|---|---|
| 01 | [First call](lesson-01-first-call/) | 5 min | Install `bowire`, run a sample API, invoke a method from the browser UI |
| 02 | [Multi-protocol session](lesson-02-multi-protocol/) | 10 min | Add a gRPC service to the same workbench, discover both wires at once |
| 03 | [Record & replay](lesson-03-record-replay/) | 10 min | Capture a session in the workbench, save it as `.bwr`, run it back as a local mock with `bowire mock` |
| 04 | [AI-agent integration](lesson-04-ai-agents/) | 10 min | Run `bowire mcp serve`, wire Bowire into Claude Desktop / Cursor as an MCP tool source |
| 05 | [Author a .NET protocol plugin](lesson-05-dotnet-plugin/) | 15 min | Implement `IBowireProtocol`, install your package into Bowire, see it discovered |
| 06 | [Author a Python sidecar plugin](lesson-06-python-sidecar/) | 15 min | Subclass `BowirePlugin`, ship via `sidecar.json` + zip, no .NET required |
| 07 | [Schema export + mock-as-stand-in](lesson-07-schema-export/) | 10 min | Discover → export OpenAPI/AsyncAPI → mock → peer-discover through the mock |
| 08 | [CI integration](lesson-08-ci/) | 15 min | `bowire test` in GitHub Actions; mock-server as a job service for downstream tests |

Lessons build on each other — start at 01 and work down, or jump in if you already know the basics.

## Prerequisites

- **.NET 10 SDK** — most lessons use it for the sample backend; the CLI itself is a global tool
- **Docker** — Lessons 03 and 08 use it for ephemeral broker services (Lesson 03 spins up MQTT, Lesson 08 runs the mock in a CI container)
- **Python 3.10+** — Lesson 06 only
- **Claude Desktop or Cursor** — Lesson 04 only

Install Bowire once globally:

```bash
dotnet tool install --global Kuestenlogik.Bowire.Tool
```

Verify:

```bash
bowire --version
```

## Layout per lesson

Every lesson directory looks the same:

```
lesson-NN-name/
├── README.md       # The walkthrough text — open this first
├── sample/         # Runnable .NET / Python / shell sample the lesson drives
└── assets/         # Screenshots, recordings, etc.
```

So you can `cd lesson-NN-name/sample && dotnet run` (or `python -m sample`) and follow along without leaving the directory.

## License

Apache 2.0 — same as the main Bowire repo. Lessons + samples can be lifted into your own training material; please link back when you do.
