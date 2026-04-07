using MediatR;

namespace GSDT.MasterData.Controllers;

/// <summary>
/// Dictionary (Danh mục) CRUD and publish endpoints.
/// Requires Admin role for writes; any authenticated user can read.
/// </summary>
[ApiController]
[Route("api/v1/dictionaries")]
[Authorize]
public sealed class DictionaryController(ISender mediator) : ApiControllerBase
{
    // --- Dictionary CRUD ---

    /// <summary>List dictionaries for a tenant (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid tenantId,
        [FromQuery] DictionaryStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new GetDictionariesQuery(tenantId, status, search, page, pageSize), ct));
    }

    /// <summary>Create a new dictionary.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDictionaryRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new CreateDictionaryCommand(
                request.Code, request.Name, request.NameVi,
                request.Description, request.TenantId,
                request.IsSystemDefined, GetActorId()), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(List), new { tenantId = request.TenantId }, new { id = result.Value });

        return ToApiResponse(result);
    }

    /// <summary>Update dictionary name/description.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDictionaryRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateDictionaryCommand(id, request.Name, request.NameVi, request.Description, GetActorId()), ct));

    /// <summary>Publish a dictionary — validates integrity, increments version, emits event.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Publish(
        Guid id,
        [FromBody] PublishDictionaryRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(
            new PublishDictionaryCommand(id, request.Notes, GetActorId()), ct));

    // --- DictionaryItem sub-resource ---

    /// <summary>List items for a dictionary (flat, ordered by SortOrder for client-side tree building).</summary>
    [HttpGet("{id:guid}/items")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListItems(
        Guid id,
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        return ToApiResponse(await mediator.Send(
            new GetDictionaryItemsQuery(id, activeOnly, page, pageSize), ct));
    }

    /// <summary>Add an item to a dictionary.</summary>
    [HttpPost("{id:guid}/items")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> CreateItem(
        Guid id,
        [FromBody] CreateDictionaryItemRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new CreateDictionaryItemCommand(
                id, request.Code, request.Name, request.NameVi,
                request.ParentId, request.SortOrder, request.EffectiveFrom,
                request.EffectiveTo, request.Metadata, request.TenantId, GetActorId()), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(ListItems), new { id }, new { itemId = result.Value });

        return ToApiResponse(result);
    }

    private Guid GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

// --- Request DTOs ---

public sealed record CreateDictionaryRequest(
    string Code, string Name, string NameVi,
    string? Description, Guid TenantId, bool IsSystemDefined = false);

public sealed record UpdateDictionaryRequest(
    string Name, string NameVi, string? Description);

public sealed record PublishDictionaryRequest(string? Notes);

public sealed record CreateDictionaryItemRequest(
    string Code, string Name, string NameVi,
    Guid? ParentId, int SortOrder,
    DateTime EffectiveFrom, DateTime? EffectiveTo,
    string? Metadata, Guid TenantId);
