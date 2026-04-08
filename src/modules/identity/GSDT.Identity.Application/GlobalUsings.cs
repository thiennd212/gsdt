// Global usings for GSDT.Identity.Application
// Covers ASP.NET Core Authorization/Identity, caching, SharedKernel contracts,
// and all Identity.Domain types referenced across command/query handlers.

// ── ASP.NET Core Authorization ────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;

// ── ASP.NET Core Identity (UserManager, RoleManager, SignInManager) ───────────
global using Microsoft.AspNetCore.Identity;

// ── Framework utilities ───────────────────────────────────────────────────────
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

// ── SharedKernel — Domain ─────────────────────────────────────────────────────
// (GSDT.SharedKernel.Domain, .Events, .Compliance, .Errors already via Directory.Build.props)
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Pagination;

// ── Identity.Domain — entities, repositories, services, models ───────────────
global using GSDT.Identity.Domain.Entities;
global using GSDT.Identity.Domain.Events;
global using GSDT.Identity.Domain.Models;
global using GSDT.Identity.Domain.Repositories;
global using GSDT.Identity.Domain.Services;

// ── Identity.Application internal ────────────────────────────────────────────
global using GSDT.Identity.Application.Authorization;
global using GSDT.Identity.Application.DTOs;
// RevokeTokenCommand is referenced cross-namespace within the same application layer
global using GSDT.Identity.Application.Commands.RevokeToken;
