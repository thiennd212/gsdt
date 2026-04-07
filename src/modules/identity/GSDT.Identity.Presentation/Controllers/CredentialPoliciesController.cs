using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Credential (password) policy management endpoints — per-tenant policy configuration.
/// Requires Admin role.
/// </summary>
[Route("api/v1/admin/credential-policies")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class CredentialPoliciesController(ISender mediator) : ApiControllerBase
{
    /// <summary>List credential policies for the current tenant (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new ListCredentialPoliciesQuery(tenantId, page, pageSize), ct));
    }

    /// <summary>Get a single credential policy by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetCredentialPolicyByIdQuery(id), ct));

    /// <summary>Create a new credential policy for the current tenant.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCredentialPolicyRequest request,
        CancellationToken ct = default)
    {
        var tenantId = ResolveTenantId();
        var result = await mediator.Send(
            new CreateCredentialPolicyCommand(
                request.Name, tenantId,
                request.MinLength, request.MaxLength,
                request.RequireUppercase, request.RequireLowercase,
                request.RequireDigit, request.RequireSpecialChar,
                request.RotationDays, request.MaxFailedAttempts,
                request.LockoutMinutes, request.PasswordHistoryCount,
                request.IsDefault, ResolveUserId()), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);

        return ToApiResponse(result);
    }

    /// <summary>Update an existing credential policy.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCredentialPolicyRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateCredentialPolicyCommand(
                id, request.Name,
                request.MinLength, request.MaxLength,
                request.RequireUppercase, request.RequireLowercase,
                request.RequireDigit, request.RequireSpecialChar,
                request.RotationDays, request.MaxFailedAttempts,
                request.LockoutMinutes, request.PasswordHistoryCount,
                request.IsDefault, ResolveUserId()), ct));

    /// <summary>Soft-delete a credential policy.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteCredentialPolicyCommand(id, ResolveUserId()), ct));
}

public sealed record CreateCredentialPolicyRequest(
    string Name,
    int MinLength,
    int MaxLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialChar,
    int RotationDays,
    int MaxFailedAttempts,
    int LockoutMinutes,
    int PasswordHistoryCount,
    bool IsDefault);

public sealed record UpdateCredentialPolicyRequest(
    string Name,
    int MinLength,
    int MaxLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireDigit,
    bool RequireSpecialChar,
    int RotationDays,
    int MaxFailedAttempts,
    int LockoutMinutes,
    int PasswordHistoryCount,
    bool IsDefault);
