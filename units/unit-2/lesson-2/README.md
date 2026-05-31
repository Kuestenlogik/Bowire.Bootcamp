# Lesson 2.2: Schema export + Mock-as-Stand-In

> **Difficulty:** Intermediate | **Duration:** 10 min | **Prerequisites:** [Lesson 2.1](../lesson-1/README.md)

## Overview

A mock is most useful when consumers can see two things at once: the **full contract** the original service advertises, and the **slice** the recording can actually replay. Without that, a code-against-the-mock team accidentally narrows its integration to whatever happened to be captured.

This lesson closes the gap. You'll:

1. Export the live API's OpenAPI document with `bowire export openapi`.
2. Capture a recording that carries the source schema verbatim (workbench captures this automatically as of v1.7).
3. Run the recording as a mock — and watch the mock re-emit `/openapi.json` to peer-discovery clients, exposing the full surface, not just the replayed slice.

The end state: a self-contained mock server that stands in for the real backend completely, **including the schema endpoint** anyone who wires up a Bowire / Swagger UI / contract-test runner against it expects to find.

## Steps

### 1. Have HelloApi running

If the Lesson 1.1 sample isn't already up:

```bash
cd ../../unit-1/lesson-1/sample/HelloApi
dotnet run                                # → http://localhost:5001
```

### 2. Export the live OpenAPI

In a second terminal:

```bash
bowire export openapi http://localhost:5001 --output hello.openapi.yaml
```

The CLI:

1. Loads the REST plugin.
2. Calls `DiscoverAsync` against `http://localhost:5001`.
3. Maps the resulting `ServiceInfo` / `MethodInfo` tree into an OpenAPI 3.0 document.
4. Writes it to `hello.openapi.yaml`.

Open the file — three operations (`GetGreeting`, `PostEcho`, `GetHealth`) plus the component schemas. Same shape as the real `MapOpenApi()` output, but built from Bowire's discovery rather than the host's reflection.

> Try `bowire export openapi http://localhost:5001 --format json` if you prefer JSON, or `bowire export asyncapi <messaging-url>` for AsyncAPI when you're working over MQTT / NATS / Kafka.

### 3. Use the bundled sample recording with embedded schema

The Lesson 2.1 recording (`hello-tour.bwr`) doesn't carry the source schema — it was written by hand for the lesson. Lesson 2.2 ships a recording that *does*:

```bash
cd units/unit-2/lesson-2
cat sample/hello-tour-with-schema.bwr | head -30
```

You'll see a `sourceSchema` block at the top:

```json
"sourceSchema": {
  "format": "openapi-3.0",
  "sourceUrl": "http://localhost:5001/openapi/v1.json",
  "content": "openapi: 3.0.4\ninfo:\n  title: HelloApi\n  ..."
}
```

Workbench-captured recordings (Lesson 2.1 style) auto-attach this block in v1.7+ — the workbench's recorder pulls the discovery cache's verbatim schema text into the recording when it saves through `PUT /api/recordings`. No user action needed.

The Lesson 2.2 sample also deliberately includes **only 2 of the 3 contract operations** (`GetGreeting` + `PostEcho`, no `GetHealth`) so you can see the coverage-gap story.

### 4. Run the schema-bearing recording as a mock

```bash
bowire mock --recording sample/hello-tour-with-schema.bwr --port 7080
```

### 5. Verify the mock serves the original OpenAPI

```bash
curl http://127.0.0.1:7080/openapi.json | head -20
```

You'll get the source OpenAPI back as JSON (converted from the YAML the recording carries). Try the YAML form:

```bash
curl http://127.0.0.1:7080/openapi.yaml | head -10
```

| URL on the mock | Returned content |
|---|---|
| `GET /openapi.json` | The original REST contract as JSON |
| `GET /openapi.yaml` / `.yml` | Same contract as YAML |
| `GET /swagger.json` | Legacy alias of `/openapi.json` |

For AsyncAPI-backed mocks the analogous endpoints are `/asyncapi.yaml` / `.yml` / `.json`. The mock-host extension picks the right shape from the `sourceSchema.format` tag.

### 6. Peer-discover through the mock

Point a second Bowire at the mock URL:

```bash
bowire --url http://127.0.0.1:7080
```

The sidebar shows **all three operations** (`GetGreeting`, `PostEcho`, `GetHealth`) — the *full* declared surface from the OpenAPI doc the mock re-emitted, not just the two the recording can actually replay.

Click each:

- **GetGreeting** → real recorded response (`{"greeting": "Hello, Bowire!", ...}`).
- **PostEcho** → real recorded response (`{"message": "Hello mock", ...}`).
- **GetHealth** → the mock returns a `404 — no match`, because the recording doesn't cover this method.

That's the mock-as-stand-in story made concrete: consumers see the contract first (rich, complete, discoverable), and the replay coverage second (a subset of the contract).

### 7. Coverage-annotate the export

```bash
bowire export openapi http://127.0.0.1:7080 \
  --recording sample/hello-tour-with-schema.bwr \
  --output api-coverage.yaml
```

Open `api-coverage.yaml`. Each operation now carries an `x-bowire-coverage` extension:

```yaml
paths:
  /hello/{name}:
    get:
      operationId: GetGreeting
      x-bowire-coverage:
        recorded: true
        stepCount: 1
  /echo:
    post:
      operationId: PostEcho
      x-bowire-coverage:
        recorded: true
        stepCount: 1
  /health:
    get:
      operationId: GetHealth
      x-bowire-coverage:
        recorded: false
        stepCount: 0
```

Now the team that codes against the mock sees the gap explicitly: GetHealth is in the contract but isn't replay-faithful — calling it would fall through to a 404. They can either capture a `GetHealth` step or treat it as out-of-scope for the mock.

## Key Takeaways

1. **Recordings carry the original schema verbatim.** Workbench-captured `.bwr` files auto-attach the OpenAPI / AsyncAPI source text as of v1.7. Hand-written recordings can include the `sourceSchema` block too (see the sample).
2. **The mock serves the schema back at conventional URLs.** `/openapi.json`, `/openapi.yaml`, `/swagger.json` for REST; `/asyncapi.yaml` / `.yml` / `.json` for messaging.
3. **Peer discovery against the mock returns the full surface.** Consumers see *what the API claims to do*, not just *what the recording happens to replay*.
4. **`x-bowire-coverage` makes the gap explicit.** `bowire export ... --recording <file>` annotates every operation with `recorded: true/false` + `stepCount`, so consumers can plan around the replay gap.

## What's Next

You're done with Unit 2's mock arc. Next: hand the workbench's toolset to an AI agent.

**Test your knowledge:** → [Knowledge Assessment](KNOWLEDGE_ASSESSMENT.md)
**Continue:** → [Unit 3: AI-Agent Integration](../../unit-3/README.md)

## Reference

- [Mock-server docs — Mock-as-stand-in section](https://bowire.io/docs/features/mock-server.html#mock-as-stand-in-recording-carries-the-original-contract)
- [Schema export docs](https://bowire.io/docs/features/export-import.html)
- [Recording file format](https://bowire.io/docs/architecture/file-formats.html)
