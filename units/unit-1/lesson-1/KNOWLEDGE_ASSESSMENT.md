# Quiz: Lesson 1.1 — Your first call

## Multiple Choice

### 1. What does Bowire's REST plugin look for when you point it at a new URL?

- [ ] A) A `.proto` file uploaded by the user
- [ ] B) A pre-built Postman-style collection
- [ ] C) An OpenAPI / Swagger document at the conventional paths (`/openapi/v1.json`, `/swagger/v1/swagger.json`, &c)
- [ ] D) The HTML of the homepage

<details>
<summary>Answer</summary>

**C)** An OpenAPI / Swagger document. The REST plugin probes the conventional discovery endpoints, parses whichever one the server advertises, and renders every operation as a method node in the sidebar. No collection, no `.proto` upload.

</details>

### 2. Which port does the standalone `bowire` workbench listen on by default?

- [ ] A) `localhost:5000`
- [ ] B) `localhost:5080/bowire`
- [ ] C) `localhost:8080`
- [ ] D) Whatever port the target API is on

<details>
<summary>Answer</summary>

**B)** `localhost:5080/bowire`. The `bowire` CLI boots a local browser UI on port 5080 and mounts the workbench under `/bowire`. The `--url` flag tells the workbench which server(s) to discover, not what port to listen on.

</details>

### 3. What's the two-process model of the standalone CLI?

- [ ] A) The workbench replaces your service while it's open
- [ ] B) Your service runs in one terminal, the workbench in another, the workbench connects to your service
- [ ] C) Both processes share the same port
- [ ] D) The workbench can only run as part of a `dotnet test` invocation

<details>
<summary>Answer</summary>

**B)** Two independent processes. The sample API runs in terminal A (here on port 5001); the workbench runs in terminal B (port 5080). The workbench is a debugger that talks to your service — it doesn't host or replace it.

</details>

## True / False

### 4. Bowire requires you to manually save each request as a "collection" before you can invoke it.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Bowire auto-discovers operations from the server's own schema document and renders them as method nodes. You don't curate a collection — the server's contract *is* the collection.

</details>

### 5. The same form-driven invoke UI is reused across REST, gRPC, GraphQL, and the other bundled protocols.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** The sidebar / invoke pane / response viewer are shared UI primitives. Per-protocol plugins map their wire-specific schema (OpenAPI, protobuf, GraphQL SDL, &c) into the same `ServiceInfo` / `MethodInfo` / `MessageInfo` shape, so the UI renders identically regardless of the underlying wire.

</details>

## Short Answer

### 6. Name two advantages of Bowire's auto-discovery model over hand-curated collections (Postman / Insomnia style).

<details>
<summary>Answer</summary>

Any two of:

- **Zero curation overhead** — point at a server, get every endpoint, including ones you didn't know existed.
- **Schema-coupled UI** — when the server's contract changes, re-discovering re-renders the UI; no per-request maintenance.
- **Same UI across protocols** — one tool covers REST, gRPC, GraphQL, &c instead of one tool per family.
- **Honest about the wire** — the form fields match what the server actually accepts (types, required-ness, &c) instead of whatever the collection-author remembered to enter.

</details>

## Score

- 6/6: Solid foundation — proceed to Lesson 1.2.
- 4–5/6: Good — skim the lesson one more time.
- < 4/6: Re-run the lesson hands-on and revisit this quiz.
