// Global usings for GSDT.Tests.Integration
// Integration tests use WebApplicationFactory + Testcontainers

// ── .NET ──────────────────────────────────────────────────────────────────────
global using System.Net.Http.Json;
global using System.Security.Claims;

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.AspNetCore.Hosting;

// ── EF Core ───────────────────────────────────────────────────────────────────
global using Microsoft.EntityFrameworkCore;

// ── Microsoft Extensions ──────────────────────────────────────────────────────
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Domain;

// ── SharedKernel.Contracts ────────────────────────────────────────────────────
global using GSDT.SharedKernel.Contracts;

// ── Infrastructure DbContexts ─────────────────────────────────────────────────
global using GSDT.Infrastructure.ApiKeys;
global using GSDT.Infrastructure.Messaging;
global using GSDT.Infrastructure.Webhooks;

// ── Module DbContexts ─────────────────────────────────────────────────────────
global using GSDT.Audit.Infrastructure.Persistence;
global using GSDT.Files.Infrastructure.Persistence;
global using GSDT.Identity.Infrastructure.Persistence;
global using GSDT.MasterData.Infrastructure.Persistence;
global using GSDT.Notifications.Infrastructure.Persistence;
global using GSDT.Organization.Infrastructure.Persistence;
global using GSDT.SystemParams.Infrastructure.Persistence;

// ── Module Seeders ────────────────────────────────────────────────────────────
global using GSDT.MasterData.Infrastructure.Services;
global using GSDT.SystemParams.Infrastructure.Services;

// ── StackExchange.Redis ───────────────────────────────────────────────────────
global using StackExchange.Redis;

// ── xunit ─────────────────────────────────────────────────────────────────────
global using Xunit;

// ── Test Infrastructure ───────────────────────────────────────────────────────
global using GSDT.Tests.Integration.Infrastructure;
