# Unit 3: AI-Agent Integration

*Time: ~10 minutes • Lessons: 1 • Previous: [Unit 1](../unit-1/README.md)*

Wire the workbench into a language model over MCP. The agent gains a `bowire.discover` / `bowire.invoke` / `bowire.recordings.list` toolset and can drive any of your APIs in plain English.

## Prerequisites

- [Unit 1](../unit-1/README.md) complete — understand the discovery + invoke surface before handing it to an agent.
- **Claude Desktop** *or* **Cursor** installed locally. Any other MCP-aware host (custom MCP gateway, Continue.dev, &c) works too — the config snippet is portable; only the location of the `mcpServers` config file changes.
- The Unit 1 sample servers (`HelloApi` + `HelloGrpc`) ideally still running — the lesson drives the agent against them.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [3.1](lesson-1/README.md) | Claude Desktop + Cursor over MCP | `bowire mcp serve` wired into the agent's `mcpServers` config, drive REST + gRPC + recordings from the chat window |

## Why this unit

Bowire and MCP intersect in [four distinct ways](https://bowire.io/docs/protocols/mcp.html); Unit 3 covers the one that lets an agent **drive Bowire**. Same SDK, same install, same workbench — but the consumer is a Claude or Cursor chat instead of a browser. Useful for: "explore an unfamiliar API by asking questions about it", "regression-check that endpoints A through Z still return the right shape", "spin up a mock fixture for a downstream test from natural-language instructions".

---

**Next:** → [Unit 4: Extending Bowire](../unit-4/README.md)
