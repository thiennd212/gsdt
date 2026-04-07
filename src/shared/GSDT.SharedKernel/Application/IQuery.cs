using FluentResults;
using MediatR;

namespace GSDT.SharedKernel.Application;

/// <summary>Query marker — read operations returning Result{TResponse}.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
