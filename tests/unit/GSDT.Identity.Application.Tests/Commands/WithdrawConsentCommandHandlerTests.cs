using GSDT.Identity.Application.Commands.RevokeToken;
using GSDT.Identity.Application.Commands.WithdrawConsent;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using MediatR;

namespace GSDT.Identity.Application.Tests.Commands;

/// <summary>
/// Tests for WithdrawConsentCommandHandler.
/// Covers: success path, ownership violation, not-found, already-withdrawn,
/// and that RevokeToken is sent after successful withdrawal.
/// </summary>
public sealed class WithdrawConsentCommandHandlerTests
{
    private readonly IConsentRepository _consents;
    private readonly ISender _sender;
    private readonly WithdrawConsentCommandHandler _sut;

    private static readonly Guid ConsentId = Guid.NewGuid();
    private static readonly Guid DataSubjectId = Guid.NewGuid();

    public WithdrawConsentCommandHandlerTests()
    {
        _consents = Substitute.For<IConsentRepository>();
        _sender = Substitute.For<ISender>();
        _sut = new WithdrawConsentCommandHandler(_consents, _sender);
    }

    // --- Happy path ---

    [Fact]
    public async Task Handle_ValidOwnerWithdraws_ReturnsSuccess()
    {
        var record = BuildConsent();
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(record);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidOwnerWithdraws_SetsWithdrawnFlagAndTimestamp()
    {
        var record = BuildConsent();
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(record);

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        record.IsWithdrawn.Should().BeTrue();
        record.WithdrawnAt.Should().NotBeNull()
            .And.BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidOwnerWithdraws_SavesChanges()
    {
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(BuildConsent());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _consents.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidOwnerWithdraws_SendsRevokeTokenForDataSubject()
    {
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(BuildConsent());

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        // Handler sends generic Send<Result> — check RevokeTokenCommand with DataSubjectId
        await _sender.Received(1).Send(
            Arg.Is<RevokeTokenCommand>(c => c.UserId == DataSubjectId),
            Arg.Any<CancellationToken>());
    }

    // --- Not found ---

    [Fact]
    public async Task Handle_ConsentNotFound_ReturnsFailure()
    {
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns((ConsentRecord?)null);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(ConsentId.ToString()));
    }

    [Fact]
    public async Task Handle_ConsentNotFound_DoesNotSendRevokeToken()
    {
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns((ConsentRecord?)null);

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        await _sender.DidNotReceiveWithAnyArgs().Send(default!, default);
    }

    // --- Ownership violation ---

    [Fact]
    public async Task Handle_DifferentUser_ReturnsForbidenFailure()
    {
        var record = BuildConsent(); // DataSubjectId != cmd.UserId below
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(record);

        var cmd = new WithdrawConsentCommand(ConsentId, UserId: Guid.NewGuid(), Reason: "other");
        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("own"));
    }

    // --- Already withdrawn ---

    [Fact]
    public async Task Handle_AlreadyWithdrawn_ReturnsConflictFailure()
    {
        var record = BuildConsent(isWithdrawn: true);
        _consents.GetByIdAsync(ConsentId, Arg.Any<CancellationToken>()).Returns(record);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Message.Contains("already") || e.Message.Contains("withdrawn"));
    }

    // --- Helpers ---

    private static ConsentRecord BuildConsent(bool isWithdrawn = false) => new()
    {
        Id = ConsentId,
        DataSubjectId = DataSubjectId,
        Purpose = "case_management",
        LegalBasis = "Consent",
        IsWithdrawn = isWithdrawn,
        WithdrawnAt = isWithdrawn ? DateTime.UtcNow.AddDays(-1) : null
    };

    private static WithdrawConsentCommand BuildCommand() =>
        new(ConsentId, UserId: DataSubjectId, Reason: "no longer needed");
}
