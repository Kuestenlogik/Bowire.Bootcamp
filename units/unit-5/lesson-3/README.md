# Lesson 5.3: UI extension — semantic kinds

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 5.1](../lesson-1/README.md); .NET 10 SDK

## Overview

Protocol plugins extend *what Bowire can talk to*. **UI extensions** extend *how Bowire renders a response*. When a response field carries a **semantic kind** — a tag like `coordinate.wgs84` — Bowire can auto-mount a purpose-built widget (a map, a chart, a hex viewer) over it instead of showing raw JSON.

## How it works

- A field is annotated with a **semantic kind** (e.g. `coordinate.wgs84` for a lat/long pair).
- A UI extension declares which kind it handles via the **`[BowireExtension]`** attribute and implements **`IBowireUiExtension`**.
- Auto-discovery (the same `[BowireExtension]` assembly scan that finds field-detectors) mounts the widget wherever a matching kind appears — no per-response wiring.

The canonical example: a `coordinate.wgs84` field auto-renders a **map widget** with a pin, instead of `{ "lat": 53.55, "lon": 9.99 }`.

## Steps

### 1. Add the extension

In a plugin assembly (scaffolded as in [Lesson 5.1](lesson-1/README.md)), implement `IBowireUiExtension` and tag it:

```csharp
[BowireExtension]
public sealed class MapWidgetExtension : IBowireUiExtension
{
    public string SemanticKind => "coordinate.wgs84";
    // render contract: emit the widget markup/props for a matching field
}
```

`[BowireExtension]` opts the type into the auto-discovery scan (the same mechanism protocol/field-detector extensions use), so no manual registration is needed.

### 2. Tag the data

A field surfaces the widget when its semantic kind resolves to `coordinate.wgs84` — either declared by the protocol's schema, or inferred by a field-detector (Bowire ships a WGS84 coordinate detector). When the workbench renders a response containing that field, your widget mounts automatically.

### 3. Verify

Invoke a method whose response carries a coordinate. Instead of raw JSON, the response pane shows the map with a pin. Remove the extension (or its assembly) and the field degrades gracefully back to JSON — extensions are additive.

> Ship UI extensions in the same NuGet as a protocol plugin, or standalone. The `Kuestenlogik.Bowire.Map` package is the shipped map-widget extension — a real reference.

## Key Takeaways

1. **Semantic kinds drive rendering** — a tag like `coordinate.wgs84` picks a widget.
2. **`[BowireExtension]` + `IBowireUiExtension`** = auto-discovered UI extension; no per-response wiring.
3. **Additive + graceful** — no matching extension → the field falls back to JSON.

## What's Next

**Continue:** → [Lesson 5.4: Plugin lifecycle](../lesson-4/README.md)

## Reference

- [Extending Bowire — UI extensions](https://bowire.io/docs/extending/)
