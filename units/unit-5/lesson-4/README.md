# Lesson 5.4: Plugin lifecycle

> **Difficulty:** Intermediate | **Duration:** 10 min | **Prerequisites:** [Lesson 5.1](../lesson-1/README.md)

## Overview

Plugins aren't fire-and-forget. Bowire can **load, unload, restart, reset-storage and health-check** a plugin at runtime — without restarting the whole workbench. This matters while authoring (iterate on a plugin without a full reboot) and in operations (isolate a misbehaving plugin).

## The lifecycle operations

| Operation | What it does |
|---|---|
| **Load** | Bring an installed-but-inactive plugin into the running workbench. |
| **Unload** | Remove a plugin from the running process (frees its discovery + handlers). |
| **Restart** | Unload + load — pick up a rebuilt assembly / sidecar without a workbench reboot. |
| **Reset storage** | Clear a plugin's persisted state back to a clean slate. |
| **Health** | The plugin's health signal, surfaced as a sidebar badge and via the plugin API. |

## Steps

### 1. Manage from the workbench

Settings → Plugins lists installed plugins with their health badge and the Load / Unload / Restart / Reset-storage controls. Restart after a `dotnet pack` + reinstall to pick up your latest build without losing your workbench session.

### 2. Manage from the CLI

`bowire plugin list` / `install` / `uninstall` / `update` / `inspect` cover the install-time lifecycle; startup gating uses `--disable-plugin <id>` ([Unit 3.6](../../unit-3/lesson-6/README.md)) to skip a plugin whose load fails or whose discovery probe is too slow.

### 3. Author loop

While building a plugin: `dotnet pack` → reinstall → **Restart** the plugin in Settings → re-discover. No full workbench reboot, so your open tabs and workspace survive the iteration.

## Key Takeaways

1. **Load / unload / restart / reset-storage / health** — full runtime control, no workbench reboot.
2. **Restart is the author's iterate loop** after a rebuild.
3. **`--disable-plugin` is the operator's safety valve** for a plugin that won't load.

## What's Next

That closes the Extend unit — and the last of the modality units. Round it off with your course's **capstone** ([user](../../capstones/user/README.md) · [developer](../../capstones/developer/README.md) · [administrator](../../capstones/administrator/README.md)), each of which extends the Harbor domain.

## Reference

- [Plugin lifecycle](https://bowire.io/docs/extending/) · [Plugin management CLI](https://bowire.io/docs/features/cli-mode.html)
