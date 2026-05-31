# Quiz: Lesson 2.1 — Record & Replay

## Multiple Choice

### 1. What does the red **● Record** button capture?

- [ ] A) Only the next single API call.
- [ ] B) Every successful invocation in the workbench until you click **Stop**, including request body, metadata, server URL, status, duration, and response payload.
- [ ] C) Just the URL and HTTP verb — no bodies.
- [ ] D) Network packets at the TCP layer.

<details>
<summary>Answer</summary>

**B)** Every successful invocation between Record and Stop is captured as a `BowireRecordingStep` carrying the full request/response, including metadata and timing.

</details>

### 2. Where does `bowire mock --recording <path>` look up the response for an incoming HTTP request?

- [ ] A) It re-runs the recording end-to-end and returns the live result.
- [ ] B) It matches the request's `(verb, path)` against the recorded steps and returns the captured response of the first matching step.
- [ ] C) It generates a response from the OpenAPI document.
- [ ] D) It forwards the request to the original server.

<details>
<summary>Answer</summary>

**B)** Per-step matching. The first matching unary step wins; the captured response is re-emitted byte-for-byte. Path templates (`/users/{id}`) auto-bind, literal paths stay strict.

</details>

### 3. The recording's `httpPath` is `/hello/{name}`. Which incoming URL matches?

- [ ] A) `/hello/Bowire`
- [ ] B) `/hello/anyone`
- [ ] C) `/hello/Star%20Wars`
- [ ] D) All of the above

<details>
<summary>Answer</summary>

**D) All of the above.** Path templates are detected automatically when the recorded path contains `{...}`, and each `{name}` segment binds to one URL-decoded path segment.

</details>

## True / False

### 4. The mock server stops working when the original backend is shut down.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** A recording is a self-contained snapshot. Once `bowire mock --recording <path>` is running, the original backend can be down, the network can be air-gapped — the mock keeps serving from the captured responses.

</details>

### 5. You can pipe `curl` directly at the mock server.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** The mock is an ordinary HTTP server (gRPC mocks speak HTTP/2 + protobuf, also ordinary). Any HTTP client works against it.

</details>

## Short Answer

### 6. Name two use cases where `.bwr`-as-mock-fixture is more useful than a hand-written mock.

<details>
<summary>Answer</summary>

Any two of:

- **Frontend dev environment** — point the frontend at the mock, work offline against a frozen-but-realistic backend.
- **CI integration tests** — bring the mock up as a service container, run downstream tests against it, tear down.
- **Reproducing a customer bug** — capture the failing session once, replay it forever to debug.
- **Demos without the stack** — show a working API to a non-technical audience without standing up the real backend.
- **Contract-test fixtures** — capture the real backend's behaviour at release time, run the mock in CI as a frozen baseline future changes must keep matching.

</details>

## Score

- 6/6: Solid — continue to Lesson 2.2.
- 4–5/6: Good — skim the path-template section once more.
- < 4/6: Re-run the lesson end-to-end.
