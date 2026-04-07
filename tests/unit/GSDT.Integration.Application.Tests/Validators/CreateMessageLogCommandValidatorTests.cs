using GSDT.Integration.Application.Commands.CreateMessageLog;
using GSDT.Integration.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace GSDT.Integration.Application.Tests.Validators;

/// <summary>
/// Unit tests for CreateMessageLogCommandValidator.
/// Verifies required fields, enum validity, and length limits.
/// </summary>
public sealed class CreateMessageLogCommandValidatorTests
{
    private readonly CreateMessageLogCommandValidator _sut = new();

    private static CreateMessageLogCommand MakeCmd(
        Guid? tenantId = null,
        Guid? partnerId = null,
        Guid? contractId = null,
        MessageDirection direction = MessageDirection.Outbound,
        string messageType = "GOV_NOTIFY",
        string? payload = null,
        string? correlationId = "corr-001") =>
        new(tenantId ?? Guid.NewGuid(),
            partnerId ?? Guid.NewGuid(),
            contractId,
            direction, messageType, payload, correlationId);

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _sut.TestValidate(MakeCmd());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_TenantId_Fails() =>
        _sut.TestValidate(MakeCmd(tenantId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.TenantId);

    [Fact]
    public void Empty_PartnerId_Fails() =>
        _sut.TestValidate(MakeCmd(partnerId: Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.PartnerId);

    [Fact]
    public void Invalid_Direction_Fails() =>
        _sut.TestValidate(MakeCmd(direction: (MessageDirection)99))
            .ShouldHaveValidationErrorFor(x => x.Direction);

    [Fact]
    public void Inbound_Direction_Passes()
    {
        var result = _sut.TestValidate(MakeCmd(direction: MessageDirection.Inbound));
        result.ShouldNotHaveValidationErrorFor(x => x.Direction);
    }

    [Fact]
    public void Empty_MessageType_Fails() =>
        _sut.TestValidate(MakeCmd(messageType: ""))
            .ShouldHaveValidationErrorFor(x => x.MessageType);

    [Fact]
    public void MessageType_Exceeds_200_Fails() =>
        _sut.TestValidate(MakeCmd(messageType: new string('x', 201)))
            .ShouldHaveValidationErrorFor(x => x.MessageType);

    [Fact]
    public void CorrelationId_Exceeds_200_Fails() =>
        _sut.TestValidate(MakeCmd(correlationId: new string('x', 201)))
            .ShouldHaveValidationErrorFor(x => x.CorrelationId);

    [Fact]
    public void Null_CorrelationId_Passes()
    {
        var result = _sut.TestValidate(MakeCmd(correlationId: null));
        result.ShouldNotHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void Null_ContractId_Passes()
    {
        var result = _sut.TestValidate(MakeCmd(contractId: null));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
