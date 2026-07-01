# Lesson 5.5: Workspace deletion — Soft vs Hard, and how Undo works

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Unit 0](../../unit-0/README.md), at least two workspaces created (so you can delete one without emptying the workbench)

## Overview

Deleting a workspace used to be a single-verb affair — the whole thing vanished, and only a short-lived toast let you undo. v2.2 splits the delete verb into two postures and adds a first-class **Trash** with a configurable retention window:

- **Soft delete** — the workspace moves to Trash. It's out of the sidebar, out of the picker, but recoverable within the retention window (7 / 14 / 30 days or `never`). This is the default.
- **Hard delete** — the workspace is removed immediately. Undo still works briefly (via the action-log side-channel), but the entry never lands in Trash.

The choice is a workspace-independent setting (Settings → Data → *Workspace deletion*). It applies to every workspace in this install / browser. Same for the retention window.

## Concepts

Two localStorage keys drive the behaviour:

| Key | Values | Default | Effect |
|---|---|---|---|
| `bowire_workspace_delete_mode` | `soft` \| `hard` | `soft` | Which branch of `deleteWorkspace()` runs |
| `bowire_trash_retention_days` | `7` \| `14` \| `30` \| `never` | `30` | How long soft-deleted entries stay in Trash before auto-purge |

Both keys live at the install / browser level, not per-workspace. Rationale: the setting is a property of *the operator's habit*, not of any one workspace. If you prefer Soft, you prefer it everywhere; if you prefer Hard, ditto.

Retention only matters when mode is `soft`. In Hard mode, the retention field greys out — the setting is still readable, just inert.

## Steps

### 1. Configure the deletion mode

Open the workbench. Gear icon → **Settings → Data**. Scroll to **Data Management**. The first two rows are:

- **Workspace deletion** — dropdown with two options:
  - *Soft — move to Trash, recover within retention period* (default)
  - *Hard — delete immediately, no recovery via Trash*
- **Trash retention** — dropdown with 7 / 14 / 30 days or *Never auto-purge* (default 30 days). Ignored in Hard mode.

Leave it on Soft for now.

### 2. Soft-delete a workspace

Right-click a workspace in the sidebar (or open its `⋯` menu) → **Delete**. A confirm dialog appears:

- **Title:** *Delete workspace*
- **Body:** the usual "are you sure" copy
- **Confirm button:** *Delete*

Confirm. The workspace disappears from the sidebar. A toast appears at the bottom: `Workspace '…' moved to Trash. [Undo]`.

The **[Undo]** button in the toast restores the workspace immediately. Undo is also wired into the action-log (Ctrl-Z / Cmd-Z from the workbench).

### 3. Recover from Trash

Even after the toast disappears, the workspace is in Trash. To recover:

1. Open the workspace picker (sidebar → workspace strip at the top).
2. There's a **Trash** section (only visible when Trash is non-empty).
3. Click the workspace name → *Restore*.
4. The workspace is back in the sidebar with the same id, same content, same everything.

Trash entries auto-purge after the retention window. Set retention to *Never* if you want the belt-and-braces where nothing is ever really gone; set it to 7 days to keep Trash tidy.

### 4. Switch to Hard mode and feel the difference

Go back to Settings → Data → *Workspace deletion*. Change to **Hard**.

Right-click a workspace → **Delete**. The confirm dialog is different now:

- **Title:** *Hard-delete workspace* (renamed to signal the posture)
- **Body:** *This workspace will be deleted IMMEDIATELY. Undo will work for the next ~200 actions, but it won't be in the Trash. Continue?*
- **Confirm button:** *Delete*

