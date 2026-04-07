namespace GSDT.Identity.Domain.Entities;

/// <summary>Access review record per QĐ742 — Hangfire 6-month cron creates pending entries.</summary>
public class AccessReviewRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public AccessReviewDecision? Decision { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime NextReviewDue { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum AccessReviewDecision
{
    Retain = 1,
    Revoke = 2
}
