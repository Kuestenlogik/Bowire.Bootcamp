# Lesson 5.2: Polyglot sidecar plugin

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 5.1](../lesson-1/README.md); Python 3.10+ (or your language of choice)

## Overview

Not every protocol lives in .NET. Bowire's **sidecar** model lets you ship a plugin in any language — Python, Rust, Node, Go — as a separate process that speaks Bowire's plugin protocol over a local channel. Bowire supervises the sidecar; the workbench treats its protocol like any bundled one.

## The shape

- A **sidecar** is a small program that implements the plugin handshake (discover / invoke / stream) and is packaged as a **`.zip`** (or an `oci://` reference).
- Bowire launches it, forwards discovery + invocation, and renders the results in the sidebar — identical UX to a .NET plugin.
- Use it when the wire's best client library is in another ecosystem (a Python SDK, a Rust protocol crate), or when your team simply isn't a .NET shop.

## Steps

### 1. Author the sidecar

Implement the handshake in your language using the sidecar SDK for that ecosystem: respond to a discovery request with your services/methods, and to invoke requests with responses (stream frames where relevant). Keep it a single, self-contained program.

### 2. Package + install

```bash
bowire plugin install --file my-sidecar.zip
# also accepts an http(s):// URL or an oci:// registry reference:
bowire plugin install --file oci://registry.example.com/team/my-sidecar:1.0
```

Bowire routes on the shape: a `.zip` / `oci://` is a sidecar (any language); a `.nupkg` is a .NET plugin ([Lesson 5.1](lesson-1/README.md)). Restart the workbench — the sidecar's protocol shows up like any other.

### 3. Run + verify

Discover against a target that speaks your protocol; the sidecar's methods render in the sidebar with the standard invoke pane. Bowire owns the sidecar's process lifecycle (start/stop/health — [Lesson 5.4](../lesson-4/README.md)).

## Key Takeaways

1. **Sidecars ship a plugin in any language** as a `.zip` / `oci://` package.
2. **`bowire plugin install --file <zip|oci|nupkg>`** — one flag routes on shape (sidecar vs .NET).
3. **Same workbench UX** — Bowire supervises the process; the protocol behaves like a bundled one.

## What's Next

**Continue:** → [Lesson 5.3: UI extension — semantic kinds](../lesson-3/README.md)

## Reference

- [Extending Bowire — sidecar plugins](https://bowire.io/docs/extending/)
