using System.Reflection;

namespace GSDT.Architecture.Tests.Fixtures;

/// <summary>
/// Centralises assembly loading for architecture tests.
/// All assemblies are loaded once via a static field — reflection is expensive.
/// To add a new module: add assembly fields + a new ModuleAssemblies entry in Modules list.
/// </summary>
public static class AssemblyFixtures
{
    // SharedKernel
    public static readonly Assembly SharedKernel =
        typeof(GSDT.SharedKernel.Domain.AuditableEntity<>).Assembly;

    // ── Cases ─────────────────────────────────────────────────────────────
    public static readonly Assembly CasesDomain =
        typeof(GSDT.Cases.Domain.Entities.Case).Assembly;
    public static readonly Assembly CasesApplication =
        typeof(GSDT.Cases.Application.Commands.CreateCase.CreateCaseCommand).Assembly;
    public static readonly Assembly CasesInfrastructure =
        Assembly.Load("GSDT.Cases.Infrastructure");
    public static readonly Assembly CasesPresentation =
        typeof(GSDT.Cases.Presentation.Controllers.CasesController).Assembly;

    // ── Identity ──────────────────────────────────────────────────────────
    public static readonly Assembly IdentityDomain =
        typeof(GSDT.Identity.Domain.Entities.ApplicationUser).Assembly;
    public static readonly Assembly IdentityApplication =
        Assembly.Load("GSDT.Identity.Application");
    public static readonly Assembly IdentityInfrastructure =
        Assembly.Load("GSDT.Identity.Infrastructure");
    public static readonly Assembly IdentityPresentation =
        Assembly.Load("GSDT.Identity.Presentation");

    // ── Signature ─────────────────────────────────────────────────────────
    public static readonly Assembly SignatureDomain =
        typeof(GSDT.Signature.Domain.Entities.SignatureRequest).Assembly;
    public static readonly Assembly SignatureApplication =
        Assembly.Load("GSDT.Signature.Application");
    public static readonly Assembly SignatureInfrastructure =
        Assembly.Load("GSDT.Signature.Infrastructure");
    public static readonly Assembly SignaturePresentation =
        typeof(GSDT.Signature.Presentation.Controllers.SignaturesController).Assembly;

    // ── Rules ─────────────────────────────────────────────────────────────
    public static readonly Assembly RulesDomain =
        typeof(GSDT.Rules.Domain.Entities.RuleSet).Assembly;
    public static readonly Assembly RulesApplication =
        Assembly.Load("GSDT.Rules.Application");
    public static readonly Assembly RulesInfrastructure =
        Assembly.Load("GSDT.Rules.Infrastructure");
    public static readonly Assembly RulesPresentation =
        typeof(GSDT.Rules.Presentation.Controllers.RuleSetsController).Assembly;

    // ── Collaboration ─────────────────────────────────────────────────────
    public static readonly Assembly CollaborationDomain =
        typeof(GSDT.Collaboration.Domain.Entities.Conversation).Assembly;
    public static readonly Assembly CollaborationApplication =
        Assembly.Load("GSDT.Collaboration.Application");
    public static readonly Assembly CollaborationInfrastructure =
        Assembly.Load("GSDT.Collaboration.Infrastructure");
    public static readonly Assembly CollaborationPresentation =
        typeof(GSDT.Collaboration.Presentation.Controllers.ConversationsController).Assembly;

    // ── Search ────────────────────────────────────────────────────────────
    public static readonly Assembly SearchDomain =
        typeof(GSDT.Search.Domain.Entities.SavedQuery).Assembly;
    public static readonly Assembly SearchApplication =
        Assembly.Load("GSDT.Search.Application");
    public static readonly Assembly SearchInfrastructure =
        Assembly.Load("GSDT.Search.Infrastructure");
    public static readonly Assembly SearchPresentation =
        typeof(GSDT.Search.Presentation.Controllers.SearchController).Assembly;

    // ── AI ────────────────────────────────────────────────────────────────
    public static readonly Assembly AiDomain =
        typeof(GSDT.Ai.Domain.Entities.AiModelProfile).Assembly;
    public static readonly Assembly AiApplication =
        Assembly.Load("GSDT.Ai.Application");
    public static readonly Assembly AiInfrastructure =
        Assembly.Load("GSDT.Ai.Infrastructure");
    public static readonly Assembly AiPresentation =
        typeof(GSDT.Ai.Presentation.Controllers.AiModelProfilesController).Assembly;

