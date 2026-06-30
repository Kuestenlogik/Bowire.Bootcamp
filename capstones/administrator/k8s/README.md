# Administrator capstone — Kubernetes starter (optional second track)

The k8s track mirrors the compose track 1:1. Same five workloads, same env vars, same `Bowire:*` configuration shape. Pick this track if your team already runs on k8s; otherwise stick with [`compose/`](../compose/).

## Manifests

| File | What it deploys |
| --- | --- |
| `00-namespace.yaml` | Namespace `bowire`. Everything below lives here. |
| `10-bowire.yaml` | Deployment + Service for the Tool. ConfigMap with the four `# GAP:` env vars + PVC for `~/.bowire/`. |
| `20-tempo.yaml` | Deployment + Service for Tempo (OTLP/gRPC on `:4317`). |
| `30-loki.yaml` | Deployment + Service for Loki. |
| `40-grafana.yaml` | Deployment + Service for Grafana, provisioned with Tempo + Loki datasources. |
| `50-ingress.yaml` | Ingress (assumes a controller — nginx-ingress or Traefik). Terminates TLS via cert-manager (Let's Encrypt). |

## Run

```bash
# Fill in the GAPs in 10-bowire.yaml (ConfigMap + Secret).
kubectl apply -f .
kubectl -n bowire get pods
kubectl -n bowire logs deploy/bowire -f
```

Browse `https://<host>/`. Same OIDC flow as the compose track.

## Things to verify against your cluster

- **PVC StorageClass.** The `bowire-data` PVC defaults to the cluster's `default` StorageClass — set it explicitly if your cluster has multiple.
- **Ingress controller annotations.** The starter targets nginx-ingress. For Traefik, swap the ingressClassName + relevant annotations.
- **cert-manager.** The Ingress assumes a `ClusterIssuer` named `letsencrypt-prod`. Swap to your issuer or wire your own TLS Secret.
- **Pod Security Standards.** Bowire's image is non-root and chiseled by default (`Kuestenlogik.Bowire.Tool.csproj::ContainerFamily=noble-chiseled-extra`). It should pass `restricted` without changes.

The reference solution under [`../solution/k8s/`](../solution/) populates every gap for a kind / k3d cluster, with notes on adapting to a real cluster.

---

**Back to:** [Administrator capstone](../README.md)
