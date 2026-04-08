namespace GSDT.InvestmentProjects.Domain.Entities;

/// <summary>
/// Investment registration certificate (GCNĐKĐT) for DNNN/NĐT/FDI projects.
/// FK to InvestmentProject base — allows reuse by NĐT and FDI in Phase 6.
/// </summary>
public sealed class RegistrationCertificate : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }

    /// <summary>Certificate number (max 100).</summary>
    public string CertificateNumber { get; set; } = string.Empty;

    /// <summary>Date the certificate was issued.</summary>
    public DateTime IssuedDate { get; set; }

    /// <summary>Guid ref to Files module: certificate document.</summary>
    public Guid? FileId { get; set; }

    /// <summary>Total investment capital stated in certificate — precision (18,4).</summary>
    public decimal InvestmentCapital { get; set; }

    /// <summary>Equity capital stated in certificate — precision (18,4).</summary>
    public decimal EquityCapital { get; set; }

    /// <summary>Equity ratio (auto-calc = EquityCapital / InvestmentCapital) — precision (18,4).</summary>
    public decimal? EquityRatio { get; set; }

    /// <summary>Additional notes (max 2000).</summary>
    public string? Notes { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation — FK to base InvestmentProject for NĐT/FDI reuse
    public InvestmentProject Project { get; set; } = default!;

    private RegistrationCertificate() { } // EF Core

    public static RegistrationCertificate Create(
        Guid tenantId, Guid projectId,
        string certificateNumber, DateTime issuedDate,
        decimal investmentCapital, decimal equityCapital,
        decimal? equityRatio = null, string? notes = null, Guid? fileId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            CertificateNumber = certificateNumber,
            IssuedDate = issuedDate,
            InvestmentCapital = investmentCapital,
            EquityCapital = equityCapital,
            EquityRatio = equityRatio,
            Notes = notes,
            FileId = fileId
        };
}
