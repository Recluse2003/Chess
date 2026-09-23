using Chess.Application.Common.Results;
using Chess.Domain.Entities;
using MediatR;

namespace Chess.Application.Games.Queries.VerifyGameMembershipQuery
{
    /// <summary>
    /// Contains the required details needed to verify if a specified user is actually player for a specified
    /// <see cref="ChessGame"/>.
    /// </summary>
    /// <param name="GameId">Id of the <see cref="ChessGame"/> that membership is being verified for.</param>
    /// <param name="UserId">Id of the user checking membership for.</param>
    public record VerifyGameMembershipQuery(Guid GameId, string UserId) : IRequest<Result>;
}
