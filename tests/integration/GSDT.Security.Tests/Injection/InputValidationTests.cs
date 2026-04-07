using GSDT.Cases.Application.Commands.CreateCase;
using GSDT.Cases.Domain.Entities;

namespace GSDT.Security.Tests.Injection;

/// <summary>
/// Input validation tests — TC-SEC-INJ-001, TC-SEC-INJ-002, TC-SEC-VAL-001.
/// Tests FluentValidation validators reject malicious input payloads.
/// SQL injection prevention at ORM level (EF Core parameterized queries) is implicit;
/// these tests verify validators don't crash on injection payloads and length limits hold.
/// </summary>
public sealed class InputValidationTests
{
    private readonly CreateCaseCommandValidator _validator = new();

    [Fact]
    [Trait("Category", "Security")]
    public void SqlInjectionPayload_InTitle_ValidatorDoesNotCrash()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(),
            "'; DROP TABLE Cases; --",  // SQL injection attempt — too short (< 10 chars? actually 27 chars)
            "A valid description that is long enough for validation",
            CaseType.Application,
            CasePriority.Medium);

        var result = _validator.Validate(cmd);

        // Validator should not throw — just validate normally
        // Title length >= 10, so it passes length check. SQL injection is handled by EF Core.
        result.Errors.Should().NotContain(e =>
            e.ErrorMessage.Contains("Exception") || e.ErrorMessage.Contains("SQL"));
    }

    [Fact]
    [Trait("Category", "Security")]
    public void XssPayload_InDescription_ValidatorDoesNotCrash()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(),
            "Test Case with XSS attempt",
            "<script>alert('xss')</script><img src=x onerror=alert(1)>",
            CaseType.Complaint,
            CasePriority.High);

        var result = _validator.Validate(cmd);

        // XSS is handled by JSON serialization (System.Text.Json encodes HTML)
        // Validator should not crash on HTML content
        result.Errors.Should().NotContain(e =>
            e.ErrorMessage.Contains("Exception"));
    }

    [Fact]
    [Trait("Category", "Security")]
    public void EmptyTitle_Rejected()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(), "", "Valid description with enough chars",
            CaseType.Application, CasePriority.Low);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    [Trait("Category", "Security")]
    public void TitleExceedsMaxLength_Rejected()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(),
            new string('A', 201), // Exceeds 200 char limit
            "Valid description with enough characters for the test",
            CaseType.Request, CasePriority.Medium);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    [Trait("Category", "Security")]
    public void DescriptionExceedsMaxLength_Rejected()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(),
            "Valid case title here",
            new string('B', 2001), // Exceeds 2000 char limit
            CaseType.Report, CasePriority.Critical);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    [Trait("Category", "Security")]
    public void EmptyTenantId_Rejected()
    {
        var cmd = new CreateCaseCommand(
            Guid.Empty, // Empty tenant
            "Valid case title",
            "Valid description with enough characters",
            CaseType.Application, CasePriority.Low);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantId");
    }

    [Fact]
    [Trait("Category", "Security")]
    public void InvalidEnumValue_Rejected()
    {
        var cmd = new CreateCaseCommand(
            Guid.NewGuid(),
            "Valid case title here",
            "Valid description with enough characters",
            (CaseType)999, // Invalid enum
            CasePriority.Low);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
    }
}
