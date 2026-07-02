# Lesson 0.3: How this bootcamp works

> **Difficulty:** Beginner | **Duration:** 7 min | **Prerequisites:** [Lesson 0.2](../lesson-2/README.md)

## Overview

The bootcamp is organised so you learn only what your role needs, in one modality at a time. Three levels:

```
Course / track   (your role — a curated list of units; not a folder)
 └─ Unit          (one modality: UI, CLI, embedded coding, or extension coding)
     └─ Lesson    (one skill; ships a start/ scaffold + a completed/ reference)
         └─ Steps (the walkthrough inside the lesson README)
```

## Courses pick units — units don't pick you

A **course** is a role-oriented, freely-composed selection of units. It's pure curation (it lives in [`LEARNING_PATHS.md`](../../LEARNING_PATHS.md), not a folder), so the **same unit is reused across courses** and a course can take units in whatever order fits.

| Course | Units (typical) |
|---|---|
| **Workbench / API operator** | 0 → 1 → 2 |
| **Integrator / DevOps / Admin** | 0 → 3 (→ 1 to inspect) |
| **Developer (embed & extend)** | 0 → 1 → 4 → 5 |

## One unit = one modality

Each unit stays in a single modality and **never makes you switch mid-unit** between UI, CLI, and coding:

| Unit | Modality |
|---|---|
| [Unit 0: Foundations](../../unit-0/README.md) | none (concepts) |
| [Unit 1: The Workbench — first contact](../../unit-1/README.md) | UI |
| [Unit 2: The Workbench — record, mock, assert, cover](../../unit-2/README.md) | UI |
| [Unit 3: CLI & operations](../../unit-3/README.md) | CLI |
| [Unit 4: Embed Bowire](../../unit-4/README.md) | embedded coding |
| [Unit 5: Extend Bowire](../../unit-5/README.md) | extension coding |

When another modality is genuinely relevant (e.g. a UI recording lesson mentioning the scriptable `bowire mock`), the unit **links** to the sibling unit instead of opening a second track inline.

## start/ and completed/

Every hands-on lesson ships two folders:

- **`start/`** — a prepared scaffold to build on, so you don't begin from zero and the skeleton already fits the task.
- **`completed/`** — the reference/finished state, so you can diff your work against "milestone achieved".

Capstones (per audience: [user](../../../capstones/user/README.md) · [developer](../../../capstones/developer/README.md) · [administrator](../../../capstones/administrator/README.md)) use the same `start/` + `completed/` shape.

## Where setup lives

There is **no install step in Unit 0**. Setup is part of the first unit of your course:

- CLI install (`dotnet tool install --global Kuestenlogik.Bowire.Tool`) → [Unit 3](../../unit-3/README.md).
- Embedded wire-in (`AddBowire()` / `MapBowire()`) → [Unit 4](../../unit-4/README.md).
- The browser workbench itself needs no separate install — it opens in-browser once either shape is running ([Unit 1](../../unit-1/README.md)).

## Key Takeaways

1. **Course → Unit → Lesson → Steps.** Courses are curation; units and lessons are folders.
2. **Pick a course, follow its units.** Each unit is single-modality; cross-modality is a link.
3. **`start/` to build on, `completed/` to check against.**

## What's Next

Head to the first unit of your course — see [Learning Paths](../../LEARNING_PATHS.md), or jump straight in:

- Operator → [Unit 1: The Workbench](../../unit-1/README.md)
- Admin / DevOps → [Unit 3: CLI & operations](../../unit-3/README.md)
- Developer → [Unit 4: Embed Bowire](../../unit-4/README.md)
