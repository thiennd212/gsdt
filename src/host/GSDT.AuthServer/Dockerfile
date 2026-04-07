# GSDT.AuthServer — Multi-stage Docker build
# Base: .NET 10 Alpine | Non-root user | Port 5000
# Build context: repo root (docker build -f src/host/GSDT.AuthServer/Dockerfile .)

# ─── Stage 1: Build & Publish ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /app

# Override NuGet package root to Linux path — prevents MSB4018 caused by
# Windows-specific C:\Program Files paths in locally generated obj/*.nuget.g.props
ENV NUGET_PACKAGES=/root/.nuget/packages

# Copy MSBuild props + solution for layer-cached restore
COPY src/Directory.Packages.props src/Directory.Build.props ./
COPY src/GSDT.slnx ./

# Copy all .csproj files preserving directory structure for restore cache
COPY src/host/*/*.csproj                    ./host-projects/
COPY src/shared/*/*.csproj                  ./shared-projects/
COPY src/modules/*/*/*.csproj               ./module-projects/

# Restore .csproj structure from flat copies (dotnet restore needs correct paths)
RUN for f in host-projects/*.csproj; do \
      name=$(basename "$f" .csproj); \
      mkdir -p "host/$name" && mv "$f" "host/$name/"; \
    done && \
    for f in shared-projects/*.csproj; do \
      name=$(basename "$f" .csproj); \
      mkdir -p "shared/$name" && mv "$f" "shared/$name/"; \
    done && \
    for f in module-projects/*.csproj; do \
      name=$(basename "$f" .csproj); \
      dir=$(echo "$name" | sed 's/GSDT\.\([^.]*\).*/\1/' | tr '[:upper:]' '[:lower:]'); \
      mkdir -p "modules/$dir/$name" && mv "$f" "modules/$dir/$name/"; \
    done && \
    rm -rf host-projects shared-projects module-projects

RUN dotnet restore host/GSDT.AuthServer/GSDT.AuthServer.csproj --runtime linux-musl-x64

# Copy full source and publish
COPY src/ ./
RUN dotnet publish host/GSDT.AuthServer/GSDT.AuthServer.csproj \
        -c Release \
        -o /app/publish \
        --runtime linux-musl-x64 \
        --self-contained false \
        /p:UseAppHost=false \
        /p:NuGetAudit=false

# ─── Stage 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# OCI labels for container metadata
LABEL org.opencontainers.image.title="GSDT AuthServer" \
      org.opencontainers.image.description="Vietnamese government case management — OpenIddict authorization server" \
      org.opencontainers.image.vendor="AQT" \
      org.opencontainers.image.source="https://github.com/thiennd212/GSDT"

# Install timezone data, CA certs, and ICU libs
# ICU required by Microsoft.Data.SqlClient on Alpine (Globalization Invariant Mode not supported)
RUN apk add --no-cache tzdata ca-certificates icu-libs && \
    update-ca-certificates

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

# Create non-root user for security compliance (NĐ85/TT12 hardening)
RUN addgroup -S appgroup && adduser -S appuser -G appgroup -H -D

# Copy published output from build stage
COPY --from=build --chown=appuser:appgroup /app/publish ./

# Read-only tmp directory for ASP.NET Core temp files
RUN mkdir -p /tmp/asp && chown appuser:appgroup /tmp/asp

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_GCConserveMemory=9 \
    DOTNET_EnableDiagnostics=0 \
    TMPDIR=/tmp/asp

# Health check — Docker/K8s can detect unhealthy containers
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

USER appuser

ENTRYPOINT ["dotnet", "GSDT.AuthServer.dll"]
