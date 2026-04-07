// Global usings for GSDT.Identity.Domain
// Pulls in internal sub-namespaces so files don't need per-file using directives.
// Microsoft.AspNetCore.Identity provides IdentityUser<TKey> and IdentityRole<TKey>.

global using Microsoft.AspNetCore.Identity;

// Internal sub-namespaces
global using GSDT.Identity.Domain.Entities;
global using GSDT.Identity.Domain.Events;
global using GSDT.Identity.Domain.Models;
global using GSDT.Identity.Domain.Repositories;
global using GSDT.Identity.Domain.Services;
