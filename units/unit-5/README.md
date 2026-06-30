# Unit 5: CI Integration

*Time: ~15 minutes • Lessons: 1 • Previous: [Unit 2](../unit-2/README.md)*

Fold Bowire into your CI pipeline. Recordings become reproducible regression assertions; the mock server becomes a job-level service container that backs the integration tests of everything downstream.

## Prerequisites

- [Unit 2](../unit-2/README.md) complete — you need a `.bwr` recording to feed into `bowire test` and `bowire mock`.
- A **GitHub repository** you can push the sample workflow to (any other CI runner is fine analogously — the workflow file is GitHub Actions, the pattern is portable).
- **Docker** is *optional* — only needed if you want to bring the mock up as a true container in `services:` rather than as a background process in the runner. The lesson covers both shapes.
- The Bowire CLI installed *inside* the CI runner: the workflow snippet's `dotnet tool install --global Kuestenlogik.Bowire.Tool` step takes care of it.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [5.1](lesson-1/README.md) | GitHub Actions integration | `bowire test` step running recordings as assertions, mock-server as a job service for downstream integration tests |

## Why this unit

A recording is portable — it's a JSON file you can check in. Once you have one, two CI patterns fall out naturally:

1. **`bowire test`** — run the recording as an assertion suite. Each step expects a captured response; any drift fails the build. Zero test infrastructure, full regression coverage of whatever you captured.
2. **`bowire mock` as a service container** — bring the mock up alongside your integration-test job. Downstream services point at the mock instead of a real backend, run their tests, tear down. No network round-trips, no flaky external dependencies.

Unit 5 wires both into a GitHub Actions workflow.

---

**Next:** → [Capstones](../../capstones/) (per-audience deliverable — Developer capstone is the established one; User + Administrator capstones are scaffolds being filled in PR 3)
