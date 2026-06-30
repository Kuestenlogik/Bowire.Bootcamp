#!/usr/bin/env bash
# smoke-test.sh — Administrator capstone smoke test.
#
# Runs eight checks against a deployed Bowire stack from OUTSIDE the host.
# Needs: curl, openssl, jq, bash >= 4. Does NOT need docker-compose or
# kubectl on the runner.
#
# Usage:
#   ./smoke-test.sh https://bowire.team.example [bearer-token]
#
# A bearer token is optional; when provided, the /api/plugins check
# uses it. Without one, /api/plugins returns 401 and the check skips.
#
# Exit code: 0 if every required check passed, 1 otherwise.

set -uo pipefail

HOST="${1:-}"
TOKEN="${2:-}"
if [[ -z "$HOST" ]]; then
    echo "usage: $0 <https://host> [bearer-token]" >&2
    exit 64
fi

PASS=0
FAIL=0
SKIP=0

ok()   { echo "PASS  $*"; PASS=$((PASS+1)); }
bad()  { echo "FAIL  $*"; FAIL=$((FAIL+1)); }
skip() { echo "SKIP  $*"; SKIP=$((SKIP+1)); }

hostname_only() {
    # Strip scheme + path so we can feed it to openssl s_client.
    echo "$1" | sed -E 's|^https?://||; s|/.*||'
}

# -----------------------------------------------------------------
# 1. TLS handshake completes + cert is valid.
# -----------------------------------------------------------------
HN=$(hostname_only "$HOST")
if echo | openssl s_client -connect "${HN}:443" -servername "$HN" -verify_return_error 2>/dev/null \
        | openssl x509 -noout -dates >/dev/null 2>&1; then
    ok "TLS handshake + cert chain valid for $HN"
else
    bad "TLS handshake / cert chain failed for $HN"
fi

# -----------------------------------------------------------------
# 2. /api/health responds 200 OK.
#    Note: under OIDC, the workbench's /api/health may itself be
#    gated. Bowire's standalone /api/health is reachable without
#    auth (it's the liveness probe used by readinessProbe in the
#    k8s manifest); we accept 200 or 401 here, where 401 means the
#    deploy gated it deliberately (also a valid posture).
# -----------------------------------------------------------------
CODE=$(curl -sS -o /dev/null -w '%{http_code}' "$HOST/api/health" --max-time 5 || echo "000")
case "$CODE" in
    200) ok "/api/health 200 OK" ;;
    401|403) ok "/api/health gated ($CODE) — auth-required posture" ;;
    *) bad "/api/health unexpected response: $CODE" ;;
esac

# -----------------------------------------------------------------
# 3. OIDC redirect: anonymous GET / should bounce to the IdP.
#    A 302 with a Location header pointing at the configured
#    Bowire:Auth:Oidc:Authority is the success signal.
# -----------------------------------------------------------------
LOC=$(curl -sS -o /dev/null -D - -w '%{http_code}\n' "$HOST/" --max-time 5 | tr -d '\r')
HTTP=$(echo "$LOC" | tail -n1)
LOC_HEADER=$(echo "$LOC" | awk '/^[Ll]ocation:/ {print $2; exit}')
if [[ "$HTTP" =~ ^30[12]$ ]] && [[ "$LOC_HEADER" =~ ^https?:// ]]; then
    ok "OIDC redirect: $HTTP -> $(echo "$LOC_HEADER" | cut -c1-60)..."
elif [[ "$HTTP" == "200" ]]; then
    # Deploy may serve the workbench shell anonymously and only gate
    # /api/*. That's also valid — Bowire's HTML stays anonymous by
    # default per OidcAuthProvider.cs's "Phase-A scope" comment.
    skip "OIDC redirect: anonymous shell served (HTML is intentionally anonymous; /api/* is gated)"
else
    bad "OIDC redirect: unexpected $HTTP / Location=$LOC_HEADER"
fi

# -----------------------------------------------------------------
# 4. Plugin disable list enforced.
#    Reads /api/plugins (auth-gated). Verifies SOAP + Pulsar are
#    NOT in the list. Skips when no bearer token was supplied.
# -----------------------------------------------------------------
if [[ -n "$TOKEN" ]]; then
    BODY=$(curl -fsS "$HOST/api/plugins" -H "Authorization: Bearer $TOKEN" --max-time 5 || echo "")
    if [[ -z "$BODY" ]]; then
        bad "Plugin disable list: /api/plugins did not respond"
    elif echo "$BODY" | jq -e '.[] | select(.id == "soap" or .id == "pulsar")' >/dev/null 2>&1; then
        bad "Plugin disable list: SOAP or Pulsar present in /api/plugins"
    else
        ok "Plugin disable list: neither SOAP nor Pulsar present"
    fi
else
    skip "Plugin disable list: no bearer token supplied"
fi

# -----------------------------------------------------------------
# 5. Strict-Transport-Security header set.
# -----------------------------------------------------------------
HSTS=$(curl -sIS "$HOST/" --max-time 5 | grep -i '^strict-transport-security:' | head -n1)
if [[ -n "$HSTS" ]]; then
    ok "HSTS header: $(echo "$HSTS" | tr -d '\r')"
else
    bad "HSTS header missing — Caddy / Ingress not setting Strict-Transport-Security"
fi

# -----------------------------------------------------------------
# 6. X-Content-Type-Options header set (basic hardening).
# -----------------------------------------------------------------
XCTO=$(curl -sIS "$HOST/" --max-time 5 | grep -i '^x-content-type-options:' | head -n1)
if [[ -n "$XCTO" ]]; then
    ok "X-Content-Type-Options header set"
else
    bad "X-Content-Type-Options header missing"
fi

# -----------------------------------------------------------------
# 7. Server header DOES NOT leak the upstream version.
#    A reverse proxy in front should mask the Kestrel banner.
# -----------------------------------------------------------------
SERVER=$(curl -sIS "$HOST/" --max-time 5 | grep -i '^server:' | head -n1 | tr -d '\r')
if [[ "$SERVER" =~ [Kk]estrel ]]; then
    bad "Server header leaks Kestrel — reverse proxy not stripping ($SERVER)"
elif [[ -n "$SERVER" ]]; then
    ok "Server header neutral: $SERVER"
else
    ok "Server header absent (proxy stripped it)"
fi

# -----------------------------------------------------------------
# 8. Telemetry endpoint reachable from inside.
#    Bowire emits to OTLP/gRPC; we can't dial Tempo from outside.
#    Instead: confirm that Bowire's /api/health round-trip leaves a
#    span trace ID via response headers when the OTel ASP.NET
#    instrumentation is wired (traceresponse / traceparent).
# -----------------------------------------------------------------
HEADERS=$(curl -sIS "$HOST/api/health" --max-time 5 | tr -d '\r')
if echo "$HEADERS" | grep -qiE '^(traceresponse|traceparent):'; then
    ok "Telemetry: traceparent/traceresponse header present (OTel instrumentation wired)"
else
    skip "Telemetry: no traceparent header on /api/health (acceptable; tracing may be sampled out)"
fi

# -----------------------------------------------------------------
echo
echo "Result: $PASS passed, $FAIL failed, $SKIP skipped"
[[ "$FAIL" -eq 0 ]] && exit 0 || exit 1
