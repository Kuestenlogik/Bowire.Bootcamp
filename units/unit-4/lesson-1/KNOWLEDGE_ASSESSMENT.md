# Quiz: Lesson 4.1 — .NET Protocol Plugin

## Multiple Choice

### 1. What is the minimal surface every Bowire protocol plugin must implement?

- [ ] A) `IBowireProtocol.{Name, Id, IconSvg, DiscoverAsync, InvokeAsync, InvokeStreamAsync, OpenChannelAsync}`
- [ ] B) A single `Process(string url)` method on a `BowirePluginBase` class.
- [ ] C) A `ConfigureServices(IServiceCollection)` method.
- [ ] D) Whatever the plugin's NuGet metadata declares.

<details>
<summary>Answer</summary>

**A)** Four methods (`DiscoverAsync`, `InvokeAsync`, `InvokeStreamAsync`, `OpenChannelAsync`) plus the identity properties (`Name`, `Id`, `IconSvg`). Streaming and channel methods can be no-ops for unary-only protocols.

</details>

### 2. How does Bowire's host find your plugin DLL at startup?

- [ ] A) The plugin manifests itself via a `BowirePluginAttribute` in `AssemblyInfo.cs`.
- [ ] B) It scans `~/.bowire/plugins/` for assemblies and reflects every `IBowireProtocol` type it finds.
- [ ] C) Plugins are loaded from a configured list in `appsettings.json`.
- [ ] D) The user explicitly registers each plugin in code.

<details>
<summary>Answer</summary>

**B)** Directory scan plus reflection. `bowire plugin install` drops the resolved NuGet (and its dependencies) into `~/.bowire/plugins/<package-id>/`; the host's plugin scanner picks them up on next startup.

</details>

### 3. The `dotnet pack` step produces a `.nupkg`. What does `bowire plugin install --source ./nupkgs MyPlugin` do with it?

- [ ] A) Copies the `.nupkg` into the bin folder of the running workbench.
- [ ] B) Pushes the `.nupkg` to nuget.org.
- [ ] C) Resolves the `.nupkg` like any NuGet client, downloads it + transitive deps, lays them out under `~/.bowire/plugins/MyPlugin/`.
- [ ] D) Extracts the `.nupkg` into `bin/Debug`.

<details>
<summary>Answer</summary>

**C)** Regular NuGet resolution. `--source` adds a feed (local folder is fine), NuGet picks the best matching version, the plugin manager unpacks the result into the plugin directory.

</details>

## True / False

### 4. Protocols that don't have a wire-discovery mechanism (e.g. MQTT, raw WebSocket) can still ship a Bowire plugin.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**True.** `DiscoverAsync` can return a hand-curated list of services / methods read from configuration, an annotation attribute, or any other source. The Lesson 4.1 Pirate plugin demonstrates this: it returns a fixed `BuccaneerService.Translate` topology, no wire involved.

</details>

### 5. `IBowireProtocol.InvokeStreamAsync` must always emit at least one frame.

- [ ] True
- [ ] False

<details>
<summary>Answer</summary>

**False.** For unary-only protocols, return an empty `IAsyncEnumerable<string>` (e.g. `yield break`). Bowire greys out the streaming-related UI affordances when streaming methods aren't applicable.

</details>

## Short Answer

### 6. Walk through the four-step deployment flow from "I edited PirateProtocol.cs" to "the plugin appears in a fresh `bowire` workbench".

<details>
<summary>Answer</summary>

1. **`dotnet pack -c Release -o nupkgs`** — package the assembly + dependencies into a `.nupkg`.
2. **`bowire plugin install <PackageId> --source ./nupkgs`** — NuGet-resolve from the local folder, lay out under `~/.bowire/plugins/<PackageId>/`.
3. **`bowire plugin list`** — confirm the plugin landed (look for the `[nuget: N files]` tag).
4. **`bowire --url <something>`** — start the workbench. The plugin scanner reflects every `IBowireProtocol` type in the plugin directory and the new tab appears in the sidebar.

</details>

## Score

- 6/6: Solid — continue to Lesson 4.2.
- 4–5/6: Good — skim the install / discovery section once more.
- < 4/6: Re-run the lesson hands-on.
