using GSDT.Identity.Application.Commands.DelegateRole;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using GSDT.SharedKernel.Domain.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GSDT.Identity.Application.Tests.Commands;

/// <summary>
/// Tests for DelegateRoleCommandHandler.
/// Covers: valid delegation, self-delegation guard, expired window guard,
/// unauthorized actor guard, and duplicate overlap guard.
/// </summary>
public sealed class DelegateRoleCommandHandlerTests
{
    private readonly IDelegationRepository _delegations;
    private readonly IDomainEventPublisher _events;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DelegateRoleCommandHandler _sut;

    private static readonly Guid DelegatorId = Guid.NewGuid();
    private static readonly Guid DelegateId = Guid.NewGuid();
    private static readonly Guid ActorId = DelegatorId; // actor == delegator by default (self-service)

    public DelegateRoleCommandHandlerTests()
    {
        _delegations = Substitute.For<IDelegationRepository>();
        _events = Substitute.For<IDomainEventPublisher>();

        var store = Substitute.For<IUserStore<ApplicationUser>,
                                   IUserPasswordStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            store,
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());

        _sut = new DelegateRoleCommandHandler(_delegations, _userManager, _events);
    }

    // --- Happy path ---

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithDelegationId()
    {
        _delegations.HasActiveOverlapAsync(
            DelegatorId, DelegateId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var cmd = BuildCommand();
        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsDelegationAndPublishesEvent()
    {
        _delegations.HasActiveOverlapAsync(
            DelegatorId, DelegateId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _delegations.Received(1).AddAsync(
            Arg.Is<UserDelegation>(d => d.DelegatorId == DelegatorId && d.DelegateId == DelegateId),
            Arg.Any<CancellationToken>());
        await _delegations.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _events.Received(1).PublishEventsAsync(Arg.Any<IReadOnlyList<GSDT.SharedKernel.Domain.Events.IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    // --- Validation guards ---

    [Fact]
    public async Task Handle_SelfDelegation_ReturnsFailure()
    {
        var cmd = BuildCommand(delegateId: DelegatorId); // delegate == delegator

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("yourself"));
    }

    [Fact]
    public async Task Handle_ValidFromAfterValidTo_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var cmd = BuildCommand(validFrom: now.AddDays(5), validTo: now.AddDays(1));

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("ValidFrom"));
    }

    [Fact]
    public async Task Handle_ValidToInPast_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var cmd = BuildCommand(validFrom: now.AddDays(-10), validTo: now.AddDays(-1));

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("future"));
    }

    // --- Authorization: actor != delegator, non-admin ---

    [Fact]
    public async Task Handle_UnauthorizedActor_NotAdminRole_ReturnsForbidenFailure()
    {
        var differentActor = Guid.NewGuid();
        var actorUser = new ApplicationUser { Id = differentActor };
        _userManager.FindByIdAsync(differentActor.ToString()).Returns(actorUser);
        _userManager.GetRolesAsync(actorUser).Returns(["GovOfficer"]); // not Admin

        var cmd = BuildCommand(actorId: differentActor);
        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("yourself") || e.Message.Contains("only"));
    }

    [Fact]
    public async Task Handle_ActorIsAdmin_CanDelegateOnBehalfOfOthers()
    {
        var adminId = Guid.NewGuid();
        var adminUser = new ApplicationUser { Id = adminId };
        _userManager.FindByIdAsync(adminId.ToString()).Returns(adminUser);
        _userManager.GetRolesAsync(adminUser).Returns(["Admin"]);
        _delegations.HasActiveOverlapAsync(
            DelegatorId, DelegateId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var cmd = BuildCommand(actorId: adminId);
        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // --- Duplicate overlap ---

    [Fact]
    public async Task Handle_OverlappingDelegationExists_ReturnsConflictFailure()
    {
        _delegations.HasActiveOverlapAsync(
            DelegatorId, DelegateId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("overlap") || e.Message.Contains("active"));
    }

    // --- Helpers ---

    private static DelegateRoleCommand BuildCommand(
        Guid? delegateId = null,
        Guid? actorId = null,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        var now = DateTime.UtcNow;
        return new DelegateRoleCommand(
            DelegatorId: DelegatorId,
            DelegateId: delegateId ?? DelegateId,
            ValidFrom: validFrom ?? now.AddMinutes(1),
            ValidTo: validTo ?? now.AddDays(7),
            Reason: "Test delegation",
            ActorId: actorId ?? ActorId);
    }
}
