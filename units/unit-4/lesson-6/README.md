# Lesson 4.6: Plugin lifecycle — Load / Unload / Restart / Reset-storage

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 4.1](../lesson-1/README.md) (a `.NET` protocol plugin installed) or any Bowire host with more than one protocol plugin loaded

## Overview

Before v2.2, a misbehaving plugin meant restarting the whole Bowire process — the `bowire` CLI, the embedded host, whatever. In practice that meant losing your open tabs, your reverse-proxy state, your active recording session, and any other in-memory context. Painful.

v2.2 turns the plugin registry into a live, mutable object. Four actions land on `/api/plugins/{pluginId}/lifecycle/{action}` and are wired to per-row buttons under **Settings → Configure → Protocols** (and the sibling widgets / modules / formats / tools / discovery panes):

| Action | What it does | When to use |
|---|---|---|
| **Load** | Bring a disabled / unloaded plugin back into the live registry. The DLL must already be in-process — this is *re-enable*, not *install*. | You disabled a plugin at startup (`--disable-plugin grpc`) and want it back without restarting. |
| **Unload** | Remove the plugin from the registry, dispose it, and persist the id to the disabled-list so it stays off across restarts. | A plugin's discovery probe is misbehaving and you need it out of the way right now. |
| **Restart** | Dispose the current instance and construct a fresh one via `Activator.CreateInstance`, then re-run `Initialize`. Registry keeps the same slot. | A plugin's internal state is stuck (cache, HTTP client pool, background reconnect loop). |
| **Reset storage** | Wipe the plugin's per-workspace localStorage buckets. | A plugin's UI state is corrupt — the discovered service list keeps showing stale entries, an extension's saved config is malformed. |

All four are HTTP POST endpoints under `/api/plugins/{pluginId}/lifecycle/{action}`; the workbench surfaces them as buttons on each plugin row. Both call sites hit the same backend.

## Steps

### 1. Locate the buttons

Open the workbench. Click the gear icon → **Settings → Configure → Protocols**. Every loaded plugin renders a row with:

- Left: `displayName` + protocol id + package name.
- Right: the pill (installed / suggested).
- Below: a **lifecycle button cluster** — `Load · Unload · Restart · Reset storage`.

The same cluster appears under **Configure → Widgets** (UI extensions) and the other configure-* panes, one row per contribution.

### 2. Restart — the low-risk starter

Pick a protocol plugin you don't mind bouncing (the bundled REST plugin — id `rest` — is a safe choice: nothing else in the workbench holds a long-lived reference to its instance).

Click **Restart**. The button label changes to `Working…` while the request is in flight; a small result banner appears below the buttons:

- On success: `Plugin 'rest' restarted.`
- On failure: `HTTP 500 — <problem-title>` with the exception type as an extension.

Behind the scenes the endpoint:

1. Grabs the current instance from `BowireProtocolRegistry`.
2. Calls `Activator.CreateInstance(existing.GetType())` to build a fresh one.
3. **Only if that succeeds** disposes the old instance (`IDisposable.Dispose`). A ctor throw leaves the live plugin intact — a broken restart shouldn't leave you worse off than before.
4. Replaces the registry slot and calls `Initialize(services)` on the new instance.

Every step emits a `bowire.plugin.lifecycle` telemetry event with `outcome=ok|not-found|ctor-threw|init-threw|…` for your OTLP collector.

### 3. Unload — the "get it out of the way" action

Click **Unload** on a protocol you're not using right now (say, `signalr` if your discovered services don't speak it).

The row's protocol pill greys out — the plugin is no longer in the live registry, and the disabled-list on disk (`disabled-plugins.json` in the Bowire user-store) now carries its id. If you restart the whole process, the plugin stays off; if you close and reopen the workbench, the plugin still doesn't load.

