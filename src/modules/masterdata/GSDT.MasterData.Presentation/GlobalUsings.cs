// Global usings for GSDT.MasterData.Presentation

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;

// ── SharedKernel ──────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.OutputCaching;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;

global using GSDT.SharedKernel.Presentation;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Api;

global using GSDT.MasterData.Infrastructure.Persistence;
global using GSDT.MasterData.Infrastructure.Services;

// ── MasterData Domain ─────────────────────────────────────────────────────────
global using GSDT.MasterData.Domain.Entities;

// ── MasterData Application Commands ──────────────────────────────────────────
global using GSDT.MasterData.Application.Commands.CreateDictionary;
global using GSDT.MasterData.Application.Commands.CreateDictionaryItem;
global using GSDT.MasterData.Application.Commands.CreateExternalMapping;
global using GSDT.MasterData.Application.Commands.PublishDictionary;
global using GSDT.MasterData.Application.Commands.UpdateDictionary;

// ── MasterData Application Queries ───────────────────────────────────────────
global using GSDT.MasterData.Application.Queries.GetDictionaries;
global using GSDT.MasterData.Application.Queries.GetDictionaryItems;
global using GSDT.MasterData.Application.Queries.GetExternalMappings;

// ── MasterData Application DTOs ───────────────────────────────────────────────
global using GSDT.MasterData.Application.DTOs;
