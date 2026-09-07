# Lesson 3.8: CI gates — lint, contracts, and one rollup

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Lesson 3.2](../lesson-2/README.md); a `.bwr` recording (capture one in [Unit 2](../../unit-2/lesson-1/README.md))

## Overview

Lesson 3.2 made Bowire a CI citizen with two verbs: `mock` and `test`. This lesson adds the three that turn a pipeline from *"the calls still work"* into *"the API is still fit to publish"* — a linter that reads the surface itself, contracts that pin what a consumer relies on, and one rollup that puts every report on a single page.

All three follow the same shape: read something, print a table, and exit non-zero when you ask them to. That last part is what makes them gates rather than reports.

## Lint the surface, before anyone calls it

`bowire lint` runs design rules over an API surface. It takes a snapshot file or a live URL, so it works on a schema in a pull request as readily as on a running service.

```bash
# a live service
bowire lint https://api.example.com --protocol rest

# a snapshot captured earlier, in CI
bowire lint api-snapshot.json --format markdown --output lint.md
```

The shipped rules flag a response carrying something that looks like personal data (email, phone, SSN, date of birth, address, passport or tax id), a response carrying something that looks secret, a collection endpoint with no pagination, an API with no versioning scheme, and a timestamp shipped as a bare string.

Severities are yours to set. `.bowire/rules.json` is discovered by walking **up** from the working directory, the way a linter should — so a repository configures its rules once and every checkout, and the pipeline, read the same file:

```bash
bowire lint https://api.example.com --rules .bowire/rules.json --fail-on medium
```

`--fail-on` is the gate: `none` (the default) always exits 0, and `info` / `low` / `medium` / `high` exit non-zero as soon as a finding reaches that severity. Start at `high`, and lower the bar as the findings get fixed rather than the other way round.

## Pin what a consumer relies on

A contract is the subset of a provider's behaviour that one consumer actually depends on. Bowire builds it from a recording you already have:

`--provider` names the provider the contract is against and is required; `--consumer` defaults to the recording's name.

```bash
# consumer side — build the contract from a recorded session
bowire contract publish harbor-tour.bwr --provider harbor-api --out harbor-web.pact.json

# provider side — replay it against the live provider
bowire contract verify harbor-web.pact.json --provider-url https://harbor.example.com
```

`verify` has three exit states, and the third is the one that matters in a pipeline:

| Exit | Meaning |
|------|---------|
| `0` | every interaction held |
| `1` | a mismatch — the provider changed |
| `2` | a request errored — the provider was unreachable |

A pipeline that treats 1 and 2 alike will page somebody about a broken contract when the real problem is a network. Distinguish them.

With `--broker-url`, `publish` pushes to a Pact Broker instead of writing a file, so the provider's pipeline pulls contracts rather than having them committed into its own repository.

Once several verifications are stored, `matrix` rolls them into a consumer × provider grid:

```bash
bowire contract matrix                 # text grid
bowire contract matrix --json          # for a dashboard
bowire contract matrix --fail-on-failures   # the CI gate
```

The Contracts rail renders the same grid in the workbench, with per-interaction drill-in — a red cell opens the interaction that failed rather than making you hunt for it.

## One rollup over everything

By now a pipeline produces lint findings, contract results, benchmark numbers, scan reports and test results — five shapes, five places to look. `bowire report rollup` reads them all and prints one row per service:

```bash
bowire report rollup --from ./reports --from ./lint.json
bowire report rollup --from ./reports --json      # for a dashboard
bowire report rollup --from ./reports --fail-on high
```

`--from` is repeatable and walks directories recursively. `--service` attributes every report to one service when the path cannot be inferred from — useful in a monorepo where the reports land in one folder.

The rollup has **one canonical JSON shape**, and every surface emits it: `GET /api/report/rollup` for the workbench, `bowire report rollup --json` for the pipeline, and the `bowire.report.rollup` MCP tool for an agent. One shape means the dashboard, the CI gate and the assistant are reading the same numbers rather than three approximations of them.

## Wire the three into one job

```yaml
# .github/workflows/api-gates.yml
- name: Lint the surface
  run: bowire lint ${{ env.API_URL }} --protocol rest --format json --output reports/lint.json --fail-on high

- name: Verify consumer contracts
  run: bowire contract verify contracts/harbor-web.pact.json --provider-url ${{ env.API_URL }}

- name: Roll it up
  run: bowire report rollup --from reports --fail-on high
```

Note the ordering: lint and contracts each gate on their own, and the rollup gates on the whole. A team that only wants one gate can set `--fail-on none` on the first two and let the rollup decide.

## Key Takeaways

- **`bowire lint`** reads the API surface for design smells — PII, unbounded collections, missing versioning — from a live URL or a snapshot, with severities configured once in `.bowire/rules.json`.
- **`bowire contract publish|verify|matrix`** pins what a consumer relies on; `verify` separates "the provider changed" (exit 1) from "the provider was unreachable" (exit 2).
- **`bowire report rollup`** reads every report kind into one row per service, in a JSON shape shared by the workbench, the CLI and the MCP tool.
- **`--fail-on`** is what makes all three gates rather than reports. Its default is `none`, so adopting them never breaks a pipeline on the first run.

## What's Next

[Lesson 3.6](../lesson-6/README.md) covers what happens after the gates pass — telemetry, scheduled probes, and the operations side.

## Reference

- [Contract testing](https://bowire.io/docs/features/contract-testing.html)
- [Reports and rollup](https://bowire.io/docs/features/report-rollup.html)
- Design-time lint has no feature page yet — `bowire lint --help` is the reference until it does.
