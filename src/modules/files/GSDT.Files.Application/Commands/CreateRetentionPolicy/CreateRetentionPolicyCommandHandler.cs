using FluentResults;
using MediatR;

namespace GSDT.Files.Application.Commands.CreateRetentionPolicy;

public sealed class CreateRetentionPolicyCommandHandler(
    IRepository<RetentionPolicy, Guid> repository)
    : IRequestHandler<CreateRetentionPolicyCommand, Result<RetentionPolicyDto>>
{
    public async Task<Result<RetentionPolicyDto>> Handle(
        CreateRetentionPolicyCommand request,
        CancellationToken cancellationToken)
    {
        var policy = RetentionPolicy.Create(
            request.Name,
            request.DocumentType,
            request.RetainDays,
            request.ArchiveAfterDays,
            request.DestroyAfterDays,
            request.TenantId,
            request.CreatedBy);

        await repository.AddAsync(policy, cancellationToken);

        return Result.Ok(MapToDto(policy));
    }

    internal static RetentionPolicyDto MapToDto(RetentionPolicy p) => new(
        p.Id, p.Name, p.DocumentType, p.RetainDays,
        p.ArchiveAfterDays, p.DestroyAfterDays, p.IsActive, p.TenantId);
}
