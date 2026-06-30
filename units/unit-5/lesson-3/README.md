# Lesson 5.3: Observability + operations

> **Difficulty:** Intermediate | **Duration:** 15 min | **Prerequisites:** [Lesson 5.2](../lesson-2/README.md) (you have Bowire running in at least one deployment shape), Docker (for the OTEL collector)

## Overview

A workbench in production needs three things from its operator: a signal stream you can graph, a way to take a plugin offline without redeploying, and a backup story for the operator's own state. Lesson 5.3 walks the surface Bowire exposes for each.

You'll wire OTLP export against `BowireTelemetry`, point it at a local OpenTelemetry collector, see a trace land for a workbench invocation, and walk the day-1 runbook for plugin health, plugin disable, and workspace backup.

> **Administrator audience.** The signals + endpoints in this lesson are the same in standalone and embedded shape — `BowireTelemetry` is a `Kuestenlogik.Bowire` core surface, not a per-shape addition.

## Step 1 — What Bowire emits

The entire Bowire telemetry surface lives behind a single `ActivitySource` + a single `Meter`, both named `"Kuestenlogik.Bowire"`. The exact instruments (verified against `src/Kuestenlogik.Bowire/Telemetry/BowireTelemetry.cs` in the main repo):

| Instrument | Kind | Unit | What it counts |
|---|---|---|---|
| `bowire.invoke.count` | Counter `<long>` | `{invoke}` | One increment per workbench invoke, every protocol |
| `bowire.invoke.duration` | Histogram `<double>` | `ms` | Wall-clock invocation latency, request build to response render |
| `bowire.discover.count` | Counter `<long>` | `{discover}` | Discovery passes against a target URL |
| `bowire.plugin.load` | Counter `<long>` | `{load}` | Plugin load attempts; `outcome` dimension distinguishes success / failure |
| `bowire.mock.requests` | Counter `<long>` | `{request}` | Inbound requests against UI-started mocks |

Traces flow through the same source name (`Kuestenlogik.Bowire` `ActivitySource`). Every invoke produces a span; spans are nested under whatever parent span the host already created via `AddAspNetCoreInstrumentation()`.

The instruments exist whether telemetry is enabled or not — with no listener, the OTel SDK's no-op fast path keeps the per-call cost at a virtual call + a null check. Opt-in happens on the host side.

## Step 2 — Wire OTLP export

Two paths, depending on shape.

### Standalone CLI

Telemetry is config-flag-gated. Enable via env or flag:

```bash
BOWIRE_ENVIRONMENT=Production \
Bowire__Telemetry__Enabled=true \
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318 \
bowire --url https://api.internal:443 --port 5080 --host 0.0.0.0
```

Or CLI:

```bash
bowire --telemetry --url https://api.internal:443
```

Set `Bowire__Telemetry__StripMethodLabels=true` (or `--telemetry-strip-method-labels`) when per-service/per-method dimensions on `bowire.invoke.*` are too high-cardinality for your backend or contain names you don't want leaked to a shared collector.

### Embedded host

The simplest path — wire OpenTelemetry yourself and add Bowire's source + meter to your existing pipeline. The canonical snippet (copied verbatim from the `AddBowireTelemetry` XML doc in main Bowire repo):

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter(BowireTelemetry.MeterName))
    .WithTracing(t => t.AddSource(BowireTelemetry.ActivityName));
```

If you don't already wire OTel, the Telemetry sibling package does it for you. Reference `Kuestenlogik.Bowire.Telemetry` and:

```csharp
builder.Services.AddBowire();
builder.Services.AddBowireTelemetry(builder.Configuration);
// ...
app.MapBowire("/bowire");
```

`AddBowireTelemetry` reads its config from the `Bowire:Telemetry` section, registers the OTel SDK with `AddOtlpExporter()` on both the metrics + tracing pipelines, and adds `AddAspNetCoreInstrumentation()` + `AddHttpClientInstrumentation()` so request spans flow alongside Bowire's own. The OTLP exporter reads its endpoint from the standard `OTEL_EXPORTER_OTLP_*` env vars — Bowire doesn't paper over OTel's own configuration vocabulary.

## Step 3 — Exercise: stand up a collector + Grafana, see a trace

1. Stand up an OpenTelemetry collector + Tempo (traces) + Prometheus (metrics) + Grafana locally. The simplest path is the `grafana/otel-lgtm` all-in-one image:

   ```bash
   docker run -d --name otel-lgtm \
     -p 3000:3000 \
     -p 4317:4317 \
     -p 4318:4318 \
     grafana/otel-lgtm:latest
   ```

   Grafana lands at `http://localhost:3000` (default creds `admin` / `admin`), OTLP at `4317` (gRPC) / `4318` (HTTP).

