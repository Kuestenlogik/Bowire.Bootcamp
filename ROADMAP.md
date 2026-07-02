# Bowire Bootcamp Roadmap

## Current Status

| Item | Status |
|------|--------|
| 6 single-modality units (Foundations · Workbench first-contact · Workbench record/mock/assert/cover · CLI & operations · Embed · Extend) | shipped |
| 25 lessons across the 6 units | Unit 0 (4) · Unit 1 (2) · Unit 2 (5) · Unit 3 (7) · Unit 4 (3) · Unit 5 (4) |
| Role-first courses (curation over the units) | shipped — User (0→1→2) · Integrator/DevOps/Admin (0→3) · Developer (0→1→4→5) + optional QA |
| Three per-audience capstones | requirements + reference solution present; refresh to extend the Harbor domain ongoing |
| DocFX learn-site scaffolding (`.docfx/`, `scripts/build-learn.ps1`, `toc.yml`) | shipped |

## Next up

- **Setup onboarding** — decide where the "get Bowire running" step lives so every course (esp. the UI operator course) starts against a running workbench (see the role-first IA plan).
- **`start/` + `completed/` code skeletons** — the convention is adopted; fill in real scaffolds for the coding lessons (Units 4–5) and the capstones so a learner can `cd start/` and diff against `completed/`.
- **Capstone refresh** — rewrite the three capstone scenarios to *extend the Harbor Control Center domain* (from `Bowire.Samples`), and align each to the `start/` + `completed/` layout.
- **DocFX branding** — pull the Bowire site's colours / logo / font stack into `.docfx/templates/`.
- **GitHub Actions CI** — build the DocFX site, lint markdown, and prove the referenced `Bowire.Samples` demos + any `start/`+`completed/` projects still compile / load cleanly.

## Recently shipped

- **Role-first IA restructure** — reorganised into six **single-modality units** (one modality each: UI / CLI / embedded coding / extension coding; no mode-switching within a unit, cross-modality via links). Courses became pure curation over numbered, reusable units. Landing / `LEARNING_PATHS` / `units-overview` / `README` rewritten to the course model; the "Pick your deployment shape" path-level framing retired. Plan: `audits/2026-07-02-role-first-ia-restructure-plan.md`.
- **Samples sourced from sibling repos** — retired the toy `unit-1-samples/` (HelloApi, HelloGrpc); units now drive [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) (Harbor demo + per-plugin `protocols/`) and study [`Bowire.Protocol.Akka`](https://github.com/Kuestenlogik/Bowire.Protocol.Akka) as the real custom-protocol example.
- **v2.2 feature lessons** — Intercept-rail postures, Flow assertions, `bowire test` Flow-runner in CI, Regression coverage, Workspace deletion, Plugin lifecycle (redistributed into the new modality units).
- Per-lesson README structure standardised on a Difficulty / Duration / Prerequisites header, Overview, Steps, Key Takeaways, Next Steps.
