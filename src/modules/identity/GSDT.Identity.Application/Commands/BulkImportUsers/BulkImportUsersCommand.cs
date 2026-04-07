using FluentResults;

namespace GSDT.Identity.Application.Commands.BulkImportUsers;

/// <summary>
/// Import users from parsed CSV rows.
/// Controller parses CSV stream → passes rows here.
/// Returns import result with per-row error details.
/// </summary>
public sealed record BulkImportUsersCommand(
    IReadOnlyList<UserImportRow> Rows,
    Guid? TenantId,
    Guid ActorId) : ICommand<BulkImportResultDto>;

public sealed record UserImportRow(
    int RowNumber,
    string FullName,
    string Email,
    string? DepartmentCode,
    string? InitialRole);
