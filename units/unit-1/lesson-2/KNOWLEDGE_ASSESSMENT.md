# Quiz: Lesson 1.2 — Multi-protocol session

## Multiple Choice

### 1. How does the `bowire` CLI handle multiple `--url` flags?

- [ ] A) The last `--url` wins; earlier ones are ignored.
- [ ] B) The CLI errors out, demanding a single URL.
- [ ] C) `--url` is repeatable; each URL is probed by every loaded plugin, and the matching plugin claims it.
- [ ] D) URLs must be comma-separated in a single `--url` flag.

<details>
<summary>Answer</summary>

**C)** `--url` is repeatable. Each URL is probed by every loaded plugin in turn; whichever protocol matches each URL claims it and renders the discovered services in the same sidebar.

</details>

### 2. How does the gRPC plugin discover a service's methods without a `.proto` upload?

- [ ] A) It downloads the `.proto` from a well-known GitHub repo.
- [ ] B) It calls the server's gRPC Server Reflection endpoint.
- [ ] C) It scans the user's local filesystem for matching `.proto` files.
- [ ] D) It can't — `.proto` upload is always required.

<details>
<summary>Answer</summary>

**B)** gRPC Server Reflection. The sample's `Grpc.AspNetCore.Server.Reflection` package enables it on the server side; the plugin's reflection client walks the service list, fetches the descriptor sets, and maps them into Bowire's `ServiceInfo` / `MethodInfo` shapes.

</details>

### 3. What changes in the workbench UI when you invoke a server-streaming method instead of a unary one?

- [ ] A) The sidebar collapses into a single node.
- [ ] B) The response pane switches to a frame list that updates live as frames arrive.
- [ ] C) The invoke button becomes greyed out.
- [ ] D) Nothing — the UI looks identical.

<details>
<summary>Answer</summary>

**B)** The response pane switches to stream mode. Frames are listed as they arrive (one per row, with a per-frame timestamp), and the stream-close event finalises the total duration.

</details>

## True / False

### 4. Server reflection on a production gRPC server should be left on by default.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** Server Reflection exposes the full service / method / message graph to anyone who can reach the gRPC port. It's enormously useful in dev / staging but is usually disabled in production — clients there ship with generated stubs that already know the schema.

</details>

## Short Answer

### 5. Why is "polyglot service mesh = one workbench" valuable in practice?

<details>
<summary>Answer</summary>

When a system spans REST + gRPC + MQTT + SignalR + &c, having one tool that speaks every wire means:

- One workbench to learn instead of five.
- One recording can capture a cross-protocol scenario (REST call triggers a gRPC stream that pushes an MQTT message, all in the same trace).
- Mock-as-stand-in (Unit 2.2) extends to every wire — the mock replays whichever protocol each recorded step used.
- AI integration (Unit 3) gives the agent one toolset that drives the whole stack rather than per-protocol bindings.

</details>

## Score

- 5/5: Solid — continue to Unit 2.
- 3–4/5: Good — re-skim the streaming section.
- < 3/5: Re-run the lesson hands-on against the sample servers and revisit this quiz.
