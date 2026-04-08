// Global usings for GSDT.Files.Application
// Covers Files.Domain entities/enums, SharedKernel application contracts,
// and framework types used across command/query handlers.

// ── Framework ─────────────────────────────────────────────────────────────────
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// ── SharedKernel application layer ───────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Pagination;
global using GSDT.SharedKernel.Domain.Repositories;

// ── Files.Domain types ────────────────────────────────────────────────────────
global using GSDT.Files.Domain.Entities;
global using GSDT.Files.Domain.Repositories;
global using GSDT.Files.Domain.Services;

// ── Files.Application internal ───────────────────────────────────────────────
global using GSDT.Files.Application.DTOs;
global using GSDT.Files.Application.Jobs;
global using GSDT.Files.Application.Options;
// Cross-handler reference: Publish/Update handlers call CreateDocumentTemplateCommandHandler.MapToDto
global using GSDT.Files.Application.Commands.CreateDocumentTemplate;
