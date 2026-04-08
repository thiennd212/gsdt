// Global usings for GSDT.MasterData.Infrastructure

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Errors;

global using GSDT.Infrastructure.Persistence;

global using GSDT.MasterData.Domain.Entities;
global using GSDT.MasterData.Domain.Events;
global using GSDT.MasterData.Application.Commands.CreateDictionary;
global using GSDT.MasterData.Application.Commands.CreateDictionaryItem;
global using GSDT.MasterData.Application.Commands.CreateExternalMapping;
global using GSDT.MasterData.Application.Commands.PublishDictionary;
global using GSDT.MasterData.Application.Commands.UpdateDictionary;
global using GSDT.MasterData.Application.Queries.GetDictionaries;
global using GSDT.MasterData.Application.Queries.GetDictionaryItems;
global using GSDT.MasterData.Application.Queries.GetExternalMappings;
global using GSDT.MasterData.Application.DTOs;
global using GSDT.MasterData.Infrastructure.Persistence;
global using GSDT.MasterData.Infrastructure.Services;
global using GSDT.Infrastructure.Security;
