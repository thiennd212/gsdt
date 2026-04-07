// Global usings for GSDT.Files.Presentation
// ASP.NET Core MVC/authorization, SharedKernel base controller, Files.Application types.

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.IO;

// ── SharedKernel presentation base ───────────────────────────────────────────
global using GSDT.SharedKernel.Presentation;

// ── Extensions ────────────────────────────────────────────────────────────────
global using Microsoft.Extensions.DependencyInjection;

// ── Files.Application types ───────────────────────────────────────────────────
global using GSDT.Files.Application.DTOs;
global using GSDT.SharedKernel.Application.Data;

// ── Files.Application Commands ────────────────────────────────────────────────
global using GSDT.Files.Application.Commands.UploadFile;
global using GSDT.Files.Application.Commands.UploadFileVersion;
global using GSDT.Files.Application.Commands.DeleteFile;
global using GSDT.Files.Application.Commands.CreateRetentionPolicy;
global using GSDT.Files.Application.Commands.CreateDocumentTemplate;
global using GSDT.Files.Application.Commands.UpdateDocumentTemplate;
global using GSDT.Files.Application.Commands.PublishDocumentTemplate;

// ── Files.Application Queries ─────────────────────────────────────────────────
global using GSDT.Files.Application.Queries.DownloadFile;
global using GSDT.Files.Application.Queries.GetFileMetadata;
global using GSDT.Files.Application.Queries.GetFileVersions;
global using GSDT.Files.Application.Queries.GetRetentionPolicies;
global using GSDT.Files.Application.Queries.GetDocumentTemplates;

// ── Files.Domain enums used in controller action params ──────────────────────
global using GSDT.Files.Domain.Entities;

// ── Data annotations — [StringLength] used in request models ─────────────────
global using System.ComponentModel.DataAnnotations;
