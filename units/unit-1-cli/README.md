# Unit 1 — CLI Setup Track (Path A)

> **You've chosen the standalone CLI path.** Two-process model: your service runs in one process, the `bowire` workbench in a second. The tracks merge again at [Unit 2](../unit-2/README.md) once setup is done — recording, mocking, AI integration, plugin work, CI all use the same workbench UI regardless of which setup track you walked.

## When this path fits

- You already have a service running and want to *poke at it from outside* without changing its code.
- Your services are genuinely separate processes (microservices in separate repos / containers), and a single workbench needs to span the boundary.
- You're exploring an unfamiliar API and don't want to add a dependency to the host.
- You're building CI integration around `bowire test` / `bowire mock` — both are CLI commands, no embedded equivalent.

If your service is .NET and you'd rather mount the workbench *inside* the host process (no second terminal, no port juggling), look at [Unit 1 — Embedded Setup Track](../unit-1-embedded/README.md) instead.

## Lessons in this track

| # | Lesson | Duration |
|---|---|---|
| 1.1 | [First call — REST](lesson-1/README.md) | 10 min |
| 1.2 | [Multi-protocol — REST + gRPC](lesson-2/README.md) | 10 min |

After Lesson 1.2 → continue to [Unit 2: Record, Replay, Mock](../unit-2/README.md).

## Mental model

```
┌─────────────────────────┐      ┌───────────────────────────┐
│  Your service           │      │  bowire CLI               │
│  (HelloApi on :5001)    │ ◀──▶ │  http://localhost:5080/   │
└─────────────────────────┘      │     bowire (workbench UI) │
                                 └───────────────────────────┘
   terminal 1                       terminal 2
```

One terminal for the service, one for the workbench. The workbench discovers the service through the URL you pass via `--url` and renders each method in the sidebar.
