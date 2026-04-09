namespace GSDT.Identity.Application.Queries.GetRoleById;

/// <summary>Returns full role detail including permissions for the given role ID.</summary>
public sealed record GetRoleByIdQuery(Guid Id) : IQuery<RoleDetailDto>;
