# Unit 4: Embed Bowire

*Time: ~45 minutes • Lessons: 3 • Modality: embedded coding*

Mount Bowire *inside* your own ASP.NET host — the workbench, an MCP endpoint for agents, and a traffic interceptor, all sharing your process, DI container, auth and config. This unit is **coding** (C# in `Program.cs`); it never switches to the CLI or the UI mid-lesson — where the workbench UI is the payoff, it links to [Unit 1](../unit-1/README.md).

Real, working reference: the `harbor-demo/` hosts in [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) already embed Bowire (`AddBowire()` / `MapBowire()` in each `Program.cs`) — read them as the "completed" example.

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [4.1](lesson-1/README.md) | Embed the workbench | `AddBowire()` + `MapBowire()`, DI/auth/config inheritance, gating out of production |
| [4.2](lesson-2/README.md) | Embedded MCP adapter | `AddBowireMcpAdapter()` / `MapBowireMcpAdapter()` — one shared HTTP MCP endpoint |
| [4.3](lesson-3/README.md) | Interceptor middleware | `UseBowireInterceptor()` + `BowireInterceptorOptions` — capture host traffic |

## Why this unit

The CLI (Unit 3) treats Bowire as an external probe. Embedding flips it: the workbench becomes a regular endpoint of *your* host, seeing everything your DI container exposes — the right shape for internal-tools teams and for debugging your own service.

---

**Next:** → [Lesson 4.1: Embed the workbench](lesson-1/README.md)
