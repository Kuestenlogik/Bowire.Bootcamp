# Quiz: Lesson 2.2 — Schema Export + Mock-as-Stand-In

## Multiple Choice

### 1. What does `bowire export openapi <url>` produce?

- [ ] A) A live HTTP server serving the OpenAPI document.
- [ ] B) An OpenAPI 3.0 document mapped from the live discovery tree, written to file or stdout.
- [ ] C) A NuGet package containing the OpenAPI document.
- [ ] D) Nothing — `bowire export` is for AsyncAPI only.

<details>
<summary>Answer</summary>

**B)** Disk artefact. `bowire export openapi` (and `asyncapi`) calls `DiscoverAsync` against the URL, maps the result into an OpenAPI 3.0 / AsyncAPI 3.0 document, and writes it as YAML (default) or JSON (`--format json`).

</details>

### 2. Which field on a `BowireRecording` carries the original API schema?

- [ ] A) `recording.schemaSnapshot`
- [ ] B) `recording.sourceSchema`
- [ ] C) `recording.openapi`
- [ ] D) The first step's metadata

<details>
<summary>Answer</summary>

**B)** `recording.sourceSchema`, a small sidecar carrying `format`, `content` (verbatim YAML or JSON of the source doc), and optionally `sourceUrl`. (`schemaSnapshot` is the *annotations* sidecar — different feature.)

</details>

### 3. Which URLs on a mock-server REST listener return the original contract?

- [ ] A) `/openapi.json`, `/openapi.yaml`, `/openapi.yml`, `/swagger.json`
- [ ] B) `/bowire/contract`
- [ ] C) `/api/openapi`
- [ ] D) Bowire mocks don't expose the contract.

<details>
<summary>Answer</summary>

**A)** Four conventional URLs. `/swagger.json` is a legacy alias of `/openapi.json`. AsyncAPI mocks expose the analogous `/asyncapi.yaml` / `.yml` / `.json`.

</details>

## True / False

### 4. A peer Bowire pointed at the mock URL sees the same surface as if it pointed at the original backend.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True** for the *contract* (every declared operation appears in the sidebar). **Operations the recording doesn't cover return 404** on invoke — coverage-gap handling is the consumer's responsibility, and `--recording` + `x-bowire-coverage` annotations make it explicit.

</details>

### 5. The workbench's recorder automatically attaches the source schema to a recording in v1.7+.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** The workbench (`PUT /api/recordings`) pulls the verbatim schema text from the discovery cache when it saves through. Pre-v1.7 recordings (or hand-written ones) need the `sourceSchema` block added manually.

</details>

## Short Answer

### 6. Why is "consumers see the full surface, not just the replayed slice" valuable?

<details>
<summary>Answer</summary>

Without the contract:

- Consumers code-against-the-mock and accidentally narrow their integration to whatever happened to be captured.
- A second team building against the mock discovers gaps only at integration time, not at design time.
- Contract-test runners (Spectral, OpenAPI-validator, &c) have nothing to validate against.
- Doc-generation tools (Redoc, Swagger UI) can't render the API.

With the contract re-emitted by the mock:

- Consumers see *what the API claims to do* — they can build against the full surface, then ask the recording author to extend coverage for uncovered methods.
- `x-bowire-coverage` makes the gap explicit per-operation, so the consumer knows when to expect a 404 from the mock and plan around it.
- Doc tooling, peer Bowires, and contract-test runners all just work against the mock URL.

</details>

## Score

- 6/6: Mock-as-stand-in story locked in — continue to Unit 3.
- 4–5/6: Skim the coverage annotation section.
- < 4/6: Re-run the lesson end-to-end and re-take the quiz.
