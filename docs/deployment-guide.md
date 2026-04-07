# Deployment Guide

## Overview

GSDT ships as a containerized .NET 10 backend + separate AuthServer:
- **`gsdt-api`** — REST API host (identity + cases + files + notifications + audit + masterdata + forms + ai + workflow + reporting modules)
- **`gsdt-authserver`** — OpenIddict 7.4.0 OIDC/OAuth2 server (separate deployment, AuthorizationController with PKCE + ROPC + login page)
- **Nginx gateway** — Reverse proxy for SPA + API routing (YARP disabled in Phase 1)

Deployments use GitHub Actions CI/CD → Docker Registry → Kubernetes 1.35+ via Helm charts. Secrets pulled from HashiCorp Vault at runtime.

**Production Readiness (P1):** Phase 11 complete — CI/CD, Vault integration, Docker hardening, all verified in test suite.

---

## 0. CI/CD Pipeline (GitHub Actions)

### GitHub Workflow (`.github/workflows/ci.yml`)

Fully automated pipeline triggered on every push:

**Stages:**
1. **Build & Test** (Ubuntu runner)
   - Checkout code
   - Restore NuGet packages from nuget.org + Nexus
   - `dotnet build` (Release config, treat warnings as errors)
   - `dotnet test` — 428+ tests (269 unit + 33 module integration + 96 legacy integration + 13 E2E Playwright + 7 k6 profiles)
   - Upload coverage to SonarQube

2. **Security Scanning (SAST)**
   - SonarQube analysis (OWASP rules, CWE, CVE checks)
   - Fail pipeline if critical findings

3. **Docker Build & Push** (V2: Parallel jobs)
   - **Backend job:** Multi-stage Dockerfile for API
     - Push to Docker registry: `docker.io/aequitas/gsdt-api:${GITHUB_SHA:0:7}`
   - **AuthServer job:** Multi-stage Dockerfile for AuthServer
     - Push to Docker registry: `docker.io/aequitas/gsdt-authserver:${GITHUB_SHA:0:7}`
   - **Frontend job:** Node build + Nginx (NEW V2)
     - Push to Docker registry: `docker.io/aequitas/gsdt-web:${GITHUB_SHA:0:7}`
   - All images also tagged as `:latest`

4. **E2E Testing** (NEW V2)
   - Docker Compose stack: API, AuthServer, DB, Redis, RabbitMQ, MinIO
   - Playwright E2E tests (smoke tests + happy path scenarios)
   - k6 performance tests (load, spike, soak)

5. **Artifact & SBOM**
   - CycloneDX SBOM (Bill of Materials) generated
   - Published to GitHub Releases

### Environment Variables (Secrets)

Store in GitHub `Settings → Secrets → Actions`:

| Secret | Used For | Example |
|--------|----------|---------|
| `DOCKER_REGISTRY_URL` | Container registry | `docker.io` |
| `DOCKER_USERNAME` | Auth for push | `aequitas` |
| `DOCKER_PASSWORD` | Registry credentials | (GitHub-managed) |
| `SONARQUBE_TOKEN` | SAST scanning | (GH secret) |
| `K6_TEST_APIKEY` | Load test auth | (Seeded in dev env) |

**Important:** No secrets in code or config files. All secrets injected via CI/CD environment.

---

## 1. Container Build (Multi-stage Dockerfile)

```dockerfile
# ── Stage 1: Build ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/GSDT.slnx", "."]
COPY ["src/", "."]

# Full restore cache: RUN before COPY (enables layer reuse)
RUN dotnet restore GSDT.slnx

RUN dotnet publish src/host/GSDT.Api/GSDT.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# OCI Labels (metadata)
LABEL org.opencontainers.image.title="GSDT API" \
      org.opencontainers.image.version="1.0.0" \
      org.opencontainers.image.created="2026-03-17" \
      org.opencontainers.image.vendor="AEQUITAS"

# Non-root user (security hardening, CVE mitigation)
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

# Health check (K8s readiness probe integration)
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/ready || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "GSDT.Api.dll"]
```

