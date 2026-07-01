# Lesson 3.2: Flow Assertions — from "it ran" to "it's correct"

> **Difficulty:** Beginner | **Duration:** 15 min | **Prerequisites:** [Unit 1](../../unit-1/README.md) complete (workbench basics), at least one saved Flow with a real backend call in it

## Overview

A Flow that runs is not a Flow that passes. Firing a request and printing the response only tells you the wire moved — it tells you nothing about whether the wire moved *correctly*. v2.2 introduces **Flow Assertions** (called *expectations* in the workbench's Expect card) — a small, declarative language for saying "this step passes when the response looks like this."

The vocabulary is deliberately tiny — five kinds of thing to look at, ten ways to compare — so you can walk a whole Flow's expectation surface in a coffee-break.

## Concepts

Every expectation is a `(kind, target, operator, expected)` tuple.

**Kind** — what part of the response to look at:

| Kind | Where it reads from |
|---|---|
| `status` | The HTTP status / protocol-equivalent status string (e.g. `"OK"`, `"200"`) |
| `header` | A named response header (case-insensitive lookup) |
| `body-path` | A value resolved from the parsed JSON body via a `$`-anchored / dotted path |
| `body-text` | The whole response body as one raw string |
| `latency` | The measured wall-clock latency in milliseconds |

**Target** — the selector inside the kind. Meaningful for `header` (the header name) and `body-path` (the JSON path); ignored for the other three.

**Operator** — how to compare:

| Operator | Semantics |
|---|---|
| `equals` / `not-equals` | Loose string equality with numeric coercion when both sides parse |
| `contains` | Substring (string) or element-match (array) |
| `less-than` / `less-than-or-equals` / `greater-than` / `greater-than-or-equals` | Numeric comparison |
| `exists` / `not-exists` | Actual is non-null / non-empty (no right-hand side) |
| `regex` | Actual matches the expected string as a regex |

**Expected** — the right-hand-side value. Stored as a string on the wire (the operators coerce as needed). `null` for `exists` / `not-exists` because those don't take a right-hand side.

Wire shape — the exact JSON the workbench persists and the CI runner reads:

```json
{ "id":"exp_a1b2", "kind":"body-path", "operator":"equals",
  "target":"$.user.id", "expected":"42" }
```

That's the whole schema. Same file the workbench writes, same file `bowire test <flow.json>` (Lesson 5.4) reads back.

## Steps

### 1. Open a Flow and add your first expectation

Point Bowire at the Unit 1 sample (or any REST backend you have running):

```bash
cd ../../unit-1-samples/HelloApi
dotnet run                                    # → http://localhost:5001
```

Open the workbench, go to the **Flows** rail, pick a Flow with at least one request step in it. Each step card has an **Expect** section (folded closed if empty). Click the `+` button to add a new expectation.

The default new expectation is `status = OK` — the vacuously-obvious one. Run the Flow. The step passes (there's a green tick in the step-header meta strip). Now break it: change the operator to `not-equals` and re-run — the step goes red, the failing expectation lights up.

### 2. The five kinds

Add one of each and see where each reads from:

1. **`status equals OK`** — the classic "did the call succeed" gate.
2. **`header contains json`** with target `content-type` — sanity-check that the server actually returned JSON.
3. **`body-path equals Ada`** with target `$.name` — assert a specific value inside the response body.
4. **`body-text contains error`** — a raw-text sweep across the whole body (`operator=not-contains` isn't in the vocabulary — use `regex ^(?!.*error).*$` if you want the negation).
5. **`latency less-than 500`** — SLO guard. Ms-scale numeric compare.

Save the Flow (autosaves on edit) and run it. Every expectation that resolves shows a pass/fail chip in the Expect card. Every expectation that couldn't resolve (bad `body-path`, unreachable backend, streaming response) is reported as a step-error separately.

### 3. `exists` — asserting shape without asserting value

Some fields are legitimately non-deterministic — server timestamps, random IDs, correlation tokens. You still want to prove they're *present*, just not that they equal a specific value. That's what `exists` is for:

- `body-path exists` with target `$.orderId` — passes whenever `$.orderId` is set to *any* non-null / non-empty value.
- `header exists` with target `traceparent` — sanity check that OpenTelemetry-style headers propagated.

`exists` and `not-exists` don't take a right-hand side; the workbench hides the expected-value input when you pick them.

### 4. Empty expectation lists are legal

A step with zero expectations still runs. It passes as long as the invocation itself didn't error (no connection refused, no plugin crash, no missing method). This is the "smoke-test" mode — Flows that exist just to sequence calls, without proving anything about the responses.

### 5. Persistence: what actually gets saved

The workbench stores the expectation list on the step object as `assertions: [ {kind, op, value, target, path} ]`. When the CI runner loads the Flow file (Lesson 5.4), it round-trips the legacy `{path, op, value}` tuple through `FlowExpectation.FromLegacyTuple` — so mixed old + new expectations in the same file both run without a migration step.

## Exercise — the four-expectation regression

Build a step that asserts four independent things about a `POST /api/users` call:

1. **`status equals 201`** — the HTTP status is Created.
2. **`header exists`** with target `location` — the server returned a canonical URL for the new resource.
3. **`body-path equals Ada`** with target `$.name` — the server echoed the name we sent.
4. **`latency less-than 200`** — the request was fast (200 ms budget).

Run the Flow. All four expectations must pass for the step to be green. Now edit the target name on the request body — change `"Ada"` to `"Betty"` — and re-run. Expectation 3 fails, the other three still pass, the step goes red. This is the shape of a regression signal in Bowire: **one broken expectation, one specific failure, everything else stays informative**.

## Key Takeaways

1. **Five kinds, ten operators, one string right-hand side.** The whole assertion language fits in your head. No test-framework, no DSL to learn.
2. **`status equals OK` is the default new expectation.** The vacuously-obvious gate — you get a passing expectation for free the moment you click `+`.
3. **`exists` is for shape without value.** Assert the field is *there* without pinning what it holds; useful for correlation IDs, server timestamps, random keys.
4. **Empty lists still run.** A step with zero expectations passes as long as the underlying call succeeds. That's smoke-test mode; use it when the Flow is a sequencer, not a validator.
5. **Same schema everywhere.** The workbench writes the expectations, the CI runner reads them. No migration, no re-declaration. This is how Lesson 5.4 (`bowire test` in CI) works.

## Knowledge Assessment

1. **The right kind for the job.** You want to assert that the server returned any 2xx status (not just 200). Which kind + operator, and which right-hand-side value?
   *Answer:* **`status regex ^2\d\d$`.** The `regex` operator matches the actual (stringified) status against the expected pattern. `equals 200` would fail for legitimate 201 / 204 responses; `less-than 300` combined with `greater-than-or-equals 200` would need two expectations. Regex handles it in one row.

2. **Non-deterministic fields.** Your endpoint returns a fresh UUID as `$.correlationId` on every call. You want a green step whenever the field is populated. Which operator?
   *Answer:* **`exists`.** With `kind=body-path`, `target=$.correlationId`, `operator=exists`, no right-hand-side. Passes whenever the field is non-null and non-empty; doesn't pin the value. `equals` would fail on every re-run because the UUID differs.

3. **What does an empty expectation list mean?** You added a step to a Flow but never clicked `+` in the Expect card. You run the Flow. What outcome does the step produce, and why?
   *Answer:* **The step passes** (vacuously) as long as the invocation itself didn't error. Empty list → the step is a smoke-test / sequencer, not a validator. This is intentional — Flows commonly have setup / teardown steps that just need to run, not to assert. If the call *errors* (connection refused, plugin missing, method blank), that shows up as a step-error separately, independent of the expectation list.

4. **Round-trip integrity.** The workbench persists an expectation as `{kind:"body-path", op:"eq", target:"$.id", value:"42"}`. The CI runner in Lesson 5.4 reads the same file. Does it understand the row?
   *Answer:* **Yes.** The CI runner uses `FlowExpectation.FromLegacyTuple` to bridge the legacy `{path, op, value}` shape that the workbench still writes into the canonical `FlowExpectation` type. Both shapes coexist in the same file; the runner doesn't require a migration pass. Same schema, both sides.

## What's Next

Expectations are the *definition* of "correct." Lesson 3.3 covers the flip side: given a set of definitions, **how much of the discovered surface have you actually exercised?** That's the Regression Coverage view.

**Continue:** → [Lesson 3.3: Regression Coverage — what have I actually tested?](../lesson-3/README.md)

## Reference

- `src/Kuestenlogik.Bowire.Flows/Expectations/FlowExpectation.cs` — the canonical type. `FromLegacyTuple` is the bridge from the v2.1 `{path, op, value}` workbench shape.
- `src/Kuestenlogik.Bowire.Flows/Expectations/FlowExpectationEvaluator.cs` — the runtime that resolves each expectation against a `FlowRequestEnvelope`.
- `src/Kuestenlogik.Bowire.Flows/wwwroot/js/flows.js` — the workbench UI, specifically `renderAssertionRow` (the Expect-card row editor).
- Main-Bowire issue [#342](https://github.com/Kuestenlogik/Bowire/issues/342) — the Flow-assertions initiative.
