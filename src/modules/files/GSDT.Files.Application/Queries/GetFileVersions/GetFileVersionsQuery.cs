using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Queries.GetFileVersions;

/// <summary>Returns all versions of a file record ordered by version number.</summary>
public sealed record GetFileVersionsQuery(
    Guid FileRecordId,
    Guid TenantId) : IRequest<Result<IReadOnlyList<FileVersionDto>>>;
