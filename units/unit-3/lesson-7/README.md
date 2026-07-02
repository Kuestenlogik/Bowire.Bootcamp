# Lesson 3.7: Workspace hygiene — soft vs hard deletion

> **Difficulty:** Beginner | **Duration:** 8 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md)

## Overview

Workspaces accumulate — per-project recordings, environments, collections, flows. Bowire gives deletion **two postures** so an operator can clean up without fear of losing the wrong thing.

## The two postures

| Posture | What happens | Recoverable? |
|---|---|---|
| **Soft delete** | The workspace (and its scoped state) moves to **Trash**; it disappears from the picker but stays on disk for the retention window. | Yes — restore from Trash, or **Undo** right after. |
| **Hard delete** | A **cascade purge** — the workspace and every artefact scoped to it (recordings, environments, collections, flows, plugin installs) are removed for good. | No. |

Soft is the default, reversible path; hard is the deliberate, irreversible one. The Action Log records the deletion so `Ctrl/Cmd+Z` (and the Activity drawer) can surface the inverse even after the toast's Undo affordance expires.

## Operating it

- **From the workbench:** Settings → Workspace exposes the Soft/Hard toggle, the Trash list, and restore.
- **From disk:** soft-deleted workspaces linger under the Trash area of `~/.bowire/workspaces/`; a hard delete removes the `<id>/` tree entirely. Back up (see [Lesson 3.6](../lesson-6/README.md)) before a hard delete you're unsure about.

## Key Takeaways

1. **Soft delete → Trash (recoverable, with Undo); hard delete → cascade purge (permanent).**
2. **Deletions are logged** — undoable via `Ctrl/Cmd+Z` and the Activity drawer, not just the toast.
3. **Back up before a hard delete** — the cascade takes every scoped artefact with it.

## What's Next

That closes the CLI & operations unit. Where you head next depends on your course — embedding (Unit 4) or extending (Unit 5). See [Learning Paths](../../LEARNING_PATHS.md).

## Reference

- [Workspaces](https://bowire.io/docs/features/) · [Action log / undo](https://bowire.io/docs/)
