// Global usings for GSDT.Integration.Application
// Covers Integration.Domain types, SharedKernel contracts, framework types.

// ── Framework ─────────────────────────────────────────────────────────────────
global using Microsoft.Extensions.DependencyInjection;

// ── SharedKernel application layer ───────────────────────────────────────────
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Application.Pagination;

// ── SharedKernel.Contracts — cross-module client interfaces ──────────────────
global using GSDT.SharedKernel.Contracts;

// ── Integration.Domain types ──────────────────────────────────────────────────
global using GSDT.Integration.Domain.Entities;
global using GSDT.Integration.Domain.Enums;
global using GSDT.Integration.Domain.Repositories;

// ── Integration.Application internal ─────────────────────────────────────────
global using GSDT.Integration.Application.DTOs;
