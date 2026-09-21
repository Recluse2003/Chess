using Chess.Application.Common.Results;
using MediatR;

namespace Chess.Application.Games.Queries.VerifyGameMembershipQuery
{
    public record VerifyGameMembershipQuery(Guid GameId, string UserId) : IRequest<Result>;
}