Build and push:
```bash
docker build -t nexus.internal/gsdt-api:${VERSION} -f Dockerfile.Api .
docker push nexus.internal/gsdt-api:${VERSION}
```

---

## 2. Kubernetes Deployment (Helm)

### Chart structure (V2)
```
helm/
├── gsdt-api/
│   ├── Chart.yaml
│   ├── values.yaml
│   └── templates/
│       ├── deployment.yaml
│       ├── service.yaml
│       ├── ingress.yaml
│       ├── configmap.yaml
│       └── db-migration-job.yaml
└── gsdt-authserver/
    ├── Chart.yaml
    ├── values.yaml
    └── templates/
        ├── deployment.yaml
        ├── service.yaml
        ├── ingress.yaml
        ├── configmap.yaml
        └── secret.yaml
```

**AuthServer Helm (NEW V2):**
Separate deployment for OpenIddict OIDC/OAuth2 server. Deploy with:
```bash
helm upgrade --install gsdt-authserver ./helm/gsdt-authserver \
  --namespace gov-apps \
  --set image.tag=${VERSION} \
  --set replicaCount=2 \
  --set ingress.host=auth.gov.internal \
  --values helm/gsdt-authserver/values.prod.yaml \
  --wait --timeout 5m
```

**AuthServer Key Differences:**
- Image: `docker.io/aequitas/gsdt-authserver:${VERSION}`
- Service port: 5000 (HTTPS in prod, HTTP in dev)
- Ingress: `auth.gov.internal` (separate host from API `api.gov.internal`)
- ConfigMap: OpenIddict client configurations, allowed redirect URIs, OIDC scopes
- Secret: JWT signing key (RS256 private key), database credentials

### Deploy
```bash
helm upgrade --install gsdt-api ./helm/gsdt-api \
  --namespace gov-apps \
  --set image.tag=${VERSION} \
  --set replicaCount=2 \
  --values helm/gsdt-api/values.prod.yaml \
  --wait --timeout 5m
```

### Key values.yaml fields
```yaml
image:
  repository: nexus.internal/gsdt-api
  tag: "1.0.0"
  pullPolicy: IfNotPresent

replicaCount: 2

resources:
  requests:
    cpu: "250m"
    memory: "512Mi"
  limits:
    cpu: "1000m"
    memory: "1Gi"

vault:
  address: "https://vault.internal:8200"
  role: "gsdt-api"   # Vault Kubernetes auth role

ingress:
  enabled: true
  host: api.gov.internal
  tls: true
```

---

## 3. Environment Variables Reference

All secrets come from Vault. Non-secret config is in `ConfigMap`:

| Variable | Source | Description | Docker Dev |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap | `Production` | `Development` |
| `ASPNETCORE_URLS` | ConfigMap | HTTP binding | `http://+:5001` for API, `http://+:5000` for AuthServer |
| `Vault__Address` | ConfigMap | Vault server URL | Not needed in docker compose dev |
| `Vault__Role` | ConfigMap | Kubernetes auth role | Not needed in docker compose dev |
| `ConnectionStrings__Default` | Vault | SQL Server connection string | `Server=sqlserver;Database=GSDT;...` |
| `ConnectionStrings__Redis` | Vault | Redis connection string | `redis:6379` |
| `OpenIddict__Authority` | ConfigMap | AuthServer IssuerUri | `http://host.docker.internal:5000` (Docker), `http://localhost:5002` (dev) |
| `OpenIddict__ClientId` | ConfigMap | API client ID | API specific ID |
| `OpenIddict__ClientSecret` | Vault | API client secret | Dev token only |
| `Jwt__SigningKey` | Vault | RS256 private key (PEM) | Dev key only |
| `Minio__AccessKey` | Vault | MinIO access key | `minioadmin` (dev) |
| `Minio__SecretKey` | Vault | MinIO secret key | `minioadmin` (dev) |
| `RabbitMQ__Password` | Vault | RabbitMQ password | `guest` (dev) |

**Docker Note:** AuthServer uses HTTP (no HTTPS in dev); IssuerUri discovery uses `host.docker.internal:5000` from within containers.

---

## 4. Database Migration Strategy

Uses **expand-contract** pattern for zero-downtime deployments.

