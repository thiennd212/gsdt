namespace GSDT.Integration.Application.DTOs;

public sealed record PartnerDto(
    Guid Id, Guid TenantId, string Name, string Code,
    string? ContactEmail, string? ContactPhone,
    string? Endpoint, string? AuthScheme, string Status,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record ContractDto(
    Guid Id, Guid TenantId, Guid PartnerId, string Title, string? Description,
    DateTime EffectiveDate, DateTime? ExpiryDate, string Status,
    string? DataScopeJson,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record MessageLogDto(
    Guid Id, Guid TenantId, Guid PartnerId, Guid? ContractId,
    string Direction, string MessageType, string? Payload,
    string Status, string? CorrelationId,
    DateTime SentAt, DateTime? AcknowledgedAt,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
