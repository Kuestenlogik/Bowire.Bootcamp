# Bowire Bootcamp Roadmap

## Current Status

| Item | Status |
|------|--------|
| 6 units (Intro, Workbench Basics, Record/Replay+Mock, AI, Plugins, CI) | scaffolded |
| 21 lessons across the 6 units | shipped — Unit 0 (3), Unit 1 (2), Unit 2 (2), Unit 3 (3), Unit 4 (6), Unit 5 (5) |
| DocFX learn-site scaffolding (`.docfx/`, `scripts/build-learn.ps1`, `toc.yml`) | shipped — minimal modern template, branding to follow |
| Capstone project — Multi-Protocol API Tour | scaffolded; needs requirements + architecture + reference solution |

## Next up

- **DocFX branding** — pull the Bowire site's colours / logo / font stack into `.docfx/templates/` so the bootcamp matches bowire.io.
- **Shepherd.js tours** — Surgewave-style guided walkthroughs of the workbench for the Unit 1 lessons. Lower priority — the screenshots in the lesson READMEs already cover the click path.
- **`start/` + `completed/` code skeletons** — currently the plugin lessons (Unit 4) reference scaffolded code generated from `dotnet new bowire-plugin`. Future revision: pre-package both `start/` (scaffolded) and `completed/` (with the lesson's edit applied) so a learner can `cd start/` and diff against `completed/` without running the scaffold themselves.
- **Capstone reference solution** — implement the full multi-protocol scenario end-to-end, ship under `capstone/solution/`, and link it from the capstone README.
- **GitHub Actions CI for the bootcamp** — build the DocFX site, lint markdown, prove the lesson samples (HelloApi, HelloGrpc, sample/.bwr files) still compile / load cleanly.

## Recently shipped

- **v2.2 feature lessons** — six new lessons anchored on the v2.2 main-Bowire initiatives: Intercept-rail postures (4.5), Flow assertions (3.2), `bowire test` Flow-runner in CI (5.4), Regression coverage (3.3), Workspace deletion mode (5.5), Plugin lifecycle (4.6).
- The eight original flat `lesson-NN-name/` directories migrated to the `units/unit-N/lesson-M/` hierarchy modelled on Surgewave Bootcamp.
- Toplevel index/landing files: `index.md`, `LEARNING_PATHS.md`, `units-overview.md`, this file, `toc.yml`.
- Per-lesson README structure standardised on a Difficulty / Duration / Prerequisites header, Overview, Concept tables, Key Takeaways, Next Steps.
