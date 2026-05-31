# Quiz: Lesson 3.1 — AI-Agent Integration

## Multiple Choice

### 1. Which MCP role does this lesson use?

- [ ] A) Role 1 (Bowire as MCP client — discovering an external MCP server)
- [ ] B) Role 2 (Bowire as MCP adapter — wrapping discovered APIs as MCP tools)
- [ ] C) Role 3 (Bowire as MCP server over HTTP)
- [ ] D) Role 4 (Bowire as MCP server over stdio)

<details>
<summary>Answer</summary>

**D)** Role 4. Claude Desktop / Cursor spawn `bowire mcp serve` as a subprocess and pipe JSON-RPC over stdio. No HTTP server to manage, no port to pick.

</details>

### 2. What does `--allow-arbitrary-urls` do?

- [ ] A) Lets the agent probe any URL it can construct.
- [ ] B) Configures CORS for arbitrary origins.
- [ ] C) Disables authentication on the MCP server.
- [ ] D) Allows arbitrary executables to be launched as plugins.

<details>
<summary>Answer</summary>

**A)** Lets the agent probe any URL it asks about. Fine for `localhost:*` against your own sample services; not fine on a shared dev box. For tighter setups, use the embedded HTTP role (Role 3) with an `AllowedServerUrls` allowlist instead.

</details>

### 3. Why does the lesson recommend stdio over HTTP for desktop-AI integrations?

- [ ] A) Stdio is faster than HTTP for small JSON messages.
- [ ] B) The agent owns the subprocess lifecycle — no port to manage, no firewall rule, no auth surface to harden.
- [ ] C) HTTP doesn't support streaming.
- [ ] D) Claude Desktop doesn't speak HTTP.

<details>
<summary>Answer</summary>

**B)** The subprocess-spawn model fits desktop AI tools: the agent starts `bowire mcp serve` when needed, kills it when it closes, restarts on demand. HTTP is the right shape when *your own service* (not the agent's desktop app) hosts the MCP endpoint.

</details>

## True / False

### 4. Streaming methods (server-streaming gRPC, &c) come back to the agent through `bowire.subscribe`, not `bowire.invoke`.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** `bowire.subscribe` samples a bounded window of frames and returns them as a list; `bowire.invoke` is unary. Streaming-into-chat would overwhelm the response surface, so the toolset deliberately bounds the window.

</details>

### 5. Configuring the agent's MCP server is a one-time JSON edit; no per-API setup needed.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** Once `bowire mcp serve` is registered in the agent's `mcpServers` config, every URL the agent asks about goes through the same toolset. Adding a second API is just another `bowire.discover` call from the chat.

</details>

## Short Answer

### 6. Name two security trade-offs of `--allow-arbitrary-urls`.

<details>
<summary>Answer</summary>

Any two of:

- **Untrusted-input risk** — if the agent's prompt is influenced by an attacker, the URL space the agent can reach is everything reachable from your machine.
- **Local-network exposure** — `--allow-arbitrary-urls` includes `localhost:*` and your LAN, which might host internal services with weaker auth than the public web.
- **Recording leakage** — the agent can call `bowire.recordings.list` and read recordings containing captured request bodies + tokens.
- **No allow-listing** — tighter setups use the embedded HTTP role (Role 3) with `AllowedServerUrls` pinned to a known list.

</details>

## Score

- 6/6: Solid — continue to Unit 4.
- 4–5/6: Good — re-skim the security note.
- < 4/6: Re-run the lesson hands-on with the agent and re-take this quiz.
