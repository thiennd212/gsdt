using FluentResults;

namespace GSDT.SharedKernel.AI;

/// <summary>
/// AI report analysis — narration, NLQ query, anomaly detection.
/// Stub returns empty/placeholder results without throwing.
/// Concrete implementation uses IAiRoutingService + QueryCatalog (no raw Text2SQL).
/// </summary>
public interface IAiReportAnalyst
{
    Task<Result<string>> AnalyzeAsync(ReportAnalysisRequest request, CancellationToken ct = default);
}
