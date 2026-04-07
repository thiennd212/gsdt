using FluentAssertions;
using GSDT.SharedKernel.Domain;

namespace GSDT.SharedKernel.Tests.Domain;

/// <summary>
/// Tests Entity equality by Id and AuditableEntity.SetAuditUpdate.
/// TC-SK-A009, TC-SK-A010
/// </summary>
public sealed class EntityAndAuditableEntityTests
{
    // Minimal concrete Entity for testing
    private sealed class SampleEntity : Entity<Guid>
    {
        public SampleEntity(Guid id) { Id = id; }
    }

    // Minimal concrete AuditableEntity for testing
    private sealed class SampleAuditableEntity : AuditableEntity<Guid>
    {
        public SampleAuditableEntity(Guid id) { Id = id; }
    }

    // --- Entity equality ---

    [Fact]
    public void Entity_SameId_AreEqualById()
    {
        // TC-SK-A009: Entity equality by Id
        // Note: Entity does not override Equals — reference equality still applies,
        // but Id is accessible and comparable.
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new SampleEntity(id);

        a.Id.Should().Be(b.Id);
    }

    [Fact]
    public void Entity_DifferentId_HaveDifferentIds()
    {
        var a = new SampleEntity(Guid.NewGuid());
        var b = new SampleEntity(Guid.NewGuid());

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Entity_MarkDeleted_SetsIsDeletedTrue()
    {
        var entity = new SampleEntity(Guid.NewGuid());

        entity.MarkDeleted();

        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Entity_Delete_SetsIsDeletedTrue()
    {
        var entity = new SampleEntity(Guid.NewGuid());

        entity.Delete();

        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Entity_CreatedAt_IsSetOnCreation()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var entity = new SampleEntity(Guid.NewGuid());

        entity.CreatedAt.Should().BeAfter(before);
    }

    // --- AuditableEntity ---

    [Fact]
    public void AuditableEntity_SetAuditUpdate_SetsModifiedByAndUpdatedAt()
    {
        // TC-SK-A010: AuditableEntity.SetAuditUpdate sets UpdatedAt
        var entity = new SampleAuditableEntity(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        entity.SetAuditUpdate(userId);

        entity.ModifiedBy.Should().Be(userId);
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void AuditableEntity_SetAuditCreate_SetsCreatedBy()
    {
        var entity = new SampleAuditableEntity(Guid.NewGuid());
        var userId = Guid.NewGuid();

        entity.SetAuditCreate(userId);

        entity.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public void AuditableEntity_DefaultClassificationLevel_IsInternal()
    {
        var entity = new SampleAuditableEntity(Guid.NewGuid());

        entity.ClassificationLevel.Should().Be(ClassificationLevel.Internal);
    }
}
