namespace GSDT.SharedKernel.Api;

/// <summary>
/// RFC 9457 Problem Details + GOV extensions.
/// ErrorCode follows GOV_MODULE_NNN convention (e.g. GOV_CAS_001).
/// DetailVi provides Vietnamese localisation for citizen-facing errors.
/// </summary>
public sealed record ApiError(
    string Code,
    string Message,
    string? DetailVi = null,
    string? TraceId = null,
    string? Property = null);
