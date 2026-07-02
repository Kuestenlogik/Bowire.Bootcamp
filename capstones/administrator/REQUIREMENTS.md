# Administrator capstone — Requirements

> **Difficulty:** Intermediate · **Time:** ~3–4 hours · **Audience:** Administrator path graduates (Units 0–5).

## Software prerequisites

| Tool | Min version | Reason |
| --- | --- | --- |
| Docker Engine + Docker Compose v2 | 24+ | The recommended track. `docker compose up` brings up the five-service stack. |
| (Optional) `kubectl` + a cluster | k8s 1.28+ | If you take the Kubernetes track. Any conformant cluster — kind / k3d / Talos / EKS / AKS / GKE / OpenShift. |
| `curl`, `openssl`, `jq`, `bash` | current | The smoke-test script uses these. Already on every Linux box and modern macOS. |
| A registered OIDC client | — | At your team's IdP (Azure AD, Auth0, Keycloak, Okta — anything OIDC-compliant). Confidential client, redirect URI `https://<host>/signin-oidc`. |
| DNS for your host name | — | Or `127.0.0.1 bowire.localhost` in `/etc/hosts` for laptop iteration. |

No .NET SDK needed on the deploy host — the Bowire Tool ships as a published container image (`kuestenlogik/bowire:<version>`).

## Knowledge prerequisites

You should be comfortable with every concept introduced in:

- **Unit 0** — Workbench shape, `~/.bowire/` data dir, `Bowire:*` config keys.
- **Unit 3** — the CLI & operations surface: install, `bowire test` in CI, deployment patterns, observability, workspace hygiene ("CI is always the CLI shape").

…plus operator-side basics that this capstone assumes you have:

- Docker / Compose: writing a service block, networks, named volumes, log drivers.
- Reverse-proxy basics: TLS termination, upstream proxy_pass / handle, automatic Let's Encrypt.
- OIDC: the authorization-code flow, client id / secret, redirect URI, scopes, claims.
- OpenTelemetry: OTLP, the `OTEL_*` env vars, traces vs metrics vs logs.

## Knobs / surfaces the capstone touches (verified against main Bowire)

| Knob | Where it's defined | Effect |
| --- | --- | --- |
| `Bowire:Auth:ProviderId` | `BowireConfiguration.cs::s_switchMappings` (`--auth-provider`) + `BrowserUiHost.cs::AddBowireAuth` | Names which `IBowireAuthProvider` plugin gates the workbench. `oidc` selects `Kuestenlogik.Bowire.Auth.Oidc::OidcAuthProvider`. |
| `Bowire:Auth:Oidc:Authority`, `:ClientId`, `:RequiredClaim:*` | `OidcAuthProvider.cs` (XML doc on `OidcAuthProvider`) | The OIDC client config. `RequiredClaim` is a dictionary; each entry becomes a `RequireClaim(type, value)` on the default policy. |
| `Bowire:Telemetry:Enabled` | `BowireTelemetryServiceCollectionExtensions.cs` (`AddBowireTelemetry` binds `Bowire:Telemetry`) | Switches on the OTel pipeline. Off by default. |
| `Bowire:Telemetry:StripMethodLabels` | same file | Drops per-service / per-method dimensions from the OTLP export. Recommended for shared deploys (compliance posture). |
| `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL`, `OTEL_SERVICE_NAME` | OTel SDK | Standard OTel envs — Bowire doesn't override them. |
| `Bowire:DisabledPlugins` | `BrowserUiOptions.cs::DisabledPlugins` (bound from `Bowire:DisabledPlugins`) | List of plugin ids to skip at startup. Merges with `--disable-plugin` CLI flags. |
| `Bowire:PluginDir` | `BowireConfiguration.cs::PluginDir` (also `--plugin-dir`, also `BOWIRE_PLUGIN_DIR`) | Override the default `~/.bowire/plugins/`. |
| Container image | `Kuestenlogik.Bowire.Tool.csproj::ContainerRepository = kuestenlogik/bowire` | Published image. Pin a version tag, not `latest`. |
| Container port | `Kuestenlogik.Bowire.Tool.csproj::ContainerPort = 5080` | The only exposed TCP port. Bind a reverse proxy in front. |
| Workspace state | `~/.bowire/` (per-user data dir; per `PluginManager.cs::Environment.GetFolderPath(SpecialFolder.UserProfile)`) | Mount a named volume here. Back up nightly via `bowire workspace export`. |
| `bowire workspace export <dir> <out.bww>` | `WorkspaceCommand.cs::RunExportAsync` | Writes the canonical v2 `.bww` envelope. Round-trips through `bowire workspace import`. |