    // ── Audit ─────────────────────────────────────────────────────────────
    public static readonly Assembly AuditDomain =
        typeof(GSDT.Audit.Domain.Entities.AuditLogEntry).Assembly;
    public static readonly Assembly AuditApplication =
        Assembly.Load("GSDT.Audit.Application");
    public static readonly Assembly AuditInfrastructure =
        Assembly.Load("GSDT.Audit.Infrastructure");
    public static readonly Assembly AuditPresentation =
        typeof(GSDT.Audit.Presentation.Controllers.AuditLogsController).Assembly;

    // ── Files ─────────────────────────────────────────────────────────────
    public static readonly Assembly FilesDomain =
        typeof(GSDT.Files.Domain.Entities.FileRecord).Assembly;
    public static readonly Assembly FilesApplication =
        Assembly.Load("GSDT.Files.Application");
    public static readonly Assembly FilesInfrastructure =
        Assembly.Load("GSDT.Files.Infrastructure");
    public static readonly Assembly FilesPresentation =
        typeof(GSDT.Files.Presentation.Controllers.FilesController).Assembly;

    // ── Forms ─────────────────────────────────────────────────────────────
    public static readonly Assembly FormsDomain =
        typeof(GSDT.Forms.Domain.Entities.FormTemplate).Assembly;
    public static readonly Assembly FormsApplication =
        Assembly.Load("GSDT.Forms.Application");
    public static readonly Assembly FormsInfrastructure =
        Assembly.Load("GSDT.Forms.Infrastructure");
    public static readonly Assembly FormsPresentation =
        typeof(GSDT.Forms.Presentation.Controllers.FormTemplatesController).Assembly;

    // ── Notifications ─────────────────────────────────────────────────────
    public static readonly Assembly NotificationsDomain =
        typeof(GSDT.Notifications.Domain.Entities.Notification).Assembly;
    public static readonly Assembly NotificationsApplication =
        Assembly.Load("GSDT.Notifications.Application");
    public static readonly Assembly NotificationsInfrastructure =
        Assembly.Load("GSDT.Notifications.Infrastructure");
    public static readonly Assembly NotificationsPresentation =
        typeof(GSDT.Notifications.Presentation.Controllers.NotificationsController).Assembly;

    // ── Reporting ─────────────────────────────────────────────────────────
    public static readonly Assembly ReportingDomain =
        typeof(GSDT.Reporting.Domain.Entities.ReportDefinition).Assembly;
    public static readonly Assembly ReportingApplication =
        Assembly.Load("GSDT.Reporting.Application");
    public static readonly Assembly ReportingInfrastructure =
        Assembly.Load("GSDT.Reporting.Infrastructure");
    public static readonly Assembly ReportingPresentation =
        typeof(GSDT.Reporting.Presentation.Controllers.ReportsController).Assembly;

    // ── Workflow ──────────────────────────────────────────────────────────
    public static readonly Assembly WorkflowDomain =
        typeof(GSDT.Workflow.Domain.Entities.WorkflowDefinition).Assembly;
    public static readonly Assembly WorkflowApplication =
        Assembly.Load("GSDT.Workflow.Application");
    public static readonly Assembly WorkflowInfrastructure =
        Assembly.Load("GSDT.Workflow.Infrastructure");
    public static readonly Assembly WorkflowPresentation =
        typeof(GSDT.Workflow.Presentation.Controllers.WorkflowDefinitionsController).Assembly;

    // ── Integration ───────────────────────────────────────────────────────
    public static readonly Assembly IntegrationDomain =
        typeof(GSDT.Integration.Domain.Entities.Partner).Assembly;
    public static readonly Assembly IntegrationApplication =
        Assembly.Load("GSDT.Integration.Application");
    public static readonly Assembly IntegrationInfrastructure =
        Assembly.Load("GSDT.Integration.Infrastructure");
    public static readonly Assembly IntegrationPresentation =
        typeof(GSDT.Integration.Presentation.Controllers.PartnersController).Assembly;

    // ── MasterData ────────────────────────────────────────────────────────
    public static readonly Assembly MasterDataDomain =
        typeof(GSDT.MasterData.Domain.Entities.Dictionary).Assembly;
    public static readonly Assembly MasterDataApplication =
        Assembly.Load("GSDT.MasterData.Application");
    public static readonly Assembly MasterDataInfrastructure =
        Assembly.Load("GSDT.MasterData.Infrastructure");
    public static readonly Assembly MasterDataPresentation =
        typeof(GSDT.MasterData.Presentation.Controllers.MasterDataController).Assembly;

