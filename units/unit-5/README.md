# Unit 5: CI Integration

*Time: ~15 minutes • Lessons: 1 • Prerequisites: [Unit 2](../unit-2/README.md)*

Fold Bowire into your CI pipeline. Recordings become reproducible regression assertions; the mock server becomes a job-level service container that backs the integration tests of everything downstream.

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

**Next:** → [Capstone](../../capstone/README.md)
