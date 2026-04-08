// Global usings for GSDT.SystemParams (combined module — domain + services + presentation)

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Errors;
global using GSDT.SharedKernel.Api;
global using GSDT.SharedKernel.Presentation;
// NOTE: GSDT.SharedKernel.Contracts intentionally omitted — IFeatureFlagService conflicts
//       with GSDT.SystemParams.Services.IFeatureFlagService. Use fully-qualified
//       SharedKernel.Contracts.IFeatureFlagService in SystemParamsRegistration.cs.

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;

// ── Module internal namespaces ────────────────────────────────────────────────
global using GSDT.SystemParams.Entities;
global using GSDT.SystemParams.Services;
global using GSDT.SystemParams.Controllers;
global using GSDT.SystemParams.Persistence;