    // ── Organization ──────────────────────────────────────────────────────
    public static readonly Assembly OrganizationDomain =
        typeof(GSDT.Organization.Domain.Entities.OrgUnit).Assembly;
    public static readonly Assembly OrganizationApplication =
        Assembly.Load("GSDT.Organization.Application");
    public static readonly Assembly OrganizationInfrastructure =
        Assembly.Load("GSDT.Organization.Infrastructure");
    public static readonly Assembly OrganizationPresentation =
        typeof(GSDT.Organization.Presentation.Controllers.OrgUnitsController).Assembly;

    // ── SystemParams ──────────────────────────────────────────────────────
    public static readonly Assembly SystemParamsDomain =
        typeof(GSDT.SystemParams.Domain.Entities.SystemParameter).Assembly;
    public static readonly Assembly SystemParamsApplication =
        Assembly.Load("GSDT.SystemParams.Application");
    public static readonly Assembly SystemParamsInfrastructure =
        Assembly.Load("GSDT.SystemParams.Infrastructure");
    public static readonly Assembly SystemParamsPresentation =
        typeof(GSDT.SystemParams.Presentation.Controllers.SystemParamsController).Assembly;

    // ── InvestmentProjects ────────────────────────────────────────────────────
    public static readonly Assembly InvestmentProjectsDomain =
        typeof(GSDT.InvestmentProjects.Domain.Entities.InvestmentProject).Assembly;
    public static readonly Assembly InvestmentProjectsApplication =
        Assembly.Load("GSDT.InvestmentProjects.Application");
    public static readonly Assembly InvestmentProjectsInfrastructure =
        Assembly.Load("GSDT.InvestmentProjects.Infrastructure");
    public static readonly Assembly InvestmentProjectsPresentation =
        typeof(GSDT.InvestmentProjects.Presentation.Controllers.DomesticProjectsController).Assembly;

    /// <summary>
    /// Structured module assembly data — used by data-driven architecture tests.
    /// Add new modules here; tests auto-cover layer isolation + cross-module checks.
    /// </summary>
    public static readonly IReadOnlyList<ModuleAssemblies> Modules =
    [
        new("Cases", CasesDomain, CasesApplication, CasesInfrastructure, CasesPresentation),
        new("Identity", IdentityDomain, IdentityApplication, IdentityInfrastructure, IdentityPresentation),
        new("Signature", SignatureDomain, SignatureApplication, SignatureInfrastructure, SignaturePresentation),
        new("Rules", RulesDomain, RulesApplication, RulesInfrastructure, RulesPresentation),
        new("Collaboration", CollaborationDomain, CollaborationApplication, CollaborationInfrastructure, CollaborationPresentation),
        new("Search", SearchDomain, SearchApplication, SearchInfrastructure, SearchPresentation),
        new("Ai", AiDomain, AiApplication, AiInfrastructure, AiPresentation),
        new("Audit", AuditDomain, AuditApplication, AuditInfrastructure, AuditPresentation),
        new("Files", FilesDomain, FilesApplication, FilesInfrastructure, FilesPresentation),
        new("Forms", FormsDomain, FormsApplication, FormsInfrastructure, FormsPresentation),
        new("Notifications", NotificationsDomain, NotificationsApplication, NotificationsInfrastructure, NotificationsPresentation),
        new("Reporting", ReportingDomain, ReportingApplication, ReportingInfrastructure, ReportingPresentation),
        new("Workflow", WorkflowDomain, WorkflowApplication, WorkflowInfrastructure, WorkflowPresentation),
        new("Integration", IntegrationDomain, IntegrationApplication, IntegrationInfrastructure, IntegrationPresentation),
        new("MasterData", MasterDataDomain, MasterDataApplication, MasterDataInfrastructure, MasterDataPresentation),
        new("Organization", OrganizationDomain, OrganizationApplication, OrganizationInfrastructure, OrganizationPresentation),
        new("SystemParams", SystemParamsDomain, SystemParamsApplication, SystemParamsInfrastructure, SystemParamsPresentation),
        new("InvestmentProjects", InvestmentProjectsDomain, InvestmentProjectsApplication, InvestmentProjectsInfrastructure, InvestmentProjectsPresentation),
    ];
}

/// <summary>Named tuple grouping a module's 4-layer assemblies for architecture validation.</summary>
public sealed record ModuleAssemblies(
    string Name,
    Assembly Domain,
    Assembly Application,
    Assembly Infrastructure,
    Assembly Presentation);
