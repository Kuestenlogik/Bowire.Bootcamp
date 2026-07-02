# Lesson 3.6: Observability & operations

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 3.5](../lesson-5/README.md)

## Overview

Run Bowire like a production service: emit traces and metrics, watch plugin health, back up state, and disable misbehaving plugins.

## Telemetry (OpenTelemetry)

Off by default — laptop installs stay quiet. Opt in:

```bash
bowire --telemetry --url http://localhost:5101
```

Bowire emits traces + Bowire-domain metrics through the canonical `Kuestenlogik.Bowire` `Meter` + `ActivitySource`:

- `bowire.invoke.count` / `bowire.invoke.duration`
- `bowire.discover.count`
- `bowire.plugin.load`
- `bowire.mock.requests`

Endpoint, headers and protocol come from the standard OTLP env vars:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://collector:4317
export OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

On shared multi-tenant installs (GDPR / HIPAA / SOX), drop the high-cardinality service + method dimensions:

```bash
bowire --telemetry --telemetry-strip-method-labels
```

## Plugin health & disable

- Every installed plugin exposes a health signal; the workbench's sidebar surfaces a badge, and the plugin lifecycle (load / unload / restart / reset-storage — [Unit 5](../../unit-5/README.md)) is scriptable.
- Skip a misbehaving protocol plugin at startup: `bowire --disable-plugin grpc --disable-plugin signalr` (repeat or comma-separate). Mirror for auto-discovered CLI subcommands with `--disable-cli-command`.
- Update checks are opt-in (`--update-check`) — never phone home by default.

## Backup

Workspace state lives under `~/.bowire` (recordings, environments, collections, flows) and, per-workspace, under `~/.bowire/workspaces/<id>/`. Back that tree up; a `.bww` export bundles a workspace for transport. In containers, mount it as a volume so state survives restarts.

## Key Takeaways

1. **`--telemetry` is opt-in**; metrics/traces flow through the `Kuestenlogik.Bowire` Meter/ActivitySource to any OTLP collector via `OTEL_EXPORTER_OTLP_*`.
2. **`--telemetry-strip-method-labels`** for shared installs that can't keep per-method cardinality.
3. **`--disable-plugin` isolates a bad plugin**; update checks + telemetry never run unless you opt in.
4. **Back up `~/.bowire`** (and per-workspace dirs); `.bww` bundles a workspace.

## What's Next

**Continue:** → [Lesson 3.7: Workspace hygiene](../lesson-7/README.md)

## Reference

- [Observability (#29)](https://bowire.io/docs/) · [Configuration](https://bowire.io/docs/setup/)
