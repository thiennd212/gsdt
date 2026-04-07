using System.Security.Cryptography;
using System.Text;

namespace GSDT.Audit.Infrastructure.Services;

/// <summary>
/// Computes and verifies the HMAC-SHA256 chain for audit log entries.
/// Chain: HmacSignature[n] = HMAC-SHA256(key, PreviousSignature[n-1] + EntryFingerprint[n])
/// Called by Hangfire job post-insert — does NOT block the write path.
/// Key source: configuration["Audit:HmacKey"] — Vault integration stub for Phase 02.
/// </summary>
public sealed class HmacChainService(
    IAuditLogRepository repository,
    IConfiguration configuration,
    ILogger<HmacChainService> logger) : IHmacChainVerifier
{
    private byte[] GetKey()
    {
        var keyStr = configuration["Audit:HmacKey"]
            ?? throw new InvalidOperationException(
                "Audit:HmacKey is required. Configure it via secrets manager or environment variable.");
        return Encoding.UTF8.GetBytes(keyStr);
    }

    /// <summary>
    /// Computes HMAC for a single entry using the previous entry's signature as chain input.
    /// Called by Hangfire job: ComputeHmacChainJob.
    /// </summary>
    public async Task ComputeAsync(Guid entryId, CancellationToken cancellationToken = default)
    {
        var entry = await repository.GetByIdAsync(entryId, cancellationToken);
        if (entry is null)
        {
            logger.LogWarning("HmacChainService: AuditLogEntry {EntryId} not found", entryId);
            return;
        }

        if (!string.IsNullOrEmpty(entry.HmacSignature))
        {
            logger.LogDebug("HmacChainService: entry {EntryId} already has HMAC — skipping", entryId);
            return;
        }

        var previous = await repository.GetLatestBeforeSequenceAsync(entry.SequenceId, cancellationToken);
        var previousHash = previous?.HmacSignature ?? string.Empty;

        var fingerprint = BuildFingerprint(entry);
        var input = previousHash + fingerprint;
        var signature = ComputeHmac(input);

        entry.SetHmacSignature(signature);
        await repository.UpdateSignatureAsync(entry.Id, signature, cancellationToken);

        logger.LogDebug("HmacChainService: computed HMAC for entry {EntryId} seq={Seq}", entryId, entry.SequenceId);
    }

    private string ComputeHmac(string input)
    {
        using var hmac = new HMACSHA256(GetKey());
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies HMAC chain integrity for all entries belonging to the given tenant.
    /// Streams entries in SequenceId order, recomputing each HMAC against the previous.
    /// Returns on first tampered entry — does not abort early on missing signatures.
    /// </summary>
    public async Task<ChainVerificationResultDto> VerifyAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        long entriesChecked = 0;
        string previousHash = string.Empty;

        await foreach (var entry in repository.GetByTenantOrderedAsync(tenantId, cancellationToken))
        {
            entriesChecked++;

            if (string.IsNullOrEmpty(entry.HmacSignature))
            {
                // Entry pending HMAC computation — skip, not a tamper
                previousHash = string.Empty;
                continue;
            }

            var fingerprint = BuildFingerprint(entry);
            var input = previousHash + fingerprint;
            var expected = ComputeHmac(input);

            if (!string.Equals(expected, entry.HmacSignature, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "HmacChainService: HMAC mismatch at entry {EntryId} seq={Seq} for tenant {TenantId}",
                    entry.Id, entry.SequenceId, tenantId);

                return new ChainVerificationResultDto(
                    IsValid: false,
                    EntriesChecked: entriesChecked,
                    FirstTamperedEntryId: entry.Id,
                    TamperDetail: $"HMAC mismatch at seq={entry.SequenceId}");
            }

            previousHash = entry.HmacSignature;
        }

        logger.LogInformation(
            "HmacChainService: chain verified for tenant {TenantId}, {Count} entries checked",
            tenantId, entriesChecked);

        return new ChainVerificationResultDto(
            IsValid: true,
            EntriesChecked: entriesChecked,
            FirstTamperedEntryId: null,
            TamperDetail: null);
    }

    private static string BuildFingerprint(AuditLogEntry e) =>
        $"{e.Id}|{e.TenantId}|{e.UserId}|{e.Action}|{e.ModuleName}|{e.ResourceType}|{e.ResourceId}|{e.OccurredAt:O}";
}
