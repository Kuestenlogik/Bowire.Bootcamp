# Quiz: Lesson 0.3 — Hello Bowire

## Multiple Choice

### 1. What does the `--url` flag tell the CLI?

- [ ] A) Which port to listen on locally.
- [ ] B) Which server URL the workbench should discover services on.
- [ ] C) Which protocol plugin to load.
- [ ] D) Which browser to open.

<details>
<summary>Answer</summary>

**B)** The server URL the workbench should discover. The CLI's own listen port is `5080` by default (override with `--port`); `--url` is independent of that.

</details>

### 2. The Petstore demo exposes its operations through which discovery mechanism?

- [ ] A) gRPC Server Reflection
- [ ] B) GraphQL introspection
- [ ] C) An OpenAPI document at a conventional path
- [ ] D) A Bowire-specific `/bowire/discover` endpoint

<details>
<summary>Answer</summary>

**C)** OpenAPI document. The Petstore publishes it at `/api/v3/openapi.json`; Bowire's REST plugin probes the conventional paths and parses whichever one the server advertises.

</details>

### 3. The workbench UI shape (sidebar / invoke pane / response viewer) varies depending on which protocol you're discovering.

- [ ] A) True
- [ ] B) False — the UI shape is constant; only the underlying serialisation differs.
- [ ] C) True for REST; false for gRPC.
- [ ] D) Depends on the operator's Settings.

<details>
<summary>Answer</summary>

**B)** Constant UI. Per-protocol plugins map their wire-specific schemas into the same `ServiceInfo` / `MethodInfo` / `MessageInfo` shape, so the rendered UI is identical regardless of the underlying wire.

</details>

## True / False

### 4. You need a sample server of your own to verify Bowire's install.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Any public API with a discoverable schema works as a first-contact target. The Petstore is a convenient choice because it's stable and exposes the canonical OpenAPI demo.

</details>

### 5. The `bowire --url <x>` invocation supports passing the flag multiple times.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** `--url` is repeatable; each URL is probed independently and the matching plugin claims it. Unit 1.2 leans heavily on this.

</details>

## Short Answer

### 6. After clicking **Invoke** on `getPetById`, what three pieces of information does the response pane show?

<details>
<summary>Answer</summary>

- **The HTTP status code** (200 for success).
- **The response body** (formatted JSON from the server).
- **The duration** of the call (wall-clock, server-side timing).

(Bonus: response headers / trailers, available on a separate tab in the response pane.)

</details>

## Score

- 6/6: Done with Unit 0 — continue to Unit 1.
- 4–5/6: Re-skim the auto-discovery section.
- < 4/6: Re-run the lesson against Petstore and re-take this quiz.
