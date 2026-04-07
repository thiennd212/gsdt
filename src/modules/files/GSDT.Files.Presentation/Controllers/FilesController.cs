using MediatR;

namespace GSDT.Files.Presentation.Controllers;

/// <summary>
/// Files REST API — upload, download, metadata, delete.
/// POST /api/v1/files          → 202 Accepted (two-phase: quarantine → ClamAV scan async)
/// GET  /api/v1/files/{id}     → stream download (Available files only)
/// GET  /api/v1/files/{id}/metadata → file metadata + current scan status
/// DELETE /api/v1/files/{id}   → soft delete
///
/// F-01: TenantId and user identity always resolved from JWT claims (ICurrentUser/ITenantContext).
///       Never accepted from query params — prevents tenant spoofing and IDOR.
/// RecyclableMemoryStream used for upload handling — avoids LOH pressure on large files.
/// </summary>
[Route("api/v1/files")]
[Authorize]
public sealed class FilesController(
    ISender mediator,
    RecyclableMemoryStreamManager streamManager,
    ICurrentUser currentUser,
    ITenantContext tenantContext) : ApiControllerBase
{
    /// <summary>List files for the current tenant (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!tenantContext.TenantId.HasValue)
            return Forbid();

        var tenantId = tenantContext.TenantId.Value;
        pageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (Math.Max(page, 1) - 1) * pageSize;

        var db = HttpContext.RequestServices.GetRequiredService<IReadDbConnection>();

        var countSql = "SELECT COUNT(*) FROM [files].FileRecords WHERE TenantId = @TenantId AND IsDeleted = 0";
        var totalCount = await db.QuerySingleAsync<int>(countSql, new { TenantId = tenantId }, cancellationToken);

        // Aliases match FE FileRecordDto field names (camelCase via Dapper dynamic)
        var sql = """
            SELECT Id, OriginalFileName, ContentType, SizeBytes,
                   CASE Status
                     WHEN 0 THEN 'Pending'
                     WHEN 1 THEN 'Clean'
                     WHEN 2 THEN 'Infected'
                     ELSE 'Failed'
                   END AS VirusScanStatus,
                   UploadedBy, CreatedAt AS UploadedAt
            FROM [files].FileRecords
            WHERE TenantId = @TenantId AND IsDeleted = 0
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;

        var items = await db.QueryAsync<object>(sql,
            new { TenantId = tenantId, Skip = skip, Take = pageSize }, cancellationToken);

        return Ok(GSDT.SharedKernel.Api.ApiResponse<object>.Ok(
            new { items, totalCount }));
    }

    /// <summary>Upload a file. Returns 202 with fileId for async scan polling.</summary>
    [HttpPost]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(202)]
    [ProducesResponseType(422)]
    [RequestSizeLimit(104_857_600)] // 100 MB — enforced at middleware level (NĐ68 Group F fix)
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromQuery] Guid? caseId,
        [FromQuery] ClassificationLevel classification = ClassificationLevel.Internal,
        CancellationToken cancellationToken = default)
    {
        // F-01: Resolve identity from JWT — never trust query params for security-critical fields
        if (!tenantContext.TenantId.HasValue)
            return Forbid();

        var tenantId = tenantContext.TenantId.Value;
        var uploadedBy = currentUser.UserId;

        // RecyclableMemoryStream — avoids MemoryStream LOH for large uploads
        await using var recyclableStream = streamManager.GetStream("file-upload");
        await file.CopyToAsync(recyclableStream, cancellationToken);
        recyclableStream.Position = 0;

        var command = new UploadFileCommand(
            recyclableStream,
            file.FileName,
            file.ContentType,
            file.Length,
            tenantId,
            uploadedBy,
            caseId,
            classification);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailed)
            return ToApiResponse(result);

        // 202 Accepted — file is quarantined, scan in progress
        return StatusCode(StatusCodes.Status202Accepted, result.Value);
    }

    /// <summary>Download file stream. Only Available (scan-passed) files returned.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Download(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // F-01: Resolve tenant and user from JWT
        if (!tenantContext.TenantId.HasValue)
            return Forbid();

        var tenantId = tenantContext.TenantId.Value;
        var requestedBy = currentUser.UserId;

        var result = await mediator.Send(
            new DownloadFileQuery(id, tenantId, requestedBy), cancellationToken);

        if (result.IsFailed)
            return ToApiResponse(result);

        var download = result.Value;
        return File(download.FileStream, download.ContentType,
            download.OriginalFileName, enableRangeProcessing: true);
    }

    /// <summary>Get file metadata including virus scan status (use for polling after upload).</summary>
    [HttpGet("{id:guid}/metadata")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMetadata(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // F-01: Resolve tenant from JWT
        if (!tenantContext.TenantId.HasValue)
            return Forbid();

        return ToApiResponse(await mediator.Send(
            new GetFileMetadataQuery(id, tenantContext.TenantId.Value), cancellationToken));
    }

    /// <summary>Soft-delete a file record.</summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // F-01: Resolve tenant and user from JWT
        if (!tenantContext.TenantId.HasValue)
            return Forbid();

        var tenantId = tenantContext.TenantId.Value;
        var deletedBy = currentUser.UserId;

        return ToApiResponse(await mediator.Send(
            new DeleteFileCommand(id, tenantId, deletedBy), cancellationToken));
    }
}
