# Lesson 5.1: Author a .NET protocol plugin

> **Difficulty:** Intermediate | **Duration:** 18 min | **Prerequisites:** [Unit 0](../../unit-0/README.md); .NET 10 SDK

## Overview

Build a protocol plugin, pack it as a NuGet, install it, and watch a fresh workbench discover it on startup. The contract is small: implement **`IBowireProtocol`**, ship the assembly, and Bowire's plugin pipeline does the rest.

## Steps

### 1. Scaffold from the template

```bash
dotnet new install Kuestenlogik.Bowire.Templates
dotnet new bowire-plugin \
  -n Bowire.Plugin.Pirate --ProtocolId pirate \
  --DisplayName "Pirate Speak" --PluginClassName PirateProtocol --Author "You"
cd Bowire.Plugin.Pirate
dotnet build
```

The scaffold emits a `.slnx` with the plugin assembly (an `IBowireProtocol` stub), an xUnit test project asserting the discovered shape, pinned `Directory.Packages.props`, and a CI workflow.

### 2. Implement `IBowireProtocol`

The interface is the whole surface: advertise a service + methods (so discovery can render them), and handle invocation:

- **Discovery** — return the services/methods the workbench shows in the sidebar.
- **`InvokeAsync`** — unary call: take the request, return the response.
- **`InvokeStreamAsync`** — server-streaming (yield frames); return an empty stream for unary-only protocols.
- **`OpenChannelAsync`** — bidirectional; return `null` when unsupported.

Swap the stub's body for real wire work — `HttpClient.SendAsync`, `MQTTnet.PublishAsync`, `GrpcChannel.ForAddress`, or anything else.

> **Study a real one.** [`Bowire.Protocol.Akka`](https://github.com/Kuestenlogik/Bowire.Protocol.Akka) implements `IBowireProtocol` with a single server-streaming method (`Tap/MonitorMessages`) that yields `TappedMessage` envelopes from an Akka.NET mailbox tap — a genuine, shipped plugin with `src/`, `tests/` and `samples/` you can read end to end.

### 3. Pack + install (CLI shape)

```bash
dotnet pack -c Release -o nupkgs
bowire plugin install Bowire.Plugin.Pirate --source ./nupkgs
bowire plugin list
```

`bowire plugin` also covers `download` (offline nupkgs), `uninstall`, `update`, and `inspect`. Restart the workbench — your protocol appears in the sidebar.

### 4. Or reference it in an embedded host

In an embedded host ([Unit 4](../../unit-4/README.md)), a plain `<PackageReference Include="Bowire.Plugin.Pirate" />` is enough — `AddBowire()` discovers registered `IBowireProtocol` implementations through DI. Same NuGet, two install paths.

## Key Takeaways

1. **`IBowireProtocol` is the contract** — discovery + `InvokeAsync` / `InvokeStreamAsync` / `OpenChannelAsync`.
2. **`dotnet new bowire-plugin` scaffolds everything**; `dotnet pack` → `bowire plugin install` (CLI) or `PackageReference` (embedded).
3. **`Bowire.Protocol.Akka` is a real, complete example** to learn from.

## What's Next

**Continue:** → [Lesson 5.2: Polyglot sidecar plugin](../lesson-2/README.md)

## Reference

- [Extending Bowire — protocol plugins](https://bowire.io/docs/extending/)
- [`Bowire.Protocol.Akka`](https://github.com/Kuestenlogik/Bowire.Protocol.Akka)
