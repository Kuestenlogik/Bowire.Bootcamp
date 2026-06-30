# Lesson 4.4: Map widget — semantic kinds + UI extensions

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Unit 1.1](../../unit-1/README.md) (Embedded shape preferred), .NET 10 SDK

## Overview

When your service returns a JSON payload with a `lat` and a `lon` next to each other, Bowire auto-mounts a MapLibre GL JS map over the response — pins per object, hover linking between the JSON tree and the map, click / dblclick gestures. No OpenAPI extension, no `x-bowire-*` hint, no per-route widget code. This lesson walks the whole chain end-to-end: the detector that recognises the coordinate pair, the semantic kind it produces, the `Kuestenlogik.Bowire.Map` UI extension that mounts the renderer, and what it takes to extend that chain with your own kind.

By the end you'll have driven `Sample.TacticalApi`'s tactical-situation feed against a workbench, watched eight callsigns appear on the map, and you'll have extended the seed with a new entity at a custom coordinate.

## How the auto-mount works

```
JSON frame from your service                        →
  IBowireFieldDetector (Wgs84CoordinateDetector)    →   annotation: coordinate.latitude / coordinate.longitude
  pair-resolver (RecordingInterpretationBuilder)    →   composite kind: coordinate.wgs84
  /api/ui/extensions enumeration                    →   match → kuestenlogik.maplibre extension
  workbench JS loader                               →   dynamic-import + window.BowireExtensions.register({...})
  viewer.mount()                                    →   map widget over the response pane
```

Three moving parts under "Map widget":

1. **`Wgs84CoordinateDetector`** — a built-in `IBowireFieldDetector` in `Kuestenlogik.Bowire.Semantics.Detectors`. Walks the decoded frame, looks for a `lat`-shaped + `lon`-shaped numeric pair at the same parent object path, emits two annotations: `coordinate.latitude` on the lat field and `coordinate.longitude` on the lon field. Both ranges are validated (`[-90, 90]` / `[-180, 180]`) so a stray `{x, y}` pixel-offset pair can't drag the map onto a non-geographic field.
2. **The pair-resolver** combines a `coordinate.latitude` + `coordinate.longitude` at the same parent path into the composite kind `coordinate.wgs84` — that's the kind UI extensions register against.
3. **`MapLibreExtension`** — an `IBowireUiExtension` in `Kuestenlogik.Bowire.Map`, marked with `[BowireExtension]`. Claims `Kinds = ["coordinate.wgs84"]` and ships the map JS bundle as an embedded resource. The core's `BowireExtensionRegistry.Discover()` scans loaded `Kuestenlogik.Bowire*` assemblies at startup, finds the type, registers it, and serves its bundle at `/api/ui/extensions/kuestenlogik.maplibre/map.js`.

### Coordinate names the detector recognises

The detector uses two anchored, case-insensitive regexes (see `Wgs84CoordinateDetector.cs:120-131`):

- **Latitude** — `^lat(itude)?(coordinate)?$`. Matches `lat`, `latitude`, `latCoordinate`, `latitudeCoordinate`.
- **Longitude** — `^(lon|lo?ng(itude)?)(coordinate)?$`. Matches `lon`, `lng`, `long`, `longitude`, plus the same optional `coordinate` suffix.

That's why all three shapes work without any per-service configuration:

```json
{ "lat": 50.0379, "lon": 8.5622 }                                                  // Sample.Embedded
{ "latitude": 50.0379, "longitude": 8.5622 }                                       // GeoJSON-ish APIs
{ "latitudeCoordinate": 50.0379, "longitudeCoordinate": 8.5622 }                   // TacticalAPI protobuf
```

Pairs are only emitted when **exactly one** of each is present at the same parent — a `start + end` object with two `lat` fields and no `lon` is intentionally ignored to avoid false positives.

## Steps

### 1. Reference the Map package

```bash
dotnet add package Kuestenlogik.Bowire.Map
```

The Bundle.Workbench meta-package and the standalone Tool already pull this in transitively, so a plain `bowire --url …` shape has the map widget out of the box. Slim embedded hosts that reference bare `Kuestenlogik.Bowire` need the explicit `Kuestenlogik.Bowire.Map` reference — adding it is enough; no extra DI call.

### 2. Run a sample that emits coordinate frames

Two ready-made samples ship in the main Bowire repo:

```bash
# REST, lat/lon — the simple shape
dotnet run --project samples/Kuestenlogik.Bowire.Sample.Embedded --urls http://localhost:5181

# gRPC, latitudeCoordinate/longitudeCoordinate — the TacticalAPI shape
dotnet run --project samples/Kuestenlogik.Bowire.Sample.TacticalApi --urls http://localhost:5182
```

For the gRPC sample you'll also need the Bowire Tool running against it:

```bash
bowire --url http://localhost:5182
```

Either way, open the workbench (`/bowire` on the embedded host, or `http://localhost:5180` for the Tool) and invoke one of the geo endpoints — `GET /api/locations` on the embedded host, or `Situation.GetSituationObjects` against TacticalAPI. The response pane shows the JSON tree, and the map widget mounts beside it with one pin per object.

