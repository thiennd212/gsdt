using FluentResults;

namespace GSDT.Identity.Application.Commands.RevokeToken;

/// <summary>Revoke a specific OpenIddict token or all tokens for a user.</summary>
public sealed record RevokeTokenCommand(
    string? TokenId,
    Guid? UserId,
    Guid ActorId) : ICommand;
