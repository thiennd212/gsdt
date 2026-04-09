using FluentAssertions;
using GSDT.InvestmentProjects.Domain.Entities;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>Unit tests for PppProject factory method — verifies ProjectType, Id, domain event.</summary>
public sealed class PppProjectTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ManagingAuthId = Guid.NewGuid();
    private static readonly Guid IndustrySectorId = Guid.NewGuid();
    private static readonly Guid ProjectOwnerId = Guid.NewGuid();
    private static readonly Guid ProjectGroupId = Guid.NewGuid();

    private static PppProject CreateDefault() =>
        PppProject.Create(
            TenantId, "PPP-001", "PPP Test Project",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId,
            PppContractType.BOT);

    [Fact]
    public void PppProject_Create_SetsProjectTypeToPpp()
    {
        var project = CreateDefault();
        project.ProjectType.Should().Be(ProjectType.Ppp);
    }

    [Fact]
    public void PppProject_Create_GeneratesUniqueId()
    {
        var a = CreateDefault();
        var b = CreateDefault();
        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void PppProject_Create_SetsRequiredFields()
    {
        var project = CreateDefault();
        project.ProjectCode.Should().Be("PPP-001");
        project.ProjectName.Should().Be("PPP Test Project");
        project.TenantId.Should().Be(TenantId);
        project.ManagingAuthorityId.Should().Be(ManagingAuthId);
        project.IndustrySectorId.Should().Be(IndustrySectorId);
        project.ProjectOwnerId.Should().Be(ProjectOwnerId);
        project.ProjectGroupId.Should().Be(ProjectGroupId);
    }

    [Fact]
    public void PppProject_Create_RaisesProjectCreatedEvent()
    {
        var project = CreateDefault();
        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }
}
