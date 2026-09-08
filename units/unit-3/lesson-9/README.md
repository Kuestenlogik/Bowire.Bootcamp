# Lesson 3.9: Schema watch — what moved while you were away

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md)

## Overview

Lesson 3.8 gated on the API's *design*. This one gates on its *change*: `bowire diff` compares two snapshots of a surface and reports what was added, removed, or changed shape. It is the schema half of the PR bot, and it needs no access to the provider's source — only two discoveries of it.

## Take a snapshot

A snapshot is the discovered service list written to a file:

```bash
bowire diff snapshot https://api.example.com --protocol rest -o base.json
```

Commit that file, or keep it as a CI artefact from the last green build. It is the "before" side.

## Diff two sides

`--base` and `--head` each take a snapshot file **or** a live URL, so you can compare a file against production, two files against each other, or two environments directly:

```bash
# what changed since the committed baseline
bowire diff --base base.json --head https://api.example.com --protocol rest

# staging against production, no files at all
bowire diff --base https://staging.example.com --head https://api.example.com --protocol rest --format markdown
```

A real run, with one method removed on the head side:

```console
$ bowire diff --base base.json --head head.json --format markdown
**API schema:** -1 method.

**Removed methods**
- `Users` `GET /api/users/{id}`
```

`--format markdown` is the one to pipe into a pull-request comment; `json` (the default) is the one to parse.

## Gate on it

```bash
bowire diff --base base.json --head https://api.example.com --fail-on breaking
```

| `--fail-on` | Exit non-zero when |
|-------------|--------------------|
| `none` (default) | never |
| `breaking` | a service or method was removed, or a signature changed |
| `any` | any change at all to the callable surface |

`breaking` is the useful default for a pipeline: adding an endpoint should not fail a build, removing one should. The removal above exits **1** under `breaking`.

## Wire it into a pull request

```yaml
# .github/workflows/api-diff.yml
- name: Snapshot the base branch's API
  run: bowire diff snapshot ${{ env.BASE_URL }} --protocol rest -o base.json

- name: Diff the PR's API against it
  run: bowire diff --base base.json --head ${{ env.PR_URL }} --protocol rest
       --format markdown --output diff.md --fail-on breaking

- name: Comment the diff
  if: always()
  run: gh pr comment ${{ github.event.number }} --body-file diff.md
```

Note `if: always()` on the comment step: the diff step exits non-zero on a breaking change, and the comment explaining *why* is exactly what you want posted in that case.

## A gRPC server without reflection

Discovery needs a schema. A gRPC server with Server Reflection switched off — the state Bowire's own scanner recommends — has none to offer over the wire, so hand it a compiled descriptor set:

```bash
protoc --descriptor_set_out=api.protoset --include_imports api.proto
bowire diff snapshot https://api.example.com --protocol grpc --grpc-descriptor-set api.protoset -o base.json
```

The flag is ignored by every other protocol, so it is harmless in a shared script.

## Key Takeaways

- **`bowire diff snapshot <url> -o <file>`** writes the "before" side; commit it or keep it as a CI artefact.
- **`bowire diff --base … --head …`** accepts a snapshot file or a live URL on either side, so file-vs-live and env-vs-env both work without a temporary file.
- **`--fail-on breaking`** fails on removals and signature changes but not on additions, which is the shape a pull-request gate wants.
- **`--format markdown`** produces the comment body; `json` is for a parser.
- A reflection-less gRPC server is diffable with `--grpc-descriptor-set`.

## What's Next

**Continue:** → [Lesson 3.8: CI gates](../lesson-8/README.md) runs alongside this one — design rules, contracts and the rollup.

## Reference

- [Service compare](https://bowire.io/docs/features/service-compare.html)
- [PR bot setup](https://bowire.io/docs/setup/pr-bot.html)
