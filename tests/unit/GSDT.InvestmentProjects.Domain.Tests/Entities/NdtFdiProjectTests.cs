using FluentAssertions;
using GSDT.InvestmentProjects.Domain.Entities;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>Unit tests for NdtProject and FdiProject factory methods.</summary>
public sealed class NdtFdiProjectTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ManagingAuthId = Guid.NewGuid();
    private static readonly Guid IndustrySectorId = Guid.NewGuid();
    private static readonly Guid ProjectOwnerId = Guid.NewGuid();
    private static readonly Guid ProjectGroupId = Guid.NewGuid();

    // ── NĐT ─────────────────────────────────────────────────────────────────

    [Fact]
    public void NdtProject_Create_SetsProjectTypeToNdt()
    {
        var project = NdtProject.Create(
            TenantId, "NDT-001", "NDT Test",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        project.ProjectType.Should().Be(ProjectType.Ndt);
    }

    [Fact]
    public void NdtProject_Create_GeneratesUniqueId()
    {
        var a = NdtProject.Create(TenantId, "NDT-A", "A", ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);
        var b = NdtProject.Create(TenantId, "NDT-B", "B", ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NdtProject_Create_SetsRequiredFields()
    {
        var project = NdtProject.Create(
            TenantId, "NDT-001", "NDT Test",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        project.ProjectCode.Should().Be("NDT-001");
        project.ProjectName.Should().Be("NDT Test");
        project.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void NdtProject_Create_RaisesProjectCreatedEvent()
    {
        var project = NdtProject.Create(
            TenantId, "NDT-001", "NDT Test",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }

    // ── FDI ─────────────────────────────────────────────────────────────────

    [Fact]
    public void FdiProject_Create_SetsProjectTypeToFdi()
    {
        var project = FdiProject.Create(
            TenantId, "FDI-001", "FDI Test",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        project.ProjectType.Should().Be(ProjectType.Fdi);
    }

    [Fact]
    public void FdiProject_Create_GeneratesUniqueId()
    {
        var a = FdiProject.Create(TenantId, "FDI-A", "A", ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);
        var b = FdiProject.Create(TenantId, "FDI-B", "B", ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void FdiProject_Create_RaisesProjectCreatedEvent()
    {
        var project = FdiProject.Create(
            TenantId, "FDI-001", "FDI Test",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }
}
