namespace GSDT.Api;

/// <summary>
/// Development-only hosted service that seeds deterministic test data for E2E test runs.
/// Registered only when IsDevelopment() is true — no-op in other environments.
/// Actual seed logic should be implemented when E2E test infrastructure is ready.
/// </summary>
internal sealed class E2ETestDataSeeder : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
