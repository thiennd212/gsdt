// Global usings for GSDT.Api
// Main API host — aggregates all module Presentation layers.

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.OpenApi;
global using Microsoft.AspNetCore.RateLimiting;

// ── EF Core ───────────────────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;

// ── Microsoft Extensions ──────────────────────────────────────────────────────
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Behaviors;
global using GSDT.SharedKernel.Application.Webhooks;
global using GSDT.SharedKernel.Api;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Errors;
global using GSDT.SharedKernel.Events;
global using GSDT.SharedKernel.Presentation;

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure;
global using GSDT.Infrastructure.Alerting;
global using GSDT.Infrastructure.ApiKeys;
global using GSDT.Infrastructure.Backup;
global using GSDT.Infrastructure.BackgroundJobs;
global using GSDT.Infrastructure.Configuration;
global using GSDT.Infrastructure.Events;
global using GSDT.Infrastructure.HealthChecks;
global using GSDT.Infrastructure.Logging;
global using GSDT.Infrastructure.Messaging;
global using GSDT.Infrastructure.Middleware;
global using GSDT.Infrastructure.Security;
global using GSDT.Infrastructure.Telemetry;
global using GSDT.Infrastructure.Webhooks;

// ── Module Application / Infrastructure registrations ─────────────────────────
global using GSDT.Audit.Application;
global using GSDT.Audit.Infrastructure;
global using GSDT.Audit.Infrastructure.Persistence;
global using GSDT.Files.Application;
global using GSDT.Files.Infrastructure;
global using GSDT.Files.Infrastructure.Persistence;
global using GSDT.Identity.Application;
global using GSDT.Identity.Infrastructure;
global using GSDT.Identity.Infrastructure.Persistence;
global using GSDT.Integration.Application;
global using GSDT.Integration.Infrastructure;
global using GSDT.Integration.Infrastructure.Persistence;
global using GSDT.MasterData.Infrastructure;
global using GSDT.MasterData.Infrastructure.Persistence;
global using GSDT.Notifications.Application;
global using GSDT.Notifications.Infrastructure;
global using GSDT.Notifications.Infrastructure.Persistence;
global using GSDT.Notifications.Infrastructure.Realtime;
global using GSDT.Organization.Infrastructure;
global using GSDT.Organization.Infrastructure.Persistence;
global using GSDT.SystemParams.Infrastructure;
global using GSDT.SystemParams.Infrastructure.Persistence;
global using GSDT.InvestmentProjects.Infrastructure;
global using GSDT.InvestmentProjects.Infrastructure.Persistence;

// ── GSDT.Api ──────────────────────────────────────────────────────────────────
global using GSDT.Api.Controllers.Admin;
global using GSDT.Api.Gateway;
