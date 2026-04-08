namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a registration certificate (GCNĐKĐT) to a project.</summary>
public sealed record AddRegistrationCertificateCommand(
    Guid ProjectId,
    string CertificateNumber,
    DateTime IssuedDate,
    decimal InvestmentCapital,
    decimal EquityCapital,
    decimal? EquityRatio,
    string? Notes,
    Guid? FileId)
    : IRequest<Result<Guid>>;

/// <summary>Updates an existing registration certificate.</summary>
public sealed record UpdateRegistrationCertificateCommand(
    Guid ProjectId,
    Guid CertificateId,
    string CertificateNumber,
    DateTime IssuedDate,
    decimal InvestmentCapital,
    decimal EquityCapital,
    decimal? EquityRatio,
    string? Notes,
    Guid? FileId)
    : IRequest<Result>;

/// <summary>Deletes a registration certificate from a project.</summary>
public sealed record DeleteRegistrationCertificateCommand(
    Guid ProjectId,
    Guid CertificateId)
    : IRequest<Result>;

public sealed class AddRegistrationCertificateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddRegistrationCertificateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddRegistrationCertificateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.CertificateNumber))
            return Result.Fail<Guid>("GOV_INV_VAL: So giay chung nhan khong duoc de trong.");

        var project = await repository.GetByIdWithCertificatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var cert = RegistrationCertificate.Create(
            tenantId, request.ProjectId,
            request.CertificateNumber.Trim(),
            request.IssuedDate,
            request.InvestmentCapital,
            request.EquityCapital,
            request.EquityRatio,
            request.Notes,
            request.FileId);

        project.RegistrationCertificates.Add(cert);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(cert.Id);
    }
}

public sealed class UpdateRegistrationCertificateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpdateRegistrationCertificateCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRegistrationCertificateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithCertificatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var cert = project.RegistrationCertificates.FirstOrDefault(c => c.Id == request.CertificateId);
        if (cert is null)
            return Result.Fail($"GOV_INV_404: Giay chung nhan khong ton tai (Id={request.CertificateId}).");

        cert.CertificateNumber = request.CertificateNumber.Trim();
        cert.IssuedDate = request.IssuedDate;
        cert.InvestmentCapital = request.InvestmentCapital;
        cert.EquityCapital = request.EquityCapital;
        cert.EquityRatio = request.EquityRatio;
        cert.Notes = request.Notes;
        cert.FileId = request.FileId;

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

public sealed class DeleteRegistrationCertificateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteRegistrationCertificateCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRegistrationCertificateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithCertificatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var cert = project.RegistrationCertificates.FirstOrDefault(c => c.Id == request.CertificateId);
        if (cert is null)
            return Result.Fail($"GOV_INV_404: Giay chung nhan khong ton tai (Id={request.CertificateId}).");

        project.RegistrationCertificates.Remove(cert);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
