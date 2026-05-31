# Unit 2: Record, Replay, Mock

*Time: ~20 minutes • Lessons: 2 • Prerequisites: [Unit 1](../unit-1/README.md)*

Capture a session against a real backend, replay it as a local mock server, and use the original schema sidecar to give consumers the *full* contract — not just the slice you happened to record.

## Lessons

| Lesson | Topic | What You'll Build |
|--------|-------|-------------------|
| [2.1](lesson-1/README.md) | Record & replay | A `.bwr` recording, replayed through `bowire mock` so the mock answers without the real backend running |
| [2.2](lesson-2/README.md) | Schema export + mock-as-stand-in | The recording carries the source OpenAPI, the mock serves `/openapi.json`, a peer Bowire discovers the *full* surface through it |

## Why this unit

A mock is most useful when consumers can see two things at once: the **full contract** the original service advertises, and the **slice** the recording can actually replay. Unit 2 walks through both — first the straight capture-and-replay arc (2.1), then the mock-as-stand-in story where the recording carries the OpenAPI doc and the mock re-emits it for peer discovery (2.2).

By the end, you'll have a self-contained mock server that *replaces* a real backend for everyone who depends on it — frontend dev environments, CI fixtures, demo stacks, &c.

---

**Next:** → [Unit 3: AI-Agent Integration](../unit-3/README.md)
