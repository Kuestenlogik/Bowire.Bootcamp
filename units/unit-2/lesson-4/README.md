# Lesson 2.4: Regression Coverage

> **Difficulty:** Beginner | **Duration:** 10 min | **Prerequisites:** [Lesson 2.3](../lesson-3/README.md)

## Overview

Once you record and assert, the next question is *what have I actually covered?* The **Regression Coverage** surface answers it per method: every discovered operation gets a small **four-state chip** so you can see, at a glance, where your recorded/tested traffic is fresh, stale, broken, or missing.

## The four states

| Chip | Meaning |
|---|---|
| **Recent** | Covered by a recording/run within the freshness window — green, you're good. |
| **Stale** | Covered once, but not recently — the coverage exists but may not reflect the current contract. |
| **Failing** | The most recent run for this method failed an assertion — needs attention. |
| **Uncovered** | No recording/run touches this method at all — a blind spot. |

Coverage is computed from the workspace's recordings + Flow runs against the discovered surface, so the chip updates as you capture and run.

## Steps

Drive the Harbor **Combined** sample (`:5101/bowire`).

### 1. See the chips in Discover

Open the sidebar. Each Harbor method carries a coverage chip. Straight after discovery — before you've recorded anything — most read **Uncovered**.

### 2. Cover a method

Record or run a Flow against one method (e.g. *list docks*). Its chip flips to **Recent**. Break an assertion on another method and run it — that one reads **Failing**.

### 3. Watch coverage decay

Coverage isn't just presence — it's freshness. A method covered long ago (outside the window) reads **Stale**, nudging you to re-capture against the current contract rather than trusting an old tape.

### 4. Work the blind spots

Filter/scan for **Uncovered** methods — that list is your regression backlog. The goal state: no `Uncovered`, no `Failing`, minimal `Stale`.

## Key Takeaways

1. **Four states per method: recent · stale · failing · uncovered.** A blind-spot map for your API surface.
2. **Coverage is freshness, not just presence.** Stale coverage is flagged distinctly from missing coverage.
3. **Uncovered = your regression backlog.** Capture/assert your way to green.

## What's Next

**Continue:** → [Lesson 2.5: Intercept rail — four postures](../lesson-5/README.md)

## Reference

- [Testing & coverage](https://bowire.io/docs/testing/)