### Pattern
1. **Expand** — add new columns as nullable, add new tables (backward compatible)
2. **Deploy** new application version (reads both old and new schema)
3. **Contract** — drop old columns/tables (in a follow-up release)

### K8s init container approach
```yaml
# In Helm deployment.yaml
initContainers:
  - name: db-migrate
    image: nexus.internal/gsdt-api:{{ .Values.image.tag }}
    command: ["dotnet", "GSDT.Api.dll", "--migrate-only"]
    env:
      - name: RUN_MIGRATIONS_ONLY
        value: "true"
```

The `--migrate-only` flag runs `dbContext.Database.MigrateAsync()` for all module contexts then exits. The main container starts only after the init container succeeds.

---

## 5. Secrets Management (HashiCorp Vault)

### Setup & Architecture

Vault replaces Kubernetes Secrets (which are base64-encoded, not encrypted). Secrets are:
- Stored in Vault transit engine (encrypted at rest)
- Injected by Vault Agent sidecar at pod startup
- Never written to disk
- Automatically rotated (database creds: 24h, API keys: 7d)

### Configuration in GSDT

**`appsettings.Production.json`:** Sanitized example (no secrets)
```json
{
  "Vault": {
    "Address": "https://vault.internal:8200",
    "Role": "gsdt-api",
    "Namespace": "admin"
  }
}
```

**`Program.cs`:** VaultSharp configuration provider
```csharp
var vaultAddress = builder.Configuration["Vault:Address"];
var vaultRole = builder.Configuration["Vault:Role"];

var authMethod = new KubernetesAuthMethodInfo(role: vaultRole);
var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
var vaultClient = new VaultClient(vaultClientSettings);

// Mount secrets to configuration
builder.Configuration.AddVault(vaultClient, "secret/gsdt");
```

### Vault Secret Paths

| Path | Content | Rotation |
|------|---------|----------|
| `secret/gsdt/database` | SQL Server connection string, SA password | 24h (auto) |
| `secret/gsdt/redis` | Redis password, connection string | 7d (manual) |
| `secret/gsdt/minio` | AccessKey, SecretKey | 7d (manual) |
| `secret/gsdt/rabbitmq` | Username, password | 24h (auto) |
| `secret/gsdt/jwt` | RS256 signing key (PEM) | 90d (manual) |
| `secret/gsdt/email` | MailKit SMTP password | 30d (manual) |

### Kubernetes Auth Setup

```bash
# 1. Configure Vault Kubernetes auth method
vault auth enable kubernetes

vault write auth/kubernetes/config \
    kubernetes_host="https://10.0.0.1:443" \
    kubernetes_ca_cert=@/var/run/secrets/kubernetes.io/serviceaccount/ca.crt \
    token_reviewer_jwt=@/var/run/secrets/kubernetes.io/serviceaccount/token

# 2. Create policy for API
vault policy write gsdt-api - <<EOF
path "secret/data/gsdt/*" {
  capabilities = ["read"]
}
path "transit/encrypt/gsdt" {
  capabilities = ["update"]
}
path "transit/decrypt/gsdt" {
  capabilities = ["update"]
}
EOF

# 3. Create role binding
vault write auth/kubernetes/role/gsdt-api \
    bound_service_account_names=gsdt-api \
    bound_service_account_namespaces=gov-apps \
    policies=gsdt-api \
    ttl=1h

# 4. Seed initial secrets
vault kv put secret/gsdt/database \
    connection_string="Server=sql.internal;Database=GSDT;..." \
    sa_password="..."

vault kv put secret/gsdt/redis \
    connection_string="redis.internal:6379" \
    password="..."
```

### Helm Values for Vault Integration

```yaml
# helm/gsdt-api/values.prod.yaml
vault:
  enabled: true
  address: "https://vault.internal:8200"
  role: "gsdt-api"
  secretPath: "secret/gsdt"
  syncInterval: "60s"

podAnnotations:
  vault.hashicorp.com/agent-inject: "true"
  vault.hashicorp.com/agent-inject-secret-database: "secret/data/gsdt/database"
  vault.hashicorp.com/agent-inject-template-database: |
    {{- with secret "secret/data/gsdt/database" -}}
    export ConnectionStrings__Default="{{ .Data.data.connection_string }}"
    {{- end }}
  vault.hashicorp.com/agent-inject-secret-redis: "secret/data/gsdt/redis"
  vault.hashicorp.com/role: "gsdt-api"
```

