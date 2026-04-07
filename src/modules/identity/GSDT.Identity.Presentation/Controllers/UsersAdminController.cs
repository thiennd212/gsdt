using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Admin endpoints for user lifecycle management.
/// Requires Admin or SystemAdmin role — enforced via [Authorize(Policy = "Admin")].
/// </summary>
[Route("api/v1/admin/users")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class UsersAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>List users with optional filters and pagination.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? departmentCode,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new ListUsersQuery(tenantId, search, departmentCode, isActive, page, pageSize), ct));
    }

    /// <summary>List distinct tenants with user counts — SystemAdmin only.</summary>
    [HttpGet("tenants")]
    [Authorize(Roles = "SystemAdmin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListTenants(CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ListTenantsQuery(), ct));

    /// <summary>Get a single user by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetUserByIdQuery(id), ct));

    /// <summary>Create a new user (admin-initiated registration).</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct = default)
    {
        var tenantId = ResolveTenantId();
        var actorId = ResolveUserId();
        var result = await mediator.Send(
            new RegisterUserCommand(request.FullName, request.Email, request.Password,
                request.DepartmentCode, tenantId), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);

        return ToApiResponse(result);
    }

    /// <summary>Update user profile (name, department).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateUserCommand(id, request.FullName, request.DepartmentCode, request.OrgUnitId, ResolveUserId()), ct));

    /// <summary>Soft-delete (deactivate) a user — audit-compliant, no hard delete.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteUserCommand(id, ResolveUserId()), ct));

    /// <summary>Lock a user account.</summary>
    [HttpPost("{id:guid}/lock")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Lock(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new LockUserCommand(id, Lock: true, ResolveUserId()), ct));

    /// <summary>Unlock a user account.</summary>
    [HttpPost("{id:guid}/unlock")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new LockUserCommand(id, Lock: false, ResolveUserId()), ct));

    /// <summary>Admin-initiated password reset — emails reset link to user (F-25: token never in response).</summary>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ResetPasswordCommand(id, ResolveUserId()), ct));

    /// <summary>Assign a role to a user (additive — keeps existing roles).</summary>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new AssignRoleCommand(id, request.Role, ResolveUserId()), ct));

    /// <summary>Replace all roles for a user — removes unlisted roles, adds new ones.</summary>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> SyncRoles(
        Guid id,
        [FromBody] SyncRolesRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new SyncUserRolesCommand(id, request.Roles, ResolveUserId()), ct));

    /// <summary>
    /// Get the full effective permission summary for a user:
    /// direct roles + group-inherited roles + active delegations + resolved data scope.
    /// Result is Redis-cached (TTL 10 min).
    /// </summary>
    [HttpGet("{id:guid}/effective-permissions")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEffectivePermissions(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetUserEffectivePermissionsQuery(id), ct));
}

/// <summary>Request body for admin user creation — not bound to Entity (GOV_SEC_004).</summary>
public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string? DepartmentCode);

/// <summary>Request body for user profile update.</summary>
public sealed record UpdateUserRequest(
    string FullName,
    string? DepartmentCode,
    Guid? OrgUnitId);

/// <summary>Request body for role assignment (single, additive).</summary>
public sealed record AssignRoleRequest(string Role);

/// <summary>Request body for role sync (replace all roles).</summary>
public sealed record SyncRolesRequest(IReadOnlyList<string> Roles);
