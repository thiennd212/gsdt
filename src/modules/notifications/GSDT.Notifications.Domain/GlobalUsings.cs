// Global usings for GSDT.Notifications.Domain
// Pulls in all internal sub-namespaces so files don't need per-file using directives.
// GSDT.SharedKernel.Domain.Events covers IDomainEvent; IExternalDomainEvent is also
// in GSDT.SharedKernel.Domain.Events (already covered by modules/Directory.Build.props).

// Internal sub-namespaces
global using GSDT.Notifications.Domain.Entities;
global using GSDT.Notifications.Domain.Events;
global using GSDT.Notifications.Domain.Repositories;
global using GSDT.Notifications.Domain.ValueObjects;

// SharedKernel — IRepository<,> lives in GSDT.SharedKernel.Domain.Repositories
global using GSDT.SharedKernel.Domain.Repositories;
