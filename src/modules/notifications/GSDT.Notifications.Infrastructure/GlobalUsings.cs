// Global usings for GSDT.Notifications.Infrastructure

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.SignalR;
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

global using GSDT.SharedKernel.Domain;
global using GSDT.SharedKernel.Domain.Events;
global using GSDT.SharedKernel.Domain.Repositories;
global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Contracts.Clients;

global using GSDT.Infrastructure.Persistence;
global using GSDT.Infrastructure.Security;
global using GSDT.Infrastructure.Configuration;

global using GSDT.Notifications.Domain.Entities;
global using GSDT.Notifications.Domain.Events;
global using GSDT.Notifications.Domain.Repositories;
global using GSDT.Notifications.Domain.ValueObjects;
global using GSDT.Notifications.Application.DTOs;
global using GSDT.Notifications.Application.Clients;
global using GSDT.Notifications.Application.Providers;
global using GSDT.Notifications.Application.Services;
global using GSDT.Notifications.Infrastructure.Email;
global using GSDT.Notifications.Infrastructure.Sms;
global using GSDT.Notifications.Infrastructure.Rendering;
global using GSDT.Notifications.Infrastructure.Persistence;
