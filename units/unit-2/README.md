# Unit 2: The Workbench — record, mock, assert, cover

*Time: ~55 minutes • Lessons: 5 • Modality: UI (workbench)*

The testing-oriented half of the workbench, all from the browser UI: capture traffic, replay it as a mock, express what "correct" means with Flow assertions, read regression coverage per method, and inspect intercepted traffic. Still **UI-only** — where a scriptable equivalent exists (`bowire mock` / `bowire test`, the reverse-proxy) this unit *links* to the CLI (Unit 3) or embedded (Unit 4) unit rather than switching modality inline.

From here on we drive the **Harbor Control Center** domain (`Ship`, `Dock`, `Crane`, `Container`, `PortCall`) via the Combined sample from [`Bowire.Samples`](https://github.com/Kuestenlogik/Bowire.Samples) — `:5101/bowire`, one host, every protocol.

## Lessons

| Lesson | Topic | What You'll Learn |
|--------|-------|-------------------|
| [2.1](lesson-1/README.md) | Record & Replay | Capture calls, export a `.bwr`, replay it as a mock from the Mocks rail |
| [2.2](lesson-2/README.md) | Schema-backed mocks | The mock re-emits the source contract; peer-discovery + coverage gaps |
| [2.3](lesson-3/README.md) | Flow Assertions | The five-kind `(kind, target, operator, expected)` expectation vocabulary |
| [2.4](lesson-4/README.md) | Regression Coverage | The four-state per-method chip: recent · stale · failing · uncovered |
| [2.5](lesson-5/README.md) | Intercept rail — four postures | Captured · Live overrides · Mock servers · Settings — when to reach for which |

## Why this unit

Discovering and invoking (Unit 1) is the "does it move" loop. This unit is the "does it move *correctly*, and can I stand in for it" loop — the surface every operator and QA role leans on.

---

**Next:** → [Lesson 2.1: Record & Replay](lesson-1/README.md)
