using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IDomainEventPublisher _events;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IDomainEventPublisher events)
    {
        _userManager = userManager;
        _events = events;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(cmd.Email);
        if (existing is not null)
            return Result.Fail(new ConflictError($"Email '{cmd.Email}' already registered"));

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = cmd.FullName,
            Email = cmd.Email,
            UserName = cmd.Email,
            DepartmentCode = cmd.DepartmentCode,
            TenantId = cmd.TenantId,
            // Password expires in 90 days per QĐ742 policy
            PasswordExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        var result = await _userManager.CreateAsync(user, cmd.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationError(e.Description));
            return Result.Fail(errors);
        }

        await _events.PublishEventsAsync(
            [new UserCreatedEvent(user.Id, user.FullName, user.Email!, user.TenantId)], ct);

        return Result.Ok(user.Id);
    }
}
