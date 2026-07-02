# Administrator capstone — Production deployment stack

> **Difficulty:** Intermediate · **Time:** ~3–4 hours · **Prerequisites:** the [Integrator / DevOps / Administrator course](../../LEARNING_PATHS.md#2-integrator--devops--administrator) — Units 0 → 3, plus operator-level Docker / shell experience.

You've worked through the Integrator / DevOps / Administrator course. This capstone proves you can ship the Bowire Tool into a non-laptop environment and keep it running past day one — auth on the door, observability wired, plugin discipline, workspace backups, a smoke test that runs from outside the box.

The deliverable is a working stack (`docker-compose.yml` or k8s manifests), a `RUNBOOK.md` covering day-1 + day-2, and a smoke-test script that exercises the deployment from outside the host. No new code, no plugin authoring — this is "operator at the deployment" end to end.

## Scenario

A backend team of five wants Bowire as a shared internal tool: one URL the whole team browses to, single sign-on, every Workspaces export persisted somewhere the team can recover from. The hosting team gives you a single Linux VM (or one namespace in the cluster) and asks you to ship it.

Constraints:

| Constraint | Reason |
| --- | --- |
| Public-facing TLS via reverse proxy | Bowire's standalone Tool binds plain HTTP on `:5080` (see `src/Kuestenlogik.Bowire.Tool/Kuestenlogik.Bowire.Tool.csproj`, `<ContainerPort Include="5080">`); TLS terminates at the edge. |
| OIDC at the door | Five-person team, no anonymous browsing. The team's IdP issues OIDC tokens (Azure AD / Auth0 / Keycloak / Okta — any RFC-compliant provider). |
| Observability — traces + logs | The team wants to see who used which workspace + see invoke counts and durations during peak hours. |
| Plugin disable list documented | Two plugins (the SOAP plugin and the Pulsar plugin) are not certified for the team's compliance posture and must stay off. |
| Workspace backup strategy | The `~/.bowire/` workspaces are the team's collective state; losing them loses the team's work. |
| Smoke test from outside | A script the SRE team can run before declaring the rollout green. |

## Deliverables

You can ship either or both stacks:

### docker-compose track (recommended starting point)

A `compose/docker-compose.yml` that brings up:

| Service | Image | Purpose |
| --- | --- | --- |
| `bowire` | `kuestenlogik/bowire:2.1.0` | The standalone Tool (`src/Kuestenlogik.Bowire.Tool/Kuestenlogik.Bowire.Tool.csproj`, `ContainerRepository = kuestenlogik/bowire`). Exposes `:5080` inside the compose network. |
| `caddy` | `caddy:2` | Reverse proxy + automatic Let's Encrypt TLS. Terminates `https://bowire.team.example/` and proxies to `bowire:5080`. |
| `grafana` | `grafana/grafana-oss` | Observability surface. Single dashboard against Tempo + Loki. |
| `tempo` | `grafana/tempo` | OTLP trace store. Receives Bowire's `Kuestenlogik.Bowire` ActivitySource (see `src/Kuestenlogik.Bowire.Telemetry/BowireTelemetryServiceCollectionExtensions.cs`). |
| `loki` | `grafana/loki` | Log store. Receives Bowire's structured stdout via a Docker log driver. |

The starter in [`compose/`](compose/) has every service wired but four commented gaps you fill in (OIDC client id / secret, the public hostname, the OTLP endpoint pointing at `tempo:4317`, the plugin disable list).

### Kubernetes track (optional second pass)

A `k8s/` directory carrying the same five workloads as Deployments + Services + a single Ingress. Mirrors the compose track 1:1; the runbook covers either.

### Runbook

A `RUNBOOK.md` under `solution/` covering:

- **Day-1 deploy** — bring up the stack, point DNS, watch the first OIDC login succeed.
- **Day-2 ops** — rotate the OIDC client secret, disable a plugin, restore a workspace from backup, verify trace + log flow.

### Smoke test

A bash script under `solution/smoke-test.sh` that exercises the deployment from outside the host: TLS handshake, `/api/health` round-trip, OIDC redirect, plugin disable list check, `Kuestenlogik.Bowire` traces visible in Tempo within ~10 s.

## Step-by-step lab

Estimated wall-clock: 2–3 hours of stack work + 30–45 minutes of runbook writing.

### 1. Read the Tool's container surface

Skim `src/Kuestenlogik.Bowire.Tool/Kuestenlogik.Bowire.Tool.csproj` in main Bowire to confirm:

- The container image is `kuestenlogik/bowire:<version>` (`ContainerRepository`).
- Port `5080` is the only exposed TCP port (`<ContainerPort Include="5080">`).
- The image starts the Tool with `ASPNETCORE_URLS=http://+:5080` (`ContainerEnvironmentVariable`).
- The image is Ubuntu Noble chiseled (no shell, non-root by default — pin a user / mounts at non-root paths only).

No surprises in the runtime contract. Everything else is configured through `Bowire:*` keys (the `Bowire:Auth`, `Bowire:Telemetry`, `Bowire:DisabledPlugins`, etc. sections — see [Unit 0.2](../../units/unit-0/lesson-2/README.md) and the bound options under `BrowserUiOptions.cs`).

### 2. Bring up the compose stack on a laptop first

Copy [`compose/`](compose/) into a working directory and fill in the four commented gaps in `docker-compose.yml`:

- `${BOWIRE_PUBLIC_HOST}` — the hostname Caddy serves on (e.g. `bowire.team.example` for prod; `bowire.localhost` for laptop iteration).
- `Bowire__Auth__Oidc__Authority` + `Bowire__Auth__Oidc__ClientId` — the team's OIDC issuer URL and the app client id. Drop these as `BOWIRE_` env vars per `BowireConfiguration.cs` (`Bowire:Auth:Oidc:Authority` → `BOWIRE_Bowire__Auth__Oidc__Authority`).
- `OTEL_EXPORTER_OTLP_ENDPOINT=http://tempo:4317` — the OTLP receiver. Bowire honours the standard OTEL env vars (see `BowireTelemetryServiceCollectionExtensions.cs`); no Bowire-specific wiring needed.
- `Bowire__DisabledPlugins__0=soap` + `Bowire__DisabledPlugins__1=pulsar` — the two plugins the compliance posture excludes (see `--disable-plugin` flag + the `Bowire:DisabledPlugins` array binding in `BrowserUiOptions.cs`).

Then:

```bash
docker compose -f compose/docker-compose.yml up -d
```

Hit `https://bowire.localhost/` — Caddy serves its internal CA on the first request, your browser warns about it, accept. You should land on Bowire's empty-state screen (no workspace yet — the OIDC client id is wired but the redirect URI is loopback for now).

### 3. Wire OIDC properly

In your IdP (Azure AD / Auth0 / Keycloak / Okta), register a confidential client with:

- Redirect URI: `https://bowire.team.example/signin-oidc` (the default `Microsoft.Identity.Web` callback the OIDC provider mounts; see `src/Kuestenlogik.Bowire.Auth.Oidc/OidcAuthProvider.cs` in main Bowire).
- Allowed scopes: `openid profile`.
- (Optional) A group claim or tenant claim if you want to scope access via `Bowire:Auth:Oidc:RequiredClaim`.

Set the env vars in `docker-compose.yml`:

```yaml
environment:
  - BOWIRE_Bowire__Auth__ProviderId=oidc
  - BOWIRE_Bowire__Auth__Oidc__Authority=https://login.example.com/
  - BOWIRE_Bowire__Auth__Oidc__ClientId=<your-app-client-id>
  # Optional: gate by a specific group claim.
  - BOWIRE_Bowire__Auth__Oidc__RequiredClaim__groups=bowire-users
```

> The `Bowire:Auth:Oidc:*` section is what `OidcAuthProvider` reads off `IConfiguration`. The values + key shape are verified against `OidcAuthProvider.cs` (the `<code>` block in its XML doc shows the canonical shape).

> The provider is opt-in via `--auth-provider oidc` *or* `Bowire:Auth:ProviderId=oidc`. The Tool image still needs the plugin available — `Kuestenlogik.Bowire.Auth.Oidc` ships in `Bundle.Workbench` which the Tool transitively references (see `Kuestenlogik.Bowire.Tool.csproj` `<ProjectReference Include="..\Kuestenlogik.Bowire.Bundle.Workbench\Kuestenlogik.Bowire.Bundle.Workbench.csproj" />`).

Restart: `docker compose up -d --force-recreate bowire`. Hit `https://bowire.localhost/` again — you bounce to the IdP, log in, bounce back, land on the workbench with your username in the header.

### 4. Wire observability

In the same compose file:

```yaml
environment:
  - BOWIRE_Bowire__Telemetry__Enabled=true
  # Drop the high-cardinality service+method dimensions for a shared deploy.
  # See BowireTelemetryServiceCollectionExtensions.cs / StripMethodLabels.
  - BOWIRE_Bowire__Telemetry__StripMethodLabels=true
  - OTEL_EXPORTER_OTLP_ENDPOINT=http://tempo:4317
  - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
  - OTEL_SERVICE_NAME=bowire
```

> The Tool's `--telemetry` flag binds to `Bowire:Telemetry:Enabled=true` per `BowireConfiguration.cs::s_switchMappings`. The OTLP endpoint comes from the standard `OTEL_*` env vars — Bowire doesn't paper over OTel's own configuration vocabulary (see the doc comment on `AddBowireTelemetry`).

Logs go to Loki via Docker's Loki log driver — configured at the `bowire` service level in the compose file. Bowire writes structured logs to stdout (standard ASP.NET host); Loki picks them up by container name.

Restart, hit `/api/health` a few times from inside the container, open Grafana at `https://grafana.team.example/`, and you should see:

- **Traces** — `bowire.invoke.*` and `bowire.discover.*` spans on the `Kuestenlogik.Bowire` ActivitySource (`BowireTelemetry.SourceName`).
- **Metrics** — `bowire.invoke.count`, `bowire.invoke.duration`, `bowire.discover.count`, `bowire.plugin.load`, `bowire.mock.requests` on the `Kuestenlogik.Bowire` Meter.
- **Logs** — labelled by container name in Loki, with the OIDC sub claim threaded into the log lines.

### 5. Disable the two non-compliant plugins

Plugins are disabled either by CLI flag (`--disable-plugin soap --disable-plugin pulsar`) or by the `Bowire:DisabledPlugins` array (`BrowserUiOptions.DisabledPlugins`). For a container, the array form via env vars is the durable shape:

```yaml
environment:
  - BOWIRE_Bowire__DisabledPlugins__0=soap
  - BOWIRE_Bowire__DisabledPlugins__1=pulsar
```

Verify with the smoke test (next step): `/api/plugins` should not list SOAP or Pulsar.

### 6. Wire workspace backup

Workspaces live under `/root/.bowire/` (Bowire's per-user data dir resolves via `Environment.SpecialFolder.UserProfile` — see `PluginManager.cs`; in the chiseled container, the `app` user's home is its working dir). Mount a named Docker volume:

```yaml
volumes:
  - bowire-data:/home/app/.bowire
```

Add a sidecar `backup` service (or an external cron job on the host) that runs nightly:

```bash
# Inside the bowire container, or via docker exec:
bowire workspace export /home/app/.bowire/workspaces/<id> /backup/$(date +%F)-<id>.bww
```

> `bowire workspace export` writes the canonical v2 `.bww` envelope (see `WorkspaceCommand.cs::CanonicalFormatVersion`). The matching `import` rehydrates the per-entity directory layout the workbench's git-backed workspace mode consumes.

### 7. Write `RUNBOOK.md`

Drop a `RUNBOOK.md` under `solution/` covering five sections:

1. **Day-1 deploy** — the exact `docker compose up` command + DNS + first OIDC login walk.
2. **Day-2: rotate the OIDC client secret** — flip the env var, `docker compose up -d --force-recreate bowire`, re-verify with the smoke test.
3. **Day-2: disable a plugin** — append to `BOWIRE_Bowire__DisabledPlugins__N`, restart, verify the plugin is gone from `/api/plugins`.
4. **Day-2: restore a workspace from backup** — pull the `.bww` from the backup volume, `bowire workspace import <path>.bww --to /home/app/.bowire/workspaces/<id>`, verify in the UI.
5. **Day-2: check trace + log flow** — Grafana dashboard URL + the three queries (Tempo trace by service, Loki log by container, Prometheus / Grafana metric on `bowire.invoke.count`).

### 8. Write the smoke test

`solution/smoke-test.sh` — runs from a CI box (or a teammate's laptop), takes the public URL as `$1`:

```bash
./solution/smoke-test.sh https://bowire.team.example
```

Eight checks; each prints `PASS` / `FAIL` and the script exits non-zero if any check failed. The script is in [`solution/smoke-test.sh`](solution/smoke-test.sh) — read it for the exact `curl` / `openssl s_client` shapes.

### 9. (Optional) Kubernetes track

If your team runs on k8s instead of Docker Compose, the [`k8s/`](k8s/) starter has the same five workloads as Deployments + Services + a single Ingress + a PVC for `~/.bowire/`. Same env vars, same observability wiring; only the packaging changes.

### 10. Submit

In your fork, the capstone is one PR with these artefacts:

- `capstones/administrator/compose/docker-compose.yml` (filled in).
- (Optional) `capstones/administrator/k8s/*.yaml` (filled in).
- `capstones/administrator/solution/RUNBOOK.md`.
- `capstones/administrator/solution/smoke-test.sh`.

A reference of each lives under [`solution/`](solution/) — gold standard, not a copy-paste source.

## Acceptance criteria

You've completed the capstone when:

1. **The stack comes up clean.** `docker compose up -d` (or `kubectl apply -f k8s/`) succeeds, Bowire serves `https://<host>/` behind Caddy/Ingress with a valid TLS chain, OIDC bounces the user to the IdP and back.
2. **The plugin disable list is enforced.** `/api/plugins` does not list SOAP or Pulsar.
3. **Observability shows life.** Within 10 s of an Invoke action in the workbench, a span under the `Kuestenlogik.Bowire` ActivitySource appears in Tempo and a log line tagged with the user's OIDC sub appears in Loki.
4. **The workspace backup round-trips.** Export a workspace, delete it from the volume, restore it from the backup file, see it in the UI.
5. **Smoke test passes from outside the host.** `./smoke-test.sh https://<host>` exits 0.
6. **Runbook is actionable.** A teammate on call who hasn't seen the stack can rotate the OIDC secret, disable a plugin, and restore a workspace by following the runbook alone.

## Out of scope

- High availability / multi-region. Single-node compose or single-replica Deployment is enough — Bowire is per-team, not customer-facing.
- Building Bowire from source. Pull the published `kuestenlogik/bowire:2.1.0` image; don't `dotnet publish` locally.
- Custom plugin authoring. That's the Developer capstone.
- Network policies / Pod Security Standards / mTLS in-mesh. Each org has its own posture; the capstone shows the Bowire-side dials, not your infra's.

---

**Back to:** [Curriculum](../../units-overview.md) · [Learning paths](../../LEARNING_PATHS.md)
