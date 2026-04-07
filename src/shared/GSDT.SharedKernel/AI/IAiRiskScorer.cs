using FluentResults;

namespace GSDT.SharedKernel.AI;

/// <summary>
/// AI-powered risk scoring for entities (cases, submissions, users).
/// Returns 0-100 score with level and reasoning.
/// Stub default: Score=0, Level=Low (safe fallback).
/// </summary>
public interface IAiRiskScorer
{
    Task<Result<RiskScore>> ScoreAsync(RiskScoringRequest request, CancellationToken ct = default);
}
