namespace GSDT.ModuleName.Application.DTOs;

public sealed record ModuleNameDto(
    Guid Id,
    Guid TenantId,
    string Title,
    string? Description,
    DateTimeOffset CreatedAt);
