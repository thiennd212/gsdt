// Global usings for GSDT.Identity.Infrastructure
// EF Core, ASP.NET Core Identity, SharedKernel, and Identity domain types.

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Identity;
// NOTE: Microsoft.AspNetCore.Identity.EntityFrameworkCore is NOT globally imported.
// Adding it globally would make IdentityDbContext<T,T,T> ambiguous with the project's own
// GSDT.Identity.Infrastructure.Persistence.IdentityDbContext class.
// It is implicitly available at the IdentityDbContext.cs class level via the project reference.

// ── EF Core ───────────────────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;

// ── Microsoft Extensions ──────────────────────────────────────────────────────
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Contracts.Clients;
global using GSDT.SharedKernel.Contracts.Rtbf;

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;
global using GSDT.Infrastructure.Configuration;

// ── Identity Domain ───────────────────────────────────────────────────────────
global using GSDT.Identity.Domain.Entities;
global using GSDT.Identity.Domain.Repositories;
global using GSDT.Identity.Domain.Services;
global using GSDT.Identity.Domain.Models;

// ── Identity Application ──────────────────────────────────────────────────────
global using GSDT.Identity.Application.Authorization;
global using GSDT.Identity.Application.DTOs;
global using GSDT.Identity.Application.Services;

// ── Identity Application Commands ────────────────────────────────────────────
global using GSDT.Identity.Application.Commands.ManageGroup;
global using GSDT.Identity.Application.Commands.ManageMenu;
global using GSDT.Identity.Application.Commands.ManageDataScope;
global using GSDT.Identity.Application.Commands.ManagePolicyRule;
global using GSDT.Identity.Application.Commands.ManageRolePermission;
global using GSDT.Identity.Application.Commands.ManageSodRule;

// ── Identity Application Queries ──────────────────────────────────────────────
global using GSDT.Identity.Application.Queries.GetGroupById;
global using GSDT.Identity.Application.Queries.GetPermissions;
global using GSDT.Identity.Application.Queries.GetRoleDataScopes;
global using GSDT.Identity.Application.Queries.GetRolePermissions;
global using GSDT.Identity.Application.Queries.ListMenus;
global using GSDT.Identity.Application.Queries.ListDataScopeTypes;
global using GSDT.Identity.Application.Queries.ListGroups;
global using GSDT.Identity.Application.Queries.ListSodRules;
global using GSDT.Identity.Application.Queries.ListPolicyRules;

// ── Identity Infrastructure ───────────────────────────────────────────────────
global using GSDT.Identity.Infrastructure.Persistence;
global using GSDT.Identity.Infrastructure.Services;
global using GSDT.Identity.Infrastructure.Authorization;
global using GSDT.Identity.Infrastructure.Jobs;
global using GSDT.Identity.Infrastructure.VNeID;