### 3. Drive the gestures

Once the map is mounted:

- **Hover** a JSON node → the corresponding pin highlights.
- **Hover** a pin → the JSON node scrolls into view.
- **Click** a pin → selects the row in the JSON tree.
- **Double-click** the map → recentres on the clicked coord.

The JS bundle (`src/Kuestenlogik.Bowire.Map/wwwroot/js/widgets/map.js`) registers itself via `window.BowireExtensions.register({ id: "kuestenlogik.maplibre", kind: "coordinate.wgs84", viewer: { mount }, editor: { mount } })`. The workbench's extension router calls `viewer.mount()` with the frame + the resolved `coordinate.wgs84` payload (lat / lon + their JSON paths so the widget can wire gestures back).

### 4. Verify the no-CDN guarantee

The Map extension ships MapLibre GL JS as an *embedded resource*. The widget's loader (`bowireLoadMapLibre` in `map.js`) injects a `<script>` tag pointing at `/api/ui/extensions/kuestenlogik.maplibre/maplibre-gl.js` — the same Bowire host serves the renderer; no `unpkg.com`, no `cdnjs`. When `Bowire:MapTileUrl` is unset the widget configures a no-source MapLibre style with no `glyphs` URL and no `sprite` URL, so no external request is made for tiles either. Air-gapped deploys keep working.

## Exercise — extend the TacticalAPI seed

`samples/Kuestenlogik.Bowire.Sample.TacticalApi/Program.cs` defines a `Seed` array of eight tactical entities. Extend it:

1. Open `Program.cs` and find the `private static readonly (string Uuid, string Name, double Lat, double Lon)[] Seed = [ ... ]` block (around line 95).
2. Add a ninth row at a coordinate of your choice — pick somewhere in the DACH region so it's visible alongside the seeded eight:
   ```csharp
   ("c5b3b5b6-1a2d-4e9b-8c0a-1f7a2d9c1a09", "India-9 — Field Hospital", 47.5596, 7.5886),  // Basel
   ```
3. Restart the gRPC sample and the Bowire Tool dialled at it.
4. Re-invoke `Situation.GetSituationObjects` from the workbench.
5. Confirm:
   - The response JSON now has nine `situationObjects`.
   - The map shows nine pins.
   - Hovering the new JSON row highlights the Basel pin.
   - The detector pinned `latitudeCoordinate` + `longitudeCoordinate` automatically — there's no `tacticalapi.proto` change needed because the *names* drive the auto-detection, not a schema annotation.

### Bonus: trip the strict-pair rule

Add a tenth row where you accidentally seed a `Lat = 95.0` (out of WGS84 range). The detector silently drops the pair — the map shows nine pins, not ten. That's the range check (`Wgs84CoordinateDetector.cs:95`) doing its job; the workbench would rather under-detect than mount the widget on a swapped or corrupt coordinate.

## Custom semantic kinds — the honest tour

You might want a widget for, say, a thermometer over a `temperature.celsius` kind. Two pieces have to land:

### Part A — a detector (DI registration)

`IBowireFieldDetector` is *not* auto-discovered by `BowireExtensionRegistry.Discover()` today — it's resolved via DI by `FrameProber`. The doc-comment on the interface explicitly calls out the third-party `[BowireExtension]` path as **future**. The way to ship a detector right now is a regular `AddSingleton<IBowireFieldDetector, MyDetector>()` from your host's startup:

```csharp
builder.Services.AddBowire();
builder.Services.AddSingleton<IBowireFieldDetector, TemperatureCelsiusDetector>();
```

`FrameProber` resolves `IEnumerable<IBowireFieldDetector>` and will pick yours up alongside the five built-ins. The detector emits a `new SemanticTag("temperature.celsius")` annotation on the matched field — `SemanticTag` is a `sealed record(string Kind)` with no closed-enum constraint, so any kind-string is valid as long as you're consistent on both sides of the chain.

### Part B — a UI extension (assembly scan)

`IBowireUiExtension` *is* auto-discovered. Mark your viewer with `[BowireExtension]`, give it `Kinds = ["temperature.celsius"]`, ship the JS bundle as an embedded resource, ship the package next to the host — `BowireExtensionRegistry.Discover()` (called by the workbench at startup over every loaded `Kuestenlogik.Bowire*` assembly) finds it. Your JS bundle registers via `window.BowireExtensions.register({ id, kind, viewer: { mount } })` and the workbench routes any field tagged with your kind through it.

> **The asymmetry is real.** Detector registration uses DI; UI extension registration uses assembly scanning. Both work today; they just don't share a discovery mechanism. If you only need a viewer for an existing kind (e.g. a *different* widget for `coordinate.wgs84`), Part B alone is enough — the built-in `Wgs84CoordinateDetector` already supplies the annotation.

## Key Takeaways

