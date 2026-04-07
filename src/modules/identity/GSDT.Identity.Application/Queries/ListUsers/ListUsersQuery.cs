using FluentResults;

namespace GSDT.Identity.Application.Queries.ListUsers;

public sealed record ListUsersQuery(
    Guid? TenantId,
    string? SearchTerm,
    string? DepartmentCode,
    bool? IsActive,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<UserDto>>;
