using FluentAssertions;
using GSDT.InvestmentProjects.Domain.Entities;
using GSDT.InvestmentProjects.Domain.Enums;

namespace GSDT.InvestmentProjects.Domain.Tests.Entities;

/// <summary>Unit tests for DnnnProject factory method — verifies ProjectType, Id, domain event.</summary>
public sealed class DnnnProjectTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ManagingAuthId = Guid.NewGuid();
    private static readonly Guid IndustrySectorId = Guid.NewGuid();
    private static readonly Guid ProjectOwnerId = Guid.NewGuid();
    private static readonly Guid ProjectGroupId = Guid.NewGuid();

    private static DnnnProject CreateDefault() =>
        DnnnProject.Create(
            TenantId, "DNNN-001", "DNNN Test Project",
            ManagingAuthId, IndustrySectorId, ProjectOwnerId, ProjectGroupId);

    [Fact]
    public void DnnnProject_Create_SetsProjectTypeToDnnn()
    {
        var project = CreateDefault();
        project.ProjectType.Should().Be(ProjectType.Dnnn);
    }

    [Fact]
    public void DnnnProject_Create_GeneratesUniqueId()
    {
        var a = CreateDefault();
        var b = CreateDefault();
        a.Id.Should().NotBe(b.Id);
        a.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DnnnProject_Create_SetsRequiredFields()
    {
        var project = CreateDefault();
        project.ProjectCode.Should().Be("DNNN-001");
        project.ProjectName.Should().Be("DNNN Test Project");
        project.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void DnnnProject_Create_RaisesProjectCreatedEvent()
    {
        var project = CreateDefault();
        project.DomainEvents.Should().ContainSingle(e =>
            e is GSDT.InvestmentProjects.Domain.Events.ProjectCreatedEvent);
    }

    [Fact]
    public void DnnnProject_Create_DefaultSubProjectType_IsNotSubProject()
    {
        var project = CreateDefault();
        project.SubProjectType.Should().Be(SubProjectType.NotSubProject);
    }
}
