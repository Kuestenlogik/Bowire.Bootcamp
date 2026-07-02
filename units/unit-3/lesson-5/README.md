# Lesson 3.5: Deployment patterns

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md)

## Overview

Ship the `bowire` CLI into a non-laptop environment — an internal-tools container, a CI runner, a sidecar — and keep it configured across layers.

## Layered configuration

Bowire reads settings in standard .NET precedence — later layers win:

1. `appsettings.json` sections (`Bowire:Mock`, `Bowire:Cli`, `Bowire:Test`, `Bowire:Plugin`, …)
2. `BOWIRE_*` environment variables
3. CLI flags

```jsonc
// appsettings.json
{ "Bowire": { "Mock": { "Port": 6000 }, "PluginUpdateCheck": { "Enabled": false } } }
```
```bash
BOWIRE_Bowire__Mock__Port=7000 bowire mock rec.bwr        # env overrides appsettings
bowire mock rec.bwr --port 8000                           # flag overrides both
```

## Container

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet tool install --global Kuestenlogik.Bowire.Tool
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["bowire", "mock", "/data/recording.bwr", "--host", "0.0.0.0", "--port", "6000"]
```

Bind `--host 0.0.0.0` so the mock is reachable outside the container; mount recordings/workspaces as volumes.

## systemd (host the CLI as a service)

```ini
# /etc/systemd/system/bowire-mock.service
[Service]
ExecStart=/usr/local/bin/bowire mock /srv/bowire/recording.bwr --host 0.0.0.0 --port 6000
Restart=always
Environment=BOWIRE_PluginUpdateCheck__Enabled=false
[Install]
WantedBy=multi-user.target
```

## Reverse-proxy in front

Terminate TLS and route at nginx/Traefik; Bowire listens plain on the loopback/pod-internal port. Lock the workbench URL in shared deploys with `--lock-server-url`, and gate the workbench out of production builds (embedded hosts: wrap `AddBowire()`/`MapBowire()` in `IsDevelopment()` — see [Unit 4](../../unit-4/README.md)).

## Key Takeaways

1. **Config layers: `appsettings.json` → `BOWIRE_*` env → CLI flags** (later wins).
2. **Containerise the global tool** with `--host 0.0.0.0` and volume-mounted recordings.
3. **Front it with a reverse-proxy** for TLS; keep update-checks/telemetry opt-in for shared installs.

## What's Next

**Continue:** → [Lesson 3.6: Observability & operations](../lesson-6/README.md)

## Reference

- [Deployment / setup](https://bowire.io/docs/setup/)
