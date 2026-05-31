# Quiz: Lesson 5.1 — GitHub Actions Integration

## Multiple Choice

### 1. What does `bowire test --recording <file> --target <url>` do?

- [ ] A) Re-captures the recording from `--target`.
- [ ] B) Replays each step in the recording against `--target`, asserting the response matches the captured value (and the captured status code).
- [ ] C) Runs `dotnet test` against the target's test project.
- [ ] D) Tests whether `--target` is reachable.

<details>
<summary>Answer</summary>

**B)** Replay-as-assertion. Each step's captured `(status, response)` becomes an expectation; the recorded request is re-issued, the actual response is compared, and any drift fails the step. Exit code 0 on all-pass, 1 otherwise.

</details>

### 2. Why is the mock-server pattern useful as a CI **service container**?

- [ ] A) It runs faster on Linux runners.
- [ ] B) It removes network round-trips, rate limits, and DNS flakiness from downstream tests by serving frozen-but-realistic upstream responses.
- [ ] C) It auto-discovers the upstream service from `appsettings.json`.
- [ ] D) It costs less than running real services in CI.

<details>
<summary>Answer</summary>

**B)** Deterministic upstream. The downstream service-under-test points at the mock URL, runs its assertions against frozen responses, and is never blocked by upstream availability / rate limits / coordination with the upstream team.

</details>

### 3. Which pipeline layout is recommended when using both `bowire test` and the mock-as-service-container?

- [ ] A) Run both in parallel — they're independent.
- [ ] B) Run `bowire test` first; gate the mock-based integration tests behind its success.
- [ ] C) Skip `bowire test` if the integration tests pass.
- [ ] D) They're mutually exclusive — pick one.

<details>
<summary>Answer</summary>

**B)** Sequential, gated. `bowire test` proves the recording still matches reality; only then do downstream tests use that recording as a fixture. If the recording has drifted, gating prevents a stale fixture from silently passing downstream tests.

</details>

## True / False

### 4. A `.bwr` recording will keep matching production responses forever, with no maintenance.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Recordings drift. The lesson covers two strategies — frozen baselines for contract slices, and scheduled refreshes for body-shape / value-bearing slices. Decline to record genuinely non-deterministic responses.

</details>

### 5. `bowire test` exits 0 even when at least one step's assertion failed.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Exit code 1 on any failed step, 0 on all-pass. That's what makes it drop-in compatible with CI runners.

</details>

## Short Answer

### 6. List three sources of CI flakiness that the mock-as-service-container pattern eliminates.

<details>
<summary>Answer</summary>

Any three of:

- **External-API rate limits** — the real upstream is throttled; the mock is unlimited.
- **DNS / network instability** — the mock is on `localhost`, no external resolution path.
- **Cross-team coordination** — the upstream team's deployment window doesn't affect your test run.
- **Non-determinism in upstream responses** — the mock serves frozen JSON; downstream assertions can be exact-match.
- **Auth-token expiration** — the recording captured a valid token's response; the mock keeps serving it indefinitely.
- **Test-data drift** — the recording carries a known starting state; nobody can rearrange it between test runs.

</details>

## Score

- 6/6: Ready for the capstone.
- 4–5/6: Skim the drift-strategy section.
- < 4/6: Re-read the lesson and try the GitHub Actions sample on a fork.
