# Administrator capstone — Production deployment stack

> **Status: scaffold (PR 1 — structure only).** Full scenario text, compose / k8s starter manifests, and reference runbook land in PR 3 per the [v2.1 + purpose-paths audit](../../audits/2026-06-30-v21-and-purpose-paths-audit.md), Section 3.

**Deliverable (per audit):** a `docker-compose.yml` (or k8s manifests) + a production
runbook covering reverse-proxy in front (Caddy / nginx / Traefik), auth
gating, observability wiring (Grafana / Prometheus / structured logs),
plugin disable list, workspace-file backup strategy, and a smoke-test
script that verifies the deployment from outside.

TODO PR-3: write the scenario brief, ship the `compose/` + `k8s/` starter manifests, ship the reference complete stack + runbook.
