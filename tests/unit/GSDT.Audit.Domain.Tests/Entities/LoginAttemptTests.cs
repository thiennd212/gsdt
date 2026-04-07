using GSDT.Audit.Domain.Entities;
using FluentAssertions;

namespace GSDT.Audit.Domain.Tests.Entities;

/// <summary>
/// Unit tests for LoginAttempt entity.
/// TC-AUD-D011: LoginAttempt records success/failure correctly.
/// </summary>
public sealed class LoginAttemptTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    // --- TC-AUD-D011: Records success ---

    [Fact]
    public void Record_SuccessfulLogin_SetsSuccessTrue()
    {
        var attempt = LoginAttempt.Record(UserId, "user@gov.vn", "10.0.0.1", "Mozilla/5.0", true);

        attempt.Success.Should().BeTrue();
    }

    [Fact]
    public void Record_SuccessfulLogin_SetsAllFields()
    {
        var attempt = LoginAttempt.Record(UserId, "user@gov.vn", "192.168.1.100", "Chrome/120", true);

        attempt.UserId.Should().Be(UserId);
        attempt.Email.Should().Be("user@gov.vn");
        attempt.IpAddress.Should().Be("192.168.1.100");
        attempt.UserAgent.Should().Be("Chrome/120");
        attempt.Success.Should().BeTrue();
        attempt.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Record_SuccessfulLogin_FailureReasonIsNull()
    {
        var attempt = LoginAttempt.Record(UserId, "user@gov.vn", "10.0.0.1", null, true);

        attempt.FailureReason.Should().BeNull();
    }

    // --- Records failure ---

    [Fact]
    public void Record_FailedLogin_SetsSuccessFalse()
    {
        var attempt = LoginAttempt.Record(null, "attacker@bad.com", "1.2.3.4", null,
            false, "Invalid password");

        attempt.Success.Should().BeFalse();
    }

    [Fact]
    public void Record_FailedLogin_SetsFailureReason()
    {
        const string reason = "Account locked after 5 attempts";
        var attempt = LoginAttempt.Record(null, "user@gov.vn", "1.2.3.4", null,
            false, reason);

        attempt.FailureReason.Should().Be(reason);
    }

    [Fact]
    public void Record_FailedLogin_WithNullUserId_UserIdIsNull()
    {
        var attempt = LoginAttempt.Record(null, "unknown@bad.com", "5.5.5.5", null,
            false, "User not found");

        attempt.UserId.Should().BeNull();
    }

    // --- Common properties ---

    [Fact]
    public void Record_GeneratesNonEmptyId()
    {
        var attempt = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.1", null, true);

        attempt.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Record_TwoAttempts_HaveDifferentIds()
    {
        var a = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.1", null, true);
        var b = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.2", null, false, "wrong pw");

        a.Id.Should().NotBe(b.Id);
    }

    [Fact]
    public void Record_AttemptedAt_IsApproximatelyUtcNow()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        var attempt = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.1", null, true);

        attempt.AttemptedAt.Should().BeAfter(before);
        attempt.AttemptedAt.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Record_WithUserAgent_StoresUserAgent()
    {
        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        var attempt = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.1", ua, true);

        attempt.UserAgent.Should().Be(ua);
    }

    [Fact]
    public void Record_WithNullUserAgent_UserAgentIsNull()
    {
        var attempt = LoginAttempt.Record(UserId, "u@gov.vn", "10.0.0.1", null, true);

        attempt.UserAgent.Should().BeNull();
    }
}
