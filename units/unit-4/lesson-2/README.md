# Lesson 4.2: Author a Python sidecar plugin

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 4.1](../lesson-1/README.md), Python 3.10+

## Overview

Build a Bowire protocol plugin **in Python**, with no .NET project, no `IBowireProtocol` interface, no NuGet package. Ship it as a zip carrying a `sidecar.json` manifest at its root — Bowire's host extracts the zip into `~/.bowire/plugins/`, reads the manifest, spawns your Python plugin as a subprocess, and bridges JSON-RPC over stdio between the two.

By the end you'll have a (silly) **Yoda Speak** protocol in the sidebar that does its translation in pure Python. The contrast with [Lesson 4.1's .NET Pirate plugin](../lesson-1/README.md) makes the polyglot story concrete: same SDK shape, same install command, same workbench, totally different language.

## How it differs from Lesson 05

| | Lesson 05 (.NET) | Lesson 06 (Python) |
|---|---|---|
| Contract | `IBowireProtocol` interface | `BowirePlugin` subclass |
| Wire | In-process method dispatch | JSON-RPC over stdio |
| Build | `dotnet pack` → `.nupkg` | `pip wheel` → `.whl` |
| Manifest | NuGet metadata | `sidecar.json` |
| Install | `bowire plugin install <id>` | `bowire plugin install --file <zip>` |
| Spawn | Bowire host loads the DLL | Bowire host spawns `python -m <pkg>` |

Both end up as a regular protocol tab in the sidebar; the user can't tell which language wrote it.

> **Sidecars are shape-agnostic by default.** They install into `~/.bowire/plugins/<id>/` and both the CLI host and embedded hosts scan that directory at startup. Path A walks the `bowire plugin install --file` flow below; Path B users can reuse the same install — the embedded host picks up the sidecar from `~/.bowire/plugins/` automatically. For production embedded deploys where you don't want a user-state dependency, ship the extracted sidecar folder inside your deploy bundle and point `Bowire:PluginDir` at it via config.

## Steps

### 1. Install the template scaffold (if you didn't already)

```bash
dotnet new install Kuestenlogik.Bowire.Templates
```

### 2. Scaffold a Python sidecar

```bash
dotnet new bowire-plugin \
  --Sidecar python \
  -n Bowire.Sidecar.Yoda \
  --ProtocolId yoda \
  --DisplayName "Yoda Speak" \
  --Author "Bowire Bootcamp"
cd Bowire.Sidecar.Yoda
```

Note `--Sidecar python` — that flag picks the polyglot scaffold instead of the default .NET one. The template emits:

- `sidecar.json` — the manifest Bowire reads after extracting the zip (`transport: "stdio"`, `executable: "python"`, `args: ["-m", "yoda"]`).
- `pyproject.toml` — hatchling-built wheel, depends on `bowire-plugin>=0.2.0`.
- `src/yoda/__init__.py` — package root.
- `src/yoda/__main__.py` — entry point: `from bowire_plugin import run; run(MyProtocol())`.
- `src/yoda/plugin.py` — your `BowirePlugin` subclass, stubbed with a parrot `DemoService.Echo`.
- `tests/test_plugin.py` — a pytest sanity suite asserting the discovered shape.
- `README.md` — same arc as this lesson, scoped to one plugin.

### 3. Install the Python SDK + your plugin in editable mode

```bash
pip install bowire-plugin                # the SDK
pip install -e .                          # your plugin, in editable mode
```

