# Lesson 3.4: Reverse-proxy interception

> **Difficulty:** Intermediate | **Duration:** 12 min | **Prerequisites:** [Lesson 3.1](../lesson-1/README.md)

## Overview

Put Bowire *in the path* of real traffic without touching the upstream's code. Two CLI shapes:

- **`bowire interceptor`** — a **reverse-proxy** in front of one upstream. Clients point at Bowire's listener; every request is forwarded upstream and captured. The clean choice when you control the client's target URL.
- **`bowire proxy`** — a classic **forward MITM proxy**. Point a browser/client's proxy setting at it; it intercepts HTTP/HTTPS across many hosts.

Both feed the same **Intercept rail** you toured in [Unit 2.5](../../unit-2/lesson-5/README.md) — this lesson stands the pipeline up; the rail is where you read it.

## Reverse-proxy an upstream

```bash
bowire interceptor --upstream https://api.example.com --listen 127.0.0.1:8080 --api-port 5089
```

- `--upstream <url>` — the service to front (required).
- `--listen host:port` — where the edge listener binds (default `127.0.0.1:0`, ephemeral).
- `--api-port` — the sidecar API the workbench's Intercept rail reads from (default 5089).
- `--capacity` / `--max-body-bytes` — FIFO retention + per-side body cap.
- `--allow-self-signed-upstream` — accept a dev upstream's self-signed cert.

Point your client at `http://127.0.0.1:8080` instead of the upstream. Open the workbench's **Intercept → Captured** to watch flows land.

### Serve HTTPS on the edge

```bash
bowire interceptor --upstream https://api.example.com --listen 127.0.0.1:8443 --tls --tls-host api.local
```

`--tls` mints a leaf cert from Bowire's MITM CA. Install the CA into the client trust store to avoid handshake warnings:

```bash
bowire proxy --export-ca ./bowire-ca.crt     # then trust the file
```

## Forward MITM proxy

```bash
bowire proxy --port 8888 --api-port 8889
```

Set your browser/client HTTP(S) proxy to `127.0.0.1:8888`. `--no-mitm` tunnels HTTPS without interception; `--ca-dir` overrides where the CA/PFX live (default `~/.bowire`).

## Key Takeaways

1. **`bowire interceptor` = reverse-proxy in front of one upstream**; **`bowire proxy` = forward MITM across many hosts.**
2. **Both feed the Intercept rail** (Unit 2.5) via the sidecar API port.
3. **HTTPS needs the CA trusted** — `bowire proxy --export-ca` then install the cert.

## What's Next

**Continue:** → [Lesson 3.5: Deployment patterns](../lesson-5/README.md)

## Reference

- [Security testing — interceptor](https://bowire.io/docs/security/)
