# Quiz: Lesson 4.2 — Python Sidecar Plugin

## Multiple Choice

### 1. What does Bowire's host read from a sidecar plugin's zip after extraction?

- [ ] A) The `.nupkg` next to the zip.
- [ ] B) The `sidecar.json` at the zip's root, then spawns whatever `executable` + `args` it declares.
- [ ] C) Every `.py` file in the zip, importing them all into the host process.
- [ ] D) A `Bowire.Sidecar.dll` from the zip.

<details>
<summary>Answer</summary>

**B)** `sidecar.json`. It tells the host which transport (`stdio` / `http`), which executable to spawn, and which CLI args. Bowire never imports your code into its own process — the sidecar is a separate process that speaks JSON-RPC to the host.

</details>

### 2. Which wire does the Python SDK speak when you call `run(plugin)`?

- [ ] A) HTTP/2 with protobuf framing.
- [ ] B) Plain HTTP/1.1 with JSON bodies.
- [ ] C) JSON-RPC 2.0 over NDJSON on stdin/stdout.
- [ ] D) Custom binary RPC.

<details>
<summary>Answer</summary>

**C)** JSON-RPC 2.0 over NDJSON (one line per frame) on stdin/stdout. `run_http(plugin, host, port)` is the alternative that switches to POST + long-lived SSE — same dispatch under the hood.

</details>

### 3. Which step makes the Python wheel available *inside* the sidecar zip Bowire installs?

- [ ] A) `pip install bowire-plugin`
- [ ] B) `pip wheel . -w dist/`
- [ ] C) `bowire plugin install <id>`
- [ ] D) `python -m yoda`

<details>
<summary>Answer</summary>

**B)** `pip wheel . -w dist/`. It builds your plugin's `.whl` into `dist/`; you then zip the wheel alongside `sidecar.json` so the resulting `.zip` contains everything the host needs to extract + invoke.

</details>

## True / False

### 4. A sidecar plugin can ship in any language that can read from stdin and write JSON to stdout.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** Python, Node, Rust, Go, C++, &c. The official SDKs (Python, Rust, Node, Go) save the JSON-RPC boilerplate, but you can implement the wire by hand in any language — see [`docs/architecture/sidecar-plugins.md`](https://bowire.io/docs/architecture/sidecar-plugins.html) for the spec.

</details>

### 5. Sidecar plugins and .NET plugins appear differently in `bowire plugin list`.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** Sidecar entries are tagged `[sidecar: <protocol-id>]`, .NET ones `[nuget: N files]`. Both are uninstalled the same way (`bowire plugin uninstall <packageId>`); the host treats them identically beyond the spawn step.

</details>

## Short Answer

### 6. When would you prefer a sidecar (polyglot) plugin over a .NET (in-process) plugin?

<details>
<summary>Answer</summary>

- **The reference implementation of your protocol lives in a non-.NET ecosystem** (e.g. `paho-mqtt` in Python, `pulsar-python`, `zenoh` in Rust, the official `bigtable` client in Go). Porting the wrapper is faster and lower-risk than porting the protocol.
- **You need OS-level isolation.** Sidecar processes can be sandboxed independently of the host.
- **You ship the plugin from a team that owns the polyglot reference impl** and doesn't write .NET day-to-day.
- **You want air-gapped distribution.** OCI registry pulls + sidecar zips work fully offline; NuGet sometimes needs feed proxies.

Reach for the in-process .NET path when (a) the reference impl is already in .NET, (b) you need access to host DI services, or (c) you want type-safe round-trips with no JSON-RPC overhead.

</details>

## Score

- 6/6: Solid — continue to Unit 5.
- 4–5/6: Good — re-skim the SDK + zip-shape section.
- < 4/6: Re-run the lesson end-to-end against the Python sample.
