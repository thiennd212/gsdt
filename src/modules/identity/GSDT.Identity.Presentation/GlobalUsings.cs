// Global usings for GSDT.Identity.Presentation
// ASP.NET Core MVC/authorization attributes, SharedKernel base controller,
// and Identity.Application command/query/DTO types.

// ── ASP.NET Core ─────────────────────────────────────────────────────────────
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;

// ── SharedKernel presentation base and API types ─────────────────────────────
global using GSDT.SharedKernel.Presentation;
global using GSDT.SharedKernel.Api;

// ── Identity.Domain types referenced from controllers ─────────────────────────
global using GSDT.Identity.Domain.Entities;
global using GSDT.Identity.Domain.Repositories;
global using GSDT.Identity.Domain.Services;

// ── Identity.Application types (commands, queries, DTOs, services) ───────────
global using GSDT.Identity.Application.DTOs;
global using GSDT.Identity.Application.Services;

// ── Identity.Application Commands ────────────────────────────────────────────
global using GSDT.Identity.Application.Commands.ApproveDelegation;
global using GSDT.Identity.Application.Commands.AssignRole;
global using GSDT.Identity.Application.Commands.BulkImportUsers;
global using GSDT.Identity.Application.Commands.ChangePassword;
global using GSDT.Identity.Application.Commands.CreateCredentialPolicy;
global using GSDT.Identity.Application.Commands.CreateExternalIdentity;
global using GSDT.Identity.Application.Commands.CreateJitProviderConfig;
global using GSDT.Identity.Application.Commands.DecideAccessReview;
global using GSDT.Identity.Application.Commands.DelegateRole;
global using GSDT.Identity.Application.Commands.DeleteCredentialPolicy;
global using GSDT.Identity.Application.Commands.DeleteExternalIdentity;
global using GSDT.Identity.Application.Commands.DeleteJitProviderConfig;
global using GSDT.Identity.Application.Commands.DeleteUser;
global using GSDT.Identity.Application.Commands.GrantConsent;
global using GSDT.Identity.Application.Commands.LockUser;
global using GSDT.Identity.Application.Commands.ManageAbacRule;
global using GSDT.Identity.Application.Commands.ManageDataScope;
global using GSDT.Identity.Application.Commands.ManageGroup;
global using GSDT.Identity.Application.Commands.ManageMenu;
global using GSDT.Identity.Application.Commands.ManagePolicyRule;
global using GSDT.Identity.Application.Commands.ManageSodRule;
global using GSDT.Identity.Application.Commands.RegisterUser;
global using GSDT.Identity.Application.Commands.ResetPassword;
global using GSDT.Identity.Application.Commands.RevokeDelegation;
global using GSDT.Identity.Application.Commands.RevokeToken;
global using GSDT.Identity.Application.Commands.SyncUserRoles;
global using GSDT.Identity.Application.Commands.UpdateCredentialPolicy;
global using GSDT.Identity.Application.Commands.UpdateExternalIdentity;
global using GSDT.Identity.Application.Commands.UpdateJitProviderConfig;
global using GSDT.Identity.Application.Commands.UpdateUser;
global using GSDT.Identity.Application.Commands.WithdrawConsent;
global using GSDT.Identity.Application.Commands.ManageRole;

// ── Identity.Application Queries ─────────────────────────────────────────────
global using GSDT.Identity.Application.Queries.GetCredentialPolicyById;
global using GSDT.Identity.Application.Queries.GetExternalIdentityById;
global using GSDT.Identity.Application.Queries.GetGroupById;
global using GSDT.Identity.Application.Queries.GetJitProviderConfigByScheme;
global using GSDT.Identity.Application.Queries.GetRoleDataScopes;
global using GSDT.Identity.Application.Queries.GetRoleById;
global using GSDT.Identity.Application.Queries.GetRoles;
global using GSDT.Identity.Application.Queries.GetUserById;
global using GSDT.Identity.Application.Queries.GetUserEffectivePermissions;
global using GSDT.Identity.Application.Queries.ListAbacRules;
global using GSDT.Identity.Application.Queries.ListActiveSessions;
global using GSDT.Identity.Application.Queries.ListCredentialPolicies;
global using GSDT.Identity.Application.Queries.ListDataScopeTypes;
global using GSDT.Identity.Application.Queries.ListDelegations;
global using GSDT.Identity.Application.Queries.ListExternalIdentities;
global using GSDT.Identity.Application.Queries.ListGroups;
global using GSDT.Identity.Application.Queries.ListJitProviderConfigs;
global using GSDT.Identity.Application.Queries.ListMenus;
global using GSDT.Identity.Application.Queries.ListPendingAccessReviews;
global using GSDT.Identity.Application.Queries.ListPolicyRules;
global using GSDT.Identity.Application.Queries.ListSodRules;
global using GSDT.Identity.Application.Queries.ListTenants;
global using GSDT.Identity.Application.Queries.ListUsers;
