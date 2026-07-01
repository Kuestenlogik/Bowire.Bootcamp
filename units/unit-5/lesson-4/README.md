# Lesson 5.4: `bowire test` in CI — Flow runs, JUnit XML, HTML reports

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Lesson 3.2](../../unit-3/lesson-2/README.md) (Flow assertions), [Lesson 5.1](../lesson-1/README.md) (`bowire test` on a recording), a GitHub repo (or any CI runner)

## Overview

Lesson 5.1 wired `bowire test` into CI against a *recording* — the v2.1 shape. v2.2 taught the same CLI a second trick: it also accepts a **Flow JSON document** (the T2 CI runner), executes the Flow's steps in order, evaluates the per-step assertions (Lesson 3.2), and emits both a **JUnit XML** and an **HTML** report.

Format is auto-detected — the runner sniffs the JSON's shape and dispatches. You don't pick a flag. Recordings still run through the same `bowire test` command as before; Flows just work.

## Concepts

`bowire test <file>` accepts two document shapes:

| Shape | Discriminator | Runner |
|---|---|---|
| **Recording** (v2.1 test-collection) | top-level `tests` array | `TestRunner` (recording-driven) |
| **Flow JSON** (v2.2 T2) | top-level `nodes` array | `FlowTestRunner` |

Exit-code contract — same for both shapes so a CI step can branch uniformly:

| Exit | Meaning |
|---|---|
| **0** | Every step invoked AND every expectation passed |
| **1** | At least one expectation didn't hold (the backend responded, the assertion failed) |
| **2** | A step errored before evaluation (backend unreachable, service/method blank, plugin missing, malformed flow) |

Flags — the ones added by the Flow codepath are annotated:

```
bowire test <file>                          # positional: recording or flow
             --url <URL>                    # override the recording's serverUrl
             --base-url <URL>               # (flow-only) fallback serverUrl for steps that don't set one
             --env KEY=VALUE                # (flow-only) resolves {{name}} / ${name} in step body / serverUrl; repeatable
             --report <path.html>           # write an HTML report
             --junit <path.xml>             # write a JUnit XML report
```

`--base-url` and `--env` are ignored for recordings (those carry their own `serverUrl` + environment).

## Steps

### 1. Run a Flow locally first

Sanity-check the runner before wiring it into CI. Start the sample backend:

```bash
cd ../../unit-1-samples/HelloApi
dotnet run                                    # → http://localhost:5001
```