1. **The auto-mount chain has four stages.** Detector → semantic-tag annotation → pair-resolver → UI-extension mount. Each stage is its own file; the chain is wired through DI (detector) and assembly scanning (UI extension).
2. **`coordinate.wgs84` is the composite kind.** The detector emits `coordinate.latitude` + `coordinate.longitude` separately; the pair-resolver collapses them. UI extensions register against the composite.
3. **The Wgs84 detector recognises three field-name shapes.** `lat/lon`, `latitude/longitude`, `*Coordinate` variants. All three are anchored, case-insensitive, and range-bounded — `{x, y}` pixel pairs can't masquerade as coordinates.
4. **Custom kinds need two registrations today.** DI for the detector, assembly scan for the UI extension. The doc-comment on `IBowireFieldDetector` flags the unified `[BowireExtension]` path as future, not present.

## Knowledge Assessment

1. **The auto-mount path.** What does the workbench see when a response carries `{ "name": "Hangar 7", "lat": 47.4933, "lon": 13.0078 }`?  
   *Answer:* `Wgs84CoordinateDetector` annotates the `lat` path as `coordinate.latitude`, the `lon` path as `coordinate.longitude`. The pair-resolver builds a composite `coordinate.wgs84` interpretation at the parent path. The workbench enumerates `/api/ui/extensions`, finds `kuestenlogik.maplibre` claims that kind, and mounts the map widget over the response pane.

2. **Coordinate shapes.** Which of these JSON snippets does the built-in detector pin on the map? `(a) { "lat": 50, "lon": 8 }`, `(b) { "latitude": 50, "longitude": 8 }`, `(c) { "latitudeCoordinate": 50, "longitudeCoordinate": 8 }`, `(d) { "x": 50, "y": 8 }`, `(e) { "lat": 50, "lat2": 51, "lon": 8 }`.  
   *Answer:* (a), (b), (c) — all three match the anchored regex. (d) doesn't match the name patterns. (e) has two `lat` fields at the same parent; the detector intentionally drops ambiguous shapes rather than guess.

3. **Range guard.** A debug build of your service emits `{ "lat": -91.2, "lon": 8.0 }` after a unit-conversion bug. What does the map show?  
   *Answer:* Nothing. The detector validates `lat ∈ [-90, 90]` and `lon ∈ [-180, 180]` before emitting the annotation pair; out-of-range values are silently skipped. The JSON tree still shows the values — the map just doesn't mount.

4. **Adding a thermometer widget.** You want `{ "temperature": 21.5 }` to mount a thermometer. What's the minimum set of pieces to ship?  
   *Answer:* (1) A `TemperatureCelsiusDetector : IBowireFieldDetector` that emits `new SemanticTag("temperature.celsius")` on numeric `temperature*`-named fields, registered via `services.AddSingleton<IBowireFieldDetector, TemperatureCelsiusDetector>()` from the host. (2) A `ThermometerExtension : IBowireUiExtension` marked with `[BowireExtension]`, claiming `Kinds = ["temperature.celsius"]`, shipping a JS bundle that registers via `window.BowireExtensions.register({ ... })`, packaged as a `Kuestenlogik.Bowire*` assembly so `BowireExtensionRegistry.Discover()` picks it up. Detector via DI; viewer via assembly scan.

## What's Next

You've now seen both extension axes Bowire exposes: protocol plugins (Lessons 4.1 / 4.2), middleware (Lesson 4.3), and UI extensions for semantic kinds (this lesson). The Developer capstone puts at least one of these into a shippable NuGet — pick the one closest to a real protocol / kind you work with.

**Continue:** → [Developer capstone](../../../capstones/developer/README.md)

## Reference

- `src/Kuestenlogik.Bowire.Map/Semantics/Extensions/MapLibreExtension.cs` — the `IBowireUiExtension` descriptor; `[BowireExtension]` attribute; embedded-resource asset list.
- `src/Kuestenlogik.Bowire.Map/wwwroot/js/widgets/map.js` — the JS bundle; `window.BowireExtensions.register({...})` registration; gesture wiring.
- `src/Kuestenlogik.Bowire/Semantics/Detectors/Wgs84CoordinateDetector.cs` — the built-in detector; name regexes; range guards.
- `src/Kuestenlogik.Bowire/Semantics/SemanticTag.cs` — `BuiltInSemanticTags.CoordinateLatitude` / `CoordinateLongitude`; the open `SemanticTag(string Kind)` record.
- `src/Kuestenlogik.Bowire/Semantics/Extensions/BowireExtensionRegistry.cs` — `Discover()`; the `Kuestenlogik.Bowire*` assembly scan.
- `src/Kuestenlogik.Bowire/Semantics/Extensions/IBowireUiExtension.cs` — the descriptor surface (`Id`, `Kinds`, `BundleResourceName`, `AdditionalAssetNames`).
- `samples/Kuestenlogik.Bowire.Sample.TacticalApi/Program.cs` — the seeded gRPC service used in the exercise.
- `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` — the REST `lat`/`lon` shape (`/api/locations`).
