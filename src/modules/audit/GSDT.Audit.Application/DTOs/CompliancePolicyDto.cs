
namespace GSDT.Audit.Application.DTOs;

/// <summary>Read model for CompliancePolicy.</summary>
public sealed record CompliancePolicyDto(
    Guid Id,
    string Name,
    ComplianceCategory Category,
    string Rules,
    PolicyEnforcement Enforcement,
    bool IsEnabled,
    DateTimeOffset CreatedAt);
