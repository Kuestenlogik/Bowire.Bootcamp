# Runbook — Bowire shared internal-tools deploy

*Reference for the [Administrator capstone](../README.md). Day-1 deploy + four day-2 procedures. Written for an on-call teammate who has SSH to the host (or `kubectl` to the namespace) but hasn't seen this deploy before.*

---

## Day-1: deploy

### Pre-deploy checklist

- [ ] DNS record points at the host.
- [ ] :80 + :443 reachable from the public internet (for Caddy ACME).
- [ ] OIDC client registered at the IdP with redirect URI `https://<host>/signin-oidc`. Confidential client. `openid profile` scopes.
- [ ] Host has Docker Engine ≥ 24 + Compose v2 (compose track) or `kubectl` to a 1.28+ cluster (k8s track).
- [ ] (k8s) cert-manager + an ingress controller installed and healthy in the cluster.

### Bring the stack up (compose track)

```bash
cd /opt/bowire
cp ../bowire.bootcamp/capstones/administrator/compose/* .

# Fill in the four GAPs in docker-compose.yml + a .env for the public host:
cat > .env <<EOF
BOWIRE_PUBLIC_HOST=bowire.team.example
EOF
mkdir -p secrets
openssl rand -base64 24 > secrets/grafana_admin_password.txt
chmod 600 secrets/grafana_admin_password.txt

docker compose up -d
docker compose ps    # all five services Running/healthy
```

### Bring the stack up (k8s track)

```bash
cd .../administrator/k8s
# Fill in the GAPs in 10-bowire.yaml (ConfigMap entries).
# Update the hostname in 50-ingress.yaml.
kubectl apply -f .
kubectl -n bowire rollout status deploy/bowire
kubectl -n bowire get pods       # all five Running, 1/1 Ready
```

### Smoke-test from outside

```bash
./smoke-test.sh https://bowire.team.example
# expects: 8 PASS, exit 0
```

### First OIDC login

Browse `https://bowire.team.example/`. The workbench bounces you to the IdP, you log in, you bounce back, the workbench renders with your username in the header chip. If you wired `Bowire:Auth:Oidc:RequiredClaim:*`, anyone outside the claim list gets a 403 from the workbench's `/api/*` surface.

### Sign-off

- [ ] All five services healthy.
- [ ] TLS chain valid (browser shows the public-CA padlock, no warnings).
- [ ] OIDC redirect round-trips and the workbench renders.
- [ ] Smoke test exits 0.
- [ ] Grafana dashboard loads against Tempo + Loki.

---

## Day-2: rotate the OIDC client secret

When the IdP-side client secret is rotated, Bowire needs the new value. Microsoft.Identity.Web reads it off `Bowire:Auth:Oidc:ClientCredentials:0:ClientSecret` (a list of credentials — multiple entries let you stage a rotation; the provider tries each in turn).

### Compose

```bash
# Update the env var (use a real secret store in production, not plaintext).
docker compose exec bowire env | grep ClientSecret  # confirm current
# Edit docker-compose.yml -> bowire.environment, then:
docker compose up -d --force-recreate bowire
docker compose logs bowire | tail -20    # look for "auth: oidc provider registered"
./smoke-test.sh https://bowire.team.example
```

### k8s

```bash
kubectl -n bowire edit configmap bowire-config  # update the secret env
kubectl -n bowire rollout restart deploy/bowire
kubectl -n bowire rollout status deploy/bowire
./smoke-test.sh https://bowire.team.example
```

### Verify

- Smoke-test "OIDC redirect" check passes.
- An interactive browser login still round-trips.

---

## Day-2: disable a plugin

When a plugin gets pulled (compliance posture change, vendor security advisory, bad release), append its id to `Bowire:DisabledPlugins`. Bowire skips the plugin at startup; it stays installed on disk but doesn't load.

### Compose

```bash
# Append a new indexed entry. If the next free index is __2:
docker compose exec bowire env | grep DisabledPlugins  # see what's there
# Edit docker-compose.yml -> bowire.environment, add:
#   - BOWIRE_Bowire__DisabledPlugins__2=signalr
docker compose up -d --force-recreate bowire
```

### k8s

```bash
kubectl -n bowire edit configmap bowire-config   # add BOWIRE_Bowire__DisabledPlugins__2: "signalr"
kubectl -n bowire rollout restart deploy/bowire
```

### Verify

```bash
# /api/plugins is the workbench's plugin-status endpoint.
curl -fsS https://bowire.team.example/api/plugins -H "Authorization: Bearer $TOKEN" | \
  jq '.[] | select(.id == "signalr")'
# expects: empty output (the disabled plugin isn't in the response)
```

