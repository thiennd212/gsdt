namespace GSDT.SharedKernel.Domain;

/// <summary>Resolved current user from JWT — injected in Application layer.</summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    Guid? TenantId { get; }
    string UserName { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// For CQCQ (Co quan chu quan) users — the managing authority they oversee.
    /// Parsed from JWT claim "managing_authority_id".
    /// </summary>
    Guid? ManagingAuthorityId { get; }

    /// <summary>
    /// For CDT (Chu dau tu) users — the project owner org unit they belong to.
    /// Parsed from JWT claim "project_owner_id".
    /// </summary>
    Guid? ProjectOwnerId { get; }
}
