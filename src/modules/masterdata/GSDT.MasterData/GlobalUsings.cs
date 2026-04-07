// Global usings for GSDT.MasterData (combined module — domain + app + infra + presentation)

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.OutputCaching;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Caching.Memory;
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
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Pagination;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Errors;
global using GSDT.SharedKernel.Api;
global using GSDT.SharedKernel.Presentation;

// ── Infrastructure ────────────────────────────────────────────────────────────
global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;

// ── Module internal namespaces ────────────────────────────────────────────────
global using GSDT.MasterData.Entities;
global using GSDT.MasterData.Entities.Catalogs;
global using GSDT.MasterData.Events;
global using GSDT.MasterData.DTOs;
global using GSDT.MasterData.Commands.CreateDictionary;
global using GSDT.MasterData.Commands.CreateDictionaryItem;
global using GSDT.MasterData.Commands.CreateExternalMapping;
global using GSDT.MasterData.Commands.PublishDictionary;
global using GSDT.MasterData.Commands.UpdateDictionary;
global using GSDT.MasterData.Queries.GetDictionaries;
global using GSDT.MasterData.Queries.GetDictionaryItems;
global using GSDT.MasterData.Queries.GetExternalMappings;
global using GSDT.MasterData.Persistence;
