# Unit 0: Introduction

*Time: ~30 minutes (CLI) · ~40 minutes (Embedded) • Lessons: 3*

Understand what Bowire is, **pick your deployment shape** (standalone CLI vs embedded in your own service), install whichever one fits, and verify the workbench renders.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) — required either way.
- A modern web browser — the workbench opens in-browser automatically, no separate client install.
- An internet connection — Lesson 0.3 points at the public Petstore reference API (CLI path only).

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [0.1](lesson-1/README.md) | What is Bowire? | Multi-protocol API workbench, the **two-process** (CLI) and **single-process** (embedded) models, when to pick which |
| [0.2](lesson-2/README.md) | Setup | Path A: install the `bowire` global tool. Path B: scaffold an ASP.NET host + `AddBowire()` / `MapBowire()`. Both paths covered side by side. |
| [0.3](lesson-3/README.md) | Hello Bowire (CLI) | Launch the workbench against a public REST API, invoke your first method. Embedded learners can skim — Unit 1's [Embedded shape](../unit-1/README.md) walks the in-process equivalent. |

## Why this unit

Before you can drive Bowire across protocols, you need a working install and a clear mental model of what it is vs the API-client tools you've used before. The two deployment shapes (CLI vs Embedded) cover meaningfully different jobs, so Unit 0 makes the choice explicit before any code gets written. If you already know the positioning and have at least one path installed, head to [Unit 1](../unit-1/README.md) — the shape is a setup tab inside each Unit 1 lesson.

---

**Next:** → [Unit 1: Workbench basics](../unit-1/README.md) — each lesson's setup section walks both the CLI shape (`bowire --url …`) and the Embedded shape (`AddBowire()` / `MapBowire()`).
