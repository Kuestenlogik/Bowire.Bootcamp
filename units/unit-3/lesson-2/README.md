# Lesson 3.2: Mock & test in CI

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md); a `.bwr` recording (capture one in [Unit 2](../../unit-2/lesson-1/README.md))

## Overview

The two headless verbs that turn Bowire into a CI citizen: `bowire mock` (replay a recording as a standalone server) and `bowire test` (run a recording or a Flow as an assertion suite, emitting CI-friendly reports).

## Replay a recording as a mock

```bash
bowire mock --recording harbor-tour.bwr --port 7070
# or the positional shorthand:
bowire mock harbor-tour.bwr --port 7070
```

The mock is its own process on its own port — frontends, CI jobs, and peer Bowires point at it instead of the real backend. Useful flags:

- `--chaos "latency:100-500,fail-rate:0.05"` — inject latency / failures to test client resilience.
- `--stateful` — each request advances a cursor through the recording.
- `--schema openapi.yaml` — schema-only mock (no recording).

This is the CLI form of the Mocks-rail replay you saw in [Unit 2.1](../../unit-2/lesson-1/README.md).

## Run assertions: `bowire test`

`bowire test` accepts **either** a recording (v2.1 test-collection) **or** a Flow JSON (v2.2) — the format is auto-detected:

```bash
bowire test harbor-flow.json --base-url http://localhost:5101 \
  --junit results.xml --report report.html
```

- `--junit <path>` — JUnit XML for the CI test-report UI.
- `--report <path>` — a self-contained HTML report.
- `--base-url` — fallback server URL for Flow steps that don't set their own.
- `--env KEY=VALUE` — variables for the `{{name}}` / `${name}` resolver (repeatable).

The assertions are the same five-kind expectations you authored in [Unit 2.3](../../unit-2/lesson-3/README.md) — same file, same semantics, now headless.

## Wire it into GitHub Actions

```yaml
# .github/workflows/api-tests.yml
jobs:
  api-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.0.x' }
      - run: dotnet tool install --global Kuestenlogik.Bowire.Tool
      - run: dotnet run --project ./src/MyApi &      # or a bowire mock as a service container
      - run: bowire test ./tests/harbor-flow.json --base-url http://localhost:5101 --junit results.xml
      - uses: actions/upload-artifact@v4
        if: always()
        with: { name: api-test-results, path: results.xml }
```

`bowire test` exits non-zero when any assertion fails, so the job fails the build. A `bowire mock` can stand in as the "service container" when the real backend isn't available in CI.

## Key Takeaways

1. **`bowire mock` = standalone replay server**, own port, `--chaos` / `--stateful` / `--schema` options.
2. **`bowire test` = headless assertion runner**, recording *or* Flow, `--junit` + `--report`, non-zero exit on failure.
3. **Same assertions as the UI** — author in the Flows rail (Unit 2.3), run in CI here.

## What's Next

**Continue:** → [Lesson 3.3: AI agents over MCP](../lesson-3/README.md)

## Reference

- [Mock server](https://bowire.io/docs/features/mock-server.html) · [Testing](https://bowire.io/docs/testing/)
