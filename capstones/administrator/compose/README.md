# Administrator capstone — compose starter

A five-service Docker Compose stack ready to fill four gaps and ship. See the [capstone README](../README.md) for the lab.

## Files

| File | Purpose |
| --- | --- |
| `docker-compose.yml` | Service definitions. Four `# GAP:` markers to fill in. |
| `Caddyfile` | Reverse-proxy + TLS. Substitutes `BOWIRE_PUBLIC_HOST` from the env. |
| `tempo.yaml` | Tempo config — OTLP/gRPC on `:4317`, local storage. |
| `grafana-datasources.yaml` | Provisions Tempo + Loki as Grafana data sources. |

## Run (laptop)

```bash
# Put your OIDC client id + the host name in a .env file.
cat > .env <<'EOF'
BOWIRE_PUBLIC_HOST=bowire.localhost
EOF

# Fill in the four GAPs in docker-compose.yml, then:
docker compose up -d
docker compose logs -f bowire
```

Browse `https://bowire.localhost/`. (Accept Caddy's internal-CA cert on first run.)

## Run (real host)

- Point a public DNS record at the host.
- Update `BOWIRE_PUBLIC_HOST` to the public name.
- Make sure :80 + :443 are reachable so Caddy can ACME.
- Register the OIDC client at the IdP with redirect URI `https://<host>/signin-oidc`.

The OIDC bounce on first request takes you through the IdP and back; the workbench then renders with your username in the header.

---

**Back to:** [Administrator capstone](../README.md)
