using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;

namespace GSDT.Infrastructure.Configuration;

/// <summary>
/// IConfigurationSource for HashiCorp Vault KV v2 secrets.
/// Loads all keys from a single secret path and flattens into IConfiguration keys.
/// Graceful fallback: logs warning if Vault unreachable — env vars still work.
/// </summary>
public sealed class VaultConfigurationSource : IConfigurationSource
{
    private readonly VaultOptions _options;

    public VaultConfigurationSource(VaultOptions options) => _options = options;

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new VaultConfigurationProvider(_options);
}

public sealed class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly VaultOptions _options;

    public VaultConfigurationProvider(VaultOptions options) => _options = options;

    public override void Load()
    {
        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // Graceful fallback — env vars / appsettings still provide config
            Console.WriteLine($"[Vault] Warning: Could not load secrets from Vault: {ex.Message}");
        }
    }

    private async Task LoadAsync()
    {
        IAuthMethodInfo authMethod = !string.IsNullOrEmpty(_options.RoleId)
            ? new AppRoleAuthMethodInfo(_options.RoleId, _options.SecretId!)
            : new TokenAuthMethodInfo(_options.Token ?? "root");

        var settings = new VaultClientSettings(_options.Address, authMethod);
        var client = new VaultClient(settings);

        // KV v2: secret/data/gsdt → returns Data.Data dictionary
        var secret = await client.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path: _options.SecretPath.Replace("secret/data/", ""));

        if (secret?.Data?.Data == null) return;

        foreach (var kvp in secret.Data.Data)
        {
            // Vault keys use __ as separator (same as env vars): ConnectionStrings__Default
            // Convert to : for .NET config: ConnectionStrings:Default
            var key = kvp.Key.Replace("__", ":");
            Data[key] = kvp.Value?.ToString() ?? string.Empty;
        }
    }
}

/// <summary>Extension method to register Vault config provider.</summary>
public static class VaultConfigurationExtensions
{
    public static IConfigurationBuilder AddVaultConfiguration(
        this IConfigurationBuilder builder,
        Action<VaultOptions> configure)
    {
        var options = new VaultOptions();
        configure(options);

        if (!options.Enabled) return builder;

        return builder.Add(new VaultConfigurationSource(options));
    }
}
