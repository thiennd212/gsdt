// Global usings for GSDT.Files.Domain
// Pulls in all internal sub-namespaces so files don't need per-file using directives.

// Internal sub-namespaces
global using GSDT.Files.Domain.Entities;
global using GSDT.Files.Domain.Events;
global using GSDT.Files.Domain.Repositories;
global using GSDT.Files.Domain.Services;

// SharedKernel — IRepository<,> lives in GSDT.SharedKernel.Domain.Repositories
global using GSDT.SharedKernel.Domain.Repositories;
