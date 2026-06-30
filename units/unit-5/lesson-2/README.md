# Lesson 5.2: Deployment patterns

> **Difficulty:** Intermediate | **Duration:** 20 min | **Prerequisites:** [Unit 0](../../unit-0/README.md) (both shapes installed), Docker, Linux host or VM for the systemd walkthrough

## Overview

Bowire ships in two shapes that pair to two deployment patterns:

1. **Standalone CLI** — the `bowire` global tool (NuGet id `Kuestenlogik.Bowire.Tool`). One workbench process you point at one or more backend services. Belongs in a container, a systemd unit, or an IIS sidecar.
2. **Embedded host** — `app.MapBowire("/bowire")` mounted inside your own ASP.NET app. One workbench per service, hosted by the service itself. Shipped together with the service's own deployment artefact.

This lesson covers when to pick each, how to package both, how to layer config (`appsettings.json` → `BOWIRE_*` env → CLI flags), and how to put a reverse-proxy in front.

> **Administrator audience.** Lesson 5.2 is operator territory. If you are an end-user driving the workbench, [Unit 0.2](../../unit-0/lesson-2/README.md) covers laptop installs; if you are a backend developer embedding Bowire, [Unit 1.1](../../unit-1/lesson-1/README.md) covers the in-process mount. This lesson assumes you are packaging Bowire for someone else to run.

## Step 1 — Pick a shape

Use this table to decide:

| Question | Standalone CLI | Embedded host |
|---|---|---|
| One workbench across many backend services? | ✓ pick this | each service gets its own |
| Bowire shipped with a single service's deployment artefact? | extra moving part | ✓ pick this |
| Discovery URLs editable by the operator at runtime? | ✓ (`BowireMode.Standalone`, URL bar visible) | hidden (`BowireMode.Embedded`, in-process discovery) |
| Need to gate behind a feature flag / environment? | run-or-don't-run at the orchestrator level | conditional `MapBowire(...)` call |
| CI / batch jobs? | ✓ pick this — see [Lesson 5.1](../lesson-1/README.md) | not applicable |

The two shapes share **exactly the same workbench UI**. Picking a shape is a deployment-topology choice, not a feature choice — every rail, every plugin, every panel behaves identically.

## Step 2 — Standalone CLI in a container

The Tool project ships with .NET SDK container properties (`ContainerRepository=kuestenlogik/bowire`, `ContainerPort=5080`, `ContainerFamily=noble-chiseled-extra`, `ASPNETCORE_URLS=http://+:5080`). Build the image straight from a checkout:

```bash
git clone https://github.com/Kuestenlogik/Bowire
cd Bowire/src/Kuestenlogik.Bowire.Tool
dotnet publish /t:PublishContainer -c Release
```

This produces a chiselled-Ubuntu OCI image with `bowire` as the entrypoint, listening on `:5080`. There is no checked-in `Dockerfile` — the SDK container build target is the supported path.

Run it pointed at a backend:

```bash
docker run --rm -p 5080:5080 \
  kuestenlogik/bowire:latest \
  --url https://api.staging.internal:443
```

Or pass discovery URLs via env (the `BOWIRE_` prefix + `__` separator maps onto the `Bowire:` config section — see Step 6):

```bash
docker run --rm -p 5080:5080 \
  -e Bowire__ServerUrls__0=https://api.staging.internal:443 \
  -e Bowire__LockServerUrl=true \
  -e Bowire__Title="Staging workbench" \
  kuestenlogik/bowire:latest
```

`LockServerUrl=true` is the production convention for shared workbenches — the operator sees the pre-configured target and can't accidentally repoint the UI at another host.

## Step 3 — Standalone CLI as a systemd unit

For single-host Linux deployments without a container runtime, the same `Kuestenlogik.Bowire.Tool` global tool installs into the system .NET tool path and runs under systemd. Install:

```bash
sudo dotnet tool install --tool-path /opt/bowire/bin Kuestenlogik.Bowire.Tool
```

Drop the following at `/etc/systemd/system/bowire.service`:

```ini
[Unit]
Description=Bowire workbench
After=network.target

[Service]
Type=simple
User=bowire
Group=bowire
WorkingDirectory=/var/lib/bowire
Environment=DOTNET_ROOT=/usr/share/dotnet
Environment=BOWIRE_ENVIRONMENT=Production
Environment=Bowire__ServerUrls__0=https://api.internal:443
Environment=Bowire__LockServerUrl=true
Environment=Bowire__Title=Internal API workbench
ExecStart=/opt/bowire/bin/bowire --port 5080 --host 0.0.0.0 --no-browser
Restart=on-failure
RestartSec=5
NoNewPrivileges=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/bowire

[Install]
WantedBy=multi-user.target
```

