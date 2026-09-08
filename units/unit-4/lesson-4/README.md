# Lesson 4.4: SCIM provisioning

> **Difficulty:** Advanced | **Duration:** 20 min | **Prerequisites:** [Lesson 4.1](../lesson-1/README.md); an identity provider you can configure (Entra ID, Okta, Keycloak)

## Overview

Authentication answers *who is calling*. Provisioning answers *who exists* — and, just as importantly, who stopped existing. SCIM 2.0 is how a directory tells Bowire that a new colleague joined and that a departing one should lose their slot without anyone filing a ticket.

This is an **embedded** capability: there is no `bowire scim` command. A host mounts the endpoints, which is why it belongs in this unit rather than Unit 3.

## Turning it on

```jsonc
{
  "Bowire": {
    "MultiTenant": { "Enabled": true },
    "Scim": {
      "Enabled": true,
      "Token": "…a long random secret…"
    }
  }
}
```

Two things are worth pausing on.

**`MultiTenant` comes first.** Provisioning identities into an install where everyone shares one `~/.bowire/` would file every colleague into the same drawer. Per-identity storage is what gives a provisioned user something to be provisioned *into*.

**Enabling without a token is refused at startup**, not served open. A provisioning endpoint reachable by anyone who can route to the host is a way to create identities, so Bowire declines to start rather than start unsafely.

The endpoints mount at `/scim/v2` — deliberately **outside** the workbench's own route group. Those routes are gated by whatever auth provider you configured, and a connector holds a shared secret rather than a user session; it could never pass that gate. SCIM authenticates itself, with its own token, on its own path.

## The settings worth knowing

| Key | Default | |
|---|---|---|
| `Bowire:Scim:Enabled` | `false` | Mounts the endpoints. |
| `Bowire:Scim:Token` | — | The bearer token the IdP presents. Required. |
| `Bowire:Scim:BasePath` | `/scim/v2` | Where they mount. |
| `Bowire:Scim:PurgeAfter` | `30.00:00:00` | How long a deprovisioned identity's state is kept. |
| `Bowire:Scim:EnforceActive` | `true` | Refuse a deactivated identity at the door. |
| `Bowire:Scim:RequireProvisioned` | `false` | Refuse an identity the directory has never heard of. |
| `Bowire:Scim:AdminGroup` | `bowire-admins` | The group whose members count as administrators. |

`RequireProvisioned` is the switch that turns the directory into the *only* way in. Left at `false`, anyone your auth provider accepts gets a slot; set to `true`, an identity the directory never sent is refused even with a valid token.

## What is implemented — and what is not

Said plainly, because a connector that is told something works and then gets a 404 retries the whole sync instead of falling back. `/ServiceProviderConfig` advertises exactly this list:

| | |
|---|---|
| **Users** | `GET` (list + by id), `POST`, `PUT`, `PATCH`, `DELETE` |
| **Groups** | `GET` (list + by id), `POST`, `PUT`, `PATCH`, `DELETE` |
| **Discovery** | `/ServiceProviderConfig`, `/ResourceTypes`, `/Schemas` |
| **Filtering** | `eq` and `pr`, joined with `and` / `or` |
| **Paging** | `startIndex` (1-based) and `count` |
| **Not implemented** | Bulk, sorting, ETags, password change |

The filter subset is deliberate. The full grammar has ten operators, complex attribute paths and value sub-filters; the connectors that matter send one shape between them — `userName eq "someone@example.com"`. Anything outside the subset is refused with `400` and `invalidFilter` rather than half-evaluated: a parser that ignores the part it did not understand answers a different question, and the caller cannot tell.

Attributes Bowire does not model — the Enterprise User extension, whatever your directory maps — are stored verbatim and returned on the next `GET`. A connector that reads back a resource missing what it just wrote concludes the write failed, and retries forever.

## When somebody leaves

Deprovisioning does not delete immediately. The identity's slot is archived and kept for `PurgeAfter` — 30 days by default — after which a daily sweep removes it for good. Set it to `00:00:00` to delete immediately if your retention policy says so.

That window is the difference between "they left the team" and "they left, and their recorded sessions went with them the same afternoon".

## Watching a round-trip

`scim/events.jsonl` records mutations and their outcome. What it does *not* record is the connector's **reads** — the paging walk, the existence filter — and those are most of what a live round-trip actually consists of.

`Bowire:Scim:TraceProvisioning` (off by default) writes one line per SCIM request: method, path and query, status, duration, and for `PATCH` the dialect read off the wire. Turn it on while wiring a connector up; turn it off afterwards. The lines carry what the connector sent — user names, e-mail addresses, the filters a directory walk used.

## Key Takeaways

1. **SCIM is embedded-only** — a host mounts it; there is no CLI verb.
2. **`MultiTenant` first, then `Scim`** — provisioning without per-identity storage files everyone into one drawer.
3. **No token, no start.** Enabling without one is refused rather than served open.
4. **`/scim/v2` sits outside the workbench's auth gate** on purpose: a connector holds a secret, not a session.
5. **The implemented surface is advertised honestly** via `/ServiceProviderConfig`, and an unsupported filter is refused rather than half-evaluated.
6. **`TraceProvisioning`** is the debugging switch for a live connector — and one you turn back off.

## What's Next

**Continue:** → [Unit 5: Extend Bowire](../../unit-5/README.md)

## Reference

- [SCIM provisioning](https://bowire.io/docs/setup/scim.html)
- [Multi-user setup](https://bowire.io/docs/setup/multi-user.html)