---

## 6. Health Check Endpoints

| Endpoint | Description | Used by |
|---|---|---|
| `GET /health/live` | Liveness — is the process alive? | K8s liveness probe |
| `GET /health/ready` | Readiness — DB + Redis + RabbitMQ healthy? | K8s readiness probe |
| `GET /health/startup` | Startup — migrations complete? | K8s startup probe |

K8s probe config:
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 15

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 20
  periodSeconds: 10
```

---

## 7. Blue-Green Deployment

```bash
# Deploy new version as "green" alongside existing "blue"
helm upgrade --install gsdt-api-green ./helm/gsdt-api \
  --set image.tag=${NEW_VERSION} \
  --set ingress.enabled=false   # no traffic yet

# Run smoke tests against green
./scripts/smoke-test.sh https://green.api.gov.internal

# Switch traffic (update ingress to point to green service)
kubectl patch ingress gsdt-api \
  -p '{"spec":{"rules":[{"host":"api.gov.internal","http":{"paths":[{"backend":{"service":{"name":"gsdt-api-green"}}}]}}]}}'

# Confirm, then remove blue
helm uninstall gsdt-api-blue
```

---

## 8. CI/CD Pipeline (Jenkins)

All deployments triggered via Jenkins declarative pipelines in `jenkinsfile/`.

### Pipeline Stages
1. **Build** — `dotnet build` + SAST scan (SonarQube)
2. **Test** — `dotnet test` (unit + integration + architecture)
3. **Security** — DAST (OWASP ZAP), SBOM generation (CycloneDX)
4. **Package** — Docker build + push to Nexus registry
5. **Deploy** — Helm upgrade to staging, then approval for production
6. **Verify** — k6 load tests, smoke tests, security checks

Jenkins credentials: Pull from Vault at pipeline runtime. No plaintext secrets in Jenkinsfile.

---

## 8.1 k6 Load Testing Configuration

### Test API Key Seeding

For dev/staging environments, seed a non-expiring test API key:

```bash
# Start API with seeding enabled
docker compose up -d
export SEED_TEST_APIKEY=true
export TEST_TENANT_ID="<tenant-id>"

dotnet run --project src/host/GSDT.Api
# Output logs will show:
# ApiKeyTestSeeder: k6 test key created. Store in CI secret K6_TEST_APIKEY: <key>
```

Store the plaintext key in CI/CD secrets:
- **GitHub:** `Settings → Secrets → Actions → K6_TEST_APIKEY`
- **Jenkins:** `Credentials → k6-test-apikey`

### Run k6 Tests

```bash
k6 run tests/performance/load-test.js \
  --env API_KEY="${K6_TEST_APIKEY}" \
  --vus 50 --duration 5m
```

Supported test types:
- `load-test.js` — steady-state (50 VUs, 5 min)
- `spike-test.js` — sudden burst (100→500 VUs)
- `soak-test.js` — endurance (10 VUs, 70 min)
- Module-specific: `audit-load-test.js`, `identity-load-test.js`, `files-load-test.js`, `notifications-load-test.js`

Key metrics to monitor:
- `http_req_duration` (p95) — target <500ms
- `http_req_failed` — target 0%
- `grpc_conn_errors` — indicates connection pool exhaustion

---

## 9. Observability

| Tool | URL | What to check |
|---|---|---|
| Grafana | https://grafana.internal | API latency P99, error rate, DB query time |
| Prometheus | https://prometheus.internal | Scrapes `/metrics` endpoint |
| Seq | https://seq.internal | Structured logs — filter by `CorrelationId` |
| Hangfire | https://api.gov.internal/hangfire | Background job status |

Key dashboards: `GSDT API Overview`, `Database Performance`, `MassTransit Consumers`.

Alert thresholds: P99 > 2s, error rate > 1%, DB connection pool > 80%.
