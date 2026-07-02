# Lesson 2.3: Flow Assertions

> **Difficulty:** Beginner | **Duration:** 15 min | **Prerequisites:** [Unit 1](../../unit-1/README.md); a saved Flow with at least one request step

## Overview

A Flow that *runs* is not a Flow that *passes*. Firing a request and printing the response only tells you the wire moved — not that it moved *correctly*. **Flow Assertions** (the workbench's *Expect* card) are a small, declarative language for "this step passes when the response looks like this."

The vocabulary is deliberately tiny — **five kinds** of thing to look at, a handful of operators — so you can walk a whole Flow's expectation surface in a coffee break.

## The expectation model

Every expectation is a `(kind, target, operator, expected)` tuple.

**Kind** — what part of the response to read:

| Kind | Reads from |
|---|---|
| `status` | The HTTP / protocol-equivalent status (`"OK"`, `"200"`) |
| `header` | A named response header (case-insensitive) |
| `body-path` | A value resolved from the parsed JSON body via a `$`-anchored / dotted path |
| `body-text` | The whole response body as one raw string |
| `latency` | Measured wall-clock latency in ms |

**Target** — the selector inside the kind (the header name for `header`, the JSON path for `body-path`; ignored otherwise).

**Operator** — how to compare: `equals` / `not-equals` (loose equality with numeric coercion), `contains`, `less-than` / `…-or-equals` / `greater-than` / `…-or-equals` (numeric), `exists` / `not-exists` (no right-hand side), `regex`.

**Expected** — the right-hand-side value (a string on the wire; `null` for `exists` / `not-exists`).

Wire shape — exactly what the workbench persists and the CI runner reads:

```json
{ "id":"exp_a1b2", "kind":"body-path", "operator":"equals",
  "target":"$.ship.id", "expected":"42" }
```

## Steps

Drive the Harbor **Combined** sample (`:5101/bowire`).

### 1. Add your first expectation

In the **Flows** rail, open a Flow with a Harbor request step. Its card has an **Expect** section — click `+`. The default new expectation is `status = OK`. Run the Flow: the step gets a green tick. Flip the operator to `not-equals` and re-run — the step goes red and the failing expectation lights up.

### 2. One of each kind

1. `status equals OK` — did the call succeed.
2. `header contains json` with target `content-type` — the server returned JSON.
3. `body-path equals <value>` with target `$.dock.name` — a specific value in the body.
4. `body-text contains error` — a raw-text sweep (use `regex` for negation).
5. `latency less-than 500` — a soft performance gate.

### 3. Run and read the result

Run the Flow. Each step's header shows pass/fail; each expectation shows actual-vs-expected on failure. The same expectations run headless later via `bowire test <flow.json>` ([Unit 3](../../unit-3/README.md)) — same file, same semantics.

## Key Takeaways

1. **Five kinds, a handful of operators.** Small enough to hold in your head.
2. **Every expectation is `(kind, target, operator, expected)`** — the same JSON the workbench writes and the CI runner reads.
3. **Authoring is UI; running is either.** Build in the Flows rail; run in the workbench or headless via `bowire test`.

## What's Next

**Continue:** → [Lesson 2.4: Regression Coverage](../lesson-4/README.md)

## Reference

- [Flow assertions](https://bowire.io/docs/testing/)
