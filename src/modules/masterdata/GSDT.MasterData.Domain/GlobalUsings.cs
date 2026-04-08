// Global usings for GSDT.MasterData.Domain
// Only IExternalDomainEvent is missing — it lives in GSDT.SharedKernel.Domain.Events
// which is already covered by modules/Directory.Build.props, but Events sub-namespace
// needs to be explicitly available for the Events folder files.

// Internal sub-namespaces
global using GSDT.MasterData.Domain.Entities;
global using GSDT.MasterData.Domain.Events;