2. Run the Sample.Embedded host from the main Bowire repo with telemetry wired:

   ```bash
   cd /path/to/Bowire
   dotnet run --project samples/Kuestenlogik.Bowire.Sample.Embedded \
     --urls http://localhost:5181 \
     --environment Production
   ```

   Set the OTLP endpoint:

   ```bash
   export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
   export Bowire__Telemetry__Enabled=true
   ```

   (Sample.Embedded does not reference `Kuestenlogik.Bowire.Telemetry` out-of-the-box — for the exercise, edit `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` to add `builder.Services.AddBowireTelemetry(builder.Configuration);` after the `AddBowire()` line. Alternatively, add the two-line `AddMeter(BowireTelemetry.MeterName)` / `AddSource(BowireTelemetry.ActivityName)` snippet from Step 2 to your own host.)

3. Open the workbench at `http://localhost:5181/bowire`. In Discover, click **Users → GetUser**, fill `id = 1`, **Invoke**. The response should be `{ Id: 1, Name: "Ada Lovelace", ... }`.

4. Open Grafana at `http://localhost:3000`. The pre-provisioned **Tempo** data source contains the trace. Filter by service name `Kuestenlogik.Bowire`. You should see one trace per invoke, with a `bowire.invoke.*` span enclosing the upstream HTTP span produced by `AddHttpClientInstrumentation()`.

5. Switch to the Prometheus data source and graph `bowire_invoke_count_total{protocol="rest"}`. Click Invoke a few more times and watch the counter advance.

## Step 4 — Day-1 ops runbook

### Plugin health

The mounted workbench exposes a plugin-health endpoint at `{mount-path}/api/plugins/health` — for the default mount, `GET /bowire/api/plugins/health`. The response is an array of `{ packageId, directory, status, errorMessage }` entries, one per plugin the loader saw. Use it from cron / a Kubernetes liveness probe — if a plugin's `status` is anything other than `Loaded`, `errorMessage` tells you why (signature mismatch, missing dep, manifest broken). Empty array means no plugins installed.

The same data drives the **Settings → System settings → Plugins** tree in the workbench UI, so an operator can also click in and see plugin state visually.

### Disable a plugin temporarily

Three paths, two scopes.

**Persistent (process-wide).** Add the plugin id to `Bowire:DisabledPlugins`:

- `appsettings.json`: `"Bowire": { "DisabledPlugins": [ "mqtt", "pulsar" ] }`
- env: `Bowire__DisabledPlugins__0=mqtt`
- CLI: `--disable-plugin mqtt --disable-plugin pulsar`

Restart required — the plugin scan happens at startup. Use this when the plugin is broken (DLL load failure, missing native dep) or when its discovery probe is too expensive on the current host's network.

**Per-browser, no restart.** The workbench UI lets an end-user hide an entire rail via the rail-strip context menu. The choice persists in the browser's `localStorage` under the key `bowire_enabled_rails`. This is per-browser, not per-deployment — for one operator's session, not a production lever.

### Back up an operator's workspace

The workbench persists the active workspace to a literal `.bww` file in the working directory the Bowire process was started in. The format (from `Endpoints/BowireWorkspaceEndpoints.cs` in the main repo):

```json
{
  "workspaceFormatVersion": 1,
  "urls": [ "https://api.example.com" ],
  "environments": [ ... ],
  "globals": { ... },
  "collections": [ ... ],
  "recordings": [ ... ],
  "flows": [ ... ],
  "pluginPins": { "grpc": "1.5.0", "mqtt": "1.5.0" }
}
```

Backup strategy:

- **Standalone CLI in systemd:** the working directory is whatever your `[Service] WorkingDirectory=` set (the Lesson 5.2 example sets `/var/lib/bowire`). Back up `/var/lib/bowire/.bww`.
- **Standalone CLI in a container:** mount a volume on the working directory the container starts in and back up `/<volume-mount>/.bww`.
- **Embedded host:** the file lands next to the host's working directory — typically the deployment's app dir. Treat it the same as any other config file the deploy produces.

