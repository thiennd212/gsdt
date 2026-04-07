using FluentResults;

namespace GSDT.Identity.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
