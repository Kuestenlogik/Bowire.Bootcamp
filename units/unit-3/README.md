# Unit 3: AI agents, assertions, coverage — turning "call" into "verify"

*Time: ~35 minutes • Lessons: 3 • Previous: [Unit 1](../unit-1/README.md)*

Wire the workbench into a language model over MCP, layer declarative assertions on top of Flows, and watch coverage decay across the discovered surface. Three lessons about *turning a running call into a verified one*.

## Prerequisites

- [Unit 1](../unit-1/README.md) complete in either shape (CLI or Embedded — the setup tab inside each Unit 1 lesson) — understand the discovery + invoke surface before layering assertions or agent access on top.
- **Claude Desktop** *or* **Cursor** installed locally (Lesson 3.1 only). Any other MCP-aware host (custom MCP gateway, Continue.dev, &c) works too — the config snippet is portable.
- The Unit 1 sample servers (`HelloApi` + `HelloGrpc`) ideally still running — every lesson drives against them.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [3.1](lesson-1/README.md) | Claude Desktop + Cursor over MCP | `bowire mcp serve` wired into the agent's `mcpServers` config, drive REST + gRPC + recordings from the chat window |
| [3.2](lesson-2/README.md) | Flow assertions — from "it ran" to "it's correct" | A Flow with five kinds of expectation (status / header / body-path / body-text / latency), a red-then-green regression |
| [3.3](lesson-3/README.md) | Regression Coverage — what have I actually tested? | The per-method coverage chip (recent / stale / failing / uncovered) + the Settings → Data run-history view |

## Why this unit

A backend that responds is not a backend that responds *correctly*, and a workbench full of green ticks tells you nothing about the methods you *haven't* touched. Unit 3 covers the three tools that close that loop:

- **3.1** hands Bowire's discovery + invoke surface to a language model — useful for exploratory testing of unfamiliar APIs.
- **3.2** layers declarative expectations on Flows so a step's outcome is `pass` / `fail`, not just "the wire moved".
- **3.3** aggregates every runner's history into a four-state coverage chip per method — recent, stale, failing, uncovered — so you can see what you haven't exercised.

---

**Next:** → [Unit 4: Extending Bowire](../unit-4/README.md)
