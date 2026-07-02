# Lesson 3.1: Install & first call (CLI)

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Overview

Install the `bowire` global tool, point it at a running service, and drive it — first through the browser workbench it launches, then headless with `list` / `describe` / `call`. This is the two-process shape: your service is one process, `bowire` is another.

## Steps

### 1. Install the tool

```bash
dotnet tool install --global Kuestenlogik.Bowire.Tool
bowire --version
```

### 2. Start a sample service

Use any single-plugin demo from `Bowire.Samples/protocols/` — they don't embed Bowire, so they're pure `--url` targets:

```bash
# in a clone of Bowire.Samples
cd protocols/Rest.PetStore
dotnet run                      # → e.g. http://localhost:5xxx
```

`Grpc.Greeter` (the classic `SayHello`) or `WebSocket.Echo` work the same way if you'd rather start with gRPC or an echo.

### 3. Launch the workbench against it

```bash
bowire --url http://localhost:5xxx
```

This boots a local workbench on `http://localhost:5080`, opens your browser, and points it at the service. Auto-discovery populates the sidebar (OpenAPI for REST, reflection for gRPC, …). From here the **UI is identical to** [Unit 1: The Workbench](../../unit-1/README.md) — that's where the invoke walkthrough lives; this unit is about the command line.

`--url` is **repeatable** — pass several services and one workbench discovers them all:

```bash
bowire --url http://localhost:5001 --url http://localhost:5002
```

### 4. Drive it headless: `list` / `describe` / `call`

For scripting (and gRPC especially), skip the browser and use the subcommands — grpcurl-style:

```bash
bowire list   --url https://localhost:5001
bowire describe greeter.Greeter --url https://localhost:5001
bowire call greeter.Greeter/SayHello -d '{"name":"Bowire"}' --url https://localhost:5001 --compact
```

- `-d` / `--data` takes JSON (or `@file`); repeat it for client-streaming.
- `-H "key: value"` adds metadata headers (repeatable).
- `--compact` prints one-line JSON — pipe-friendly for `jq`.
- `-plaintext` for non-TLS targets.

> Since v2.2 the CLI validates arguments up front: `--port` must be `1..65535`, `--recording` must exist, `--chaos` is parsed before dispatch, and parse errors print in colour to stderr with a `--help` hint. Tab-completion is available via `dotnet-suggest` — see [CLI mode docs](https://bowire.io/docs/features/cli-mode.html).

## Key Takeaways

1. **`bowire --url <service>` is the entry point.** It launches the workbench and points it at the URL — no config file.
2. **`--url` is repeatable** — one workbench, many backends; auto-discovery sorts out which plugin claims which URL.
3. **`list` / `describe` / `call` are the headless path** — scriptable, pipe-friendly, no browser.

## What's Next

**Continue:** → [Lesson 3.2: Mock & test in CI](../lesson-2/README.md)

## Reference

- [CLI mode](https://bowire.io/docs/features/cli-mode.html)
