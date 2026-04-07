// Global usings for GSDT.Notifications.Application
// Covers Notifications.Domain types, SharedKernel contracts, framework types.

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

// ── Notifications.Domain types ────────────────────────────────────────────────
global using GSDT.Notifications.Domain.Entities;
global using GSDT.Notifications.Domain.Repositories;
global using GSDT.Notifications.Domain.ValueObjects;

// ── Notifications.Application internal ───────────────────────────────────────
global using GSDT.Notifications.Application.DTOs;
global using GSDT.Notifications.Application.Clients;
global using GSDT.Notifications.Application.Providers;
// Cross-namespace command reference within the same Application layer
global using GSDT.Notifications.Application.Commands.SendNotification;