To bring it back: click **Load** on the same row (it's still visible because the DLL is in-process). The disabled-list entry is dropped and discovery re-runs against the merged disabled set.

> **`Load` is *re-enable*, not *install*.** The plugin's assembly has to be in the AppDomain already. If you want a brand-new plugin from disk, that's `bowire plugin install <packageId>` (or `--file <sidecar.zip>`) — the install flow, not the lifecycle flow.

### 4. Reset storage — the last-resort UI wipe

Some plugins persist per-workspace UI state to `localStorage` (Bowire's browser storage): recent selections, favourite services, filter presets. When that state corrupts — the discovered list keeps showing services from three deployments ago, the plugin's extension pane rejects your saved config as malformed — **Reset storage** wipes the plugin's localStorage buckets.

Click it, confirm the toast. The plugin's state is gone; the plugin itself keeps running (contrast with Unload, which removes the instance). Refresh the browser tab; the plugin renders with a clean slate.

### 5. Watch the telemetry

If you have `--telemetry` on (Lesson 5.3), every lifecycle call emits an event on the `Kuestenlogik.Bowire` Meter with:

- `plugin.id` — the id you clicked.
- `lifecycle.action` — `restart` | `unload` | `load` | `reset-storage`.
- `outcome` — `ok`, `not-found`, `bad-request`, `ctor-threw`, `init-threw`, or `unknown-action`.

Grep your OTLP collector for `bowire.plugin.lifecycle` to see the audit trail. Useful when investigating why a plugin was cycled during an incident.

## Exercise — the four-verb round trip

Pick one protocol plugin (say `signalr`, since your discovered services probably don't use it) and walk every verb:

1. **Restart** it. Confirm the result banner says `Plugin 'signalr' restarted.`
2. **Unload** it. Confirm the pill greys out and the row's other buttons stay clickable.
3. **Load** it. Confirm it comes back — the pill re-colours and the disabled-list entry is dropped.
4. **Reset storage.** Refresh the page and confirm the plugin's saved UI state is gone (it renders as if freshly installed).

If any verb fails with a non-200, read the result banner. The endpoint speaks RFC 7807 Problem Details, so the `type` URI (`urn:bowire:plugin:lifecycle-*`) tells you exactly which stage failed.

## Key Takeaways

1. **Four verbs, one endpoint.** `POST /api/plugins/{id}/lifecycle/{action}` with `action ∈ {load, unload, restart, reset-storage}`. Same handler dispatches on the action name; wire-controlled tokens are lowercase.
2. **Restart is safe.** The old instance is only disposed after the new one is successfully constructed. A ctor throw leaves you with the previous working instance in the registry.
3. **Unload is persistent.** The plugin id gets written to `disabled-plugins.json` in the Bowire user-store — it stays off across process restarts until you Load it (or delete the entry manually).
4. **Load is *re-enable*, not install.** The DLL has to already be in-process. New-plugin-from-disk is still the `bowire plugin install` flow.
5. **Reset storage doesn't touch the plugin itself.** It wipes per-workspace localStorage buckets only; the running instance keeps going. Refresh the page to see the clean UI state.

## Knowledge Assessment

1. **Restart vs Unload+Load.** Both cycle the plugin instance. What's the difference?
   *Answer:* **Restart** replaces the registry slot in one atomic step — no window where the plugin is missing, no persisted disabled-list mutation. **Unload+Load** removes it, writes the id to `disabled-plugins.json`, then removes that entry and re-runs discovery. Restart is the right verb when you just want a fresh instance; Unload+Load is what happens when you were disabling the plugin and then changed your mind.

2. **Load-a-brand-new-plugin.** You built a new protocol plugin, ran `dotnet pack`, and put the .nupkg in `~/.bowire/plugins/`. You click **Load** on... which row? What actually needs to happen?
   *Answer:* **You can't Load it from the lifecycle buttons** — there's no row for a plugin that isn't in the AppDomain yet. Load *re-enables* an already-loaded assembly; brand-new plugins go through `bowire plugin install --file <path.nupkg>` (or the equivalent Settings → Configure → Protocols → *Install package* button), which drops the DLL into the plugin dir and re-runs `PluginManager.LoadPlugins`. Only *then* does a lifecycle row appear.

3. **A hung Restart.** You click Restart on a protocol plugin. The result banner comes back with `HTTP 500 — urn:bowire:plugin:lifecycle-init-failed`. What state is the registry in?
   *Answer:* The **fresh instance IS in the registry**, but `Initialize` threw. The old instance was already disposed (the endpoint disposes only after `Activator.CreateInstance` succeeded, which it did). This means the plugin is half-wired: constructed but not initialised. A follow-up **Restart** will retry the full sequence and can recover if the transient cause is gone. The `exceptionType` field on the Problem Details response tells you what threw.

4. **Reset-storage on a headless CI runner.** You're running Bowire via `bowire test` against a Flow (Lesson 5.4). Does Reset storage do anything for you here?
   *Answer:* **No.** `bowire test` is a headless CI runner — it doesn't open a browser, so there's no localStorage to reset. Reset storage is exclusively a workbench-UI action. The CI runner starts with a clean in-memory state per invocation anyway; nothing to wipe.

## What's Next

You've closed out the extension unit — plugins can now be authored (4.1 / 4.2), observed (4.3), extended in the UI (4.4), driven through their four postures (4.5), and cycled without process restart (4.6). Unit 5's Administrator lessons cover the `bowire test` CI runner and workspace-scale operations.

**Continue:** → [Unit 5 — CI · deploy · operate](../../unit-5/README.md)

## Reference

- `src/Kuestenlogik.Bowire/Endpoints/BowirePluginEndpoints.cs` — the lifecycle handler and its four action implementations (`RestartPlugin`, `UnloadPlugin`, `LoadPlugin`, `ResetPluginStorage`).
- `src/Kuestenlogik.Bowire/wwwroot/js/settings.js` — the `_renderPluginLifecycleButtons` helper mounted on every plugin row.
- `src/Kuestenlogik.Bowire/Plugins/BowireDisabledPluginsStore.cs` — how Unload persists the disabled-list to disk.
- Main-Bowire issue [#340](https://github.com/Kuestenlogik/Bowire/issues/340) — the runtime-lifecycle initiative.
