# Lesson 2.2: Schema export + Mock-as-Stand-In

> **Difficulty:** Intermediate | **Duration:** 10 min | **Prerequisites:** [Lesson 2.1](../lesson-1/README.md)

## Overview

A mock is most useful when consumers can see two things at once: the **full contract** the original service advertises, and the **slice** the recording can actually replay. Without that, a code-against-the-mock team accidentally narrows its integration to whatever happened to be captured.

This lesson closes the gap. You'll:

1. Get the live API's OpenAPI document — `bowire export openapi` from outside, or read the host's own `MapOpenApi()` output from inside.
2. Capture a recording that carries the source schema verbatim (workbench captures this automatically — has done since v1.7; the v2.1 **Recordings** rail surfaces the same capture flow).
3. Run the recording as a mock — and watch the mock re-emit `/openapi.json` to peer-discovery clients, exposing the full surface, not just the replayed slice.

The end state: a self-contained mock server that stands in for the real backend completely, **including the schema endpoint** anyone who wires up a Bowire / Swagger UI / contract-test runner against it expects to find.

## Steps

### 1. Have HelloApi running

If the Lesson 1.1 sample isn't already up:

```bash
cd ../../unit-1-samples/HelloApi
dotnet run                                # → http://localhost:5001
```

### 2. Get the OpenAPI document

Two equivalent ways — both work regardless of which Unit 1 track you walked:

```bash
# Via Bowire's discovery + export (works for every protocol Bowire understands):
bowire export openapi http://localhost:5001 --output hello.openapi.yaml

# Or pull the host's own MapOpenApi() output directly (REST/OpenAPI only):
curl -sSL http://localhost:5001/openapi/v1.json -o hello.openapi.json
```

Both produce the same contract — the host's `MapOpenApi()` reflects the route table, and `bowire export` reconstructs the same information through discovery. Pick whichever you reach for first.

> `bowire export` earns its keep when the contract isn't OpenAPI-native — for messaging plugins (MQTT / NATS / Kafka) the AsyncAPI export is the only way to materialise the contract as a file, because hosts don't render an `/asyncapi.yaml` of their own. Try `bowire export asyncapi <messaging-url>` once you get to those protocols.

Open `hello.openapi.yaml` — three operations (`GetGreeting`, `PostEcho`, `GetHealth`) plus the component schemas. That's the full contract; the mock will re-emit it in Step 5.

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

Workbench-captured recordings (Lesson 2.1 style) auto-attach this block (since v1.7; unchanged through v2.1's Recordings-rail rewrite) — the workbench's recorder pulls the discovery cache's verbatim schema text into the recording when it saves through `PUT /api/recordings`. No user action needed.

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

1. **Recordings carry the original schema verbatim.** Workbench-captured `.bwr` files auto-attach the OpenAPI / AsyncAPI source text (since v1.7; the v2.1 **Recordings** rail surfaces the same capture flow). Hand-written recordings can include the `sourceSchema` block too (see the sample).
2. **The mock serves the schema back at conventional URLs.** `/openapi.json`, `/openapi.yaml`, `/swagger.json` for REST; `/asyncapi.yaml` / `.yml` / `.json` for messaging.
3. **Peer discovery against the mock returns the full surface.** Consumers see *what the API claims to do*, not just *what the recording happens to replay*.
4. **`x-bowire-coverage` makes the gap explicit.** `bowire export ... --recording <file>` annotates every operation with `recorded: true/false` + `stepCount`, so consumers can plan around the replay gap.
5. **Embedded hosts already have the OpenAPI** via their own `MapOpenApi()`; `bowire export` is the way to get the equivalent for messaging contracts (AsyncAPI from MQTT / NATS / Kafka plugins) since hosts don't render those natively.

## What's Next

You're done with Unit 2's mock arc. Next: hand the workbench's toolset to an AI agent.

**Continue:** → [Unit 3: AI-Agent Integration](../../unit-3/README.md)

## Reference

- [Mock-server docs — Mock-as-stand-in section](https://bowire.io/docs/features/mock-server.html#mock-as-stand-in-recording-carries-the-original-contract)
- [Schema export docs](https://bowire.io/docs/features/export-import.html)
- [Recording file format](https://bowire.io/docs/architecture/file-formats.html)
