// Global usings for GSDT.Audit.Application
// Covers Audit.Domain entities/enums, SharedKernel application contracts,
// and framework types used across command/query handlers.

// ── Framework ─────────────────────────────────────────────────────────────────
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// ── SharedKernel application layer ───────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Pagination;

// ── SharedKernel.Contracts — cross-module client interfaces ──────────────────
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Contracts.Clients;
global using GSDT.SharedKernel.Contracts.Rtbf;

// ── SharedKernel.AI — IAiPromptTracer, AiPromptTraceInput ────────────────────
global using GSDT.SharedKernel.AI;

// ── Audit.Domain types ────────────────────────────────────────────────────────
global using GSDT.Audit.Domain.Entities;
global using GSDT.Audit.Domain.Enums;
global using GSDT.Audit.Domain.Repositories;
global using GSDT.Audit.Domain.Services;
global using GSDT.Audit.Domain.ValueObjects;

// ── Audit.Application internal ───────────────────────────────────────────────
global using GSDT.Audit.Application.DTOs;
global using GSDT.Audit.Application.Services;
// Cross-namespace command references within the same Application layer
global using GSDT.Audit.Application.Commands.LogAuditEvent;
global using GSDT.Audit.Application.Commands.CreateAiPromptTrace;
