---
_layout: landing
---

# Bowire Bootcamp

Welcome to the Bowire Bootcamp — a hands-on tutorial that takes you from your first API call to building, mocking, AI-integrating and shipping protocol plugins for [Bowire](https://bowire.io), Küstenlogik's multi-protocol API workbench.

## Pick your deployment shape

Bowire runs in two shapes; both expose the same workbench. The bootcamp covers both side by side.

| Shape | Best for | Wire-in |
|---|---|---|
| **CLI (two-process)** | Pointing at any URL — your own service, a teammate's port-forward, a public API, CI scans, MCP agents | `bowire --url …` after `dotnet tool install --global Kuestenlogik.Bowire.Tool` |
| **Embedded (single-process)** | Building / debugging your own ASP.NET service — workbench inherits your DI, `[Authorize]` policies, logging, config | `AddBowire()` + `MapBowire()` in `Program.cs` |

[Unit 0 → Lesson 0.1](units/unit-0/lesson-1/README.md) breaks the choice down with diagrams; [Lesson 0.2](units/unit-0/lesson-2/README.md) walks both installs.

## Getting Started

Choose a [Learning Path](LEARNING_PATHS.md) based on your role:

| Path | For | Units |
|------|-----|-------|
| [Workbench Fundamentals](LEARNING_PATHS.md#1-workbench-fundamentals) | All API developers | 3 units |
| [Mock-as-Stand-In](LEARNING_PATHS.md#2-mock-as-stand-in) | Frontend devs, QA engineers | 2 units |
| [AI & Automation](LEARNING_PATHS.md#3-ai--automation) | Agent / LLM builders | 2 units |
| [Plugin Author](LEARNING_PATHS.md#4-plugin-author) | Protocol-plugin authors (.NET + polyglot) | 3 units |
| [Production / CI](LEARNING_PATHS.md#5-production--ci) | DevOps, platform engineers | 2 units |

Unit 1 splits into two parallel **setup tracks** — pick [CLI](units/unit-1-cli/README.md) or [Embedded](units/unit-1-embedded/README.md) based on your deployment shape. From Unit 2 onwards the tracks merge.

## Capstone

Once the units are done, work through the **[Multi-Protocol API Tour](capstone/README.md)** — a single end-to-end scenario that weaves recording, mocking, AI integration, and CI into one runnable project. Comes with a [sample backend](capstone/sample/HarborTour/) and a [reference solution](capstone/solution/README.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **One** of:
  - Bowire CLI (`dotnet tool install --global Kuestenlogik.Bowire.Tool`), or
  - The `Kuestenlogik.Bowire` NuGet referenced from your ASP.NET service for embedded mode (`dotnet add package Kuestenlogik.Bowire`)
- Docker (only for Units 2 + 5)
- Python 3.10+ (only for Unit 4.2)
- Claude Desktop or Cursor (only for Unit 3.1)

## Start Learning

Begin with [Unit 0: Introduction](units/unit-0/README.md) to pick your deployment shape and verify the install.
