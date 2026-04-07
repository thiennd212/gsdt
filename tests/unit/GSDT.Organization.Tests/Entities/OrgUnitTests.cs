using GSDT.Organization.Entities;
using FluentAssertions;

namespace GSDT.Organization.Tests.Entities;

/// <summary>
/// Unit tests for OrgUnit entity.
/// Covers Create factory, lifecycle methods (Update, Deactivate, SetSuccessor),
/// and hierarchy properties (Level, ParentId).
/// </summary>
public sealed class OrgUnitTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ParentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // --- Factory ---

    private static OrgUnit CreateRoot(
        string name = "Ministry of Justice",
        string nameEn = "Ministry of Justice",
        string code = "MOJ",
        Guid? parentId = null,
        int level = 1) =>
        OrgUnit.Create(name, nameEn, code, TenantId, parentId, level);

    // --- Create ---

    [Fact]
    public void Create_SetsNameAndCode()
    {
        var unit = CreateRoot(name: "Department A", code: "DEPT-A");

        unit.Name.Should().Be("Department A");
        unit.Code.Should().Be("DEPT-A");
    }

    [Fact]
    public void Create_SetsNameEn()
    {
        var unit = CreateRoot(nameEn: "Finance Division");

        unit.NameEn.Should().Be("Finance Division");
    }

    [Fact]
    public void Create_DefaultLevel_IsOne()
    {
        var unit = OrgUnit.Create("Root", "Root", "ROOT", TenantId);

        unit.Level.Should().Be(1);
    }

    [Fact]
    public void Create_WithLevel_SetsCorrectLevel()
    {
        var unit = OrgUnit.Create("Division", "Division", "DIV-01", TenantId, ParentId, level: 3);

        unit.Level.Should().Be(3);
    }

    [Fact]
    public void Create_WithParentId_SetsParentId()
    {
        var unit = CreateRoot(parentId: ParentId);

        unit.ParentId.Should().Be(ParentId);
    }

    [Fact]
    public void Create_WithoutParent_ParentIdIsNull()
    {
        var unit = OrgUnit.Create("Ministry", "Ministry", "MIN", TenantId);

        unit.ParentId.Should().BeNull();
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var unit = CreateRoot();

        unit.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_GeneratesNewId()
    {
        var unit = CreateRoot();

        unit.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_TwoUnits_HaveDifferentIds()
    {
        var a = CreateRoot(code: "A");
        var b = CreateRoot(code: "B");

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Create_SetsTenantId()
    {
        var unit = CreateRoot();

        unit.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Create_SuccessorId_IsNull()
    {
        var unit = CreateRoot();

        unit.SuccessorId.Should().BeNull();
    }

    [Fact]
    public void Create_Children_IsEmpty()
    {
        var unit = CreateRoot();

        unit.Children.Should().BeEmpty();
    }

    // --- Update ---

    [Fact]
    public void Update_ChangesNameAndNameEn()
    {
        var unit = CreateRoot(name: "Old Name", nameEn: "Old English");

        unit.Update("New Name", "New English");

        unit.Name.Should().Be("New Name");
        unit.NameEn.Should().Be("New English");
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var unit = CreateRoot();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        unit.Update("Changed", "Changed");

        unit.UpdatedAt.Should().NotBeNull();
        unit.UpdatedAt.Should().BeAfter(before);
    }

    // --- Deactivate ---

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var unit = CreateRoot();

        unit.Deactivate();

        unit.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_SetsUpdatedAt()
    {
        var unit = CreateRoot();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        unit.Deactivate();

        unit.UpdatedAt.Should().NotBeNull();
        unit.UpdatedAt.Should().BeAfter(before);
    }

    // --- SetSuccessor ---

    [Fact]
    public void SetSuccessor_SetsSuccessorId()
    {
        var unit = CreateRoot();
        var successorId = Guid.NewGuid();

        unit.SetSuccessor(successorId);

        unit.SuccessorId.Should().Be(successorId);
    }

    [Fact]
    public void SetSuccessor_SetsUpdatedAt()
    {
        var unit = CreateRoot();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        unit.SetSuccessor(Guid.NewGuid());

        unit.UpdatedAt.Should().NotBeNull();
        unit.UpdatedAt.Should().BeAfter(before);
    }

    // --- Hierarchy semantics ---

    [Fact]
    public void Create_DeptUnderMinistry_LevelTwoParentSet()
    {
        var ministry = CreateRoot(level: 1);
        var dept = OrgUnit.Create("Department", "Department", "DEPT", TenantId, ministry.Id, level: 2);

        dept.ParentId.Should().Be(ministry.Id);
        dept.Level.Should().Be(2);
    }
}
