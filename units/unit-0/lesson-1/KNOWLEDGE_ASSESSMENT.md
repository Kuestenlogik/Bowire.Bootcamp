# Quiz: Lesson 0.1 — What is Bowire?

## Multiple Choice

### 1. What is the relationship between the `bowire` CLI and the API it discovers?

- [ ] A) The CLI hosts the API.
- [ ] B) The CLI replaces the API while it's running.
- [ ] C) The CLI is a separate process that talks to the API (two-process model).
- [ ] D) The CLI requires the API to be deployed to the cloud first.

<details>
<summary>Answer</summary>

**C)** Two-process model. Your service runs on its own port; `bowire --url http://your-service` runs in another terminal and opens a browser UI that drives the service over the wire.

</details>

### 2. Which of the following is **NOT** a built-in Bowire capability?

- [ ] A) Recording a session against a live API.
- [ ] B) Replaying that recording as a mock server.
- [ ] C) Cloud sync of recordings across team accounts.
- [ ] D) Exposing the workbench's toolset over MCP so an agent can drive it.

<details>
<summary>Answer</summary>

**C)** Cloud sync. Bowire is local-first by design — recordings live in `~/.bowire/recordings.json`, shared by checking the file into git or pasting it into Slack, never via a vendor-hosted sync service.

</details>

### 3. When is Postman a better fit than Bowire?

- [ ] A) Your stack is gRPC + MQTT + REST.
- [ ] B) You're working alone on a single REST API and your team already lives in Postman's ecosystem.
- [ ] C) You need an MCP server to drive APIs from an AI agent.
- [ ] D) Your environment is air-gapped.

<details>
<summary>Answer</summary>

**B)** Single REST API + existing Postman ecosystem. The other three are cases Bowire was deliberately designed for — polyglot stacks (A), AI integration (C), and local-first / air-gapped (D).

</details>

## True / False

### 4. Bowire requires a Küstenlogik account to use any of its features.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Bowire has no account system, no cloud, no SaaS tier. `dotnet tool install --global Kuestenlogik.Bowire.Tool` is the whole onboarding step.

</details>

### 5. The same recording can be replayed as a mock for REST, gRPC, *and* MQTT calls.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** A recording carries each step's protocol, so a single `.bwr` file can mix wires. `bowire mock` dispatches each incoming request to the matching step regardless of which protocol it speaks.

</details>

## Short Answer

### 6. List four primitives the workbench exposes for any discovered API.

<details>
<summary>Answer</summary>

Any four of:

- **Discover** — auto-detect the surface from a server URL.
- **Invoke** — form-driven request pane built from the schema.
- **Record** — capture a session with one click.
- **Replay / Mock** — run a recording back as a local mock server.
- **Export** — round-trip a discovered surface back to OpenAPI / AsyncAPI.
- **Test** — run a recording as an assertion suite (`bowire test`).
- **MCP serve** — expose the toolset over MCP for AI agents.
- **Plugin install** — extend with new protocols in .NET or polyglot.

</details>

## Score

- 6/6: You have a clear mental model — proceed to setup.
- 4–5/6: Re-skim the comparison table.
- < 4/6: Re-read the lesson and try this quiz again before installing.