Then:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now bowire
sudo systemctl status bowire
```

`/var/lib/bowire` becomes the working directory for the workspace file (Bowire persists the active workspace to a literal `.bww` file in CWD — see [Lesson 5.3](../lesson-3/README.md#workspace-backup)). The `--no-browser` flag suppresses the auto-launch that's useful on a laptop and noise on a server.

## Step 4 — Embedded host as a container

For the embedded shape, you are containerising **your own ASP.NET service** with Bowire wired in. The reference is `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` in the main Bowire repo — `AddBowire()` + `MapBowire("/bowire")` lines 18 and 110.

A minimal `Dockerfile` for an embedded-host service (write this in your own service repo; Bowire core does not ship one):

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyService.dll"]
```

The workbench mounts at `/bowire` inside your service's own port (`8080` in the example). No separate process, no extra port.

## Step 5 — Gating the embedded mount for production

`MapBowire(...)` is **not gated by default**. The Sample.Embedded reference (`samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` line 110) mounts unconditionally because it's a demo. In a production host, decide explicitly whether to expose the workbench.

Pattern A — environment-gated (most common):

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddBowire();
// ... your own service registrations

var app = builder.Build();
// ... your own pipeline

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapBowire("/bowire");
}

app.Run();
```

Pattern B — config-flag-gated (production-overridable):

```csharp
if (builder.Configuration.GetValue<bool>("Bowire:Expose"))
{
    app.MapBowire("/bowire");
}
```

Pattern A is the right default for shared hosts: production never exposes the workbench, staging does, dev always does. Pattern B is the right default when you need to flip it on transiently in production for incident debugging — a deploy-time variable rather than a build-time decision.

The workbench reads `BowireMode.Embedded` by default when mounted via `MapBowire(...)`, so the URL bar is hidden and discovery runs against the host's own `IServiceProvider` — there is no path to "accidentally repoint embedded Bowire at another host".

## Step 6 — Config layering (appsettings → env → CLI flags)

The `Kuestenlogik.Bowire.Tool` standalone CLI reads config in this order (precedence, last wins):

1. `appsettings.json` (working directory; optional)
2. `appsettings.{Environment}.json` (driven by `BOWIRE_ENVIRONMENT`, falling back to `DOTNET_ENVIRONMENT` then `ASPNETCORE_ENVIRONMENT`)
3. Environment variables with the `BOWIRE_` prefix; `__` in the variable name maps to `:` in the config key
4. CLI flags via `AddCommandLine` switch mappings

Every CLI flag has a config-key equivalent (and vice versa for the keys that map to one). The verified set:

| CLI flag | Config key |
|---|---|
| `--port` | `Bowire:Port` |
| `--host` | `Bowire:Host` |
| `--title` | `Bowire:Title` |
| `--url` | `Bowire:ServerUrl` (single) / `Bowire:ServerUrls:0,1,...` (multiple) |
| `--no-browser` | `Bowire:NoBrowser` |
| `--enable-mcp-adapter` | `Bowire:EnableMcpAdapter` |
| `--update-check` | `Bowire:PluginUpdateCheck:Enabled` |
| `--telemetry` | `Bowire:Telemetry:Enabled` |
| `--telemetry-strip-method-labels` | `Bowire:Telemetry:StripMethodLabels` |
| `--auth-provider` | `Bowire:Auth:ProviderId` |
| `--map-basemap` | `Bowire:MapBasemap` |
| `--plugin-dir` | `Bowire:PluginDir` |
| `--disable-plugin` | `Bowire:DisabledPlugins:0,1,...` |

Worked example — same configuration expressed three ways:

`appsettings.Production.json`:

```json
{
  "Bowire": {
    "Title": "Internal API workbench",
    "ServerUrls": [ "https://api.internal:443" ],
    "LockServerUrl": true,
    "DisabledPlugins": [ "mqtt", "pulsar" ]
  }
}
```

Env:

```bash
BOWIRE_ENVIRONMENT=Production
Bowire__Title="Internal API workbench"
Bowire__ServerUrls__0=https://api.internal:443
Bowire__LockServerUrl=true
Bowire__DisabledPlugins__0=mqtt
Bowire__DisabledPlugins__1=pulsar
```

CLI:

```bash
bowire \
  --title "Internal API workbench" \
  --url https://api.internal:443 \
  --disable-plugin mqtt \
  --disable-plugin pulsar
