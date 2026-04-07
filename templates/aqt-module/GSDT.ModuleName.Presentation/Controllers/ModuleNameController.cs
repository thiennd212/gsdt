using GSDT.ModuleName.Application.Commands.CreateModuleName;
using GSDT.ModuleName.Application.DTOs;
using GSDT.ModuleName.Application.Queries.GetModuleName;
using GSDT.SharedKernel.Domain;
using GSDT.SharedKernel.Presentation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSDT.ModuleName.Presentation.Controllers;

/// <summary>
/// ModuleName REST API — authenticated CRUD.
/// Inherits ApiControllerBase for consistent ApiResponse{T} envelope.
/// TenantId resolved from ITenantContext (JWT claim).
/// </summary>
[Route("api/v1/modulename")]
[Authorize]
public sealed class ModuleNameController(ISender sender, ITenantContext tenantContext) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        var result = await sender.Send(new GetModuleNameQuery(id, tenantId), ct);
        return ToApiResponse(result);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateModuleNameRequest request,
        CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        var userId = tenantContext.UserId ?? Guid.Empty;

        var command = new CreateModuleNameCommand(
            tenantId, request.Title, request.Description, userId);

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ToApiResponse(result);
    }
}

public sealed record CreateModuleNameRequest(string Title, string? Description);
