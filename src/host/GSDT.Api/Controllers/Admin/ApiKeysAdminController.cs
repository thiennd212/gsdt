
namespace GSDT.Api.Controllers.Admin;

/// <summary>
/// Admin API for API key lifecycle management (create / list / revoke / rotate).
/// Requires Admin policy (Admin or SystemAdmin role).
/// Key plaintext is returned ONCE at creation/rotation — GitHub PAT pattern.
/// </summary>
[Route("api/v1/admin/api-keys")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class ApiKeysAdminController(
    ApiKeyService apiKeyService) : ApiControllerBase
{
    // ── Create ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Create a new API key. Returns plaintext key once — store securely, not retrievable again.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ApiKeyCreatedDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateApiKeyRequest body,
        CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
            return BadRequest(ApiResponse<object>.Fail(
                [new ValidationError("TenantId is required.", "tenantId")]));

        var createdBy = User.Identity?.Name ?? "system";
        var result = await apiKeyService.CreateAsync(
            body.Name, body.ClientId, tenantId.Value,
            createdBy, body.ExpiresAt, body.Scopes, ct);

        return StatusCode(201, ApiResponse<ApiKeyCreatedDto>.Ok(
            new ApiKeyCreatedDto(result.Id, result.Plaintext, result.Prefix, result.CreatedAt)));
    }

    // ── List ─────────────────────────────────────────────────────────────────

    /// <summary>List active API keys for tenant — metadata only, no key value.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ApiKeyMetaDto>>), 200)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var resolved = ResolveTenantId();
        if (resolved is null)
            return Ok(ApiResponse<List<ApiKeyMetaDto>>.Ok([]));

        var keys = await apiKeyService.ListAsync(resolved.Value, ct);
        var dtos = keys.Select(k => new ApiKeyMetaDto(
            k.Id, k.Name, k.Prefix, k.ClientId,
            k.Scopes.Select(s => s.ScopePermission).ToList(),
            k.ExpiresAt, k.CreatedAt)).ToList();

        return Ok(ApiResponse<List<ApiKeyMetaDto>>.Ok(dtos));
    }

    // ── Revoke ───────────────────────────────────────────────────────────────

    /// <summary>Revoke an API key immediately. Effect is near-instant (Redis cache TTL 5min).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
            return BadRequest(ApiResponse<object>.Fail(
                [new ValidationError("TenantId is required.", "tenantId")]));

        var ok = await apiKeyService.RevokeAsync(id, tenantId.Value, ct);
        if (!ok)
            return NotFound(ApiResponse<object>.Fail(
                [new NotFoundError($"API key {id} not found.")]));

        return NoContent();
    }

    // ── Rotate ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Rotate an API key — atomically revokes old key and issues a new one.
    /// New plaintext returned once.
    /// </summary>
    [HttpPost("{id:guid}/rotate")]
    [ProducesResponseType(typeof(ApiResponse<ApiKeyCreatedDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Rotate(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        if (tenantId is null)
            return BadRequest(ApiResponse<object>.Fail(
                [new ValidationError("TenantId is required.", "tenantId")]));

        var rotatedBy = User.Identity?.Name ?? "system";
        var result = await apiKeyService.RotateAsync(id, tenantId.Value, rotatedBy, ct);
        if (result is null)
            return NotFound(ApiResponse<object>.Fail(
                [new NotFoundError($"API key {id} not found.")]));

        return Ok(ApiResponse<ApiKeyCreatedDto>.Ok(
            new ApiKeyCreatedDto(result.Id, result.Plaintext, result.Prefix, result.CreatedAt)));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private new Guid? ResolveTenantId()
    {
        var tc = HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (tc.TenantId.HasValue) return tc.TenantId;

        // Only SystemAdmin can override tenant via X-Tenant-Id header
        if (tc.IsSystemAdmin
            && Request.Headers.TryGetValue("X-Tenant-Id", out var header)
            && Guid.TryParse(header.FirstOrDefault(), out var parsed))
            return parsed;

        return null;
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public sealed record CreateApiKeyRequest(
    string Name,
    string ClientId,
    IReadOnlyList<string>? Scopes = null,
    DateTimeOffset? ExpiresAt = null);

/// <summary>Returned once at create/rotate — contains plaintext key.</summary>
public sealed record ApiKeyCreatedDto(
    Guid Id,
    string Key,
    string Prefix,
    DateTimeOffset CreatedAt);

/// <summary>Metadata only — no key value exposed.</summary>
public sealed record ApiKeyMetaDto(
    Guid Id,
    string Name,
    string Prefix,
    string ClientId,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt);
