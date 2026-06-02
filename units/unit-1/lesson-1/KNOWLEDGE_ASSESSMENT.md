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

### 2. Where does the workbench mount on each deployment shape?

- [ ] A) Both at `localhost:8080/bowire`
- [ ] B) CLI at `localhost:5080/bowire`; embedded at `<your-host>/bowire`
- [ ] C) Both at the same port the target API is on
- [ ] D) CLI only — embedded mode has no browser UI

<details>
<summary>Answer</summary>

**B)** The standalone CLI boots its own host on `localhost:5080` and mounts the workbench at `/bowire`. The embedded path mounts the workbench at `/bowire` inside whichever ASP.NET host called `MapBowire()` — so on whatever port that host is already listening on (in this lesson's sample: `localhost:5001/bowire`). Same UI, different host process.

</details>

### 3. What's the difference between the two-process (CLI) and single-process (embedded) models?

- [ ] A) CLI replaces your service while it's open; embedded shares its database
- [ ] B) CLI runs as a separate process and talks to your service over the wire; embedded mounts the workbench inside your service's process, sharing its DI / auth / logging
- [ ] C) Both run as separate processes; only the port differs
- [ ] D) Embedded is read-only — you can browse but not invoke

<details>
<summary>Answer</summary>

**B)** Two-process (CLI): the workbench is a separate process that talks to your service over the wire — same pattern as a debugger attached to a target. Single-process (embedded): the workbench mounts inside your service's process via `AddBowire()` + `MapBowire()` and shares the host's `IServiceProvider`, `[Authorize]` policies, `IOptions<T>` config, and logging providers. Same UI surface, different relationship to the host.

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
