// Global usings for GSDT.Files.Infrastructure

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IO;

global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
// NOTE: GSDT.SharedKernel.Contracts.Clients is included for IFilesModuleClient/FileReferenceInfo.
// StubDigitalSignatureService.cs has a file-level alias to resolve SignatureVerificationResult ambiguity.
global using GSDT.SharedKernel.Contracts.Clients;

global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Configuration;

global using GSDT.Files.Domain.Entities;
global using GSDT.Files.Domain.Repositories;
global using GSDT.Files.Domain.Services;
global using GSDT.Files.Application.DTOs;
global using GSDT.Files.Application.Jobs;
global using GSDT.Files.Application.Options;
global using GSDT.Files.Infrastructure.Persistence;
global using GSDT.Files.Infrastructure.Persistence.Configurations;
global using GSDT.Files.Infrastructure.Options;
global using GSDT.Files.Infrastructure.Security;
global using GSDT.Files.Infrastructure.Storage;
global using GSDT.Files.Infrastructure.Jobs;
global using GSDT.Files.Infrastructure.Services;
global using GSDT.Files.Infrastructure.Clients;
global using GSDT.Infrastructure.Security;
