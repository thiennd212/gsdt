using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListDelegations;

/// <summary>Query active or historical delegations filtered by delegator/delegate.</summary>
public sealed record ListDelegationsQuery(
    Guid? DelegatorId,
    Guid? DelegateId,
    bool? ActiveOnly) : IRequest<Result<IReadOnlyList<DelegationDto>>>;

/// <summary>Read model returned by ListDelegationsQuery.</summary>
public sealed record DelegationDto(
    Guid Id,
    Guid DelegatorId,
    Guid DelegateId,
    DateTime ValidFrom,
    DateTime ValidTo,
    string? Reason,
    bool IsRevoked,
    DateTime? RevokedAt);
