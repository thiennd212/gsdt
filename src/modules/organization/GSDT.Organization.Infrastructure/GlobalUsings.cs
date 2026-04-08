// Global usings for GSDT.Organization.Infrastructure

global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Application.Caching;
global using GSDT.SharedKernel.Application.Data;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Contracts.Clients;
global using GSDT.SharedKernel.Errors;

global using GSDT.Infrastructure.Persistence;

global using GSDT.Organization.Domain.Entities;
global using GSDT.Organization.Application.DTOs;
global using GSDT.Organization.Application.Commands;
global using GSDT.Organization.Application.Queries;
global using GSDT.Organization.Infrastructure.Persistence;
global using GSDT.Organization.Infrastructure.Services;
global using GSDT.Organization.Infrastructure.Clients;
global using GSDT.Infrastructure.Security;