> **Note (2026):** the `bowire-plugin` package is shipped from the [`Bowire.Sdk.Python`](https://github.com/Kuestenlogik/Bowire.Sdk.Python) repo. If your installer reports it isn't on PyPI yet, fall back to `pip install git+https://github.com/Kuestenlogik/Bowire.Sdk.Python` or clone the repo and `pip install <path>`.

### 4. Make it do something Yoda-ish

The scaffolded `src/yoda/plugin.py` advertises a `DemoService.Echo` that parrots the request. Replace it with `sample/plugin.py` from this lesson:

```bash
cp ../sample/plugin.py src/yoda/plugin.py
```

The replacement:

- Renames the discovered service to **JediCouncil** with one **Translate** method (`text → translated`).
- Swaps the parrot `invoke` for a tiny English-to-Yoda translator (reverse the word order, append `", hmmmm."`).

Verify locally that the plugin loads and translates:

```bash
python -c "from yoda.plugin import MyProtocol; print(MyProtocol().invoke('','','',['{\"text\":\"you have powerful become\"}'],False,{}))"
# InvokeResult(response='{"translated": "become powerful have you, hmmmm."}', ...)
```

### 5. Build the wheel + bundle the zip

```bash
pip wheel . -w dist/ --no-deps              # build the plugin wheel (no SDK)
python -c "import zipfile, os; \
  z = zipfile.ZipFile('Bowire.Sidecar.Yoda.zip','w'); \
  z.write('sidecar.json'); \
  [z.write(os.path.join('dist',f), f) for f in os.listdir('dist')]; \
  z.close()"
```

`Bowire.Sidecar.Yoda.zip` now contains `sidecar.json` at the root + the built wheel next to it.

> On Linux / macOS, the equivalent is `zip -j Bowire.Sidecar.Yoda.zip sidecar.json dist/*.whl`. The Python `zipfile` form above works everywhere.

### 6. Install into Bowire

```bash
bowire plugin install --file Bowire.Sidecar.Yoda.zip
```

`bowire plugin install` accepts three `--file` shapes: a local `.zip` (this lesson), an `https://` URL pointing at a zip, or an `oci://registry/repo:tag` reference for OCI-distribution registries (GHCR, Docker Hub, Harbor, …).

Confirm it landed:

```bash
bowire plugin list
```

You'll see something like:

```
Installed plugins (1):
  Bowire.Sidecar.Yoda  v0.1.0  [sidecar: yoda]
```

The `[sidecar: yoda]` tag distinguishes it from `[nuget: ...]` .NET plugins (like Lesson 05's Pirate).

### 7. Run Bowire and invoke your protocol

Same as Lesson 05 — Yoda Speak has no real wire, so any URL works.

#### Path A — CLI

```bash
bowire --url http://yoda.local
```

#### Path B — Embedded

Add `http://yoda.local` to the host's `Bowire:ServerUrls` config (`appsettings.json` or `--Bowire:ServerUrls:0=http://yoda.local` flag) and `dotnet run`. The sidecar entry shows up at `<your-host>/bowire` next to whatever other plugins the host runs — identical sidebar shape on both paths, identical subprocess spawn under the covers.

The sidebar now contains:

```
🤖 JediCouncil (Yoda Speak)
   └─ Translate              (unary)
```

Click **Translate**, send:

```json
{ "text": "you have powerful become" }
```

→ response:

```json
{ "translated": "become powerful have you, hmmmm." }
```

That JSON came from your Python `_yodify` — through `bowire_plugin.run`, which serialised it as a JSON-RPC frame over stdout, which Bowire's sidecar host parsed and rendered. End-to-end, no .NET in your half of the stack.

### 8. Uninstall when you're done

```bash
bowire plugin uninstall Bowire.Sidecar.Yoda
```

Same removal command as a .NET plugin — `uninstall` doesn't care which side of the bridge a plugin lives on.

## Key Takeaways

1. **The plugin host is language-agnostic.** Bowire spawns whatever `sidecar.json` says under `executable` / `args`, talks JSON-RPC 2.0 NDJSON over the pipe, and presents the result the same way `IBowireProtocol` plugins are presented. The wire contract is the integration surface; the language is yours.
2. **Same install command, different `--file` shape.** `bowire plugin install` is the single entry point — pass a NuGet package id and it talks to a feed; pass `--file <zip>` and it extracts a sidecar; pass `--file oci://...` and it pulls from a registry.
3. **The SDK does the JSON-RPC work.** Subclass `BowirePlugin`, override `discover` and `invoke` (plus `invoke_stream` / `open_channel` / `settings` / `shutdown` when relevant), call `run()`. The SDK handles the framing, the dispatch, and the streaming pump.
4. **Polyglot is for *when the wire library lives elsewhere*.** Reach for sidecar plugins when the reference implementation of your protocol is already in Python / Node / Go / Rust — port the wrapper, not the protocol.

## What's Next

You're ready to head back to the workbench and the mock — Unit 5 wires Bowire into your CI pipeline as a regression-test runner and mock-server fixture.

**Continue:** → [Unit 5: CI Integration](../../unit-5/README.md)

## Reference

- [`Bowire.Sdk.Python`](https://github.com/Kuestenlogik/Bowire.Sdk.Python) — the official Python SDK source, full API docs, dual-transport details (`run` for stdio / `run_http` for streamable-HTTP).
- [Sidecar plugins architecture](https://bowire.io/docs/architecture/sidecar-plugins.html) — the JSON-RPC wire spec, manifest schema, OCI distribution path.
- [`dotnet new bowire-plugin --Sidecar python`](https://github.com/Kuestenlogik/Bowire.Templates) — the template repo (also scaffolds Node / Rust / Go sidecars).
