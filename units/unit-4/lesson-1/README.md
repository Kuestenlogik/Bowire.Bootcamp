# Lesson 4.1: Author a .NET protocol plugin

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Unit 1](../../unit-1/README.md) (CLI or Embedded shape), .NET 10 SDK

## Overview

Build your own protocol plugin from scratch, install it into Bowire, and watch a fresh workbench discover it on startup. By the end you'll have a (silly) **Pirate Speak** protocol in the sidebar that turns plain English into pirate-speak when invoked.

The protocol is deliberately self-contained — no external wire, no broker, no schema-discovery step. The point is the **plugin contract**: implement `IBowireProtocol`, ship the NuGet, get your code into Bowire's plugin pipeline. Replace the substitution-table body with `HttpClient.SendAsync` / `MQTTnet.PublishAsync` / `GrpcChannel.ForAddress` and you have a real protocol plugin.

> **Path-split is narrow.** Authoring (scaffold, code, pack) is identical on both paths — the same NuGet works in both. Only the **install** + **run** steps differ: CLI uses `bowire plugin install`, embedded uses a `PackageReference` in your host project. Path-B insets show the diff inline at Steps 5 and 6.

## Steps

### 1. Install the plugin scaffold template

```bash
dotnet new install Kuestenlogik.Bowire.Templates
```

You should see `bowire-plugin` listed under the installed templates.

### 2. Scaffold a fresh plugin

```bash
dotnet new bowire-plugin \
  -n Bowire.Plugin.Pirate \
  --ProtocolId pirate \
  --DisplayName "Pirate Speak" \
  --PluginClassName PirateProtocol \
  --Author "Bowire Bootcamp"
cd Bowire.Plugin.Pirate
```

The template emits a self-contained .slnx with:

- `src/Bowire.Plugin.Pirate/Bowire.Plugin.Pirate.csproj` — the plugin assembly.
- `src/Bowire.Plugin.Pirate/PirateProtocol.cs` — the `IBowireProtocol` stub returning a `DemoService.Echo` method that parrots the request.
- `tests/Bowire.Plugin.Pirate.Tests/` — an xUnit test project asserting the discovered shape.
- `Directory.Packages.props` — pinned versions for `Kuestenlogik.Bowire` and friends.
- `.github/workflows/ci.yml` — GitHub Actions that builds + tests on every push.

Build it once to confirm the scaffold restores cleanly:

```bash
dotnet build
```

### 3. Make it do something pirate-ish

The scaffolded `PirateProtocol.cs` advertises a `DemoService.Echo` that echoes the request payload. Replace its contents with `sample/PirateProtocol.cs` from this lesson:

```bash
cp ../sample/PirateProtocol.cs src/Bowire.Plugin.Pirate/PirateProtocol.cs
```

The replacement:

- Renames the discovered service to **BuccaneerService** with one **Translate** method (`text → translated`).
- Replaces the parrot `InvokeAsync` body with a tiny English-to-Pirate substitution table (`the → th'`, `you → ye`, &c, plus a `🏴‍☠️` suffix).
- Makes `InvokeStreamAsync` an empty stream (Pirate Speak is unary-only) and `OpenChannelAsync` return `null` (no duplex).

Rebuild — should be clean:

```bash
dotnet build
```

> The scaffold's tests assert on `DemoService.Echo` so they'll fail after this edit. Either delete the failing assertions or update them to check for `BuccaneerService.Translate`. Out of scope for this lesson.

### 4. Pack the plugin as a NuGet

```bash
dotnet pack -c Release -o nupkgs
```

`nupkgs/Bowire.Plugin.Pirate.1.0.0.nupkg` lands in the output directory.

### 5. Install into Bowire

#### Path A — CLI install

```bash
bowire plugin install Bowire.Plugin.Pirate --source ./nupkgs
```

Confirm it landed:

```bash
bowire plugin list --verbose
```

You'll see `Bowire.Plugin.Pirate@1.0.0 [nuget: 1 files]` next to the bundled plugins (REST, gRPC, &c).

> The same install also shows up in-product under the **Settings rail → Plugins** extension-point tree (post-v2.1 Settings IA) — kind + capability chips per loaded plugin, so the operator can see what's installed without dropping back to the CLI.

#### Path B — `PackageReference` from the embedded host

In your embedded host project (`HelloApi` from Lesson 1.1 Path B, or any other ASP.NET host with `AddBowire()` + `MapBowire()`), reference the nupkg directly:

```bash
cd path/to/your/embedded-host
dotnet add package Bowire.Plugin.Pirate --source <abs-path>/Bowire.Plugin.Pirate/nupkgs
```

