using FluentValidation;

namespace GSDT.Integration.Application.Commands.CreateMessageLog;

public sealed class CreateMessageLogCommandValidator : AbstractValidator<CreateMessageLogCommand>
{
    public CreateMessageLogCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.Direction).IsInEnum();
        RuleFor(x => x.MessageType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CorrelationId).MaximumLength(200);
    }
}
