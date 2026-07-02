# Lesson 2.2: Schema-backed mocks

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 2.1](../lesson-1/README.md)

## Overview

A mock is most useful when consumers see two things at once: the **full contract** the original service advertises, and the **slice** the recording can actually replay. Workbench-captured recordings carry the source schema verbatim, so the mock can re-emit the contract — and make the replay gap explicit.

## Steps

### 1. Capture (or reuse) a schema-bearing recording

Workbench-captured `.bwr` files auto-attach a `sourceSchema` block (the discovery cache's verbatim OpenAPI / AsyncAPI text) when they save — no action needed. Reuse `harbor-tour.bwr` from [Lesson 2.1](lesson-1/README.md), or capture a fresh one that deliberately covers only *some* of the contract's operations so you can see the gap.

### 2. Run it as a mock and check the schema endpoints

Start the mock (Mocks rail, or `bowire mock` — [Unit 3](../../unit-3/README.md)). The mock re-emits the source contract at conventional URLs:

| URL on the mock | Returns |
|---|---|
| `GET /openapi.json` | The original REST contract as JSON |
| `GET /openapi.yaml` / `.yml` | Same contract as YAML |
| `GET /swagger.json` | Legacy alias |

For messaging-backed mocks the analogous endpoints are `/asyncapi.yaml` / `.yml` / `.json`; the mock picks the shape from `sourceSchema.format`.

### 3. Peer-discover through the mock

Point a second workbench at the mock. The sidebar shows **every declared operation** — the full surface from the re-emitted contract, not just the replayed slice. Operations the recording covers return the recorded response; uncovered ones fall through to `404 — no match`. That's the mock-as-stand-in story: consumers see the contract first, the replay coverage second.

### 4. Annotate the gap (CLI cross-ref)

`bowire export openapi <mock-url> --recording harbor-tour.bwr` writes an `x-bowire-coverage` extension onto every operation (`recorded: true/false`, `stepCount`), so a team coding against the mock plans around the gap. `bowire export` is a CLI workflow → [Unit 3](../../unit-3/README.md).

## Key Takeaways

1. **Recordings carry the original schema.** Workbench-captured `.bwr` files auto-attach the OpenAPI / AsyncAPI source text.
2. **The mock serves the schema back** at `/openapi.*` (REST) or `/asyncapi.*` (messaging).
3. **Peer discovery returns the full surface** — *what the API claims to do*, not just *what the recording replays*.
4. **`x-bowire-coverage` makes the gap explicit** for consumers.

## What's Next

You can stand in for a service. Next: say what "correct" means.

**Continue:** → [Lesson 2.3: Flow Assertions](../lesson-3/README.md)

## Reference

- [Mock-server — mock-as-stand-in](https://bowire.io/docs/features/mock-server.html)
- [Schema export](https://bowire.io/docs/features/export-import.html)
