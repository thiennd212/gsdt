namespace GSDT.SharedKernel.Application.Pagination;

/// <summary>Standard paginated response supporting both offset and cursor pagination.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    PaginationMeta Meta);

public sealed record PaginationMeta(
    int? Page,
    int? PageSize,
    int? TotalPages,
    string? NextCursor,
    string? PrevCursor,
    bool HasNextPage);
