using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.BulkImportUsers;

public sealed class BulkImportUsersCommandHandler
    : IRequestHandler<BulkImportUsersCommand, Result<BulkImportResultDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public BulkImportUsersCommandHandler(
        UserManager<ApplicationUser> userManager,
    {
        _userManager = userManager;
        _events = events;
    }

    public async Task<Result<BulkImportResultDto>> Handle(
        BulkImportUsersCommand cmd, CancellationToken ct)
    {
        var errors = new List<BulkImportRowError>();
        int successCount = 0;

        // Generate temp password — user must reset on first login
        const string tempPasswordTemplate = "Temp@{0}!Gov";

        foreach (var row in cmd.Rows)
        {
            try
            {
                var existing = await _userManager.FindByEmailAsync(row.Email);
                if (existing is not null)
                {
                    errors.Add(new BulkImportRowError(row.RowNumber, row.Email, "Email already exists"));
                    continue;
                }

                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    FullName = row.FullName,
                    Email = row.Email,
                    UserName = row.Email,
                    DepartmentCode = row.DepartmentCode,
                    TenantId = cmd.TenantId,
                    PasswordExpiresAt = DateTime.UtcNow // Force immediate password change
                };

                var tempPassword = string.Format(tempPasswordTemplate, Guid.NewGuid().ToString("N")[..8]);
                var result = await _userManager.CreateAsync(user, tempPassword);

                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    errors.Add(new BulkImportRowError(row.RowNumber, row.Email, msg));
                    continue;
                }

                if (!string.IsNullOrEmpty(row.InitialRole))
                    await _userManager.AddToRoleAsync(user, row.InitialRole);

                await _events.PublishAsync(
                    [new UserCreatedEvent(user.Id, user.FullName, user.Email!, user.TenantId)], ct);

                successCount++;
            }
            catch (Exception)
            {
                errors.Add(new BulkImportRowError(row.RowNumber, row.Email, "Import failed for this row."));
            }
        }

        return Result.Ok(new BulkImportResultDto(successCount, errors.Count, errors));
    }
}
