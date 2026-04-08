// Global usings for GSDT.Integration.Presentation

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;

global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Presentation;

global using GSDT.Integration.Application.DTOs;
global using GSDT.Integration.Domain.Enums;

// ── Commands ──────────────────────────────────────────────────────────────────
global using GSDT.Integration.Application.Commands.CreateContract;
global using GSDT.Integration.Application.Commands.CreateMessageLog;
global using GSDT.Integration.Application.Commands.CreatePartner;
global using GSDT.Integration.Application.Commands.DeleteContract;
global using GSDT.Integration.Application.Commands.DeletePartner;
global using GSDT.Integration.Application.Commands.UpdateContract;
global using GSDT.Integration.Application.Commands.UpdateMessageLogStatus;
global using GSDT.Integration.Application.Commands.UpdatePartner;

// ── Queries ───────────────────────────────────────────────────────────────────
global using GSDT.Integration.Application.Queries.GetContract;
global using GSDT.Integration.Application.Queries.GetMessageLog;
global using GSDT.Integration.Application.Queries.GetPartner;
global using GSDT.Integration.Application.Queries.ListContracts;
global using GSDT.Integration.Application.Queries.ListMessageLogs;
global using GSDT.Integration.Application.Queries.ListPartners;