For production publish from your own internal NuGet feed (Azure DevOps Artifacts, GitHub Packages, MyGet, &c) — the `--source` flag accepts URLs. There's no `bowire plugin install` step in embedded mode — the package is just a normal transitive dependency of your host; the plugin host's `AssemblyLoadContext` picks it up at startup the same way as in CLI mode.

### 6. Run Bowire and invoke your protocol

Pirate Speak has no wire — the protocol implements its translation *inside the plugin*. But Bowire's discovery loop still needs a "URL" to associate the protocol with, so feed it any placeholder. Use `http://pirate.local` so it's obvious in the sidebar.

#### Path A — CLI

```bash
bowire --url http://pirate.local
```

The browser opens at `http://localhost:5050/bowire` and the sidebar now contains a **Pirate Speak** entry alongside REST / gRPC / &c:

```
🏴‍☠️ BuccaneerService (Pirate Speak)
   └─ Translate              (unary)
```

Click **Translate**. The form shows a free-form input pane (no field constraints because the scaffold passes `Fields: []`). Send a request body like:

```json
{ "text": "the gold is yours, you scurvy dog" }
```

Click **Invoke**. The response pane shows your code's output:

```json
{
  "translated": "th' gold be yers, ye scurvy dog 🏴‍☠️"
}
```

That JSON came from `PirateProtocol.InvokeAsync`. Your plugin is live inside Bowire.

#### Path B — Embedded

Add the placeholder URL to the embedded host's config so the workbench discovers against it on startup. Two options:

```bash
# appsettings.json
"Bowire": { "ServerUrls": [ "http://pirate.local" ] }

# Or as a CLI flag when launching the host
dotnet run -- --Bowire:ServerUrls:0=http://pirate.local
```

Then open <http://localhost:5001/bowire> (or whichever port your host runs on). Same Pirate Speak sidebar entry, same form, same translated response — but everything lives in the same process as your other routes. No second `bowire` CLI needed.

> **In production:** your real protocol plugin probably *does* have a wire — you'll pass the real URL (`mqtt://broker:1883`, `https://api.example.com`, &c.) through the same config. The placeholder shape here only exists because Pirate Speak is a toy with no wire.

### 7. Uninstall when you're done

#### Path A — CLI

```bash
bowire plugin uninstall Bowire.Plugin.Pirate
bowire plugin list                    # Pirate Speak is gone
```

The bundled plugins are untouched — `uninstall` only removes the directory under `~/.bowire/plugins/`.

#### Path B — Embedded

`dotnet remove package Bowire.Plugin.Pirate` from your host project, rebuild, and the next startup won't load the plugin. No `bowire plugin uninstall` equivalent — embedded plugins are just regular project dependencies, managed the same way as any other.

## Key Takeaways

1. **One interface, four methods.** `IBowireProtocol.{DiscoverAsync, InvokeAsync, InvokeStreamAsync, OpenChannelAsync}` is the entire contract. Streaming and channel methods can return `yield break` / `null` if your protocol is unary-only — Bowire respects that and grays out the corresponding UI affordances.
2. **NuGet is the install format on both paths.** `dotnet pack` produces one nupkg; CLI installs via `bowire plugin install`, embedded references it as a `PackageReference` from the host project. Same scaffold, same authoring, two install mechanics.
3. **Auto-discovery is "the assembly is loadable".** CLI: drop the DLL into `~/.bowire/plugins/`. Embedded: it's just a transitive dependency of your host. Either way Bowire scans `IBowireProtocol` implementations at startup; no registration code, no extra manifest beyond the NuGet metadata.
4. **`dotnet new bowire-plugin` saves the scaffolding cost.** The template emits a buildable solution; you only fill in the protocol-specific body of `DiscoverAsync` / `InvokeAsync`.

## What's Next

You're ready to author the same protocol in Python — same contract, JSON-RPC over stdio, no .NET on the plugin side.

**Continue:** → [Lesson 4.2: Python sidecar plugin](../lesson-2/README.md)

## Reference

- [`dotnet new bowire-plugin` template repo](https://github.com/Kuestenlogik/Bowire.Templates) — the scaffold's source, all parameters and presets.
- [Plugin system docs](https://bowire.io/docs/features/plugin-system.html) — install / list / update / uninstall semantics, the workbench's plugin-management panel.
- [Plugin architecture](https://bowire.io/docs/architecture/plugin-architecture.html) — how Bowire's plugin AssemblyLoadContext isolation works, why your plugin can ship its own dependency versions without colliding with the host.
