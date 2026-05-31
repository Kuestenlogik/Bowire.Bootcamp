# Lesson 05 — Author a .NET protocol plugin

**Time:** 15 minutes • **Prerequisites:** Lesson 01 done • **Need:** .NET 10 SDK

## Goal

Build your own protocol plugin from scratch, install it into Bowire, and watch a fresh workbench discover it on startup. By the end you'll have a (silly) **Pirate Speak** protocol in the sidebar that turns plain English into pirate-speak when invoked.

The protocol is deliberately self-contained — no external wire, no broker, no schema-discovery step. The point is the **plugin contract**: implement `IBowireProtocol`, ship the NuGet, install via `bowire plugin install`, and your code lands inside Bowire's plugin pipeline. Replace the substitution-table body with `HttpClient.SendAsync` / `MQTTnet.PublishAsync` / `GrpcChannel.ForAddress` and you have a real protocol plugin.

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

```bash
bowire plugin install Bowire.Plugin.Pirate --source ./nupkgs
```

Confirm it landed:

```bash
bowire plugin list --verbose
```

You'll see `Bowire.Plugin.Pirate@1.0.0 [nuget: 1 files]` next to the bundled plugins (REST, gRPC, &c).

### 6. Run Bowire and invoke your protocol

Pirate Speak has no wire — the protocol implements its translation *inside the plugin*. But Bowire's discovery loop still needs a "URL" to associate the protocol with, so feed it any placeholder. Use `http://pirate.local` so it's obvious in the sidebar:

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

### 7. Uninstall when you're done

```bash
bowire plugin uninstall Bowire.Plugin.Pirate
bowire plugin list                    # Pirate Speak is gone
```

The bundled plugins are untouched — `uninstall` only removes the directory under `~/.bowire/plugins/`.

## What you just saw

- **One interface, four methods.** `IBowireProtocol.{DiscoverAsync, InvokeAsync, InvokeStreamAsync, OpenChannelAsync}` is the entire contract. Streaming and channel methods can return `yield break` / `null` if your protocol is unary-only — Bowire respects that and grays out the corresponding UI affordances.
- **NuGet is the install format.** `dotnet pack` → `bowire plugin install` from any source (local folder, internal feed, NuGet.org). Same machinery the bundled plugins ship through.
- **Auto-discovery is "drop the DLL into `~/.bowire/plugins/`".** No registration code, no manifest beyond the NuGet metadata. Bowire scans installed plugin directories at startup and surfaces every `IBowireProtocol` type it finds.

## What's next

[Lesson 06 — Author a Python sidecar plugin](../lesson-06-python-sidecar/) builds the same surface in Python — no .NET project, no NuGet, no IBowireProtocol interface. You subclass `BowirePlugin` from the official Python SDK, ship a zip with `sidecar.json` at its root, and Bowire spawns the sidecar over JSON-RPC. Same workbench, same discovery, different language.

## Reference

- [`dotnet new bowire-plugin` template repo](https://github.com/Kuestenlogik/Bowire.Templates) — the scaffold's source, all parameters and presets.
- [Plugin system docs](https://bowire.io/docs/features/plugin-system.html) — install / list / update / uninstall semantics, the workbench's plugin-management panel.
- [Plugin architecture](https://bowire.io/docs/architecture/plugin-architecture.html) — how Bowire's plugin AssemblyLoadContext isolation works, why your plugin can ship its own dependency versions without colliding with the host.