## What you deliver

| Artefact | Path in your fork | Format |
| --- | --- | --- |
| Compose file | `capstones/administrator/compose/docker-compose.yml` | Compose v3+. |
| (Optional) k8s manifests | `capstones/administrator/k8s/*.yaml` | Plain `apiVersion`/`kind` YAML. |
| Runbook | `capstones/administrator/solution/RUNBOOK.md` | Plain markdown, five sections (day-1 + four day-2 procedures). |
| Smoke test | `capstones/administrator/solution/smoke-test.sh` | Bash, exit-code based, eight checks. |

## Grading checklist

### Stack (the compose / k8s files)

- [ ] **Five services up clean.** Bowire, Caddy (or nginx / Traefik), Grafana, Tempo, Loki. The exact image tags pinned, no `latest`.
- [ ] **Caddy fronts Bowire.** `https://<host>/` reaches the workbench through the proxy; the Tool's `:5080` is not directly exposed.
- [ ] **TLS chain is valid.** Caddy serves a public-CA chain (Let's Encrypt) in production, or its internal-CA chain for laptop iteration.
- [ ] **OIDC env vars set.** `Bowire:Auth:ProviderId=oidc` + `Bowire:Auth:Oidc:Authority` + `Bowire:Auth:Oidc:ClientId`.
- [ ] **Telemetry env vars set.** `Bowire:Telemetry:Enabled=true` + `OTEL_EXPORTER_OTLP_ENDPOINT` pointing at `tempo:4317`.
- [ ] **Plugin disable list set.** `Bowire:DisabledPlugins` contains `soap` and `pulsar` (or whatever your team's compliance posture excludes).
- [ ] **Workspace volume mounted.** A named volume at `~/.bowire/` so workspaces survive a container restart.

### Runbook

- [ ] **Day-1 deploy** procedure with exact commands.
- [ ] **Day-2: rotate OIDC secret** — env-var update + restart command + smoke-test verification.
- [ ] **Day-2: disable a plugin** — env-var update + restart command + `/api/plugins` check.
- [ ] **Day-2: restore a workspace** — `bowire workspace import` invocation with the backup file path + UI verification.
- [ ] **Day-2: check trace + log flow** — Grafana dashboard URL + the three queries.

### Smoke test

- [ ] **Runs from outside the host.** No docker-compose / kubectl needed on the runner.
- [ ] **Eight checks** (or however many your runbook calls out), each `PASS` / `FAIL`, exit non-zero on any failure.
- [ ] **Covers TLS, OIDC redirect, plugin disable list, trace flow.**

### Observability evidence

- [ ] **A Tempo span on `Kuestenlogik.Bowire` ActivitySource** visible within 10 s of an Invoke in the workbench.
- [ ] **A Loki log line tagged with the OIDC sub claim** visible within the same window.
- [ ] **A metric on `bowire.invoke.count`** visible in Grafana (Prometheus-style scrape against Bowire's OTLP metric pipeline, or via Grafana's Tempo metrics generator).

## What's intentionally not covered

- **High availability / multi-region.** Bowire is per-team, not customer-facing; single-node is enough.
- **Building Bowire from source.** Pull the published image; don't `dotnet publish` locally.
- **Custom plugin authoring.** That's the Developer capstone.
- **The embedded shape.** Administrators ship the Tool standalone; teams that embed Bowire ship their own host and own its deployment.

---

**Back to:** [Administrator capstone README](README.md) · [Curriculum](../../units-overview.md)
