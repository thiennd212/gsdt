// Global usings for GSDT.SystemParams.Presentation

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;

// ── EF Core ───────────────────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Presentation;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Api;

// ── SystemParams Domain (entities + enums) ────────────────────────────────────
global using GSDT.SystemParams.Domain.Entities;

// ── SystemParams Application Services ────────────────────────────────────────
// NOTE: GSDT.SharedKernel.Contracts intentionally omitted — IFeatureFlagService conflict.
// Module-local IFeatureFlagService and ISystemParamService resolve from this namespace:
global using GSDT.SystemParams.Application.Services;

// ── SystemParams Infrastructure Persistence ───────────────────────────────────
global using GSDT.SystemParams.Infrastructure.Persistence;
