// Global usings for GSDT.SystemParams.Infrastructure

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Errors;
// NOTE: GSDT.SharedKernel.Contracts intentionally omitted at global scope.
//       It defines IFeatureFlagService which conflicts with the module-local one.
//       SystemParamsRegistration.cs uses fully-qualified SharedKernel.Contracts.IFeatureFlagService.

global using GSDT.Infrastructure.Persistence;

global using GSDT.SystemParams.Domain.Entities;
// Module-local IFeatureFlagService and ISystemParamService — unqualified references resolve here.
global using GSDT.SystemParams.Application.Services;
global using GSDT.SystemParams.Infrastructure.Services;
global using GSDT.SystemParams.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;
