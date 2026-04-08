// Global usings for GSDT.Audit.Presentation

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;
global using System.ComponentModel.DataAnnotations;

global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Presentation;

global using GSDT.Audit.Domain.Enums;
global using GSDT.Audit.Domain.ValueObjects;
global using GSDT.Audit.Application.Commands.CreateRtbfRequest;
global using GSDT.Audit.Application.Commands.ProcessRtbfRequest;
global using GSDT.Audit.Application.Commands.RejectRtbfRequest;
global using GSDT.Audit.Application.Commands.ReportSecurityIncident;
global using GSDT.Audit.Application.Commands.UpdateIncidentStatus;
global using GSDT.Audit.Application.Queries.GetAuditLogs;
global using GSDT.Audit.Application.Queries.GetAuditStatistics;
global using GSDT.Audit.Application.Queries.GetRtbfRequests;
global using GSDT.Audit.Application.Queries.GetLoginAudit;
global using GSDT.Audit.Application.Queries.GetSecurityIncidents;
global using GSDT.Audit.Application.Queries.ExportAuditLogs;
global using GSDT.Audit.Application.Queries.VerifyAuditChain;
