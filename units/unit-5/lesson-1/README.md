# Lesson 5.1: GitHub Actions Integration

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Unit 2](../../unit-2/README.md), Docker, a GitHub repository

## Overview

Fold Bowire into CI. Two complementary patterns fall out once you have a `.bwr` recording in hand:

1. **`bowire test`** — run a recording as a regression-assertion suite. Each step expects a captured response shape; any drift fails the build.
2. **`bowire mock` as a service container** — bring the mock up alongside the integration-test job. Downstream tests point at the mock URL, run their assertions, tear down. No network round-trip, no flaky external dependencies.

You'll wire both into a single GitHub Actions workflow.

> **CI is CLI-only.** Lesson 5.1 walks the CLI path because CI agents run discrete test commands against the *deployed* service over the wire — there's no embedded host to mount the workbench into at CI time. The deployed service itself might run embedded Bowire in dev / staging (the [Embedded Backend Workflow path](../../LEARNING_PATHS.md#6-embedded-backend-workflow) covers that part of the loop); CI's job is to verify what the deploy artefact exposes, with no Bowire inside it.

## Steps

### 1. Convert a recording into an assertion suite

Recordings start out as "what did the server return?" When you click **Convert to Tests** in the workbench's recordings manager, the captured `(service, method)` pairs get two assertions appended to their existing assertion list:

1. `path=status, op=eq, expected=<captured-status>`
2. `path=response, op=eq, expected=<captured-response-body>`

Existing manual assertions are kept; the convert is append-only.

(For this lesson you can also use the bundled sample recording `units/unit-2/lesson-1/sample/hello-tour.bwr` directly — `bowire test` will treat its captured responses as the expected values automatically.)

### 2. Run `bowire test` locally first

Make sure the sample backend is running:

```bash
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run                              # → http://localhost:5001
```

In another terminal:

```bash
bowire test \
  --recording ../../unit-2/lesson-1/sample/hello-tour.bwr \
  --target http://localhost:5001
```

Output:

```
Running 3 steps from "Hello tour"...

  ✅  GetGreeting (12ms)
  ✅  PostEcho (8ms)
  ✅  GetHealth (3ms)

3/3 passed.
```

`bowire test` exits `0` on all-pass, `1` on at least one failure. CI-friendly.

### 3. Wire `bowire test` into GitHub Actions

Create `.github/workflows/bowire-test.yml`:

```yaml
name: Bowire regression
on: [push, pull_request]

jobs:
  bowire-test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install Bowire
        run: dotnet tool install --global Kuestenlogik.Bowire.Tool

      - name: Start the sample API
        run: |
          dotnet run --project units/unit-1/lesson-1/sample/HelloApi &
          # Wait until the API is responding before continuing.
          timeout 30 bash -c 'until curl -sf http://localhost:5001/health > /dev/null; do sleep 1; done'

      - name: Bowire regression tests
        run: |
          bowire test \
            --recording units/unit-2/lesson-1/sample/hello-tour.bwr \
            --target http://localhost:5001
```

Push, watch the green check.

### 4. Use the mock as a service container

For the case where the *downstream* service-under-test (not Bowire itself) needs an upstream API to talk to, run the mock as a sidecar to the test job:

```yaml
jobs:
  integration-tests:
    runs-on: ubuntu-latest

    services:
      # GitHub Actions service containers can't pull arbitrary images from
      # GHCR with workflow-scoped tokens, so we run bowire as a Docker
      # sidecar from the script instead.

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install Bowire
        run: dotnet tool install --global Kuestenlogik.Bowire.Tool

      - name: Start the mock from the recording
        run: |
          bowire mock \
            --recording units/unit-2/lesson-1/sample/hello-tour.bwr \
            --port 7080 \
            --no-watch &
          timeout 30 bash -c 'until curl -sf http://localhost:7080/health > /dev/null; do sleep 1; done'

      - name: Run downstream integration tests against the mock
        run: |
          # Whatever your service-under-test is — point it at http://localhost:7080
          # for its UPSTREAM_URL config and run its test suite.
          dotnet test path/to/IntegrationTests.csproj \
            -- UPSTREAM_URL=http://localhost:7080
```

The downstream test suite never talks to the real backend. Faster, deterministic, no rate limits, no flaky DNS, no cross-team coordination.

### 5. Layer both patterns

In a real pipeline you usually want both — `bowire test` to catch regressions on the recording itself, *and* the mock-as-service-container so downstream services have a stable target. Either as separate jobs or fanned out in a matrix:

```yaml
jobs:
  bowire-test:
    # ... runs `bowire test` against the live backend
  integration-tests:
    needs: bowire-test    # only run downstream tests if the recording is still faithful
    # ... brings up the mock + runs downstream suite against it
```

`bowire-test` proves the recording still matches the backend; `integration-tests` then uses that proven recording as a fixture for everything else.

## When to capture vs. when to refresh recordings

Recordings drift. A captured `.bwr` is a frozen baseline, and the backend it was captured against will change. Strategy:

- **Capture once, replay forever in CI** — for *frozen contract* slices. Auth flows, error responses, "this endpoint always returns 401 for an invalid token", &c. Drift in these means a regression in the backend.
- **Refresh on a schedule** — for the body-shape and value-bearing slices. A nightly job re-captures the recording (workbench-headless mode, `bowire record --headless`) and commits the result; the next morning's PR catches whatever moved.
- **Decline to record** — for genuinely non-deterministic responses (random IDs, server timestamps). Use schema-only assertions instead, or assertions on the *shape* of the response (`response.id is string`) rather than the literal value.

## Key Takeaways

1. **`bowire test` is `dotnet test` for recordings.** Exit 0 / 1, structured output, no test-framework boilerplate. Drop straight into any CI runner.
2. **The mock is reproducible by construction.** Frozen JSON in, identical responses out. Downstream tests get the same upstream behaviour on every run.
3. **Two patterns complement each other.** `bowire test` catches recording drift; the mock-as-service-container handles downstream integration. Use both, gated so the second only runs when the first passes.
4. **Plan for drift up front.** Decide per-recording whether you want frozen baselines or scheduled refreshes. Don't try to make a single recording cover both jobs.

## What's Next

Unit 5 ends the curriculum proper. The capstone weaves recording + mock + AI + plugin + CI into one continuous scenario.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Capstone: Multi-Protocol API Tour](../../../capstone/README.md)

## Reference

- [`bowire test` docs](https://bowire.io/docs/features/cli-mode.html#bowire-test)
- [Mock-server docs](https://bowire.io/docs/features/mock-server.html)
- [Recording feature docs — Convert to Tests](https://bowire.io/docs/features/recording.html#convert-to-tests)