Save one of your Unit 3 Flows to disk (Flows rail → *Export as JSON* on the Flow's context menu) — call it `hello-flow.json`. Then:

```bash
bowire test hello-flow.json --base-url http://localhost:5001
```

Output — one line per step, then a summary:

```
  Bowire Flow Test Runner   flow: hello-flow

  ✅  greeting-step (12 ms) — 2/2 expectations
  ✅  echo-step (8 ms) — 1/1 expectations
  ⚠️  slow-step (1043 ms) — 1/2 expectations
      latency < 500 : actual 1043

  4/5 expectations passed   3/3 steps invoked   in 1063 ms
```

Exit code is 1 (one failed expectation). If the sample backend was down, exit would be 2 (step-error). If everything passed, exit would be 0.

### 2. Emit a JUnit report

Same run, plus `--junit`:

```bash
bowire test hello-flow.json \
  --base-url http://localhost:5001 \
  --junit artifacts/bowire.junit.xml
```

The XML follows the standard JUnit surface — `<testsuites>` wrapping one `<testsuite>` for the whole Flow, one `<testcase>` per step, `<failure>` children on failed expectations. Every CI system that reads JUnit (GitHub Actions test-summary, GitLab, TeamCity, CircleCI, Buildkite) will render it.

### 3. Emit an HTML report

Same run, plus `--report`:

```bash
bowire test hello-flow.json \
  --base-url http://localhost:5001 \
  --report artifacts/bowire.html \
  --junit artifacts/bowire.junit.xml
```

The HTML report is a single self-contained file — no external CSS / JS to path in — so it works as a CI-uploaded artefact you download and open locally. One section per step, expandable expectation rows, pass/fail chips, latency histogram at the top.

Both flags are cumulative: set both, get both.

### 4. Variable substitution — `--env`

Flow steps can reference variables via `{{name}}` or `${name}` in their body / serverUrl. The `--env` flag repeats:

```bash
bowire test hello-flow.json \
  --base-url http://localhost:5001 \
  --env USERNAME=ada \
  --env TOKEN=$CI_TOKEN
```

Unknown placeholders are left in place (so the operator sees the typo verbatim in the failing response); known ones are substituted before the call goes out.

### 5. Wire it into GitHub Actions

Full workflow — starts the backend, runs the flow, uploads both artefacts:

```yaml
name: Bowire flow tests
on: [push, pull_request]

jobs:
  bowire-flow-test:
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
          dotnet run --project units/unit-1-samples/HelloApi &
          timeout 30 bash -c 'until curl -sf http://localhost:5001/health > /dev/null; do sleep 1; done'

      - name: Bowire flow tests
        run: |
          mkdir -p artifacts
          bowire test flows/hello-flow.json \
            --base-url http://localhost:5001 \
            --report artifacts/bowire.html \
            --junit artifacts/bowire.junit.xml \
            --env TOKEN=${{ secrets.SAMPLE_TOKEN }}

      - name: Publish test report
        if: always()
        uses: dorny/test-reporter@v1
        with:
          name: Bowire flows
          path: artifacts/bowire.junit.xml
          reporter: java-junit

      - name: Upload HTML report
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: bowire-html
          path: artifacts/bowire.html
```

Two `if: always()` guards so the reports get uploaded even on a red step — that's when you most want them.

### 6. Failure triage — from CI back to the workbench

A red CI step gives you the JUnit summary + the HTML report. To debug interactively:

1. Download the HTML report from the CI artefact tab.
2. Open it locally; expand the failing step. The report carries the request body, the response body, and the specific expectation that broke.
3. Open the same Flow in the workbench (Flows rail → import from the JSON), run it locally against the same backend. The step-error / assertion-failure is now reproducible with the full introspection surface.

CI catches the drift; the workbench lets you diagnose it.

## Exercise — a red-then-green CI round trip

1. Author a Flow with three steps and four expectations total.
2. Run it locally against your backend; confirm green + `--junit` file lands.
3. Break one assertion (change an expected value to something the server won't return). Re-run. Confirm exit code 1 and the JUnit `<failure>` on the specific expectation.
4. Wire the Actions workflow above. Push. Watch it fail.
5. Un-break the assertion. Push. Watch it go green.

The point is not the Flow — it's the fact that the same Flow file, run identically in local + CI, produces identical outcomes and identical artefacts. Repeatable, mechanical, boring. That's the shape you want.

## Key Takeaways

1. **Format auto-detected.** `bowire test` accepts either a recording or a Flow. You don't pick a flag; the runner sniffs the JSON shape and dispatches.
2. **Exit-code contract is uniform.** 0 / 1 / 2 for pass / assertion-fail / step-error, regardless of shape. CI branching is the same either way.
3. **Both reports, or one, or neither.** `--report path.html` and `--junit path.xml` are independent flags. Set both for CI (JUnit for the badge, HTML for triage), neither for a local quick-check.
4. **`--env` is Flow-only.** Recordings carry their own environment inside the file; Flows use `--env KEY=VALUE` (repeatable) resolved into `{{name}}` / `${name}` placeholders.
5. **The workbench is the debugger.** CI catches the drift; you re-import the Flow in the workbench to see the request / response bodies interactively.

## Knowledge Assessment

1. **Exit-code branching.** Your CI workflow needs to send a Slack alert on assertion failure but *page* an on-call engineer on step-error (backend down). Which exit codes drive which action?
   *Answer:* **Exit 1 → Slack alert** (the backend responded, an assertion didn't hold — a code / contract regression, not urgent). **Exit 2 → page** (a step errored before evaluation — the backend is unreachable, a plugin is missing, a Flow is malformed — the infrastructure is broken). Exit 0 does nothing. The two failure modes are distinct on purpose because they need different responses.

2. **Which report format for which consumer?** You want an at-a-glance summary in the GitHub Actions PR check *and* a downloadable artefact your team can open to debug. Which flags?
   *Answer:* **Both `--junit artifacts/bowire.junit.xml` and `--report artifacts/bowire.html`.** JUnit renders in the PR check via `dorny/test-reporter` (or GitHub's native summary); the HTML is a self-contained file you upload as an artefact and open locally. They're additive — one call produces both.

3. **Variable substitution.** Your Flow references `{{TOKEN}}` in the request body. You run `bowire test flow.json --env TOKEN=abc123`. What happens if you forget the `--env` flag?
   *Answer:* **The placeholder is left intact** — the resolver only substitutes known variables and preserves unknown ones verbatim. The request goes out with the literal string `{{TOKEN}}` in the body, which the server will almost certainly reject (400 / 401), which fails your assertion, which lands as exit 1. That's *by design* — the failing response makes the typo self-diagnosing, no silent empty-string substitution to chase down.

4. **Recording vs Flow.** You pass a recording file (`--recording foo.bwr` from Lesson 5.1) through `bowire test foo.bwr --env TOKEN=x`. Does the `--env` flag do anything?
   *Answer:* **No** — `--env` is Flow-only. Recordings carry their own environment inside the file (v2.1 test-collection shape); the CLI ignores `--env` when the auto-detect picks the recording branch. It's not an error — the flag just doesn't apply — but the recording's own environment wins. If you need per-run variable substitution, convert the recording to a Flow first.

## What's Next

CI is wired. Lesson 5.5 covers workspace-scale operations — the soft/hard deletion split and how Undo works.

**Continue:** → [Lesson 5.5: Workspace deletion mode](../lesson-5/README.md)

## Reference

- `src/Kuestenlogik.Bowire.Tool/Cli/BowireCli.cs` — the `BuildTestCommand` System.CommandLine wiring; the source of truth for every flag.
- `src/Kuestenlogik.Bowire.Tool/FlowTestRunner.cs` — the Flow runner; `LooksLikeFlow` is the auto-detect discriminator.
- `src/Kuestenlogik.Bowire.Tool/FlowJUnitReport.cs` / `FlowHtmlReport.cs` — the two report emitters.
- Main-Bowire issue [#344](https://github.com/Kuestenlogik/Bowire/issues/344) — the v2.2 CI-runner (T2) initiative.
