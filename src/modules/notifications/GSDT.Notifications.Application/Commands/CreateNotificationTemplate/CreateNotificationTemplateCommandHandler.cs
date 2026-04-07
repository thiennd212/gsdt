using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandHandler(
    INotificationTemplateRepository repository,
    ICurrentUser currentUser) : IRequestHandler<CreateNotificationTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateNotificationTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var channel = NotificationChannel.From(request.Channel);

        // Check key uniqueness per tenant+channel
        var existing = await repository.FindByKeyAsync(
            request.TemplateKey, request.Channel, request.TenantId, cancellationToken);
        if (existing is not null)
            return Result.Fail($"Template key '{request.TemplateKey}' already exists for channel '{request.Channel}'.");

        var template = NotificationTemplate.Create(
            request.TenantId,
            request.TemplateKey,
            request.SubjectTemplate,
            request.BodyTemplate,
            channel,
            currentUser.UserId);

        await repository.AddAsync(template, cancellationToken);
        return Result.Ok(template.Id);
    }
}