---

## Day-2: restore a workspace from backup

Workspaces live in the Bowire container under `/home/app/.bowire/workspaces/<id>/`. The backup sidecar (or your external cron) exports each one nightly as a `bowire-workspace` v2 envelope (`.bww`). To restore:

### Compose

```bash
# 1. Copy the backup into the bowire container.
docker compose cp /backup/2026-06-29-checkout-flake.bww bowire:/tmp/restore.bww

# 2. Import into the per-entity directory layout.
docker compose exec bowire \
  bowire workspace import /tmp/restore.bww --to /home/app/.bowire/workspaces/checkout-flake

# 3. Verify in the UI.
# Open https://bowire.team.example/ -> Workspaces rail -> checkout-flake should be present.
```

### k8s

```bash
kubectl -n bowire cp /backup/2026-06-29-checkout-flake.bww \
    $(kubectl -n bowire get pod -l app=bowire -o jsonpath='{.items[0].metadata.name}'):/tmp/restore.bww
kubectl -n bowire exec deploy/bowire -- \
    bowire workspace import /tmp/restore.bww --to /home/app/.bowire/workspaces/checkout-flake
```

### Verify

- The `Workspaces` rail in the UI lists the restored workspace.
- Opening it shows the original URLs, Compose tabs, recordings.

> `bowire workspace import` is defined in `src/Kuestenlogik.Bowire.Tool/Cli/WorkspaceCommand.cs::RunImportAsync`. The roundtrip is what the canonical v2 envelope is designed for (see `CanonicalFormatVersion = 2`).

---

## Day-2: check trace + log flow

The workbench's `Kuestenlogik.Bowire` ActivitySource emits spans for every Invoke + every Discover (`BowireTelemetry.SourceName` in main Bowire). The `Kuestenlogik.Bowire` Meter emits `bowire.invoke.count`, `bowire.invoke.duration`, `bowire.discover.count`, `bowire.plugin.load`, `bowire.mock.requests`. Both flow through the OTLP exporter to Tempo. Logs flow to Loki via the Docker log driver (compose) or Promtail / Vector (k8s — wire your own).

### Drive a span deliberately

1. Open `https://bowire.team.example/`, log in.
2. Click on any discovered method in Discover, fill the form, Invoke.
3. Open Grafana → Explore → Tempo. Query `{service.name="bowire"}`.
4. Click the most recent trace. The root span is the HTTP request to `/api/invoke`; the child spans are Bowire's protocol-plugin work.

### Drive a metric deliberately

In Grafana → Explore → Tempo metrics generator (or your Prometheus scraping the OTLP metric pipeline), query:

```
rate(bowire_invoke_count_total[1m])
```

You should see a 1/min uplift right after your Invoke click. (Bowire's metrics use the canonical `Kuestenlogik.Bowire` Meter; OTel SDK names them with dots, Prometheus exposers convert dots to underscores.)

### Drive a log line deliberately

```bash
# Tail the Bowire logs.
docker compose logs -f bowire    # or: kubectl -n bowire logs -f deploy/bowire
# Hit /api/health from outside.
curl -fsS https://bowire.team.example/api/health
```

A log line for the request appears within ~1 s. In Loki, the same line appears under `{container="bowire"}` (compose) or your equivalent label.

### Verify

- A Tempo span on `service.name="bowire"` exists for the deliberate Invoke.
- A Prometheus / Tempo-metrics-generator counter increment on `bowire_invoke_count` matches.
- A Loki log line for the `/api/health` curl exists.

---

## Reference

- Container image: `kuestenlogik/bowire:2.1.0` (`Kuestenlogik.Bowire.Tool.csproj::ContainerRepository`).
- Container port: 5080 (`Kuestenlogik.Bowire.Tool.csproj::<ContainerPort>`).
- Workspace data dir: `~/.bowire/` (resolves via `Environment.SpecialFolder.UserProfile` in `PluginManager.cs`; in the chiseled container, `/home/app/.bowire`).
- Auth provider id: `oidc` (`OidcAuthProvider.Id = "oidc"`).
- Telemetry section: `Bowire:Telemetry` (`AddBowireTelemetry`).
- Plugin disable list: `Bowire:DisabledPlugins` (`BrowserUiOptions.DisabledPlugins`).
- Workspace CLI: `bowire workspace export <dir> <out.bww>` / `bowire workspace import <in.bww> --to <dir>` (`WorkspaceCommand.cs`).

---

**Back to:** [Administrator capstone](../README.md) · [Curriculum](../../../units/README.md)