The file is JSON; commit it to a private repo, ship it via your config-distribution path, or stash it in S3. It is portable — same shape across standalone + embedded.

### Liveness / readiness

Bowire does not register a `MapHealthChecks(...)` endpoint by default. Use the plugin-health endpoint (`{mount-path}/api/plugins/health`) for a meaningful readiness probe:

```yaml
livenessProbe:
  httpGet:
    path: /bowire/api/plugins/health
    port: 5080
  initialDelaySeconds: 10
  periodSeconds: 30
```

For an embedded host, your service's own `MapHealthChecks(...)` (if you add one) handles the orchestrator's liveness; the Bowire mount is a feature inside, not the service contract.

## Step 5 — Plug a leak

Two sharp edges to know about:

1. **Per-service / per-method labels on `bowire.invoke.*`.** These dimensions can carry your internal service names + method ids into the collector. For shared collectors, enable `Bowire:Telemetry:StripMethodLabels=true` (or `--telemetry-strip-method-labels`) — the OTel view rewrite drops `protocol` and `outcome` tag keys before export.
2. **`BowireOptions.LockServerUrl=true` for shared standalones.** When a standalone CLI is the front-door workbench for many operators, leaving the URL bar editable means any operator can repoint it. Pair the lock with a single pre-configured `ServerUrls` list — see [Lesson 5.2 Step 2](../lesson-2/README.md#step-2--standalone-cli-in-a-container).

## Key Takeaways

1. **One source name, every signal.** `Kuestenlogik.Bowire` is both the `ActivitySource` and the `Meter`. Add it once on the host's existing OTel pipeline; you get traces + metrics. The five domain instruments (`bowire.invoke.count`, `bowire.invoke.duration`, `bowire.discover.count`, `bowire.plugin.load`, `bowire.mock.requests`) are stable across minor releases — you can pin Grafana panels against them.
2. **OTLP endpoint reads from standard OTel env vars.** `OTEL_EXPORTER_OTLP_ENDPOINT` and friends — Bowire doesn't introduce a Bowire-specific override. The `AddBowireTelemetry` helper wires `AddOtlpExporter()` on both pipelines; bring-your-own-OTel hosts skip it and add the source + meter directly.
3. **Plugin health is a real endpoint.** `{mount-path}/api/plugins/health` returns structured per-plugin status; use it as a readiness probe and as the diagnostic when a plugin doesn't appear in the workbench's Discover tree.
4. **`.bww` is the operator's state.** Treat it like any other deploy artefact — mount a volume, commit to a private repo, ship through your config-distribution path. The format is versioned (`workspaceFormatVersion`); missing fields in older files deserialize to defaults so adding a new field never breaks an existing `.bww`.

## What's Next

Unit 5 ends here. Take what you've built — the Lesson 5.1 CI workflow, the Lesson 5.2 container + reverse-proxy, the Lesson 5.3 telemetry wiring — into the Administrator capstone, which weaves them into a single `docker-compose.yml` + production runbook.

**Continue:** → [Administrator capstone: Production stack + runbook](../../../capstones/administrator/README.md)

## Reference

- Main Bowire repo: `src/Kuestenlogik.Bowire/Telemetry/BowireTelemetry.cs` — `SourceName`, `ActivityName`, `MeterName`, every domain instrument
- Main Bowire repo: `src/Kuestenlogik.Bowire.Telemetry/BowireTelemetryServiceCollectionExtensions.cs` — `AddBowireTelemetry(IConfiguration, Action<BowireTelemetryOptions>?)`, section `Bowire:Telemetry`, OTLP wiring
- Main Bowire repo: `src/Kuestenlogik.Bowire/Endpoints/BowirePluginEndpoints.cs` — `GET {basePath}/api/plugins/health`
- Main Bowire repo: `src/Kuestenlogik.Bowire/Endpoints/BowireWorkspaceEndpoints.cs` — `.bww` schema (`workspaceFormatVersion`, `urls`, `environments`, `collections`, `recordings`, `flows`, `pluginPins`)
- Main Bowire repo: `src/Kuestenlogik.Bowire/BowireOptions.cs` — `DisabledPlugins`, `LockServerUrl`
- [OpenTelemetry OTLP exporter env vars](https://opentelemetry.io/docs/specs/otel/protocol/exporter/)
