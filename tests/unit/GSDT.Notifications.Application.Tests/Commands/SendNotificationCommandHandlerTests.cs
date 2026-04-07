using GSDT.Notifications.Application.Commands.SendNotification;
using GSDT.Notifications.Application.Providers;
using GSDT.Notifications.Domain.Repositories;
using GSDT.SharedKernel.Domain;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GSDT.Notifications.Application.Tests.Commands;

/// <summary>
/// Unit tests for SendNotificationCommandHandler.
/// Tests channel routing (InApp, Email, SMS), dedup guard, and failure path.
/// All external dependencies are NSubstitute mocks — no database, no HTTP.
/// </summary>
public sealed class SendNotificationCommandHandlerTests
{
    private readonly INotificationRepository _repo = Substitute.For<INotificationRepository>();
    private readonly INotificationLogRepository _logRepo = Substitute.For<INotificationLogRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly ISmsProvider _smsProvider = Substitute.For<ISmsProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly SendNotificationCommandHandler _sut;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid RecipientId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public SendNotificationCommandHandlerTests()
    {
        _currentUser.UserId.Returns(UserId);
        // Default: no duplicate
        _logRepo.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _sut = new SendNotificationCommandHandler(
            _repo, _logRepo, _emailSender, _smsProvider, _currentUser);
    }

    // --- InApp channel happy path ---

    [Fact]
    public async Task Handle_InAppChannel_ReturnsSuccessWithNotificationId()
    {
        var command = BuildCommand("inapp");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_InAppChannel_PersistsNotificationToRepository()
    {
        var command = BuildCommand("inapp");

        await _sut.Handle(command, CancellationToken.None);

        await _repo.Received(1).AddAsync(Arg.Any<Domain.Entities.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InAppChannel_DoesNotCallEmailOrSms()
    {
        var command = BuildCommand("inapp");

        await _sut.Handle(command, CancellationToken.None);

        await _emailSender.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _smsProvider.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // --- Email channel routing ---

    [Fact]
    public async Task Handle_EmailChannel_CallsEmailSender()
    {
        var command = BuildCommand("email", subject: "Welcome", body: "Hello there");

        await _sut.Handle(command, CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            RecipientId.ToString(),
            "Welcome",
            "Hello there",
            Arg.Any<CancellationToken>());
    }

    // --- SMS channel routing ---

    [Fact]
    public async Task Handle_SmsChannel_CallsSmsProvider()
    {
        var command = BuildCommand("sms", body: "Your OTP is 123456");

        await _sut.Handle(command, CancellationToken.None);

        await _smsProvider.Received(1).SendAsync(
            RecipientId.ToString(),
            "Your OTP is 123456",
            Arg.Any<CancellationToken>());
    }

    // --- Dedup guard ---

    [Fact]
    public async Task Handle_DuplicateCorrelationId_ReturnsFailResult()
    {
        var templateId = Guid.NewGuid();
        _logRepo.ExistsAsync(templateId, RecipientId, "corr-001", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = BuildCommand("inapp", templateId: templateId, correlationId: "corr-001");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("already sent"));
    }

    [Fact]
    public async Task Handle_DuplicateCheck_SkippedWhenNoTemplateId()
    {
        // No templateId → dedup check must NOT be called
        var command = BuildCommand("inapp"); // templateId = null

        await _sut.Handle(command, CancellationToken.None);

        await _logRepo.DidNotReceive().ExistsAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // --- Failure path: email sender throws ---

    [Fact]
    public async Task Handle_EmailSenderThrows_MarksNotificationFailedAndRethrows()
    {
        _emailSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var command = BuildCommand("email");

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SMTP unavailable*");
        // UpdateAsync called twice: once for MarkAsSent attempt, once for MarkAsFailed
        await _repo.Received().UpdateAsync(
            Arg.Is<Domain.Entities.Notification>(n => n.Status == Domain.ValueObjects.NotificationStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    // --- Dedup log recorded after successful send ---

    [Fact]
    public async Task Handle_WithTemplateAndCorrelation_LogsDedupEntryAfterSend()
    {
        var templateId = Guid.NewGuid();
        var command = BuildCommand("inapp", templateId: templateId, correlationId: "evt-abc");

        await _sut.Handle(command, CancellationToken.None);

        await _logRepo.Received(1).AddAsync(
            Arg.Is<Domain.Entities.NotificationLog>(l =>
                l.TemplateId == templateId && l.CorrelationId == "evt-abc"),
            Arg.Any<CancellationToken>());
    }

    // --- Helpers ---

    private static SendNotificationCommand BuildCommand(
        string channel,
        string subject = "Test Subject",
        string body = "Test Body",
        Guid? templateId = null,
        string? correlationId = null) =>
        new(TenantId, RecipientId, subject, body, channel, templateId, correlationId);
}
