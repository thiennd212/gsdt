
namespace GSDT.Identity.Infrastructure.Persistence;

/// <summary>Write-side repository wrapping UserManager — keeps EF tracking semantics.</summary>
public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _userManager.FindByEmailAsync(email);

    public async Task AddAsync(ApplicationUser user, CancellationToken ct = default)
        => await _userManager.CreateAsync(user);

    public async Task UpdateAsync(ApplicationUser user, CancellationToken ct = default)
        => await _userManager.UpdateAsync(user);
}