Confirm. The workspace disappears. The Undo toast still appears — the action-log carries a snapshot of the workspace's state — but **there's no Trash entry**. If you dismiss the toast without clicking Undo *and* fire more than ~200 subsequent actions (the action-log's ring buffer), the workspace is gone for good.

### 5. Understand the Undo mechanism

Two separate Undo layers, both wired for delete:

- **Trash** — long-lived, disk-persisted (per-browser), scoped to soft delete. This is the *durable* recovery path. Retention window applies.
- **Action-log** — in-memory, short-lived, ring-buffered at ~200 entries, scoped to both modes. This is the *ephemeral* recovery path. Every action in the workbench pushes to it (edit, delete, create, ...); older entries are evicted.

Soft delete gets both. Hard delete gets only the action-log. The action-log snapshot for a hard-delete carries the full workspace data inline (because there's no Trash entry to look it up from later); soft-delete's action-log entry can point at the Trash entry by id.

### 6. The cascade — what actually gets purged

`deleteWorkspace(id)` in prologue.js clears every localStorage key namespaced with the workspace id — the request history, the environment overrides, the tour-completion state, the reverse-proxy settings, the flow definitions, the coverage ring buffer (Lesson 3.3), &c. Soft mode snapshots *all* of that into the Trash entry before wiping; Hard mode snapshots it into the action-log entry.

Restoring from either path re-inflates every one of those buckets. No half-restored state; no orphaned per-workspace keys hanging around from a workspace you already deleted.

## Exercise — the four-corner walk

Prove all four combinations behave as expected:

1. **Soft + Undo via toast.** Delete a workspace, click *Undo* in the toast. Confirm the workspace is back with identical content (fire a couple of requests before deletion, confirm history is intact after restore).
2. **Soft + Undo via Trash.** Delete a workspace, dismiss the toast, wait 30 seconds, restore it from the Trash section of the workspace picker. Same integrity check.
3. **Hard + Undo via toast.** Switch to Hard mode. Delete a workspace, click *Undo*. Confirm it's back. Confirm the workspace picker's Trash section is empty (Hard never touched Trash).
4. **Hard + no Undo.** Switch to Hard. Delete a workspace, dismiss the toast, do a dozen other things in the workbench (edit history, create environments, save flows). Now try Ctrl-Z. Eventually the action-log evicts the deletion entry and it becomes unrecoverable. This is the failure mode Hard is *supposed* to have — pick Hard on purpose, knowing the trade-off.

## Key Takeaways

1. **Two postures, one setting.** *Workspace deletion* in Settings → Data flips between Soft (Trash-backed) and Hard (immediate). Default is Soft.
2. **Trash retention is a Soft-only concept.** In Hard mode the retention setting is inert (no Trash entries to age out).
3. **Undo has two layers.** Trash is durable + Soft-only; action-log is ephemeral + covers both. Soft-delete gets both; Hard-delete gets only the action-log.
4. **Delete cascades across every per-workspace bucket.** History, environments, flows, coverage — all wiped together, all restored together. No orphaned data.
5. **Hard is a deliberate choice.** It's here for operators who explicitly *don't want* deleted workspaces lingering (shared laptop, compliance retention limits, keeping the Trash tidy without manual purge). Pick it knowing the recovery window shortens dramatically.

## Knowledge Assessment

1. **When to pick Hard.** You share a laptop with a colleague. Both of you use the same browser profile. Your compliance policy says deleted work-in-progress must not be recoverable after 30 days. Which mode + which retention?
   *Answer:* **Soft mode + 30-day retention** is fine — the auto-purge sweep enforces the 30-day deadline for you. **Hard mode** would remove immediately, which is *safer* if compliance says "not recoverable at all" rather than "not recoverable after 30 days". Read your policy carefully. Both modes are compliant with a "30-day maximum retention" rule; only Hard is compliant with "no lingering deleted state".

2. **Trash retention set to Never.** You leave the retention on `never auto-purge` and soft-delete workspaces regularly. What eventually breaks?
   *Answer:* **Nothing "breaks" — but the Trash grows unbounded** and eats browser localStorage quota. Bowire's localStorage caches (history, flow definitions, coverage ring buffers) all share the browser's per-origin cap (typically 5–10 MB). When the cap fills, new writes silently fail; the workbench keeps running but state stops persisting. Retention set to Never should be paired with a habit of manually purging Trash from the workspace picker.

3. **Cascade integrity.** You soft-delete a workspace, restore it a week later. Which of the following comes back: (a) the workspace's call history, (b) its saved environments, (c) its saved Flows, (d) its coverage ring buffer (Lesson 3.3)?
   *Answer:* **All four.** The cascade is symmetric: everything the delete cleared, the restore re-inflates from the Trash snapshot. This is why Trash entries can be non-trivially large — a working workspace's per-namespace localStorage footprint (history + flows + coverage) all rides along with it. Restore is transactional at the workspace level.

4. **The action-log's 200-entry limit.** You're in Hard mode. You delete a workspace. You do NOT click Undo. Then you edit request bodies, save new flows, and fire ~250 requests. Now you Ctrl-Z. What happens?
   *Answer:* **Undo either restores the *last* action you took (something recent, not the delete), or, if you Ctrl-Z past the action-log's ~200-entry ring buffer, the deletion entry is gone — the workspace is unrecoverable.** Hard mode is *supposed* to fail closed like this. If you knew you wanted the deletion reversible, you should have been in Soft mode. Undo-in-Hard-mode is a courtesy window, not a guarantee.

## What's Next

You've closed the CI + deploy + operate unit. The Administrator capstone weaves CI (`bowire test`), the two deployment shapes, observability, and workspace hygiene into one end-to-end runbook.

**Continue:** → [Administrator capstone](../../../capstones/administrator/README.md)

## Reference

- `src/Kuestenlogik.Bowire/wwwroot/js/settings.js` — `renderSettingsData` mounts the two dropdowns (`DELETE_MODE_KEY` / `TRASH_RETENTION_KEY`).
- `src/Kuestenlogik.Bowire/wwwroot/js/prologue.js` — `getWorkspaceDeleteMode`, `deleteWorkspace`, `_lastWorkspaceDeleteSnapshot` (the action-log side-channel).
- `src/Kuestenlogik.Bowire/wwwroot/js/render-sidebar.js` — the delete-confirm dialog that renames itself to *Hard-delete workspace* in Hard mode.
- Main-Bowire issue [#337](https://github.com/Kuestenlogik/Bowire/issues/337) — the workspace-deletion posture split.