```

Note: `--lock-server-url` is not on the CLI-flag list (no switch mapping). For shared deployments use env or `appsettings.json` for that key.

The embedded host (`AddBowire()` + `MapBowire(...)`) reads the same `Bowire:` section from the host's own `IConfiguration` — whatever provider chain the host's `WebApplication.CreateBuilder(args)` set up. No separate Bowire-specific bootstrap.

## Step 7 — Reverse-proxy in front

The standalone CLI listens HTTP-only on `5080` by default. For external traffic put a reverse-proxy in front and terminate TLS there. `MapBowire(...)`'s mount path is path-only (`/bowire` by default, configurable to anything via the first argument; leading slash optional — the implementation calls `TrimStart('/')` and stores the prefix without it), so a reverse-proxy can mount it at any location.

Caddyfile (standalone CLI behind Caddy at `workbench.example.com`):

```caddyfile
workbench.example.com {
    encode gzip
    reverse_proxy localhost:5080
}
```

nginx (embedded host behind nginx at `app.example.com/bowire`):

```nginx
server {
    listen 443 ssl;
    server_name app.example.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # /bowire is mounted by MapBowire("/bowire") inside the embedded host —
    # no separate proxy_pass needed; it rides the same upstream as the rest
    # of the app. Add an auth layer here for production.
    location /bowire {
        # auth_request /auth-check;   # delegate to your own auth backend
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

For embedded mounts under a non-default prefix (`/internal-tools/api-workbench`, say):

```csharp
app.MapBowire("/internal-tools/api-workbench");
```

Bowire reads its own mount path from the runtime configuration and rewrites internal links accordingly — the reverse-proxy doesn't need a `proxy_redirect` rule.

## Exercise — Sample.Embedded behind Caddy

Deploy the Bowire `Sample.Embedded` host behind Caddy locally and confirm:

1. Clone the main Bowire repo, run `dotnet run --project samples/Kuestenlogik.Bowire.Sample.Embedded --urls http://localhost:5181`.
2. Put this Caddyfile in front (run `caddy run --config Caddyfile`):

   ```caddyfile
   :8888 {
       reverse_proxy localhost:5181
   }
   ```

3. Open `http://localhost:8888/bowire` in a browser. Confirm: the workbench loads, the rail strip is visible, the Sample.Embedded's `/api/users`, `/api/products`, `/api/locations`, `/api/health` services appear under Discover.
4. Invoke `GetUser` with id `1`. Confirm: response shows Ada Lovelace. The trace this generates feeds into [Lesson 5.3's](../lesson-3/README.md) observability surface.

## Key Takeaways

1. **Two shapes, one UI.** Pick standalone CLI for one-workbench-across-many-backends, embedded for one-workbench-per-service. The lesson choice is deployment topology, not feature set.
2. **Config layers in standard ASP.NET order.** `appsettings.json` → `appsettings.{Env}.json` → `BOWIRE_*` env (`__` separator) → CLI flags. Last wins. Every CLI flag maps to a config key; not every config key has a CLI flag.
3. **`MapBowire(...)` is never gated by default.** The Sample.Embedded reference mounts unconditionally. For production, gate explicitly with `IsDevelopment()` / `IsStaging()` or a `Bowire:Expose` config flag — code-level, not infrastructure-level.
4. **Reverse-proxy mount path stays clean.** `MapBowire("/anything")` rewrites internal links from the mount path it received; the proxy needs to forward the URL through, nothing more. Auth lives at the proxy layer.

## What's Next

[Lesson 5.3](../lesson-3/README.md) wires observability — OTLP export, the `Kuestenlogik.Bowire` ActivitySource + Meter, and the day-1 ops runbook (disable a misbehaving plugin, back up an operator's workspace, probe plugin health).

## Reference

- Main Bowire repo: `src/Kuestenlogik.Bowire.Tool/Kuestenlogik.Bowire.Tool.csproj` — SDK container properties (`ContainerRepository`, `ContainerFamily`, `ContainerPort`, `ContainerImageFormat`)
- Main Bowire repo: `src/Kuestenlogik.Bowire.Tool/Configuration/BowireConfiguration.cs` — config provider chain + CLI-to-config switch mappings
- Main Bowire repo: `src/Kuestenlogik.Bowire/BowireEndpointRouteBuilderExtensions.cs` — `MapBowire(this IEndpointRouteBuilder, string pattern = "/bowire", Action<BowireOptions>? configure = null)` signature
- Main Bowire repo: `src/Kuestenlogik.Bowire/BowireOptions.cs` — every config key with its default
- Main Bowire repo: `samples/Kuestenlogik.Bowire.Sample.Embedded/Program.cs` — canonical embedded-shape `Program.cs`
- [bowire.io setup docs — Standalone](https://bowire.io/docs/setup/standalone.html)
- [bowire.io setup docs — Embedded](https://bowire.io/docs/setup/embedded.html)
- [bowire.io setup docs — Docker](https://bowire.io/docs/setup/docker.html)
