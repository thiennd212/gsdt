using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using GSDT.SharedKernel.Application;
using GSDT.SharedKernel.Application.Behaviors;

namespace GSDT.SharedKernel.Tests.Application.Behaviors;

// Must be public/internal (not nested private) so NSubstitute can proxy ILogger<AuditBehavior<T,R>>
public sealed record AuditTestCommand : IBaseCommand, IRequest<Result>;

/// <summary>
/// Tests AuditBehavior: logs on success, skips failed results, never breaks pipeline on audit failure.
/// TC-SK-A005
/// </summary>
public sealed class AuditBehaviorTests
{
    private static (AuditBehavior<T, TResponse> sut, IAuditLogger auditLogger)
        CreateSut<T, TResponse>() where T : IBaseCommand
    {
        var auditLogger = Substitute.For<IAuditLogger>();
        // Use NullLogger to avoid Castle proxy accessibility issue with private nested types
        var logger = NullLogger<AuditBehavior<T, TResponse>>.Instance;
        var sut = new AuditBehavior<T, TResponse>(auditLogger, logger);
        return (sut, auditLogger);
    }

    [Fact]
    public async Task Handle_SuccessfulCommand_CallsAuditLogger()
    {
        // TC-SK-A005: AuditBehavior logs command execution on success
        var (sut, auditLogger) = CreateSut<AuditTestCommand, Result>();

        await sut.Handle(
            new AuditTestCommand(),
            ct => Task.FromResult(Result.Ok()),
            CancellationToken.None);

        await auditLogger.Received(1).LogCommandAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FailedResult_DoesNotCallAuditLogger()
    {
        // Audit only logs success — failed commands are not audited
        var (sut, auditLogger) = CreateSut<AuditTestCommand, Result>();

        await sut.Handle(
            new AuditTestCommand(),
            ct => Task.FromResult(Result.Fail("something went wrong")),
            CancellationToken.None);

        await auditLogger.DidNotReceive().LogCommandAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AuditLoggerThrows_DoesNotPropagateException()
    {
        // Audit failure must never crash the main pipeline
        var (sut, auditLogger) = CreateSut<AuditTestCommand, Result>();
        auditLogger
            .LogCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => Task.FromException(new Exception("audit store unavailable")));

        var act = async () => await sut.Handle(
            new AuditTestCommand(),
            ct => Task.FromResult(Result.Ok()),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_SuccessfulCommand_ReturnsOriginalResponse()
    {
        var (sut, _) = CreateSut<AuditTestCommand, Result>();

        var result = await sut.Handle(
            new AuditTestCommand(),
            ct => Task.FromResult(Result.Ok()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
