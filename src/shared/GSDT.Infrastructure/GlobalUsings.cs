// Global usings for GSDT.Infrastructure
// Infrastructure is NOT under modules/Directory.Build.props so SharedKernel usings
// from that props file are NOT inherited here — they must be declared explicitly.

// ── BCL / ASP.NET Core framework ─────────────────────────────────────────────
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// ── Entity Framework Core ─────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Diagnostics;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ── ADO.NET (DbCommand, DbConnection, DbDataReader) ──────────────────────────
global using System.Data.Common;

// ── ASP.NET Core Health Checks ────────────────────────────────────────────────
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// ── ASP.NET Core Rate Limiting ────────────────────────────────────────────────
global using System.Threading.RateLimiting;
global using Microsoft.AspNetCore.RateLimiting;

// ── SQL Server / Dapper ───────────────────────────────────────────────────────
global using Microsoft.Data.SqlClient;

// ── RecyclableMemoryStream ────────────────────────────────────────────────────
global using Microsoft.IO;

// ── SharedKernel — Domain ─────────────────────────────────────────────────────
global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Errors;

// ── SharedKernel — Application ────────────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Archive;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Export;
global using GSDT.SharedKernel.Application.Search;
global using GSDT.SharedKernel.Application.Webhooks;
global using GSDT.SharedKernel.Events;
global using GSDT.SharedKernel.Extensions;

// ── SharedKernel.Contracts — IServiceIdentityProvider ────────────────────────
global using GSDT.SharedKernel.Contracts;

// ── Internal Infrastructure sub-namespaces ────────────────────────────────────
global using GSDT.Infrastructure.BackgroundJobs;
global using GSDT.Infrastructure.Caching;
global using GSDT.Infrastructure.Extensions;
global using GSDT.Infrastructure.HealthChecks;
global using GSDT.Infrastructure.Search;
global using GSDT.Infrastructure.Telemetry;
global using GSDT.Infrastructure.Configuration;
global using GSDT.Infrastructure.Export;
global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Persistence.Outbox;
global using GSDT.Infrastructure.RateLimiting;
global using GSDT.Infrastructure.Resilience;
global using GSDT.Infrastructure.Security;
global using GSDT.Infrastructure.Webhooks;
