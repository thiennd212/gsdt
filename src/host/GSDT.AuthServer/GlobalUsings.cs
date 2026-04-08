// Global usings for GSDT.AuthServer
// OpenIddict-based OAuth/OIDC authorization server host.

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;

// ── EF Core ───────────────────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;

// ── Microsoft Extensions ──────────────────────────────────────────────────────
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Domain;

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure.Caching;
global using GSDT.Infrastructure.Security;

// ── Identity Domain ───────────────────────────────────────────────────────────
global using GSDT.Identity.Domain.Entities;
global using GSDT.Identity.Domain.Repositories;

// ── Identity Infrastructure (includes IdentityDbContext) ─────────────────────
global using GSDT.Identity.Infrastructure.Persistence;
global using GSDT.Identity.Infrastructure.Services;

// ── Audit Domain ─────────────────────────────────────────────────────────────
global using GSDT.Audit.Domain.Services;

// ── AuthServer namespaces ─────────────────────────────────────────────────────
global using GSDT.AuthServer;
global using GSDT.AuthServer.Models;
global using GSDT.AuthServer.Services;
