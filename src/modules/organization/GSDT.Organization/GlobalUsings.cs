// Global usings for GSDT.Organization (combined module — domain + app + infra + presentation)

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

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
global using GSDT.SharedKernel.Errors;
global using GSDT.SharedKernel.Presentation;

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;

// ── Module internal namespaces ────────────────────────────────────────────────
global using GSDT.Organization.Entities;
global using GSDT.Organization.DTOs;
global using GSDT.Organization.Commands;
global using GSDT.Organization.Queries;
global using GSDT.Organization.Persistence;
